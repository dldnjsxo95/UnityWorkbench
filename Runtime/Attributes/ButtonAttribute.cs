using System;
using UnityEngine;

namespace LWT.UnityWorkbench.Attributes
{
    /// <summary>
    /// Adds a button in the inspector that calls the specified method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ButtonAttribute : Attribute
    {
        public string Label { get; private set; }
        public ButtonMode Mode { get; private set; }

        public ButtonAttribute()
        {
            Label = null;
            Mode = ButtonMode.Always;
        }

        public ButtonAttribute(string label)
        {
            Label = label;
            Mode = ButtonMode.Always;
        }

        public ButtonAttribute(ButtonMode mode)
        {
            Label = null;
            Mode = mode;
        }

        public ButtonAttribute(string label, ButtonMode mode)
        {
            Label = label;
            Mode = mode;
        }
    }

    public enum ButtonMode
    {
        /// <summary>Always show the button.</summary>
        Always,
        /// <summary>Only show in edit mode.</summary>
        EditorOnly,
        /// <summary>Only show in play mode.</summary>
        PlayModeOnly
    }
}
