using UnityEngine;
using UnityEditor;
using LWT.UnityWorkbench.Attributes;

namespace LWT.UnityWorkbench.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredDrawer : PropertyDrawer
    {
        private const float HelpBoxHeight = 24f;
        private const float Padding = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var required = attribute as RequiredAttribute;
            bool isEmpty = IsEmpty(property);

            if (isEmpty)
            {
                // Draw warning box
                var helpBoxRect = new Rect(position.x, position.y, position.width, HelpBoxHeight);
                EditorGUI.HelpBox(helpBoxRect, required.Message, MessageType.Warning);

                // Draw field with red tint
                var fieldRect = new Rect(position.x, position.y + HelpBoxHeight + Padding, position.width, EditorGUI.GetPropertyHeight(property, label, true));

                var prevColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1f, 0.5f, 0.5f, 1f);
                EditorGUI.PropertyField(fieldRect, property, label, true);
                GUI.backgroundColor = prevColor;
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUI.GetPropertyHeight(property, label, true);

            if (IsEmpty(property))
            {
                height += HelpBoxHeight + Padding;
            }

            return height;
        }

        private bool IsEmpty(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue == null;

                case SerializedPropertyType.String:
                    return string.IsNullOrEmpty(property.stringValue);

                case SerializedPropertyType.ArraySize:
                    return property.arraySize == 0;

                default:
                    return false;
            }
        }
    }
}
