using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Export.Enums
{
    /// <summary>
    /// 控制方式枚举类
    /// </summary>
    public enum ControlEnum
    {
        /// <summary>
        /// 无控制方式
        /// </summary>
        None = 0,
        /// <summary>
        /// 键鼠控制
        /// </summary>
        Keyboard = 1,
        /// <summary>
        /// 手柄控制
        /// </summary>
        Handle = 2,
        /// <summary>
        /// 触控控制
        /// </summary>
        UI = 3,
    }
}
