using UnityEngine;
using UnityEditor;
using LWT.UnityWorkbench.Attributes;

namespace LWT.UnityWorkbench.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(SeparatorAttribute))]
    public class SeparatorDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            var separator = attribute as SeparatorAttribute;

            float y = position.y + (position.height - separator.Height) / 2f;

            if (!string.IsNullOrEmpty(separator.Title))
            {
                // Draw title with lines on both sides
                var style = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter
                };

                var titleContent = new GUIContent(separator.Title);
                float titleWidth = style.CalcSize(titleContent).x + 10f;
                float lineWidth = (position.width - titleWidth) / 2f - 5f;

                // Left line
                var leftLineRect = new Rect(position.x, y, lineWidth, separator.Height);
                EditorGUI.DrawRect(leftLineRect, separator.Color);

                // Title
                var titleRect = new Rect(position.x + lineWidth + 5f, position.y, titleWidth, position.height);
                EditorGUI.LabelField(titleRect, separator.Title, style);

                // Right line
                var rightLineRect = new Rect(position.x + lineWidth + titleWidth + 10f, y, lineWidth, separator.Height);
                EditorGUI.DrawRect(rightLineRect, separator.Color);
            }
            else
            {
                // Draw simple line
                var lineRect = new Rect(position.x, y, position.width, separator.Height);
                EditorGUI.DrawRect(lineRect, separator.Color);
            }
        }

        public override float GetHeight()
        {
            var separator = attribute as SeparatorAttribute;
            return string.IsNullOrEmpty(separator.Title) ? separator.Height + 8f : 20f;
        }
    }
}
