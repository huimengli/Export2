#define Record
#define Record2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Export.Tools;
using System.Threading;
using Export.SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

using Device = SharpDX.Direct3D11.Device;
using Resource = SharpDX.DXGI.Resource;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using ResultCode = SharpDX.DXGI.ResultCode;
using Object = System.Object;
using Screen = System.Windows.Forms.Screen;

namespace Export
{
    class RecordWindow : EditorWindow
    {
        /// <summary>
        /// 录制位置
        /// </summary>
        private static string Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\output.mkv";

        /// <summary>
        /// 页面
        /// </summary>
        public static RecordWindow window;

        /// <summary>
        /// 录制
        /// </summary>
        [MenuItem("我的工具/录制... %g")]
        [STAThread]
        static void Record()
        {
            Debug.Log(Path);
            if (window == null)
            {
                window = CreateInstance<RecordWindow>();
            }
            window.Show();
        }

        /// <summary>
        /// 判断是否能录制
        /// </summary>
        /// <returns></returns>
        [MenuItem("我的工具/录制... %g", true)]
        [STAThread]
        static bool CanRecord()
        {
            return true;
        }

        /// <summary>
        /// 设备
        /// </summary>
        private Device device;

        /// <summary>
        /// 录制线程
        /// </summary>
        private Thread operation;

        /// <summary>
        /// 锁对象
        /// </summary>
        private readonly object _locker = new Object();

        /// <summary>
        /// 是否启用录制
        /// </summary>
        private bool _isRecording = false;

        private void OnGUI()
        {
            //开始垂直线性布局
            GUILayout.BeginVertical();

            //录制存放位置
            GUILayout.Label("录制文件保存位置:");
            GUILayout.BeginHorizontal();
            GUILayout.TextArea(Path);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                Item.ChoiceFolder(ref Path, "选择保存文件夹");
                Path = Path += "\\output.mkv";
            }
            GUILayout.EndHorizontal();

            #region 用Record.cs(Accord.Video.FFMPEG模块)录制
#if !Record
            //录制按钮
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("开始录制"))
            {
                if (FFMPEG)
                {
                    record = new Record(Path);
                    record.Start();
                }
                else
                {
                    Debug.LogError("Accord.Video.FFMPEG.dll未加载或者无法使用!");
                }
            }
            if (GUILayout.Button("结束录制"))
            {
                if (record != null)
                {
                    record.Stop();
                    record = null;
                }
                else
                {
                    Debug.LogAssertion("录制尚未开启");
                }
            }
            GUILayout.EndHorizontal(); 
#endif
            #endregion

            #region 用Record2.cs(ffmpeg.exe)录制
#if !Record2
            //ffmpeg.exe位置
            GUILayout.Label("ffmpeg.exe存放位置:");
            GUILayout.BeginHorizontal();
            GUILayout.TextArea(FFMPEGEXEPATH);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                //Item.ChoiceFolder(ref FFMPEGEXEPATH, "选择ffmpeg.exe所在的位置");
                if (string.IsNullOrEmpty(FFMPEGEXEPATH))
                {
                    Item.ChoiceFile(ref FFMPEGEXEPATH, "选择ffmpeg.exe所在的位置", FFMPEGEXEPATH, "ffmpeg.exe");
                }
                else
                {
                    Item.ChoiceFile(ref FFMPEGEXEPATH, "选择ffmpeg.exe所在的位置", Environment.SpecialFolder.MyDocuments, "ffmpeg.exe");
                }
            }
            GUILayout.EndHorizontal();
            //录制按钮
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("开始录制"))
            {
                if (string.IsNullOrEmpty(FFMPEGEXEPATH))
                {
                    Debug.LogAssertion("ffmpeg.exe未定位!");
                }
                else if (string.IsNullOrEmpty(Path))
                {
                    Debug.LogAssertion("录制位置未定位!");
                }
                else if (record2 == null)
                {
                    record2 = new Record2(Path, FFMPEGEXEPATH);
                    record2.Start();
                    Debug.Log("开始录制");
                }
                else
                {
                    record2.SetPATH(Path);
                    var ffpath = string.IsNullOrWhiteSpace(record2.FFMPEGPATH) ? FFMPEGEXEPATH : record2.FFMPEGPATH;
                    record2.Start(ffpath);
                    Debug.Log("开始录制");
                }
            }
            if (GUILayout.Button("结束录制"))
            {
                if (record2 != null)
                {
                    record2.Stop();
                    record2 = null;
                    Debug.Log("录制完成,等待文件写入");
                    //hread.Sleep(10 * 1000);
                    Item.UseCmd($"explorer /select,{Path}");
                    window.Close();
                }
                else
                {
                    Debug.LogAssertion("录制尚未开启");
                }
            }
            GUILayout.EndHorizontal();
#endif
            #endregion

            #region 用DirectX录制
            // 判断是否支持DirectX录制
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("开始录制"))
            {
                device?.Dispose();
                device = new Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport);
                _isRecording = true;
            }
            if (GUILayout.Button("结束录制"))
            {
                _isRecording = false;
            }
            GUILayout.EndHorizontal();
            #endregion

            GUILayout.EndVertical();
        }

        /// <summary>
        /// 初始化录制
        /// </summary>
        private void Start()
        {
            lock (_locker)
            {
                operation?.Abort();
                operation = new Thread(() =>
                {
                    using (var dxgiSC = new DXGIScreenCapture(Screen.PrimaryScreen))
                    {
                        while (_isRecording)
                        {
                            // 执行操作
                            using (var bitmap = dxgiSC.Capture())
                            {
                                if (bitmap == null)
                                {
                                    continue;
                                }
                                // 处理捕获的位图

                            }
                        }
                    }
                });
            }
        }

        /// <summary>
        /// 停止录制
        /// </summary>
        private void Stop()
        {
            lock (_locker)
            {

            }
        }
    }
}
