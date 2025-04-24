using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Export.Tools
{
    /// <summary>
    /// 日志打印
    /// </summary>
    public class UnityLogger:Logger
    {
        /// <summary>
        /// 打印日志
        /// </summary>
        /// <param name="message"></param>
        public override void Log(string message)
        {
            Debug.Log(message);
        }
    }
}
