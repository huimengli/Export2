using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Export.Tools
{
    /// <summary>
    /// 日志打印
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// 打印日志
        /// </summary>
        /// <param name="message"></param>

        public virtual void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
