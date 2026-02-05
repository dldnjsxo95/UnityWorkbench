using System;
using UnityEngine;

namespace LWT.UnityWorkbench.Attributes
{
    /// <summary>
    /// Makes a field read-only in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ReadOnlyAttribute : PropertyAttribute
    {
    }
}
