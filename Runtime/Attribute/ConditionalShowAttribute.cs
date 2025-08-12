using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Export.Attribute
{
    /// <summary>
    /// 指定该特性只能应用于字段(Field)，且不能被继承(Inherited = true)，同时不允许重复应用(AllowMultiple = false)
    /// 定义一个条件隐藏特性，继承自PropertyAttribute，用于Unity编辑器中根据条件控制字段的显示/隐藏
    /// </summary>
    /// <example>
    /// // ExampleBehaviour.cs
    /// public class ExampleBehaviour : MonoBehaviour
    /// {
    ///     public enum WeaponType
    ///     {
    ///         Sword,
    ///         Bow,
    ///         Staff
    ///     }
    ///     public WeaponType currentWeapon;
    ///     [ConditionalShow("currentWeapon", (int)WeaponType.Bow)]
    ///     public int arrowCount;
    ///     [ConditionalShow("currentWeapon", (int)WeaponType.Staff)]
    ///     public float magicPower;
    /// }
    /// </example>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ConditionalShowAttribute : PropertyAttribute
    {
        /// <summary>
        /// 获取或设置条件源字段名称，该字段的值将决定当前字段是否显示
        /// </summary>
        public string ConditionalSourceField { get; private set; }

        /// <summary>
        /// 获取或设置当条件源字段值为这些值时，当前字段才会显示
        /// </summary>
        public int[] ShowValues { get; private set; }

        /// <summary>
        /// 构造函数，初始化条件隐藏特性
        /// </summary>
        /// <param name="conditionalSourceField"> 作为条件判断依据的字段名称</param>
        /// <param name="showValues">当条件源字段等于这些值时，当前字段才会显示</param>
        public ConditionalShowAttribute(string conditionalSourceField, params int[] showValues)
        {
            ConditionalSourceField = conditionalSourceField;
            ShowValues = showValues;
        }
    }

}
