#define Record
#define Record2

using Export.Tools;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Threading;
using UnityEditor;
using UnityEngine;

using Object = System.Object;

namespace Export
{
    class RecordWindow : EditorWindow
    {
        /// <summary>
        /// 录制位置
        /// </summary>
        private static string Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\output.mp4";

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
        /// 录制线程
        /// </summary>
        private Thread operation;

        /// <summary>
        /// 锁对象
        /// </summary>
        private readonly object _locker = new Object();

        /// <summary>
        /// 写入对象
        /// </summary>
        private VideoWriter writer;

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
                Path = Path += "\\output.mp4";
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
                Start();
            }
            if (GUILayout.Button("结束录制"))
            {
                Stop();
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
            _isRecording = true;
            Debug.Log("开始录制");

            lock (_locker)
            {
                operation?.Abort();
                operation = new Thread(() =>
                {
                    var desktopSize = new Size();
                    while (true)
                    {
                        var desktop = ImageProcessing.GetScreen();
                        if (desktop != null)
                        {
                            desktopSize.Width = desktop.Width;
                            desktopSize.Height = desktop.Height;
                            desktop?.Dispose();
                            desktop = null;
                            break;
                        }
                    }
                    writer?.Dispose();
                    writer = new VideoWriter(Path, FourCC.H264, 10, desktopSize);
                    while (_isRecording)
                    {
                        // 执行操作
                        using (var bitmap = ImageProcessing.GetScreen())
                        {
                            if (bitmap == null)
                            {
                                continue;
                            }
                            // 处理捕获的位图
                            Mat mat = BitmapConverter.ToMat(bitmap);
                            writer.Write(mat);

                            // 等待
                            Thread.Sleep(100);
                        }
                    }
                    writer.Release();
                });
                operation.Start();
            }
        }

        /// <summary>
        /// 停止录制
        /// </summary>
        private void Stop()
        {
            lock (_locker)
            {
                _isRecording = false;
                Debug.Log("结束录制");
                
                // 打开文件
                Item.OpenFile(Path);
            }
        }
    }
}
