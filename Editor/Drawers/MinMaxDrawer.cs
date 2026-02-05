using UnityEngine;
using UnityEditor;
using LWT.UnityWorkbench.Attributes;

namespace LWT.UnityWorkbench.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(MinMaxAttribute))]
    public class MinMaxDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var minMax = attribute as MinMaxAttribute;

            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                DrawVector2Slider(position, property, label, minMax);
            }
            else if (property.propertyType == SerializedPropertyType.Vector2Int)
            {
                DrawVector2IntSlider(position, property, label, minMax);
            }
            else
            {
                EditorGUI.HelpBox(position, "[MinMax] Only works with Vector2 or Vector2Int", MessageType.Error);
            }
        }

        private void DrawVector2Slider(Rect position, SerializedProperty property, GUIContent label, MinMaxAttribute minMax)
        {
            float labelWidth = EditorGUIUtility.labelWidth;
            float fieldWidth = 50f;
            float sliderPadding = 5f;

            Vector2 value = property.vector2Value;
            float minValue = value.x;
            float maxValue = value.y;

            // Label
            var labelRect = new Rect(position.x, position.y, labelWidth, position.height);
            EditorGUI.LabelField(labelRect, label);

            // Min field
            var minFieldRect = new Rect(position.x + labelWidth, position.y, fieldWidth, position.height);
            minValue = EditorGUI.FloatField(minFieldRect, minValue);

            // Slider
            var sliderRect = new Rect(
                position.x + labelWidth + fieldWidth + sliderPadding,
                position.y,
                position.width - labelWidth - fieldWidth * 2 - sliderPadding * 2,
                position.height);
            EditorGUI.MinMaxSlider(sliderRect, ref minValue, ref maxValue, minMax.Min, minMax.Max);

            // Max field
            var maxFieldRect = new Rect(position.x + position.width - fieldWidth, position.y, fieldWidth, position.height);
            maxValue = EditorGUI.FloatField(maxFieldRect, maxValue);

            // Clamp and apply
            minValue = Mathf.Clamp(minValue, minMax.Min, maxValue);
            maxValue = Mathf.Clamp(maxValue, minValue, minMax.Max);

            property.vector2Value = new Vector2(minValue, maxValue);
        }

        private void DrawVector2IntSlider(Rect position, SerializedProperty property, GUIContent label, MinMaxAttribute minMax)
        {
            float labelWidth = EditorGUIUtility.labelWidth;
            float fieldWidth = 50f;
            float sliderPadding = 5f;

            Vector2Int value = property.vector2IntValue;
            float minValue = value.x;
            float maxValue = value.y;

            // Label
            var labelRect = new Rect(position.x, position.y, labelWidth, position.height);
            EditorGUI.LabelField(labelRect, label);

            // Min field
            var minFieldRect = new Rect(position.x + labelWidth, position.y, fieldWidth, position.height);
            minValue = EditorGUI.IntField(minFieldRect, (int)minValue);

            // Slider
            var sliderRect = new Rect(
                position.x + labelWidth + fieldWidth + sliderPadding,
                position.y,
                position.width - labelWidth - fieldWidth * 2 - sliderPadding * 2,
                position.height);
            EditorGUI.MinMaxSlider(sliderRect, ref minValue, ref maxValue, minMax.Min, minMax.Max);

            // Max field
            var maxFieldRect = new Rect(position.x + position.width - fieldWidth, position.y, fieldWidth, position.height);
            maxValue = EditorGUI.IntField(maxFieldRect, (int)maxValue);

            // Clamp and apply
            int minInt = Mathf.Clamp((int)minValue, (int)minMax.Min, (int)maxValue);
            int maxInt = Mathf.Clamp((int)maxValue, minInt, (int)minMax.Max);

            property.vector2IntValue = new Vector2Int(minInt, maxInt);
        }
    }
}
