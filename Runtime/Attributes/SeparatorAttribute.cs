using System;
using UnityEngine;

namespace LWT.UnityWorkbench.Attributes
{
    /// <summary>
    /// Draws a separator line in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class SeparatorAttribute : PropertyAttribute
    {
        public string Title { get; private set; }
        public float Height { get; private set; }
        public Color Color { get; private set; }

        public SeparatorAttribute()
        {
            Title = null;
            Height = 1f;
            Color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }

        public SeparatorAttribute(string title)
        {
            Title = title;
            Height = 1f;
            Color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }

        public SeparatorAttribute(float height)
        {
            Title = null;
            Height = height;
            Color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }

        public SeparatorAttribute(string title, float height)
        {
            Title = title;
            Height = height;
            Color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }
    }
}
