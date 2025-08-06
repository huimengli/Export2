using UnityEditor;
using UnityEngine;

namespace Export.Attribute
{
    [CustomPropertyDrawer(typeof(ReadOnlyTextAreaAttribute))]
    class ReadOnlyTextAreaDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, "ReadOnlyTextArea只能用于string类型字段", MessageType.Error);
                return;
            }

            // 计算高度
            float labelHeight = EditorGUIUtility.singleLineHeight;
            float textHeight = EditorStyles.textArea.CalcHeight(
                new GUIContent(property.stringValue),
                position.width);

            // 绘制标签
            EditorGUI.LabelField(
                new Rect(position.x, position.y, position.width, labelHeight),
                label);

            // 绘制只读文本区域
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.TextArea(
                new Rect(
                    position.x,
                    position.y + labelHeight + 2f,  // 添加2像素间距
                    position.width,
                    textHeight),
                property.stringValue,
                EditorStyles.textArea);
            EditorGUI.EndDisabledGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
                return EditorGUIUtility.singleLineHeight;

            float textHeight = EditorStyles.textArea.CalcHeight(
                new GUIContent(property.stringValue),
                EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth);
            return EditorGUIUtility.singleLineHeight + textHeight + 4f; // 总间距4像素
        }
    }
}
