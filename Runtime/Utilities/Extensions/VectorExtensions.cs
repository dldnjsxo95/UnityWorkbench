using UnityEngine;

namespace LWT.UnityWorkbench.Utilities
{
    /// <summary>
    /// Extension methods for Vector2, Vector3, and Vector4.
    /// </summary>
    public static class VectorExtensions
    {
        #region Vector3 Extensions

        /// <summary>
        /// Returns a new Vector3 with modified x value.
        /// </summary>
        public static Vector3 WithX(this Vector3 v, float x) => new Vector3(x, v.y, v.z);

        /// <summary>
        /// Returns a new Vector3 with modified y value.
        /// </summary>
        public static Vector3 WithY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);

        /// <summary>
        /// Returns a new Vector3 with modified z value.
        /// </summary>
        public static Vector3 WithZ(this Vector3 v, float z) => new Vector3(v.x, v.y, z);

        /// <summary>
        /// Returns XY components as Vector2.
        /// </summary>
        public static Vector2 XY(this Vector3 v) => new Vector2(v.x, v.y);

        /// <summary>
        /// Returns XZ components as Vector2.
        /// </summary>
        public static Vector2 XZ(this Vector3 v) => new Vector2(v.x, v.z);

        /// <summary>
        /// Returns YZ components as Vector2.
        /// </summary>
        public static Vector2 YZ(this Vector3 v) => new Vector2(v.y, v.z);

        /// <summary>
        /// Flattens the vector on the XZ plane (y = 0).
        /// </summary>
        public static Vector3 Flat(this Vector3 v) => new Vector3(v.x, 0f, v.z);

        /// <summary>
        /// Returns the direction to another point.
        /// </summary>
        public static Vector3 DirectionTo(this Vector3 from, Vector3 to) => (to - from).normalized;

        /// <summary>
        /// Returns the distance to another point.
        /// </summary>
        public static float DistanceTo(this Vector3 from, Vector3 to) => Vector3.Distance(from, to);

        /// <summary>
        /// Returns the squared distance to another point (faster than DistanceTo).
        /// </summary>
        public static float SqrDistanceTo(this Vector3 from, Vector3 to) => (to - from).sqrMagnitude;

        /// <summary>
        /// Checks if the point is within range of another point.
        /// </summary>
        public static bool IsWithinRange(this Vector3 from, Vector3 to, float range)
        {
            return from.SqrDistanceTo(to) <= range * range;
        }

        /// <summary>
        /// Clamps the vector magnitude.
        /// </summary>
        public static Vector3 ClampMagnitude(this Vector3 v, float maxMagnitude)
        {
            return Vector3.ClampMagnitude(v, maxMagnitude);
        }

        /// <summary>
        /// Returns the absolute value of each component.
        /// </summary>
        public static Vector3 Abs(this Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        /// <summary>
        /// Rounds each component to the nearest integer.
        /// </summary>
        public static Vector3 Round(this Vector3 v)
        {
            return new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
        }

        /// <summary>
        /// Floors each component.
        /// </summary>
        public static Vector3 Floor(this Vector3 v)
        {
            return new Vector3(Mathf.Floor(v.x), Mathf.Floor(v.y), Mathf.Floor(v.z));
        }

        /// <summary>
        /// Ceils each component.
        /// </summary>
        public static Vector3 Ceil(this Vector3 v)
        {
            return new Vector3(Mathf.Ceil(v.x), Mathf.Ceil(v.y), Mathf.Ceil(v.z));
        }

        /// <summary>
        /// Returns Vector3Int with rounded values.
        /// </summary>
        public static Vector3Int ToVector3Int(this Vector3 v)
        {
            return new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
        }

        /// <summary>
        /// Multiplies component-wise with another vector.
        /// </summary>
        public static Vector3 Multiply(this Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        /// <summary>
        /// Divides component-wise by another vector.
        /// </summary>
        public static Vector3 Divide(this Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }

        /// <summary>
        /// Returns the average of all components.
        /// </summary>
        public static float Average(this Vector3 v) => (v.x + v.y + v.z) / 3f;

        /// <summary>
        /// Returns the maximum component value.
        /// </summary>
        public static float MaxComponent(this Vector3 v) => Mathf.Max(v.x, Mathf.Max(v.y, v.z));

        /// <summary>
        /// Returns the minimum component value.
        /// </summary>
        public static float MinComponent(this Vector3 v) => Mathf.Min(v.x, Mathf.Min(v.y, v.z));

        /// <summary>
        /// Rotates the vector around an axis.
        /// </summary>
        public static Vector3 RotateAround(this Vector3 v, Vector3 axis, float angle)
        {
            return Quaternion.AngleAxis(angle, axis) * v;
        }

        #endregion

        #region Vector2 Extensions

        /// <summary>
        /// Returns a new Vector2 with modified x value.
        /// </summary>
        public static Vector2 WithX(this Vector2 v, float x) => new Vector2(x, v.y);

        /// <summary>
        /// Returns a new Vector2 with modified y value.
        /// </summary>
        public static Vector2 WithY(this Vector2 v, float y) => new Vector2(v.x, y);

        /// <summary>
        /// Converts to Vector3 with z = 0.
        /// </summary>
        public static Vector3 ToVector3(this Vector2 v) => new Vector3(v.x, v.y, 0f);

        /// <summary>
        /// Converts to Vector3 on XZ plane (y = 0).
        /// </summary>
        public static Vector3 ToVector3XZ(this Vector2 v) => new Vector3(v.x, 0f, v.y);

        /// <summary>
        /// Returns the direction to another point.
        /// </summary>
        public static Vector2 DirectionTo(this Vector2 from, Vector2 to) => (to - from).normalized;

        /// <summary>
        /// Returns the distance to another point.
        /// </summary>
        public static float DistanceTo(this Vector2 from, Vector2 to) => Vector2.Distance(from, to);

        /// <summary>
        /// Returns the perpendicular vector (rotated 90 degrees).
        /// </summary>
        public static Vector2 Perpendicular(this Vector2 v) => new Vector2(-v.y, v.x);

        /// <summary>
        /// Returns the angle in degrees from the positive X axis.
        /// </summary>
        public static float ToAngle(this Vector2 v) => Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;

        /// <summary>
        /// Creates a Vector2 from an angle in degrees.
        /// </summary>
        public static Vector2 FromAngle(float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }

        /// <summary>
        /// Rotates the vector by the given angle in degrees.
        /// </summary>
        public static Vector2 Rotate(this Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        /// <summary>
        /// Returns the absolute value of each component.
        /// </summary>
        public static Vector2 Abs(this Vector2 v) => new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));

        /// <summary>
        /// Rounds each component to the nearest integer.
        /// </summary>
        public static Vector2 Round(this Vector2 v) => new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));

        /// <summary>
        /// Returns Vector2Int with rounded values.
        /// </summary>
        public static Vector2Int ToVector2Int(this Vector2 v)
        {
            return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
        }

        #endregion

        #region Vector Int Extensions

        /// <summary>
        /// Converts Vector3Int to Vector3.
        /// </summary>
        public static Vector3 ToVector3(this Vector3Int v) => new Vector3(v.x, v.y, v.z);

        /// <summary>
        /// Converts Vector2Int to Vector2.
        /// </summary>
        public static Vector2 ToVector2(this Vector2Int v) => new Vector2(v.x, v.y);

        #endregion
    }
}
