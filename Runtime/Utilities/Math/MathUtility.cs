using UnityEngine;

namespace LWT.UnityWorkbench.Utilities
{
    /// <summary>
    /// General math utility functions.
    /// </summary>
    public static class MathUtility
    {
        #region Remap

        /// <summary>
        /// Remaps a value from one range to another.
        /// </summary>
        public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            float t = Mathf.InverseLerp(fromMin, fromMax, value);
            return Mathf.Lerp(toMin, toMax, t);
        }

        /// <summary>
        /// Remaps a value from one range to another (clamped).
        /// </summary>
        public static float RemapClamped(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            float t = Mathf.InverseLerp(fromMin, fromMax, value);
            t = Mathf.Clamp01(t);
            return Mathf.Lerp(toMin, toMax, t);
        }

        /// <summary>
        /// Remaps 0-1 to 0-1 using a curve.
        /// </summary>
        public static float RemapCurve(float t, AnimationCurve curve)
        {
            return curve.Evaluate(t);
        }

        #endregion

        #region Wrap & Clamp

        /// <summary>
        /// Wraps a value between min and max.
        /// </summary>
        public static float Wrap(float value, float min, float max)
        {
            float range = max - min;
            while (value < min) value += range;
            while (value >= max) value -= range;
            return value;
        }

        /// <summary>
        /// Wraps an integer between min and max.
        /// </summary>
        public static int Wrap(int value, int min, int max)
        {
            int range = max - min;
            while (value < min) value += range;
            while (value >= max) value -= range;
            return value;
        }

        /// <summary>
        /// Clamps a value between 0 and 1.
        /// </summary>
        public static float Clamp01(float value) => Mathf.Clamp01(value);

        /// <summary>
        /// Clamps a value to a minimum.
        /// </summary>
        public static float ClampMin(float value, float min) => Mathf.Max(value, min);

        /// <summary>
        /// Clamps a value to a maximum.
        /// </summary>
        public static float ClampMax(float value, float max) => Mathf.Min(value, max);

        #endregion

        #region Interpolation

        /// <summary>
        /// Smoothly interpolates towards a target.
        /// </summary>
        public static float SmoothStep(float from, float to, float t)
        {
            t = Mathf.Clamp01(t);
            t = t * t * (3f - 2f * t);
            return from + (to - from) * t;
        }

        /// <summary>
        /// Even smoother interpolation (Ken Perlin's improved version).
        /// </summary>
        public static float SmootherStep(float from, float to, float t)
        {
            t = Mathf.Clamp01(t);
            t = t * t * t * (t * (6f * t - 15f) + 10f);
            return from + (to - from) * t;
        }

        /// <summary>
        /// Exponential decay interpolation.
        /// </summary>
        public static float ExpDecay(float current, float target, float decay, float deltaTime)
        {
            return target + (current - target) * Mathf.Exp(-decay * deltaTime);
        }

        /// <summary>
        /// Spring-like interpolation.
        /// </summary>
        public static float Spring(float current, float target, ref float velocity, float frequency, float damping, float deltaTime)
        {
            float omega = frequency * 2f * Mathf.PI;
            float x = current - target;
            float exp = Mathf.Exp(-damping * omega * deltaTime);
            float cos = Mathf.Cos(omega * Mathf.Sqrt(1f - damping * damping) * deltaTime);
            float sin = Mathf.Sin(omega * Mathf.Sqrt(1f - damping * damping) * deltaTime);

            float newX = exp * (x * cos + (velocity + damping * omega * x) / (omega * Mathf.Sqrt(1f - damping * damping)) * sin);
            velocity = -omega * exp * (x * (damping * cos - Mathf.Sqrt(1f - damping * damping) * sin) +
                       (velocity + damping * omega * x) / (omega * Mathf.Sqrt(1f - damping * damping)) *
                       (damping * sin + Mathf.Sqrt(1f - damping * damping) * cos));

            return target + newX;
        }

        /// <summary>
        /// Critically damped spring (smooth, no oscillation).
        /// </summary>
        public static float DampedSpring(float current, float target, ref float velocity, float smoothTime, float deltaTime)
        {
            float omega = 2f / smoothTime;
            float x = omega * deltaTime;
            float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
            float change = current - target;
            float temp = (velocity + omega * change) * deltaTime;
            velocity = (velocity - omega * temp) * exp;
            return target + (change + temp) * exp;
        }

        #endregion

        #region Angles

        /// <summary>
        /// Normalizes an angle to -180 to 180.
        /// </summary>
        public static float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle > 180f) angle -= 360f;
            else if (angle < -180f) angle += 360f;
            return angle;
        }

        /// <summary>
        /// Normalizes an angle to 0 to 360.
        /// </summary>
        public static float NormalizeAngle360(float angle)
        {
            angle %= 360f;
            if (angle < 0f) angle += 360f;
            return angle;
        }

        /// <summary>
        /// Gets the shortest angle difference between two angles.
        /// </summary>
        public static float DeltaAngle(float from, float to)
        {
            return Mathf.DeltaAngle(from, to);
        }

        /// <summary>
        /// Lerps between two angles using the shortest path.
        /// </summary>
        public static float LerpAngle(float from, float to, float t)
        {
            return Mathf.LerpAngle(from, to, t);
        }

        /// <summary>
        /// Checks if an angle is within a range of another angle.
        /// </summary>
        public static bool IsAngleWithinRange(float angle, float targetAngle, float range)
        {
            return Mathf.Abs(DeltaAngle(angle, targetAngle)) <= range;
        }

        #endregion

        #region Comparison

        /// <summary>
        /// Checks if two floats are approximately equal.
        /// </summary>
        public static bool Approximately(float a, float b, float epsilon = 0.0001f)
        {
            return Mathf.Abs(a - b) < epsilon;
        }

        /// <summary>
        /// Returns the sign of a value (-1, 0, or 1).
        /// </summary>
        public static int Sign(float value)
        {
            if (value > 0f) return 1;
            if (value < 0f) return -1;
            return 0;
        }

        /// <summary>
        /// Returns the sign of a value (-1 or 1, never 0).
        /// </summary>
        public static int SignNonZero(float value)
        {
            return value >= 0f ? 1 : -1;
        }

        #endregion

        #region Misc

        /// <summary>
        /// Gets the fractional part of a float.
        /// </summary>
        public static float Frac(float value)
        {
            return value - Mathf.Floor(value);
        }

        /// <summary>
        /// Rounds to the nearest multiple.
        /// </summary>
        public static float RoundToMultiple(float value, float multiple)
        {
            return Mathf.Round(value / multiple) * multiple;
        }

        /// <summary>
        /// Floors to the nearest multiple.
        /// </summary>
        public static float FloorToMultiple(float value, float multiple)
        {
            return Mathf.Floor(value / multiple) * multiple;
        }

        /// <summary>
        /// Ceils to the nearest multiple.
        /// </summary>
        public static float CeilToMultiple(float value, float multiple)
        {
            return Mathf.Ceil(value / multiple) * multiple;
        }

        /// <summary>
        /// Calculates the percentage between min and max.
        /// </summary>
        public static float Percentage(float value, float min, float max)
        {
            return (value - min) / (max - min);
        }

        /// <summary>
        /// Calculates the average of values.
        /// </summary>
        public static float Average(params float[] values)
        {
            if (values.Length == 0) return 0f;
            float sum = 0f;
            foreach (var v in values) sum += v;
            return sum / values.Length;
        }

        /// <summary>
        /// Calculates the weighted average.
        /// </summary>
        public static float WeightedAverage(float[] values, float[] weights)
        {
            if (values.Length != weights.Length || values.Length == 0) return 0f;

            float sum = 0f;
            float weightSum = 0f;
            for (int i = 0; i < values.Length; i++)
            {
                sum += values[i] * weights[i];
                weightSum += weights[i];
            }
            return sum / weightSum;
        }

        /// <summary>
        /// Ping-pong value between 0 and length.
        /// </summary>
        public static float PingPong(float t, float length)
        {
            return Mathf.PingPong(t, length);
        }

        /// <summary>
        /// Repeats value between 0 and length.
        /// </summary>
        public static float Repeat(float t, float length)
        {
            return Mathf.Repeat(t, length);
        }

        #endregion
    }
}
