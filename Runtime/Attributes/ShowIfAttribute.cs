using System;
using UnityEngine;

namespace LWT.UnityWorkbench.Attributes
{
    /// <summary>
    /// Shows the field only if the condition is true.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public string ConditionName { get; private set; }
        public object CompareValue { get; private set; }
        public bool Invert { get; private set; }

        /// <summary>
        /// Show if the boolean field/property is true.
        /// </summary>
        public ShowIfAttribute(string conditionName)
        {
            ConditionName = conditionName;
            CompareValue = true;
            Invert = false;
        }

        /// <summary>
        /// Show if the field/property equals the compare value.
        /// </summary>
        public ShowIfAttribute(string conditionName, object compareValue)
        {
            ConditionName = conditionName;
            CompareValue = compareValue;
            Invert = false;
        }

        /// <summary>
        /// Show if condition matches (or doesn't match if inverted).
        /// </summary>
        public ShowIfAttribute(string conditionName, object compareValue, bool invert)
        {
            ConditionName = conditionName;
            CompareValue = compareValue;
            Invert = invert;
        }
    }

    /// <summary>
    /// Hides the field if the condition is true.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class HideIfAttribute : ShowIfAttribute
    {
        public HideIfAttribute(string conditionName) : base(conditionName, true, true) { }
        public HideIfAttribute(string conditionName, object compareValue) : base(conditionName, compareValue, true) { }
    }
}
