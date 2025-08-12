using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace Export.Attribute
{
    /// <summary>
    /// 自定义属性绘制器，用于处理ConditionalHideAttribute特性的字段绘制
    /// </summary>
    [CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
    public class ConditionalHideDrawer : PropertyDrawer
    {
        /// <summary>
        /// 重写OnGUI方法，处理字段在Unity编辑器中的绘制逻辑
        /// </summary>
        /// <param name="position"></param>
        /// <param name="property"></param>
        /// <param name="label"></param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 获取当前字段上的ConditionalHideAttribute特性
            ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
            // 查找条件源字段
            SerializedProperty sourceProperty = property.serializedObject.FindProperty(condHAtt.ConditionalSourceField);

            // 检查条件是否满足显示要求
            bool show = ShouldShow(sourceProperty, condHAtt.ShowValues);

            // 如果满足条件，则正常绘制字段
            if (show)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        /// <summary>
        /// 重写GetPropertyHeight方法，控制字段在Unity编辑器中的高度
        /// </summary>
        /// <param name="property"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 获取当前字段上的ConditionalHideAttribute特性
            ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
            // 查找条件源字段
            SerializedProperty sourceProperty = property.serializedObject.FindProperty(condHAtt.ConditionalSourceField);

            // 条件不满足时高度设为负值（完全隐藏）
            if (!ShouldShow(sourceProperty, condHAtt.ShowValues))
            {
                return -EditorGUIUtility.standardVerticalSpacing; // 完全移除空间
            }

            // 条件满足时返回正常高度
            return EditorGUI.GetPropertyHeight(property, label);
        }

        /// <summary>
        /// 私有方法，判断字段是否应该显示
        /// </summary>
        /// <param name="sourceProperty"></param>
        /// <param name="showValues"></param>
        /// <returns></returns>
        private bool ShouldShow(SerializedProperty sourceProperty, int[] showValues)
        {
            // 检查源字段是否存在且是否为枚举类型
            if (sourceProperty == null || sourceProperty.propertyType != SerializedPropertyType.Enum)
            {
                Debug.LogWarning("ConditionalHide requires an Enum field");
                return true; // 默认显示
            }

            // 获取当前枚举值索引
            int currentValue = sourceProperty.enumValueIndex;
            // 检查当前值是否在允许显示的数值列表中
            foreach (int value in showValues)
            {
                if (currentValue == value) return true;
            }

            // 不满足显示条件
            return false;
        }
    }

}
