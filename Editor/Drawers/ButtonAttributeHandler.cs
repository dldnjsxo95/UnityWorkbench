using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using LWT.UnityWorkbench.Attributes;

namespace LWT.UnityWorkbench.Editor.Drawers
{
    /// <summary>
    /// Custom editor that handles Button attributes on methods.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class ButtonAttributeHandler : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var type = target.GetType();
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                var buttonAttr = method.GetCustomAttribute<ButtonAttribute>();
                if (buttonAttr == null) continue;

                // Check mode
                bool shouldShow = buttonAttr.Mode switch
                {
                    ButtonMode.EditorOnly => !Application.isPlaying,
                    ButtonMode.PlayModeOnly => Application.isPlaying,
                    _ => true
                };

                if (!shouldShow) continue;

                // Get label
                string label = buttonAttr.Label ?? ObjectNames.NicifyVariableName(method.Name);

                // Check parameters
                var parameters = method.GetParameters();
                if (parameters.Length > 0)
                {
                    EditorGUILayout.HelpBox($"[Button] {method.Name} has parameters and cannot be called from inspector.", MessageType.Warning);
                    continue;
                }

                // Draw button
                if (GUILayout.Button(label))
                {
                    foreach (var t in targets)
                    {
                        method.Invoke(t, null);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Same handler for ScriptableObjects.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScriptableObject), true)]
    public class ButtonAttributeHandlerSO : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var type = target.GetType();
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                var buttonAttr = method.GetCustomAttribute<ButtonAttribute>();
                if (buttonAttr == null) continue;

                bool shouldShow = buttonAttr.Mode switch
                {
                    ButtonMode.EditorOnly => !Application.isPlaying,
                    ButtonMode.PlayModeOnly => Application.isPlaying,
                    _ => true
                };

                if (!shouldShow) continue;

                string label = buttonAttr.Label ?? ObjectNames.NicifyVariableName(method.Name);

                var parameters = method.GetParameters();
                if (parameters.Length > 0)
                {
                    EditorGUILayout.HelpBox($"[Button] {method.Name} has parameters.", MessageType.Warning);
                    continue;
                }

                if (GUILayout.Button(label))
                {
                    foreach (var t in targets)
                    {
                        method.Invoke(t, null);
                    }
                }
            }
        }
    }
}
