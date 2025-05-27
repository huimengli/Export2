using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Export.Tools
{
    /// <summary>
    /// 方法追加
    /// </summary>
    public static class ItemAdd
    {
        /// <summary>
        /// 追加方法: 判断对象是否有某属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static bool HasComponent<T>(this GameObject gameObject) where T : Component
        {
            try
            {
                return gameObject.GetComponent<T>() != null;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
