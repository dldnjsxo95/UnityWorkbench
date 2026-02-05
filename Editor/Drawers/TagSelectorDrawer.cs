using UnityEngine;
using UnityEditor;
using LWT.UnityWorkbench.Attributes;

namespace LWT.UnityWorkbench.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(TagSelectorAttribute))]
    public class TagSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, "[TagSelector] Only works with string fields", MessageType.Error);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(LayerSelectorAttribute))]
    public class LayerSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.HelpBox(position, "[LayerSelector] Only works with int fields", MessageType.Error);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            property.intValue = EditorGUI.LayerField(position, label, property.intValue);
            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(SceneSelectorAttribute))]
    public class SceneSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, "[SceneSelector] Only works with string fields", MessageType.Error);
                return;
            }

            var scenes = EditorBuildSettings.scenes;
            var sceneNames = new string[scenes.Length + 1];
            sceneNames[0] = "(None)";

            int currentIndex = 0;
            for (int i = 0; i < scenes.Length; i++)
            {
                var sceneName = System.IO.Path.GetFileNameWithoutExtension(scenes[i].path);
                sceneNames[i + 1] = sceneName;

                if (sceneName == property.stringValue)
                {
                    currentIndex = i + 1;
                }
            }

            EditorGUI.BeginProperty(position, label, property);
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, sceneNames);

            if (newIndex != currentIndex)
            {
                property.stringValue = newIndex == 0 ? "" : sceneNames[newIndex];
            }
            EditorGUI.EndProperty();
        }
    }
}
