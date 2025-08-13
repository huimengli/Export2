using UnityEngine;
using UnityEditor;

namespace Export.Attribute
{
    /// <summary>
    /// 只读状态绘制
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 保存当前GUI状态
            bool originalEnabled = GUI.enabled;

            try
            {
                // 禁用GUI
                GUI.enabled = false;

                // 没有其他绘制器，使用默认绘制
                EditorGUI.PropertyField(position, property, label);
            }
            finally
            {
                // 恢复原始GUI状态
                GUI.enabled = originalEnabled;
            }
        }
    }
}