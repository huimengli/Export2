using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Export.Tools
{
    /// <summary>
    /// 图像处理模块
    /// </summary>
    public class ImageProcessing
    {
        #region Win32 API
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr ptr);
        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(
        IntPtr hdc, // handle to DC
        int nIndex // index of capability
        );
        [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
        static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);


        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest,
            IntPtr hdcSrc, int xSrc, int ySrc, int rop);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);
        #endregion
        #region DeviceCaps常量
        const int HORZRES = 8;
        const int VERTRES = 10;
        const int LOGPIXELSX = 88;
        const int LOGPIXELSY = 90;
        const int DESKTOPVERTRES = 117;
        const int DESKTOPHORZRES = 118;
        #endregion
        #region 属性
        /// <summary>
        /// 获取屏幕分辨率当前物理大小
        /// </summary>
        public static Size WorkingArea
        {
            get
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                Size size = new Size();
                size.Width = GetDeviceCaps(hdc, HORZRES);
                size.Height = GetDeviceCaps(hdc, VERTRES);
                ReleaseDC(IntPtr.Zero, hdc);
                return size;
            }
        }
        /// <summary>
        /// 当前系统DPI_X 大小 一般为96
        /// </summary>
        public static int DpiX
        {
            get
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                int DpiX = GetDeviceCaps(hdc, LOGPIXELSX);
                ReleaseDC(IntPtr.Zero, hdc);
                return DpiX;
            }
        }
        /// <summary>
        /// 当前系统DPI_Y 大小 一般为96
        /// </summary>
        public static int DpiY
        {
            get
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                int DpiX = GetDeviceCaps(hdc, LOGPIXELSY);
                ReleaseDC(IntPtr.Zero, hdc);
                return DpiX;
            }
        }
        /// <summary>
        /// 获取真实设置的桌面分辨率大小
        /// </summary>
        public static Size DESKTOP
        {
            get
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                Size size = new Size();
                size.Width = GetDeviceCaps(hdc, DESKTOPHORZRES);
                size.Height = GetDeviceCaps(hdc, DESKTOPVERTRES);
                ReleaseDC(IntPtr.Zero, hdc);
                return size;
            }
        }

        /// <summary>
        /// 获取宽度缩放百分比
        /// </summary>
        public static float ScaleX
        {
            get
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                int t = GetDeviceCaps(hdc, DESKTOPHORZRES);
                int d = GetDeviceCaps(hdc, HORZRES);
                float ScaleX = (float)GetDeviceCaps(hdc, DESKTOPHORZRES) / (float)GetDeviceCaps(hdc, HORZRES);
                ReleaseDC(IntPtr.Zero, hdc);
                return ScaleX;
            }
        }
        /// <summary>
        /// 获取高度缩放百分比
        /// </summary>
        public static float ScaleY
        {
            get
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                float ScaleY = (float)(float)GetDeviceCaps(hdc, DESKTOPVERTRES) / (float)GetDeviceCaps(hdc, VERTRES);
                ReleaseDC(IntPtr.Zero, hdc);
                return ScaleY;
            }
        }
        #endregion

        /// <summary>
        /// 原片
        /// </summary>
        public static Bitmap NowScreen;

        /// <summary>
        /// 截图
        /// </summary>
        /// <returns></returns>
        public static Bitmap GetScreen()
        {
            Screen main = Screen.PrimaryScreen;//获取主显示屏
            var ScreenArea = DESKTOP;
            var ret = new Bitmap(ScreenArea.Width, ScreenArea.Height);
            using (Graphics g = Graphics.FromImage(ret))
            {
                g.CopyFromScreen(0, 0, 0, 0, new Size(ScreenArea.Width, ScreenArea.Height));
            }
            return ret;
        }

        /// <summary>
        /// 根据屏幕获取位图
        /// </summary>
        /// <param name="screen"></param>
        public static Bitmap GetScreen(Screen screen)
        {
            var bounds = screen.Bounds;
            var size = new Size(
                bounds.Width % 2 == 0 ? bounds.Width : bounds.Width + 1,
                bounds.Height % 2 == 0 ? bounds.Height : bounds.Height + 1
            );
            Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size);
            }
            return bitmap;
        }

        /// <summary>
        /// 画面捕获
        /// </summary>
        /// <returns></returns>
        public static Bitmap CaptureScreen()
        {
            IntPtr hdcSrc = GetWindowDC(GetDesktopWindow());
            IntPtr hdcDest = CreateCompatibleDC(hdcSrc);
            IntPtr hBitmap = CreateCompatibleBitmap(hdcSrc,
                System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
            IntPtr hOld = SelectObject(hdcDest, hBitmap);
            BitBlt(hdcDest, 0, 0,
                System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height,
                hdcSrc, 0, 0, 0x00CC0020);

            Bitmap bmp = Image.FromHbitmap(hBitmap);

            SelectObject(hdcDest, hOld);
            DeleteObject(hBitmap);
            DeleteDC(hdcDest);
            DeleteDC(hdcSrc);

            return bmp;
        }

        /// <summary>
        /// 获取变动
        /// </summary>
        /// <returns></returns>
        public static Bitmap GetScreenChange()
        {
            if (NowScreen == null)
            {
                NowScreen = GetScreen();
                return NowScreen;
            }
            else
            {
                var newScreen = GetScreen();
                var wight = NowScreen.Width;
                var height = NowScreen.Height;
                var retScreen = new Bitmap(wight, height);
                var pixel1 = new Color();
                var pixel2 = new Color();
                var retPixel = new Color();

                for (int i = 0; i < wight; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        int r, g, b;
                        pixel1 = NowScreen.GetPixel(i, j);
                        pixel2 = newScreen.GetPixel(i, j);

                        r = pixel2.R - pixel1.R;
                        g = pixel2.G - pixel1.G;
                        b = pixel2.B - pixel1.B;

                        retPixel = retPixel.FromRgb(r, g, b);
                        retScreen.SetPixel(i, j, retPixel);
                    }
                }

                return retScreen;
            }
        }

        #region 快速处理，需要Unsafe编译和多线程分割
        ///// <summary>
        ///// 快速获取变动
        ///// </summary>
        ///// <returns></returns>
        //public unsafe static Bitmap GetScreenChangeFast()
        //{
        //    if (NowScreen==null)
        //    {
        //        NowScreen = GetScreen();
        //        return NowScreen;
        //    }
        //    Bitmap newScreen = GetScreen();
        //    Bitmap result = new Bitmap(NowScreen.Width, NowScreen.Height);

        //    Rectangle rect = new Rectangle(0, 0, NowScreen.Width, NowScreen.Height);
        //    BitmapData bmpData1 = NowScreen.LockBits(rect, ImageLockMode.ReadOnly, NowScreen.PixelFormat);
        //    BitmapData bmpData2 = newScreen.LockBits(rect, ImageLockMode.ReadOnly, newScreen.PixelFormat);
        //    BitmapData bmpDataResult = result.LockBits(rect, ImageLockMode.WriteOnly, result.PixelFormat);

        //    int bytesPerPixel = Bitmap.GetPixelFormatSize(NowScreen.PixelFormat) / 8;
        //    int heightInPixels = bmpData1.Height;
        //    int widthInBytes = bmpData1.Width * bytesPerPixel;
        //    byte* ptrFirstPixel1 = (byte*)bmpData1.Scan0;
        //    byte* ptrFirstPixel2 = (byte*)bmpData2.Scan0;
        //    byte* ptrFirstPixelResult = (byte*)bmpDataResult.Scan0;

        //    // 使用多线程加速计算过程
        //    int numThreads = Environment.ProcessorCount;
        //    Task[] tasks = new Task[numThreads];
        //    int rowsPerTask = heightInPixels / numThreads;

        //    for (int i = 0; i < numThreads; i++)
        //    {
        //        int startY = i * rowsPerTask;
        //        int endY = (i == numThreads - 1) ? heightInPixels : startY + rowsPerTask;
        //        tasks[i] = Task.Run(() =>
        //        {
        //            for (int y = startY; y < endY; y++)
        //            {
        //                byte* currentLine1 = ptrFirstPixel1 + (y * bmpData1.Stride);
        //                byte* currentLine2 = ptrFirstPixel2 + (y * bmpData2.Stride);
        //                byte* currentLineResult = ptrFirstPixelResult + (y * bmpDataResult.Stride);
        //                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
        //                {
        //                    byte* currentPixel1 = currentLine1 + x;
        //                    byte* currentPixel2 = currentLine2 + x;
        //                    byte* currentPixelResult = currentLineResult + x;

        //                    // 计算当前像素的差值
        //                    currentPixelResult[0] = (byte)Math.Abs(currentPixel1[0] - currentPixel2[0]);
        //                    if (bytesPerPixel > 1)
        //                    {
        //                        currentPixelResult[1] = (byte)Math.Abs(currentPixel1[1] - currentPixel2[1]);
        //                        currentPixelResult[2] = (byte)Math.Abs(currentPixel1[2] - currentPixel2[2]);
        //                    }
        //                    if (bytesPerPixel > 3)
        //                    {
        //                        currentPixelResult[3] = (byte)Math.Abs(currentPixel1[3] - currentPixel2[3]);
        //                    }
        //                }
        //            }
        //        });
        //    }

        //    Task.WaitAll(tasks);

        //    NowScreen.UnlockBits(bmpData1);
        //    newScreen.UnlockBits(bmpData2);
        //    result.UnlockBits(bmpDataResult);

        //    return result;
        //} 
        #endregion

        #region OpenCV处理方法
        //using OpenCvSharp;
        //public static Bitmap Subtract(Bitmap bitmap1, Bitmap bitmap2)
        //{
        //    // 将 Bitmap 转换为 Mat 类型
        //    Mat mat1 = bitmap1.ToMat();
        //    Mat mat2 = bitmap2.ToMat();

        //    // 计算两个 Mat 相减
        //    Mat diffMat = new Mat();
        //    Cv2.Subtract(mat1, mat2, diffMat);

        //    // 将 Mat 转换为 Bitmap 类型
        //    Bitmap diffBitmap = diffMat.ToBitmap();

        //    return diffBitmap;
        //}
        #endregion

        /// <summary>
        /// 图片变更
        /// </summary>
        /// <param name="baseScreen"></param>
        /// <param name="changeScreen"></param>
        /// <returns></returns>
        public static Bitmap AddScreen(Bitmap baseScreen,Bitmap changeScreen)
        {
            var wight = baseScreen.Width;
            var height = baseScreen.Height;
            var retScreen = new Bitmap(wight, height);
            var pixel1 = new Color();
            var pixel2 = new Color();
            var retPixel = new Color();

            for (int i = 0; i < wight; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int r, g, b;
                    pixel1 = baseScreen.GetPixel(i, j);
                    pixel2 = changeScreen.GetPixel(i, j);

                    r = pixel2.R + pixel1.R;
                    g = pixel2.G + pixel1.G;
                    b = pixel2.B + pixel1.B;

                    retPixel = retPixel.FromRgb(r, g, b);
                    retScreen.SetPixel(i, j, retPixel);
                }
            }

            return retScreen;
        }
    }

    /// <summary>
    /// 图像处理模块追加
    /// </summary>
    public static class ImageProcessingAdd{

        /// <summary>
        /// 读取RGB
        /// </summary>
        /// <param name="color"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Color FromRgb(this Color color,int r,int g,int b)
        {
            byte R = (byte)r, G = (byte)g, B = (byte)b;
            color = Color.FromArgb(R, G, B);
            return color;
        }
    }
}
