using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Export.AddFunc;

namespace Export.BehaviourEX
{
    /// <summary>
    /// 专门设计给Nreal项目用
    /// <br />
    /// 放弃继承此类需要放弃使用Start和Update以防止出错
    /// </summary>
    public abstract class NrealBehaviour : MonoBehaviour
    {
        /// <summary>
        /// NRInput对象名称
        /// </summary>
        public static readonly List<string> NRINPUT = new List<string> {
            "NRInput"
        };
        /// <summary>
        /// NRInput右摇杆控制器名称
        /// </summary>
        public static readonly List<string> NRINPUT_RIGHT = new List<string> {
            "Right"
        };
        /// <summary>
        /// NRInput手机屏幕名称
        /// </summary>
        public static readonly List<string> NR_VIRTUAL_DISPLAYER = new List<string> {
            "NRVirtualDisplayer",
            "NRVirtualDisplayer(Clone)"
        };
        /// <summary>
        /// NR摄像头对象
        /// </summary>
        public static readonly List<string> NR_CAMERA_RIG = new List<string>
        {
            "NRCameraRig",
            "NRCameraRig(Clone)"
        };
        /// <summary>
        /// 最大重试次数
        /// </summary>
        public static readonly int MAX_ERROR_TIMES = 10;
        /// <summary>
        /// 重试等待时间
        /// <br />
        /// (单位：秒)
        /// </summary>
        public static readonly float WAIT_ERROR_TIME = 0.2f;

        /// <summary>
        /// NRInput对象
        /// </summary>
        private static GameObject nrInput;

        /// <summary>
        /// NRInput
        /// 右摇杆控制器
        /// </summary>
        private static GameObject nrInputRight;

        /// <summary>
        /// NRInput手机屏幕对象
        /// </summary>
        private static GameObject nrVirtualDisplayer;

        /// <summary>
        /// Nreal摄像头
        /// </summary>
        private static GameObject nrCameraRig;

        /// <summary>
        /// NRInput对象
        /// </summary>
        public static GameObject NRInput
        {
            get
            {
                if (nrInput != null)
                {
                    return nrInput;
                }
                else
                {
                    // nrInput = GameObject.Find(NRINPUT);
                    nrInput = NRINPUT.Map(str =>
                    {
                        return GameObject.Find(str);
                    }).GetOnlyOne();
                    return nrInput;
                }
            }
        }

        /// <summary>
        /// NRInput
        /// 右遥感控制器
        /// </summary>
        public static GameObject NRInputRight
        {
            get
            {
                if (nrInputRight != null)
                {
                    return nrInputRight;
                }
                else
                {
                    if (NRInput == null)
                    {
                        return null;
                    }
                    else
                    {
                        // var t = NRInput.transform.Find(NRINPUT_RIGHT);
                        var t = NRINPUT_RIGHT.Map(str =>
                        {
                            return NRInput.transform.Find(str);
                        }).GetOnlyOne();
                        if (t == null)
                        {
                            return null;
                        }
                        else
                        {
                            nrInputRight = t.gameObject;
                            return nrInputRight;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// NRInput手机屏幕对象
        /// </summary>
        public static GameObject NRVirtualDisplayer
        {
            get
            {
                if (nrVirtualDisplayer != null)
                {
                    return nrVirtualDisplayer;
                }
                else
                {
                    // nrVirtualDisplayer = GameObject.Find(NR_VIRTUAL_DISPLAYER);
                    nrVirtualDisplayer = NR_VIRTUAL_DISPLAYER.Map(str =>
                    {
                        return GameObject.Find(str);
                    }).GetOnlyOne();
                    return nrVirtualDisplayer;
                }
            }
        }

        /// <summary>
        /// Nreal摄像头
        /// </summary>
        public static GameObject NRCameraRig
        {
            get
            {
                if (nrCameraRig!=null)
                {
                    return nrCameraRig;
                }
                else
                {
                    nrCameraRig = NR_CAMERA_RIG.Map(str =>
                    {
                        return GameObject.Find(str);
                    }).GetOnlyOne();
                    return nrCameraRig;
                }
            }
        }

        /// <summary>
        /// 根据名称查找父对象下的对象
        /// </summary>
        /// <param name="objName">对象名称</param>
        /// <param name="parent">父对象</param>
        /// <returns></returns>
        public static GameObject Find(string objName, GameObject parent)
        {
            if (parent == null)
            {
                return null;
            }
            var t = parent.transform.Find(objName);
            if (t == null)
            {
                return null;
            }
            else
            {
                return t.gameObject;
            }
        }

        /// <summary>
        /// 查找右摇杆下的对象
        /// </summary>
        /// <param name="objName"></param>
        /// <returns></returns>
        public static GameObject Find(string objName)
        {
            return Find(objName, NRInputRight);
        }

        /// <summary>
        /// 启动错误列表
        /// </summary>
        private List<string> StartErrorList = new List<string>();

        /// <summary>
        /// 启动错误次数
        /// </summary>
        private int errorTimes = 0;
        /// <summary>
        /// 当前模块是否启动
        /// </summary>
        private bool nrealToolsIsInit = false;
        /// <summary>
        /// 当前模块是否启动
        /// </summary>
        public bool NrealToolsIsInit
        {
            get
            {
                return nrealToolsIsInit;
            }
        }

        /// <summary>
        /// 获取启动错误列表
        /// </summary>
        /// <remarks>
        /// 如果想要添加加载项，请修改此函数
        /// </remarks>
        private void GetStartErrorList()
        {
            if (NRInput == null)
            {
                StartErrorList.Add("NRInput初始化失败!");
            }
            if (NRInputRight == null)
            {
                StartErrorList.Add("NRInputRight初始化失败!");
            }
            if (NRVirtualDisplayer == null)
            {
                StartErrorList.Add("NRVirtualDisplayer初始化失败!");
            }
            if (NRCameraRig == null)
            {
                StartErrorList.Add("NRCameraRig初始化失败!");
            }
        }

        /// <summary>
        /// 等待NRInput初始化完成<br />
        /// 运行结束时会调用NewStart
        /// </summary>
        /// <returns></returns>
        IEnumerator WaitNRInputInit()
        {
            while (nrealToolsIsInit == false)
            {
                //检测初始化
                StartErrorList.Clear();
                GetStartErrorList();
                if (StartErrorList.Length() == 0)
                {
                    nrealToolsIsInit = true;
                    Debug.Log("NRInput组件加载完成");
                }
                else
                {
                    errorTimes++;
                    if (errorTimes >= MAX_ERROR_TIMES)
                    {
                        nrealToolsIsInit = true;
                        Debug.LogWarning(StartErrorList.Join(",\n") + "组件无法加载成功");
                    }
                }

                // 运行NewStart
                if (nrealToolsIsInit)
                {
                    NewStart();
                }

                // 等待一段时间
                yield return new WaitForSeconds(WAIT_ERROR_TIME); // 处理等待时间
            }
        }

        // Start is called before the first frame update
        /// <summary>
        /// 此方法已经被废弃<br />
        /// 请使用NewStart
        /// </summary>
        [Obsolete("此方法已经被废弃,请使用NewStart")]
        protected virtual void Start()
        {
            //GetStartErrorList();
            //Debug.LogWarning(StartErrorList.Join(",\n"));
            // // 启动继承类的Start方法
            //NewStart();

            //启动协程
            StartCoroutine(WaitNRInputInit());
        }

        /// <summary>
        /// 让继承此类的代码放弃使用Start
        /// 转而使用NewStart
        /// </summary>
        /// <remarks>
        /// 此函数会在等待NRInput模块加载完或者超过最大重试次数后运行<br />
        /// 只在第一帧运行一次
        /// </remarks>
        protected abstract void NewStart();

        // Update is called once per frame
        /// <summary>
        /// 此方法已经被弃用
        /// 请使用NewUpdate
        /// </summary>
        [Obsolete("此方法已经被废弃，请使用NewUpdate")]
        void Update()
        {
            if (NrealToolsIsInit)
            {
                NewUpdate();
            }
        }

        /// <summary>
        /// 让继承此类的代码放弃使用Update
        /// 转为使用NewUpdate
        /// </summary>
        /// <remarks>
        /// 此函数会在等待NRInput模块加载完或者超过最大重试次数后运行<br />
        /// 每帧运行一次
        /// </remarks>
        protected abstract void NewUpdate();

        /// <summary>
        /// 此方法已经被废弃<br />
        /// 请使用OnNewDestroy
        /// </summary>
        /// <remarks>
        /// 添加对象时,这里也需要修改
        /// </remarks>
        [Obsolete("此方法已经被废弃,请使用OnNewDestroy")]
        private void OnDestroy()
        {
            //清空数据
            nrInput = null;
            nrInputRight = null;
            nrVirtualDisplayer = null;
            nrCameraRig = null;

            OnNewDestroy();
        }

        /// <summary>
        /// 让继承此类的代码放弃使用OnDestroy
        /// 转而使用OnNewDestroy
        /// </summary>
        protected virtual void OnNewDestroy()
        {

        }
    }
}