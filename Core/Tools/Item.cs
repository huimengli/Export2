﻿using Export.Forms;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Export.Tools
{
    /// <summary>
    /// 工具类
    /// </summary>
    public class Item
    {
        /// <summary>
        /// 日志打印器
        /// </summary>
        public static Logger logger = new Logger();

        /// <summary>
        /// 选择的文件夹路径
        /// </summary>
        private static string ChousePath;

        /// <summary>
        /// SHA256签名
        /// (不适用于签名中文内容,中文加密和js上的加密不同)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string SHA256(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            byte[] hash = SHA256Managed.Create().ComputeHash(bytes);

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("X2"));
            }

            return builder.ToString();
        }

        /// <summary>
        /// MD5签名
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string MD5(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            var hash = MD5CryptoServiceProvider.Create().ComputeHash(bytes);

            var builder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("x2"));
            }

            return builder.ToString();
        }

        /// <summary>
        /// MD5签名
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string MD5(byte[] data)
        {
            var hash = MD5CryptoServiceProvider.Create().ComputeHash(data);

            var builder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("x2"));
            }

            return builder.ToString();
        }

        /// <summary>
        /// 新建一个UID
        /// </summary>
        /// <returns></returns>
        public static string NewUUID()
        {
            return NewUUID(DateTime.Now.ToJSTime().ToString());
        }

        /// <summary>
        /// 新建一个UID
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string NewUUID(string input)
        {
            input = MD5(input);
            var sha = SHA256(input);
            var uuid = "uuxxxuuy-1xxx-7xxx-yxxx-xxx0xxxy";
            var temp = new StringBuilder();
            var dex = new StringBuilder();
            for (int i = 0; i < uuid.Length; i++)
            {
                var e = uuid[i];
                if (e == 'u')
                {
                    temp.Append(sha[2 * i]);
                    dex.Append(sha[2 * i]);
                }
                else if (e == 'x')
                {
                    temp.Append(input[i]);
                    dex.Append(input[i]);
                }
                else if (e == 'y')
                {
                    temp.Append(MD5(dex.ToString())[i]);
                }
                else
                {
                    temp.Append(e);
                }
            }
            uuid = temp.ToString();
            return uuid;
        }

        /// <summary>
        /// 选择文件夹
        /// </summary>
        /// <param name="label">选择文件夹地址</param>
        /// <param name="tishi">选择时候提示内容</param>
        public static void ChoiceFolder(ref string label, string tishi)
        {
            ChoiceFolder(ref label, tishi, Environment.SpecialFolder.MyDocuments);
        }

        /// <summary>
        /// 选择文件夹
        /// </summary>
        /// <param name="label">选择文件夹地址</param>
        /// <param name="tishi">选择时候提示内容</param>
        /// <param name="path">已经存在的文件路径</param>
        public static void ChoiceFolder(ref string label, string tishi, string path)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = tishi;
            dialog.ShowNewFolderButton = true;
            if (path != String.Empty && path != null)
            {
                dialog.SelectedPath = path;
            }
            //else
            //{
            //    dialog.SelectedPath = dialog.SelectedPath + "\\Hinterland\\TheLongDark";
            //}
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    return;
                }
                //this.LoadingText = "处理中...";
                //this.LoadingDisplay = true;
                //Action<string> a = DaoRuData;
                //a.BeginInvoke(dialog.SelectedPath, asyncCallback, a);
                path = dialog.SelectedPath;
            }
            label = path;
        }

        /// <summary>
        /// 选择文件夹
        /// </summary>
        /// <param name="label">选择文件夹地址</param>
        /// <param name="tishi">选择时候提示内容</param>
        /// <param name="folder">系统文件夹枚举项</param>
        public static void ChoiceFolder(ref string label, string tishi, Environment.SpecialFolder folder)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = tishi;
            dialog.ShowNewFolderButton = true;
            //dialog.RootFolder = folder;
            dialog.SelectedPath = Environment.GetFolderPath(folder);
            var path = dialog.SelectedPath;
            if (ChousePath != String.Empty && ChousePath != null)
            {
                dialog.SelectedPath = ChousePath;
            }
            //else
            //{
            //    dialog.SelectedPath = dialog.SelectedPath + "\\Hinterland\\TheLongDark";
            //}
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    return;
                }
                //this.LoadingText = "处理中...";
                //this.LoadingDisplay = true;
                //Action<string> a = DaoRuData;
                //a.BeginInvoke(dialog.SelectedPath, asyncCallback, a);
                ChousePath = dialog.SelectedPath;
            }
            else if (dialog.ShowDialog() == DialogResult.Cancel)
            {
                ChousePath = path;
            }
            label = ChousePath;
        }

        /// <summary>
        /// 选择指定文件
        /// </summary>
        /// <param name="label"></param>
        /// <param name="tishi">选择时候提示内容</param>
        /// <param name="folder">系统文件夹枚举项</param>
        /// <param name="name">限定文件</param>
        public static void ChoiceFile(ref string label, string tishi, Environment.SpecialFolder folder, string name)
        {
            var openDialog = new OpenFileDialog();

            openDialog.InitialDirectory = Environment.GetFolderPath(folder);
            openDialog.Filter = $"({name})|{name}";
            openDialog.FilterIndex = 1;
            openDialog.RestoreDirectory = true;
            openDialog.Title = tishi;

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                Log(openDialog.FileName);
                label = openDialog.FileName;
            }
        }

        /// <summary>
        /// 打印内容
        /// </summary>
        /// <param name="value"></param>
        private static void Log(string value)
        {
            logger.Log(value);
        }

        /// <summary>
        /// 选择指定文件
        /// </summary>
        /// <param name="label"></param>
        /// <param name="tishi">选择时候提示内容</param>
        /// <param name="folder">系统文件夹枚举项</param>
        /// <param name="name">限定文件</param>
        public static void ChoiceFile(ref string label, string tishi, string folder, string name)
        {
            var openDialog = new OpenFileDialog();

            openDialog.InitialDirectory = folder;
            openDialog.Filter = $"({name})|{name}";
            openDialog.FilterIndex = 1;
            openDialog.RestoreDirectory = true;
            openDialog.Title = tishi;

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                Log(openDialog.FileName);
                label = openDialog.FileName;
            }
        }

        /// <summary>
        /// 使用cmd命令
        /// </summary>
        /// <param name="cmdCode"></param>
        public static void UseCmd(string cmdCode)
        {
            System.Diagnostics.Process proIP = new System.Diagnostics.Process();
            proIP.StartInfo.FileName = "cmd.exe";
            proIP.StartInfo.UseShellExecute = false;
            proIP.StartInfo.RedirectStandardInput = true;
            proIP.StartInfo.RedirectStandardOutput = true;
            proIP.StartInfo.RedirectStandardError = true;
            proIP.StartInfo.CreateNoWindow = true;
            proIP.Start();
            proIP.StandardInput.WriteLine(cmdCode);
            proIP.StandardInput.WriteLine("exit");
            string strResult = proIP.StandardOutput.ReadToEnd();
            Console.WriteLine(strResult);
            proIP.Close();
        }

        /// <summary>
        /// 判断dll是否能用
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public static bool IsAssemblyLoaded(string assemblyName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Any(assembly => assembly.GetName().Name == assemblyName);
        }

        /// <summary>
        /// 获取dll位置
        /// 找不到会报错
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public static string GetDllPath(string assemblyName)
        {
            var assembly = System.Reflection.Assembly.Load(assemblyName);
            return assembly.Location;
        }

        /// <summary>
        /// 打开网站|其他东西
        /// </summary>
        /// <param name="web">网址|地址</param>
        public static void OpenOnWindows(string web)
        {
            System.Diagnostics.Process.Start(web);
        }

        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="path"></param>
        public static void OpenFile(string path)
        {
            var command = string.Format("explorer /select,{0}", path);
            UseCmd(command);
        }

        /// <summary>
        /// 根据INI内容创建regex对象
        /// </summary>
        /// <param name="iniValue"></param>
        /// <returns></returns>
        public static Regex CreateRegex(string iniValue)
        {
            if (iniValue == null)
            {
                return new Regex("");
            }
            var read = new Regex("^[\"']?([^\r\n]+)[\"']?$");
            var value = read.Match(iniValue);
            if (value.Success)
            {
                var regex = new Regex(value.Groups[1].ToString());
                return regex;
            }
            else
            {
                return new Regex(iniValue);
            }
        }

        /// <summary>
        /// 将regex对象转为ini内容
        /// </summary>
        /// <param name="regex"></param>
        /// <returns></returns>
        public static string RegexToIni(Regex regex)
        {
            var sb = new StringBuilder();
            sb.Append('"');
            sb.Append(regex.ToString());
            sb.Append('"');
            return sb.ToString();
        }

        /// <summary>
        /// 获取输入
        /// </summary>
        /// <param name="callBack">回调函数</param>
        /// <returns></returns>
        public static void GetInput(Action<string> callBack)
        {
            GetInput("请输入内容:", callBack);
        }

        /// <summary>
        /// 获取输入
        /// </summary>
        /// <param name="tishi">输入框提示</param>
        /// <param name="callBack">回调函数</param>
        /// <returns></returns>
        public static void GetInput(string tishi, Action<string> callBack)
        {
            GetInput(tishi, "", callBack);
        }

        /// <summary>
        /// 获取输入
        /// </summary>
        /// <param name="tishi">输入框提示</param>
        /// <param name="value">输入框内已有内容</param>
        /// <param name="callBack">回调函数</param>
        /// <returns></returns>
        public static void GetInput(string tishi, string value, Action<string> callBack)
        {
            var form = GetForm("InputBox");
            if (form == null)
            {
                Task.Factory.StartNew(() =>
                {
                    System.Windows.Forms.Application.EnableVisualStyles();
                    System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
                    var input = new InputBox(tishi, value);
                    System.Windows.Forms.Application.Run(input);

                    return input.value;
                }).ContinueWith(t =>
                {
                    var ret = t.Result;
                    // 输出用户输入到 Unity 控制台，或者根据需要处理用户输入
                    Log("用户输入内容: " + ret);
                    if (callBack != null && !string.IsNullOrEmpty(ret))
                    {
                        //在主线程上执行回调
                        //Dispatcher.Invoke(() => callBack(ret));
                        //不使用Dispatcher反而可以使用...
                        callBack(ret);
                    }
                });
            }
            else
            {
                form.ShowInTheCurrentScreenCenter();
                form.Activate();
                form.Focus();
            }
        }

        /// <summary>
        /// 检查窗口是否已经打开
        /// </summary>
        /// <param name="formName">窗体名称</param>
        /// <returns></returns>
        public static Form GetForm(string formName)
        {
            foreach (Form openForm in System.Windows.Forms.Application.OpenForms)
            {
                if (openForm.Name == formName || openForm is InputBox)
                {
                    return openForm;
                }
            }
            return null;
        }

        /// <summary>
        /// 或者
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static T Or<T>(T t1, T t2) where T : class
        {
            if (t1 == null)
            {
                return t2;
            }
            else
            {
                return t1;
            }
        }

        /// <summary>
        /// 或者
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t1"></param>
        /// <param name="getT2"></param>
        /// <returns></returns>
        public static T Or<T>(T t1, Func<T> getT2) where T : class
        {
            if(t1 == null)
            {
                return getT2();
            }
            else
            {
                return t1;
            }
        }
    }

    /// <summary>
    /// 工具类追加函数
    /// </summary>
    public static class ItemAdd
    {
        /// <summary>
        /// 获取js的时间戳
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long ToJSTime(this DateTime time)
        {
            var ret = time.ToFileTime();

            ret -= new DateTime(1970, 1, 1, 8, 0, 0).ToFileTime();
            //ret = Math.Floor(ret / 10000);
            ret = ret / 10000;

            return ret;
        }

        /// <summary>
        /// 异步运行
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Task Run(Action action)
        {
            return Task.Factory.StartNew(action);
        }
    }
}
