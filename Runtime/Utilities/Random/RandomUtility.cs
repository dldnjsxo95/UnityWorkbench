using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Utilities
{
    /// <summary>
    /// Extended random utility functions.
    /// </summary>
    public static class RandomUtility
    {
        #region Basic

        /// <summary>
        /// Returns true or false with 50% probability.
        /// </summary>
        public static bool Bool() => UnityEngine.Random.value > 0.5f;

        /// <summary>
        /// Returns true with the given probability (0-1).
        /// </summary>
        public static bool Chance(float probability) => UnityEngine.Random.value < probability;

        /// <summary>
        /// Returns true with the given percentage chance (0-100).
        /// </summary>
        public static bool ChancePercent(float percent) => UnityEngine.Random.value * 100f < percent;

        /// <summary>
        /// Returns 1 or -1 randomly.
        /// </summary>
        public static int Sign() => UnityEngine.Random.value > 0.5f ? 1 : -1;

        /// <summary>
        /// Returns a random float between 0 and 1.
        /// </summary>
        public static float Value => UnityEngine.Random.value;

        /// <summary>
        /// Returns a random float between min and max.
        /// </summary>
        public static float Range(float min, float max) => UnityEngine.Random.Range(min, max);

        /// <summary>
        /// Returns a random int between min (inclusive) and max (exclusive).
        /// </summary>
        public static int Range(int min, int max) => UnityEngine.Random.Range(min, max);

        /// <summary>
        /// Returns a random int between 0 (inclusive) and max (exclusive).
        /// </summary>
        public static int Range(int max) => UnityEngine.Random.Range(0, max);

        #endregion

        #region Vectors

        /// <summary>
        /// Returns a random point inside a unit circle.
        /// </summary>
        public static Vector2 InsideUnitCircle() => UnityEngine.Random.insideUnitCircle;

        /// <summary>
        /// Returns a random point on the edge of a unit circle.
        /// </summary>
        public static Vector2 OnUnitCircle()
        {
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        /// <summary>
        /// Returns a random point inside a circle with given radius.
        /// </summary>
        public static Vector2 InsideCircle(float radius) => InsideUnitCircle() * radius;

        /// <summary>
        /// Returns a random point on the edge of a circle with given radius.
        /// </summary>
        public static Vector2 OnCircle(float radius) => OnUnitCircle() * radius;

        /// <summary>
        /// Returns a random point inside a unit sphere.
        /// </summary>
        public static Vector3 InsideUnitSphere() => UnityEngine.Random.insideUnitSphere;

        /// <summary>
        /// Returns a random point on the surface of a unit sphere.
        /// </summary>
        public static Vector3 OnUnitSphere() => UnityEngine.Random.onUnitSphere;

        /// <summary>
        /// Returns a random point inside a sphere with given radius.
        /// </summary>
        public static Vector3 InsideSphere(float radius) => InsideUnitSphere() * radius;

        /// <summary>
        /// Returns a random point on the surface of a sphere with given radius.
        /// </summary>
        public static Vector3 OnSphere(float radius) => OnUnitSphere() * radius;

        /// <summary>
        /// Returns a random point inside a box defined by size.
        /// </summary>
        public static Vector3 InsideBox(Vector3 size)
        {
            return new Vector3(
                UnityEngine.Random.Range(-size.x / 2f, size.x / 2f),
                UnityEngine.Random.Range(-size.y / 2f, size.y / 2f),
                UnityEngine.Random.Range(-size.z / 2f, size.z / 2f)
            );
        }

        /// <summary>
        /// Returns a random point inside bounds.
        /// </summary>
        public static Vector3 InsideBounds(Bounds bounds)
        {
            return new Vector3(
                UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
                UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
            );
        }

        /// <summary>
        /// Returns a random Vector2 between min and max.
        /// </summary>
        public static Vector2 Vector2(Vector2 min, Vector2 max)
        {
            return new Vector2(
                UnityEngine.Random.Range(min.x, max.x),
                UnityEngine.Random.Range(min.y, max.y)
            );
        }

        /// <summary>
        /// Returns a random Vector3 between min and max.
        /// </summary>
        public static Vector3 Vector3(Vector3 min, Vector3 max)
        {
            return new UnityEngine.Vector3(
                UnityEngine.Random.Range(min.x, max.x),
                UnityEngine.Random.Range(min.y, max.y),
                UnityEngine.Random.Range(min.z, max.z)
            );
        }

        /// <summary>
        /// Returns a random direction (normalized Vector3).
        /// </summary>
        public static Vector3 Direction() => OnUnitSphere();

        /// <summary>
        /// Returns a random direction on the XZ plane.
        /// </summary>
        public static Vector3 DirectionXZ()
        {
            Vector2 dir2D = OnUnitCircle();
            return new Vector3(dir2D.x, 0f, dir2D.y);
        }

        /// <summary>
        /// Returns a random rotation.
        /// </summary>
        public static Quaternion Rotation() => UnityEngine.Random.rotation;

        /// <summary>
        /// Returns a random rotation with uniform distribution.
        /// </summary>
        public static Quaternion RotationUniform() => UnityEngine.Random.rotationUniform;

        /// <summary>
        /// Returns a random rotation around the Y axis.
        /// </summary>
        public static Quaternion RotationY()
        {
            return Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
        }

        #endregion

        #region Colors

        /// <summary>
        /// Returns a random color with full alpha.
        /// </summary>
        public static Color Color()
        {
            return new Color(Value, Value, Value, 1f);
        }

        /// <summary>
        /// Returns a random color with random alpha.
        /// </summary>
        public static Color ColorWithAlpha()
        {
            return new Color(Value, Value, Value, Value);
        }

        /// <summary>
        /// Returns a random color with specified saturation and value.
        /// </summary>
        public static Color ColorHSV(float saturation = 1f, float value = 1f)
        {
            return UnityEngine.Color.HSVToRGB(Value, saturation, value);
        }

        /// <summary>
        /// Returns a random color from a palette.
        /// </summary>
        public static Color ColorFromPalette(params Color[] colors)
        {
            if (colors.Length == 0) return UnityEngine.Color.white;
            return colors[Range(colors.Length)];
        }

        #endregion

        #region Weighted Selection

        /// <summary>
        /// Selects an index based on weights.
        /// </summary>
        public static int WeightedIndex(float[] weights)
        {
            float total = 0f;
            foreach (var w in weights) total += w;

            float random = UnityEngine.Random.Range(0f, total);
            float current = 0f;

            for (int i = 0; i < weights.Length; i++)
            {
                current += weights[i];
                if (random <= current) return i;
            }

            return weights.Length - 1;
        }

        /// <summary>
        /// Selects an index based on weights (int version).
        /// </summary>
        public static int WeightedIndex(int[] weights)
        {
            int total = 0;
            foreach (var w in weights) total += w;

            int random = UnityEngine.Random.Range(0, total);
            int current = 0;

            for (int i = 0; i < weights.Length; i++)
            {
                current += weights[i];
                if (random < current) return i;
            }

            return weights.Length - 1;
        }

        /// <summary>
        /// Selects a value from a weighted dictionary.
        /// </summary>
        public static T WeightedSelect<T>(Dictionary<T, float> weightedItems)
        {
            float total = 0f;
            foreach (var kvp in weightedItems) total += kvp.Value;

            float random = UnityEngine.Random.Range(0f, total);
            float current = 0f;

            foreach (var kvp in weightedItems)
            {
                current += kvp.Value;
                if (random <= current) return kvp.Key;
            }

            // Fallback
            foreach (var kvp in weightedItems) return kvp.Key;
            return default;
        }

        #endregion

        #region Distribution

        /// <summary>
        /// Returns a random value with gaussian/normal distribution.
        /// </summary>
        public static float Gaussian(float mean = 0f, float standardDeviation = 1f)
        {
            // Box-Muller transform
            float u1 = 1f - UnityEngine.Random.value;
            float u2 = 1f - UnityEngine.Random.value;
            float randStdNormal = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Sin(2f * Mathf.PI * u2);
            return mean + standardDeviation * randStdNormal;
        }

        /// <summary>
        /// Returns a random value biased towards the center.
        /// </summary>
        public static float CenterBiased(float min, float max)
        {
            return (UnityEngine.Random.Range(min, max) + UnityEngine.Random.Range(min, max)) / 2f;
        }

        /// <summary>
        /// Returns a random value biased towards the edges.
        /// </summary>
        public static float EdgeBiased(float min, float max)
        {
            float mid = (min + max) / 2f;
            float value = UnityEngine.Random.Range(min, max);
            return value < mid ? min + (mid - value) : max - (value - mid);
        }

        /// <summary>
        /// Returns a random value with exponential distribution.
        /// </summary>
        public static float Exponential(float lambda = 1f)
        {
            return -Mathf.Log(1f - UnityEngine.Random.value) / lambda;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Sets the random seed.
        /// </summary>
        public static void SetSeed(int seed)
        {
            UnityEngine.Random.InitState(seed);
        }

        /// <summary>
        /// Gets the current random state.
        /// </summary>
        public static UnityEngine.Random.State GetState()
        {
            return UnityEngine.Random.state;
        }

        /// <summary>
        /// Sets the random state.
        /// </summary>
        public static void SetState(UnityEngine.Random.State state)
        {
            UnityEngine.Random.state = state;
        }

        /// <summary>
        /// Generates a random string of given length.
        /// </summary>
        public static string String(int length, string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789")
        {
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = characters[Range(characters.Length)];
            }
            return new string(result);
        }

        /// <summary>
        /// Generates a random GUID-like string.
        /// </summary>
        public static string Guid()
        {
            return System.Guid.NewGuid().ToString();
        }

        #endregion
    }
}
