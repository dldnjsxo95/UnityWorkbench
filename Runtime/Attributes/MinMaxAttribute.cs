using System;
using UnityEngine;

namespace LWT.UnityWorkbench.Attributes
{
    /// <summary>
    /// Displays a min-max slider for Vector2 or Vector2Int fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class MinMaxAttribute : PropertyAttribute
    {
        public float Min { get; private set; }
        public float Max { get; private set; }

        public MinMaxAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}
