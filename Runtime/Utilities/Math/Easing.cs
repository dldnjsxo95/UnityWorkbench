using UnityEngine;

namespace LWT.UnityWorkbench.Utilities
{
    /// <summary>
    /// Easing functions for smooth animations.
    /// All functions take t (0-1) and return the eased value (0-1).
    /// </summary>
    public static class Easing
    {
        public enum Type
        {
            Linear,
            QuadIn, QuadOut, QuadInOut,
            CubicIn, CubicOut, CubicInOut,
            QuartIn, QuartOut, QuartInOut,
            QuintIn, QuintOut, QuintInOut,
            SineIn, SineOut, SineInOut,
            ExpoIn, ExpoOut, ExpoInOut,
            CircIn, CircOut, CircInOut,
            BackIn, BackOut, BackInOut,
            ElasticIn, ElasticOut, ElasticInOut,
            BounceIn, BounceOut, BounceInOut
        }

        /// <summary>
        /// Evaluates an easing function by type.
        /// </summary>
        public static float Evaluate(Type type, float t)
        {
            return type switch
            {
                Type.Linear => Linear(t),
                Type.QuadIn => QuadIn(t),
                Type.QuadOut => QuadOut(t),
                Type.QuadInOut => QuadInOut(t),
                Type.CubicIn => CubicIn(t),
                Type.CubicOut => CubicOut(t),
                Type.CubicInOut => CubicInOut(t),
                Type.QuartIn => QuartIn(t),
                Type.QuartOut => QuartOut(t),
                Type.QuartInOut => QuartInOut(t),
                Type.QuintIn => QuintIn(t),
                Type.QuintOut => QuintOut(t),
                Type.QuintInOut => QuintInOut(t),
                Type.SineIn => SineIn(t),
                Type.SineOut => SineOut(t),
                Type.SineInOut => SineInOut(t),
                Type.ExpoIn => ExpoIn(t),
                Type.ExpoOut => ExpoOut(t),
                Type.ExpoInOut => ExpoInOut(t),
                Type.CircIn => CircIn(t),
                Type.CircOut => CircOut(t),
                Type.CircInOut => CircInOut(t),
                Type.BackIn => BackIn(t),
                Type.BackOut => BackOut(t),
                Type.BackInOut => BackInOut(t),
                Type.ElasticIn => ElasticIn(t),
                Type.ElasticOut => ElasticOut(t),
                Type.ElasticInOut => ElasticInOut(t),
                Type.BounceIn => BounceIn(t),
                Type.BounceOut => BounceOut(t),
                Type.BounceInOut => BounceInOut(t),
                _ => Linear(t)
            };
        }

        // Linear
        public static float Linear(float t) => t;

        // Quadratic
        public static float QuadIn(float t) => t * t;
        public static float QuadOut(float t) => t * (2f - t);
        public static float QuadInOut(float t) => t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;

        // Cubic
        public static float CubicIn(float t) => t * t * t;
        public static float CubicOut(float t) { t--; return t * t * t + 1f; }
        public static float CubicInOut(float t) => t < 0.5f ? 4f * t * t * t : (t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f;

        // Quartic
        public static float QuartIn(float t) => t * t * t * t;
        public static float QuartOut(float t) { t--; return 1f - t * t * t * t; }
        public static float QuartInOut(float t) { t *= 2f; if (t < 1f) return 0.5f * t * t * t * t; t -= 2f; return -0.5f * (t * t * t * t - 2f); }

        // Quintic
        public static float QuintIn(float t) => t * t * t * t * t;
        public static float QuintOut(float t) { t--; return t * t * t * t * t + 1f; }
        public static float QuintInOut(float t) { t *= 2f; if (t < 1f) return 0.5f * t * t * t * t * t; t -= 2f; return 0.5f * (t * t * t * t * t + 2f); }

        // Sine
        public static float SineIn(float t) => 1f - Mathf.Cos(t * Mathf.PI / 2f);
        public static float SineOut(float t) => Mathf.Sin(t * Mathf.PI / 2f);
        public static float SineInOut(float t) => 0.5f * (1f - Mathf.Cos(Mathf.PI * t));

        // Exponential
        public static float ExpoIn(float t) => t == 0f ? 0f : Mathf.Pow(2f, 10f * (t - 1f));
        public static float ExpoOut(float t) => t == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);
        public static float ExpoInOut(float t)
        {
            if (t == 0f) return 0f;
            if (t == 1f) return 1f;
            t *= 2f;
            if (t < 1f) return 0.5f * Mathf.Pow(2f, 10f * (t - 1f));
            return 0.5f * (2f - Mathf.Pow(2f, -10f * (t - 1f)));
        }

        // Circular
        public static float CircIn(float t) => 1f - Mathf.Sqrt(1f - t * t);
        public static float CircOut(float t) { t--; return Mathf.Sqrt(1f - t * t); }
        public static float CircInOut(float t)
        {
            t *= 2f;
            if (t < 1f) return -0.5f * (Mathf.Sqrt(1f - t * t) - 1f);
            t -= 2f;
            return 0.5f * (Mathf.Sqrt(1f - t * t) + 1f);
        }

        // Back
        private const float BackC1 = 1.70158f;
        private const float BackC2 = BackC1 * 1.525f;
        private const float BackC3 = BackC1 + 1f;

        public static float BackIn(float t) => BackC3 * t * t * t - BackC1 * t * t;
        public static float BackOut(float t) { t--; return 1f + BackC3 * t * t * t + BackC1 * t * t; }
        public static float BackInOut(float t)
        {
            t *= 2f;
            if (t < 1f) return 0.5f * (t * t * ((BackC2 + 1f) * t - BackC2));
            t -= 2f;
            return 0.5f * (t * t * ((BackC2 + 1f) * t + BackC2) + 2f);
        }

        // Elastic
        private const float ElasticC4 = (2f * Mathf.PI) / 3f;
        private const float ElasticC5 = (2f * Mathf.PI) / 4.5f;

        public static float ElasticIn(float t)
        {
            if (t == 0f) return 0f;
            if (t == 1f) return 1f;
            return -Mathf.Pow(2f, 10f * t - 10f) * Mathf.Sin((t * 10f - 10.75f) * ElasticC4);
        }

        public static float ElasticOut(float t)
        {
            if (t == 0f) return 0f;
            if (t == 1f) return 1f;
            return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * ElasticC4) + 1f;
        }

        public static float ElasticInOut(float t)
        {
            if (t == 0f) return 0f;
            if (t == 1f) return 1f;
            t *= 2f;
            if (t < 1f) return -0.5f * Mathf.Pow(2f, 10f * t - 10f) * Mathf.Sin((t * 10f - 11.125f) * ElasticC5);
            return 0.5f * Mathf.Pow(2f, -10f * t + 10f) * Mathf.Sin((t * 10f - 11.125f) * ElasticC5) + 1f;
        }

        // Bounce
        public static float BounceOut(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (t < 1f / d1) return n1 * t * t;
            if (t < 2f / d1) { t -= 1.5f / d1; return n1 * t * t + 0.75f; }
            if (t < 2.5f / d1) { t -= 2.25f / d1; return n1 * t * t + 0.9375f; }
            t -= 2.625f / d1;
            return n1 * t * t + 0.984375f;
        }

        public static float BounceIn(float t) => 1f - BounceOut(1f - t);

        public static float BounceInOut(float t)
        {
            return t < 0.5f
                ? 0.5f * (1f - BounceOut(1f - 2f * t))
                : 0.5f * (1f + BounceOut(2f * t - 1f));
        }

        /// <summary>
        /// Lerp with easing.
        /// </summary>
        public static float Lerp(float from, float to, float t, Type easingType)
        {
            return Mathf.Lerp(from, to, Evaluate(easingType, t));
        }

        /// <summary>
        /// Lerp Vector3 with easing.
        /// </summary>
        public static Vector3 Lerp(Vector3 from, Vector3 to, float t, Type easingType)
        {
            return Vector3.Lerp(from, to, Evaluate(easingType, t));
        }

        /// <summary>
        /// Lerp Color with easing.
        /// </summary>
        public static Color Lerp(Color from, Color to, float t, Type easingType)
        {
            return Color.Lerp(from, to, Evaluate(easingType, t));
        }
    }
}
