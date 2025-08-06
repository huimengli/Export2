using System;
using System.Drawing;
using System.Drawing.Imaging;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;
using SharpDX.Direct3D;
using System.Windows.Forms;
using System.IO;

using Device = SharpDX.Direct3D11.Device;
using Resource = SharpDX.DXGI.Resource;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using ResultCode = SharpDX.DXGI.ResultCode;

namespace Export.SharpDX
{
    /// <summary>
    /// DXGI屏幕捕获类
    /// </summary>
    public class DXGIScreenCapture : IDisposable
    {
        private readonly Device _device;
        private readonly OutputDuplication _duplicatedOutput;
        private readonly Texture2D _screenTexture;
        private readonly OutputDescription _outputDesc;

        // 新增字段
        private Bitmap _referenceFrame;  // 参考帧（基础帧）
        private bool _hasReferenceFrame = false;
        private readonly int _width;
        private readonly int _height;
        private readonly object _lock = new object();
        private byte _differenceThreshold = 10; // 差异阈值
        private bool _isDisposed = false;

        // 额外字段
        private int _referenceFrameCount = 0; // 参考帧计数

        /// <summary>
        /// 构造函数，初始化DXGI设备和输出
        /// </summary>
        /// <param name="screen">画面</param>
        /// <param name="differenceThreshold">差异阈值</param>
        public DXGIScreenCapture(Screen screen, byte differenceThreshold = 10)
        {
            _differenceThreshold = differenceThreshold;

            try
            {
                // 1. 初始化Direct3D设备
                _device = new Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport);

                // 2. 查找匹配的显示输出
                bool outputFound = false;
                using (var factory = new Factory1())
                {
                    foreach (var adapter in factory.Adapters)
                    {
                        foreach (var output in adapter.Outputs)
                        {
                            var outputDesc = output.Description;
                            var screenBounds = screen.Bounds;

                            // 精确匹配屏幕位置和尺寸
                            if (outputDesc.DesktopBounds.Left == screenBounds.Left &&
                                outputDesc.DesktopBounds.Top == screenBounds.Top &&
                                outputDesc.DesktopBounds.Right == screenBounds.Right &&
                                outputDesc.DesktopBounds.Bottom == screenBounds.Bottom)
                            {
                                try
                                {
                                    var output1 = output.QueryInterface<Output1>();
                                    _duplicatedOutput = output1.DuplicateOutput(_device);
                                    _outputDesc = outputDesc;
                                    outputFound = true;
                                    break;
                                }
                                catch (SharpDXException ex)
                                {
                                    throw new Exception($"无法复制输出: {ex.Message}", ex);
                                }
                            }
                        }
                        if (outputFound) break;
                    }
                }

                if (!outputFound)
                {
                    throw new InvalidOperationException($"找不到匹配屏幕的输出设备: {screen.DeviceName}");
                }

                // 3. 计算屏幕尺寸
                _width = _outputDesc.DesktopBounds.Right - _outputDesc.DesktopBounds.Left;
                _height = _outputDesc.DesktopBounds.Bottom - _outputDesc.DesktopBounds.Top;

                // 4. 创建纹理用于存储截图
                _screenTexture = new Texture2D(_device, new Texture2DDescription
                {
                    Width = _width,
                    Height = _height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = Format.B8G8R8A8_UNorm,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Staging,
                    BindFlags = BindFlags.None,
                    CpuAccessFlags = CpuAccessFlags.Read,
                    OptionFlags = ResourceOptionFlags.None
                });
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// 设置或获取差异检测阈值(0-255)
        /// </summary>
        public byte DifferenceThreshold
        {
            get => _differenceThreshold;
            set => _differenceThreshold = Math.Min((byte)255, Math.Max((byte)0, value));
        }

        /// <summary>
        /// 获取当前参考帧（如果存在）
        /// </summary>
        /// <returns></returns>
        public Bitmap GetReferenceFrame()
        {
            lock (_lock)
            {
                if (_hasReferenceFrame == false)
                {
                    SetReferenceFrame(Capture());
                }
                return _referenceFrame;
            }
        }

        /// <summary>
        /// 捕获当前屏幕帧
        /// </summary>
        public Bitmap Capture()
        {
            lock (_lock)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException("DXGIScreenCapture");

                if (_duplicatedOutput == null || _duplicatedOutput.IsDisposed)
                    throw new InvalidOperationException("输出复制接口未正确初始化");

                try
                {
                    Resource screenResource;
                    OutputDuplicateFrameInformation frameInfo;

                    // 尝试获取下一帧
                    if (_duplicatedOutput.TryAcquireNextFrame(1000, out frameInfo, out screenResource)!=Result.Ok)
                    {
                        return null;
                    }

                    using (screenResource)
                    using (var screenTexture2D = screenResource.QueryInterface<Texture2D>())
                    {
                        _device.ImmediateContext.CopyResource(screenTexture2D, _screenTexture);
                    }

                    // 映射纹理到内存
                    var mapSource = _device.ImmediateContext.MapSubresource(
                        _screenTexture,
                        0,
                        MapMode.Read,
                        MapFlags.None);

                    try
                    {
                        var bitmap = new Bitmap(_width, _height, PixelFormat.Format32bppArgb);
                        var boundsRect = new Rectangle(0, 0, _width, _height);

                        var bitmapData = bitmap.LockBits(boundsRect,
                            ImageLockMode.WriteOnly,
                            bitmap.PixelFormat);

                        var sourcePtr = mapSource.DataPointer;
                        var destPtr = bitmapData.Scan0;
                        for (int y = 0; y < _height; y++)
                        {
                            Utilities.CopyMemory(destPtr, sourcePtr, _width * 4);
                            sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                            destPtr = IntPtr.Add(destPtr, bitmapData.Stride);
                        }

                        bitmap.UnlockBits(bitmapData);
                        return bitmap;
                    }
                    finally
                    {
                        _device.ImmediateContext.UnmapSubresource(_screenTexture, 0);
                        _duplicatedOutput.ReleaseFrame();
                    }
                }
                catch (SharpDXException ex) when (ex.ResultCode.Code == ResultCode.WaitTimeout.Result.Code)
                {
                    return null;
                }
                catch (SharpDXException ex) when (ex.ResultCode.Code == ResultCode.AccessLost.Result.Code)
                {
                    Dispose();
                    throw new InvalidOperationException("显示器访问丢失，需要重新初始化捕获器", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception("屏幕捕获失败", ex);
                }
            }
        }

        /// <summary>
        /// 设置参考帧（用于后续差异比较）
        /// </summary>
        public void SetReferenceFrame(Bitmap referenceFrame)
        {
            lock (_lock)
            {
                _referenceFrameCount++;
                _referenceFrame?.Dispose();
                _referenceFrame = referenceFrame.Clone() as Bitmap;
                _hasReferenceFrame = true;
            }
        }

        /// <summary>
        /// 捕获并设置当前帧为参考帧
        /// </summary>
        public void CaptureReferenceFrame()
        {
            var frame = Capture();
            if (frame != null)
            {
                SetReferenceFrame(frame);
                frame.Dispose();
            }
        }

        /// <summary>
        /// 获取参考帧计数
        /// </summary>
        /// <returns></returns>
        public int GetReferenceFrameCount()
        {

            lock (_lock)
            {
                return _referenceFrameCount;
            }
        }

        /// <summary>
        /// 获取当前帧与参考帧的差异图像
        /// </summary>
        /// <param name="highlightColor">高亮颜色（默认为红色）</param>
        /// <returns>差异图像（透明背景）</returns>
        public Bitmap GetDifferenceImage(Color? highlightColor = null)
        {
            if (!_hasReferenceFrame)
            {
                CaptureReferenceFrame();
                return null;
            }

            using (var currentFrame = Capture())
            using (var currentFrameMemory = new MemoryStream())
            {
                if (currentFrame == null) return null;
                currentFrame.Save(currentFrameMemory, ImageFormat.Png);
                var currentFrameLength = currentFrameMemory.Length;

                // 使用ImageDiffer计算差异
                using (var diffImage = ImageDiffer.GetDifferenceImageOptimized(
                    _referenceFrame,
                    currentFrame,
                    _differenceThreshold,
                    highlightColor
                ))
                using (var diffImageMemory = new MemoryStream())
                {
                    diffImage.Save(diffImageMemory, ImageFormat.Png);
                    var diffImageLength = diffImageMemory.Length;

                    if (diffImageLength > currentFrameLength)
                    {
                        SetReferenceFrame(currentFrame);
                        return currentFrame.Clone() as Bitmap;
                    }
                    else
                    {
                        return diffImage.Clone() as Bitmap;
                    }
                }
            }
        }

        /// <summary>
        /// 获取增量图像（包含实际像素变化值）
        /// </summary>
        public Bitmap GetIncrementalImage()
        {
            lock (_lock)
            {
                if (!_hasReferenceFrame)
                {
                    CaptureReferenceFrame();
                    return null;
                }

                var currentFrame = Capture();
                if (currentFrame == null) return null;

                // 使用ImageDiffer计算增量
                var incremental = ImageDiffer.GetIncrementalImage(
                    _referenceFrame,
                    currentFrame,
                    _differenceThreshold
                );

                currentFrame.Dispose();
                return incremental;
            }
        }

        /// <summary>
        /// 将增量图像应用到参考帧
        /// </summary>
        public Bitmap ApplyIncrementalImage(Bitmap incrementalImage)
        {
            lock (_lock)
            {
                if (!_hasReferenceFrame || incrementalImage == null)
                    return null;

                // 使用ImageDiffer合并图像
                return ImageDiffer.CombineImages(_referenceFrame, incrementalImage);
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_isDisposed) return;

                try
                {
                    _duplicatedOutput?.Dispose();
                    _screenTexture?.Dispose();
                    _device?.Dispose();
                    _referenceFrame?.Dispose();
                }
                finally
                {
                    _isDisposed = true;
                }
            }
        }
    }
}