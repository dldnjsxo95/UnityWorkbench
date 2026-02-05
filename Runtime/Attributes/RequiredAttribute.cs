using System;
using UnityEngine;

namespace LWT.UnityWorkbench.Attributes
{
    /// <summary>
    /// Marks a field as required. Shows warning if null or empty.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class RequiredAttribute : PropertyAttribute
    {
        public string Message { get; private set; }

        public RequiredAttribute()
        {
            Message = "This field is required!";
        }

        public RequiredAttribute(string message)
        {
            Message = message;
        }
    }
}
