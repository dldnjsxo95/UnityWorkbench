using System.Reflection;
using UnityEngine;
using UnityEditor;
using LWT.UnityWorkbench.Attributes;

namespace LWT.UnityWorkbench.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    [CustomPropertyDrawer(typeof(HideIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (ShouldShow(property))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (ShouldShow(property))
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }
            return 0f;
        }

        private bool ShouldShow(SerializedProperty property)
        {
            var showIf = attribute as ShowIfAttribute;
            if (showIf == null) return true;

            var targetObject = property.serializedObject.targetObject;
            var conditionValue = GetConditionValue(targetObject, showIf.ConditionName);

            if (conditionValue == null) return true;

            bool matches;
            if (showIf.CompareValue is bool boolCompare && conditionValue is bool boolCondition)
            {
                matches = boolCondition == boolCompare;
            }
            else
            {
                matches = conditionValue.Equals(showIf.CompareValue);
            }

            return showIf.Invert ? !matches : matches;
        }

        private object GetConditionValue(object target, string conditionName)
        {
            var type = target.GetType();

            // Try field
            var field = type.GetField(conditionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                return field.GetValue(target);
            }

            // Try property
            var prop = type.GetProperty(conditionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null)
            {
                return prop.GetValue(target);
            }

            // Try method
            var method = type.GetMethod(conditionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null && method.GetParameters().Length == 0)
            {
                return method.Invoke(target, null);
            }

            Debug.LogWarning($"[ShowIf] Could not find condition: {conditionName}");
            return null;
        }
    }
}
