using UnityEngine;
using UnityEditor;

namespace Export.Attribute
{
    /// <summary>
    /// 只读状态绘制
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyAttributeDrawer : PropertyDrawerEX
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 保存当前GUI状态
            bool originalEnabled = GUI.enabled;

            try
            {
                // 禁用GUI
                GUI.enabled = false;

                // 获取下一个绘制器
                var nextDrawer = GetNextDrawer(property, label);

                if (nextDrawer != null)
                {
                    // 使用下一个绘制器绘制（如RangeDrawer）
                    nextDrawer.OnGUI(position, property, label);
                }
                else
                {
                    // 没有其他绘制器，使用默认绘制
                    EditorGUI.PropertyField(position, property, label);
                }
            }
            finally
            {
                // 恢复原始GUI状态
                GUI.enabled = originalEnabled;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 获取下一个绘制器
            var nextDrawer = GetNextDrawer(property, label);

            if (nextDrawer != null)
            {
                return nextDrawer.GetPropertyHeight(property, label);
            }

            return EditorGUI.GetPropertyHeight(property, label);
        }
    }
}