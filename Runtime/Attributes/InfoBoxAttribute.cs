using System;
using UnityEngine;

namespace LWT.UnityWorkbench.Attributes
{
    /// <summary>
    /// Displays an info box above the field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class InfoBoxAttribute : PropertyAttribute
    {
        public string Message { get; private set; }
        public InfoBoxType Type { get; private set; }

        public InfoBoxAttribute(string message)
        {
            Message = message;
            Type = InfoBoxType.Info;
        }

        public InfoBoxAttribute(string message, InfoBoxType type)
        {
            Message = message;
            Type = type;
        }
    }

    public enum InfoBoxType
    {
        Info,
        Warning,
        Error
    }
}
