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
    public class ConditionalHideDrawer : PropertyDrawerEX
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
            SerializedProperty sourceProperty = property.serializedObject.FindProperty(condHAtt.ConditionalSourceField);

            bool shouldHide = ShouldHide(sourceProperty, condHAtt.ShowValues);

            if (shouldHide) return;

            // 获取下一个绘制器
            var nextDrawer = GetNextDrawer(property, label);
            if (nextDrawer != null)
            {
                // 使用下一个绘制器绘制
                nextDrawer.OnGUI(position, property, label);
            }
            else
            {
                // 没有其他绘制器，使用默认绘制
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
            SerializedProperty sourceProperty = property.serializedObject.FindProperty(condHAtt.ConditionalSourceField);

            if (ShouldHide(sourceProperty, condHAtt.ShowValues))
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }

            // 获取下一个绘制器
            var nextDrawer = GetNextDrawer(property, label);

            if (nextDrawer != null)
            {
                return nextDrawer.GetPropertyHeight(property, label);
            }

            return EditorGUI.GetPropertyHeight(property, label);
        }

        private bool ShouldHide(SerializedProperty sourceProperty, int[] hideValues)
        {
            // ======== 增强类型安全 ========
            if (sourceProperty == null)
            {
                Debug.LogWarning("ConditionalSourceField not found");
                return false;
            }

            // 支持枚举和整数类型
            if (sourceProperty.propertyType == SerializedPropertyType.Enum)
            {
                int currentValue = sourceProperty.intValue;
                return hideValues.Contains(currentValue);
            }
            else if (sourceProperty.propertyType == SerializedPropertyType.Integer)
            {
                return hideValues.Contains(sourceProperty.intValue);
            }

            Debug.LogWarning($"Unsupported type: {sourceProperty.propertyType}");
            return false;
        }
    }
}
