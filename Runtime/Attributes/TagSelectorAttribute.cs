using System;
using UnityEngine;

namespace LWT.UnityWorkbench.Attributes
{
    /// <summary>
    /// Shows a dropdown for selecting Unity tags.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class TagSelectorAttribute : PropertyAttribute
    {
    }

    /// <summary>
    /// Shows a dropdown for selecting Unity layers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class LayerSelectorAttribute : PropertyAttribute
    {
    }

    /// <summary>
    /// Shows a dropdown for selecting scene names from build settings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class SceneSelectorAttribute : PropertyAttribute
    {
    }
}
