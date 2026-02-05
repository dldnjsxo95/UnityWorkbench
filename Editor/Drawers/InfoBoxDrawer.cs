using UnityEngine;
using UnityEditor;
using LWT.UnityWorkbench.Attributes;

namespace LWT.UnityWorkbench.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    public class InfoBoxDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            var infoBox = attribute as InfoBoxAttribute;
            var messageType = infoBox.Type switch
            {
                InfoBoxType.Info => MessageType.Info,
                InfoBoxType.Warning => MessageType.Warning,
                InfoBoxType.Error => MessageType.Error,
                _ => MessageType.None
            };

            EditorGUI.HelpBox(position, infoBox.Message, messageType);
        }

        public override float GetHeight()
        {
            var infoBox = attribute as InfoBoxAttribute;
            var content = new GUIContent(infoBox.Message);
            var style = EditorStyles.helpBox;

            float height = style.CalcHeight(content, EditorGUIUtility.currentViewWidth - 40);
            return Mathf.Max(40f, height + 4f);
        }
    }
}
