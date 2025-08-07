using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Export.Tools
{
    /// <summary>
    /// 图片差异计算类
    /// </summary>
    public class ImageDiffer
    {
        private readonly static object _imageLock = new object();

        /// <summary>
        /// 计算两个图像之间的增量图像
        /// </summary>
        /// <param name="baseImage">基础图像</param>
        /// <param name="newImage">新图像</param>
        /// <param name="threshold">差异阈值 (0-255)</param>
        /// <param name="highlightColor">高亮颜色</param>
        /// <returns>增量图像</returns>
        public static Bitmap GetIncrementalImage(Bitmap baseImage, Bitmap newImage,
                                               byte threshold = 10,
                                               Color? highlightColor = null)
        {
            lock (_imageLock)
            {
                // 输入验证
                if (baseImage == null) throw new ArgumentNullException(nameof(baseImage));
                if (newImage == null) throw new ArgumentNullException(nameof(newImage));
                if (baseImage?.Size != newImage?.Size)
                    throw new ArgumentException("图像尺寸必须相同");

                Color highlight = highlightColor ?? Color.Red;

                // 创建增量图像
                Bitmap incremental = new Bitmap(baseImage.Width, baseImage.Height,
                                              PixelFormat.Format32bppArgb);

                // 锁定位图数据
                var baseData = baseImage.LockBits(new Rectangle(0, 0, baseImage.Width, baseImage.Height),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                var newData = newImage.LockBits(new Rectangle(0, 0, newImage.Width, newImage.Height),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                var incrementalData = incremental.LockBits(new Rectangle(0, 0, incremental.Width, incremental.Height),
                    ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                try
                {
                    int bytesPerPixel = 4;
                    int byteCount = baseData.Stride * baseImage.Height;

                    byte[] baseBuffer = new byte[byteCount];
                    byte[] newBuffer = new byte[byteCount];
                    byte[] incrementalBuffer = new byte[byteCount];

                    Marshal.Copy(baseData.Scan0, baseBuffer, 0, byteCount);
                    Marshal.Copy(newData.Scan0, newBuffer, 0, byteCount);

                    for (int i = 0; i < byteCount; i += bytesPerPixel)
                    {
                        byte baseB = baseBuffer[i];
                        byte baseG = baseBuffer[i + 1];
                        byte baseR = baseBuffer[i + 2];
                        byte baseA = baseBuffer[i + 3];

                        byte newB = newBuffer[i];
                        byte newG = newBuffer[i + 1];
                        byte newR = newBuffer[i + 2];
                        byte newA = newBuffer[i + 3];

                        int diffB = Math.Abs(newB - baseB);
                        int diffG = Math.Abs(newG - baseG);
                        int diffR = Math.Abs(newR - baseR);
                        int diffA = Math.Abs(newA - baseA);

                        int maxDiff = Math.Max(Math.Max(diffR, diffG), Math.Max(diffB, diffA));

                        if (maxDiff > threshold)
                        {
                            // 设置增量值
                            incrementalBuffer[i] = (byte)(newB - baseB);
                            incrementalBuffer[i + 1] = (byte)(newG - baseG);
                            incrementalBuffer[i + 2] = (byte)(newR - baseR);
                            incrementalBuffer[i + 3] = 255;
                        }
                        else
                        {
                            // 无差异区域
                            incrementalBuffer[i] = 0;
                            incrementalBuffer[i + 1] = 0;
                            incrementalBuffer[i + 2] = 0;
                            incrementalBuffer[i + 3] = 0;
                        }
                    }

                    Marshal.Copy(incrementalBuffer, 0, incrementalData.Scan0, byteCount);
                }
                finally
                {
                    baseImage.UnlockBits(baseData);
                    newImage.UnlockBits(newData);
                    incremental.UnlockBits(incrementalData);
                }

                return incremental;
            }
        }

        /// <summary>
        /// 将基础图像与增量图像拼合
        /// </summary>
        /// <param name="baseImage">基础图像</param>
        /// <param name="incrementalImage">增量图像</param>
        /// <returns>拼合后的图像</returns>
        public static Bitmap CombineImages(Bitmap baseImage, Bitmap incrementalImage)
        {
            lock (_imageLock)
            {
                if (baseImage == null) throw new ArgumentNullException(nameof(baseImage));
                if (incrementalImage == null) throw new ArgumentNullException(nameof(incrementalImage));
                if (baseImage.Size != incrementalImage.Size)
                    throw new ArgumentException("图像尺寸必须相同");

                Bitmap combined = new Bitmap(baseImage.Width, baseImage.Height,
                                           PixelFormat.Format32bppArgb);

                var baseData = baseImage.LockBits(new Rectangle(0, 0, baseImage.Width, baseImage.Height),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                var incData = incrementalImage.LockBits(new Rectangle(0, 0, incrementalImage.Width, incrementalImage.Height),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                var combData = combined.LockBits(new Rectangle(0, 0, combined.Width, combined.Height),
                    ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                try
                {
                    int bytesPerPixel = 4;
                    int byteCount = baseData.Stride * baseImage.Height;

                    byte[] baseBuffer = new byte[byteCount];
                    byte[] incBuffer = new byte[byteCount];
                    byte[] combBuffer = new byte[byteCount];

                    Marshal.Copy(baseData.Scan0, baseBuffer, 0, byteCount);
                    Marshal.Copy(incData.Scan0, incBuffer, 0, byteCount);

                    for (int i = 0; i < byteCount; i += bytesPerPixel)
                    {
                        byte b = baseBuffer[i];
                        byte g = baseBuffer[i + 1];
                        byte r = baseBuffer[i + 2];
                        byte a = baseBuffer[i + 3];

                        byte incB = incBuffer[i];
                        byte incG = incBuffer[i + 1];
                        byte incR = incBuffer[i + 2];
                        byte incA = incBuffer[i + 3];

                        combBuffer[i] = (byte)(b + incB);
                        combBuffer[i + 1] = (byte)(g + incG);
                        combBuffer[i + 2] = (byte)(r + incR);
                        combBuffer[i + 3] = (byte)Math.Min(255, a + incA);
                    }

                    Marshal.Copy(combBuffer, 0, combData.Scan0, byteCount);
                }
                finally
                {
                    baseImage.UnlockBits(baseData);
                    incrementalImage.UnlockBits(incData);
                    combined.UnlockBits(combData);
                }

                return combined;
            }
        }

        /// <summary>
        /// 计算两个图像之间的差异（优化版本，仅返回差异区域）
        /// </summary>
        public static Bitmap GetDifferenceImageOptimized(Bitmap baseImage, Bitmap newImage,
                                                       byte threshold = 10,
                                                       Color? highlightColor = null)
        {
            // 验证输入
            if (baseImage == null || newImage == null || baseImage?.Tag == null || newImage?.Tag == null || baseImage?.Size != newImage?.Size)
                return GetIncrementalImage(baseImage, newImage, threshold, highlightColor);

            // 设置默认高亮色
            Color highlight = highlightColor ?? Color.Red;

            // 创建结果图像（透明背景）
            Bitmap diffImage = new Bitmap(baseImage.Width, baseImage.Height, PixelFormat.Format32bppArgb);

            // 锁定位图数据
            var baseData = baseImage.LockBits(new Rectangle(0, 0, baseImage.Width, baseImage.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var newData = newImage.LockBits(new Rectangle(0, 0, newImage.Width, newImage.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var diffData = diffImage.LockBits(new Rectangle(0, 0, diffImage.Width, diffImage.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            try
            {
                int bytesPerPixel = 4;
                int width = baseImage.Width;
                int height = baseImage.Height;
                int stride = baseData.Stride;

                // 遍历所有像素
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = y * stride + x * bytesPerPixel;

                        // 获取基础像素
                        byte baseB = Marshal.ReadByte(baseData.Scan0, index);
                        byte baseG = Marshal.ReadByte(baseData.Scan0, index + 1);
                        byte baseR = Marshal.ReadByte(baseData.Scan0, index + 2);
                        byte baseA = Marshal.ReadByte(baseData.Scan0, index + 3);

                        // 获取新像素
                        byte newB = Marshal.ReadByte(newData.Scan0, index);
                        byte newG = Marshal.ReadByte(newData.Scan0, index + 1);
                        byte newR = Marshal.ReadByte(newData.Scan0, index + 2);
                        byte newA = Marshal.ReadByte(newData.Scan0, index + 3);

                        // 计算最大差异
                        int maxDiff = Math.Max(
                            Math.Max(
                                Math.Abs(newR - baseR),
                                Math.Abs(newG - baseG)),
                            Math.Max(
                                Math.Abs(newB - baseB),
                                Math.Abs(newA - baseA)));

                        // 应用阈值
                        if (maxDiff > threshold)
                        {
                            // 写入高亮差异
                            Marshal.WriteByte(diffData.Scan0, index, highlight.B);
                            Marshal.WriteByte(diffData.Scan0, index + 1, highlight.G);
                            Marshal.WriteByte(diffData.Scan0, index + 2, highlight.R);
                            Marshal.WriteByte(diffData.Scan0, index + 3, 255); // 不透明
                        }
                        else
                        {
                            // 透明背景（无差异）
                            Marshal.WriteByte(diffData.Scan0, index + 3, 0); // 完全透明
                        }
                    }
                }
            }
            finally
            {
                // 解锁所有位图
                baseImage.UnlockBits(baseData);
                newImage.UnlockBits(newData);
                diffImage.UnlockBits(diffData);
            }

            return diffImage;
        }
    }
}
