using UnityEngine;

namespace LWT.UnityWorkbench.DebugTools
{
    /// <summary>
    /// Utility class for drawing extended gizmos.
    /// </summary>
    public static class GizmoUtility
    {
        /// <summary>
        /// Draw a wire circle in 3D space.
        /// </summary>
        public static void DrawWireCircle(Vector3 center, float radius, Color color, int segments = 32)
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = color;

            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 point = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }

            Gizmos.color = prevColor;
        }

        /// <summary>
        /// Draw a wire circle on a specific axis.
        /// </summary>
        public static void DrawWireCircle(Vector3 center, float radius, Vector3 normal, Color color, int segments = 32)
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = color;

            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + rotation * new Vector3(radius, 0, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 localPoint = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                Vector3 point = center + rotation * localPoint;
                Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }

            Gizmos.color = prevColor;
        }

        /// <summary>
        /// Draw an arrow from start to end.
        /// </summary>
        public static void DrawArrow(Vector3 from, Vector3 to, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20f)
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = color;

            Gizmos.DrawLine(from, to);

            Vector3 direction = (to - from).normalized;
            float length = (to - from).magnitude;
            float headLength = Mathf.Min(arrowHeadLength, length * 0.5f);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;

            Gizmos.DrawLine(to, to + right * headLength);
            Gizmos.DrawLine(to, to + left * headLength);

            Gizmos.color = prevColor;
        }

        /// <summary>
        /// Draw a direction arrow from a point.
        /// </summary>
        public static void DrawDirection(Vector3 from, Vector3 direction, float length, Color color)
        {
            DrawArrow(from, from + direction.normalized * length, color);
        }

        /// <summary>
        /// Draw a wire box with rotation.
        /// </summary>
        public static void DrawWireBox(Vector3 center, Vector3 size, Quaternion rotation, Color color)
        {
            Color prevColor = Gizmos.color;
            Matrix4x4 prevMatrix = Gizmos.matrix;

            Gizmos.color = color;
            Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, size);

            Gizmos.matrix = prevMatrix;
            Gizmos.color = prevColor;
        }

        /// <summary>
        /// Draw bounds.
        /// </summary>
        public static void DrawBounds(Bounds bounds, Color color)
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = color;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            Gizmos.color = prevColor;
        }

        /// <summary>
        /// Draw a wire sphere.
        /// </summary>
        public static void DrawWireSphere(Vector3 center, float radius, Color color)
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = color;
            Gizmos.DrawWireSphere(center, radius);
            Gizmos.color = prevColor;
        }

        /// <summary>
        /// Draw a solid sphere.
        /// </summary>
        public static void DrawSphere(Vector3 center, float radius, Color color)
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = color;
            Gizmos.DrawSphere(center, radius);
            Gizmos.color = prevColor;
        }

        /// <summary>
        /// Draw a wire cylinder.
        /// </summary>
        public static void DrawWireCylinder(Vector3 center, float radius, float height, Color color, int segments = 16)
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = color;

            float halfHeight = height * 0.5f;
            Vector3 topCenter = center + Vector3.up * halfHeight;
            Vector3 bottomCenter = center - Vector3.up * halfHeight;

            // Draw top and bottom circles
            DrawWireCircle(topCenter, radius, color, segments);
            DrawWireCircle(bottomCenter, radius, color, segments);

            // Draw vertical lines
            float angleStep = 360f / segments;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(topCenter + offset, bottomCenter + offset);
            }

            Gizmos.color = prevColor;
        }

        /// <summary>
        /// Draw a wire capsule.
        /// </summary>
        public static void DrawWireCapsule(Vector3 center, float radius, float height, Color color, int segments = 16)
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = color;

            float halfHeight = Mathf.Max(0, (height - radius * 2) * 0.5f);
            Vector3 topCenter = center + Vector3.up * halfHeight;
            Vector3 bottomCenter = center - Vector3.up * halfHeight;

            // Draw hemispheres
            DrawWireCircle(topCenter, radius, color, segments);
            DrawWireCircle(bottomCenter, radius, color, segments);

            // Draw connecting lines
            float angleStep = 360f / segments;
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(topCenter + offset, bottomCenter + offset);
            }

            // Draw hemisphere arcs
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f * Mathf.Deg2Rad;
                DrawHemisphereArc(topCenter, radius, angle, true, color, segments / 4);
                DrawHemisphereArc(bottomCenter, radius, angle, false, color, segments / 4);
            }

            Gizmos.color = prevColor;
        }

        private static void DrawHemisphereArc(Vector3 center, float radius, float rotationAngle, bool top, Color color, int segments)
        {
            float sign = top ? 1f : -1f;
            float angleStep = 90f / segments;

            Vector3 prevPoint = center + new Vector3(
                Mathf.Cos(rotationAngle) * radius,
                0,
                Mathf.Sin(rotationAngle) * radius);

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                float y = Mathf.Sin(angle) * radius * sign;
                float horizontalRadius = Mathf.Cos(angle) * radius;

                Vector3 point = center + new Vector3(
                    Mathf.Cos(rotationAngle) * horizontalRadius,
                    y,
                    Mathf.Sin(rotationAngle) * horizontalRadius);

                Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }
        }

        /// <summary>
        /// Draw a wire cone.
        /// </summary>
        public static void DrawWireCone(Vector3 tip, Vector3 direction, float angle, float length, Color color, int segments = 16)
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = color;

            Vector3 baseCenter = tip + direction.normalized * length;
            float radius = Mathf.Tan(angle * Mathf.Deg2Rad) * length;

            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);

            // Draw base circle
            DrawWireCircle(baseCenter, radius, direction, color, segments);

            // Draw lines from tip to base
            float angleStep = 360f / segments;
            for (int i = 0; i < segments; i++)
            {
                float a = i * angleStep * Mathf.Deg2Rad;
                Vector3 localPoint = new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius);
                Vector3 point = baseCenter + rotation * localPoint;
                Gizmos.DrawLine(tip, point);
            }

            Gizmos.color = prevColor;
        }

        /// <summary>
        /// Draw a path through points.
        /// </summary>
        public static void DrawPath(Vector3[] points, Color color, bool closed = false)
        {
            if (points == null || points.Length < 2) return;

            Color prevColor = Gizmos.color;
            Gizmos.color = color;

            for (int i = 0; i < points.Length - 1; i++)
            {
                Gizmos.DrawLine(points[i], points[i + 1]);
            }

            if (closed && points.Length > 2)
            {
                Gizmos.DrawLine(points[points.Length - 1], points[0]);
            }

            Gizmos.color = prevColor;
        }

        /// <summary>
        /// Draw a grid on the XZ plane.
        /// </summary>
        public static void DrawGrid(Vector3 center, int gridSize, float cellSize, Color color)
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = color;

            float halfSize = gridSize * cellSize * 0.5f;

            for (int i = 0; i <= gridSize; i++)
            {
                float offset = i * cellSize - halfSize;

                // Horizontal lines
                Gizmos.DrawLine(
                    center + new Vector3(-halfSize, 0, offset),
                    center + new Vector3(halfSize, 0, offset));

                // Vertical lines
                Gizmos.DrawLine(
                    center + new Vector3(offset, 0, -halfSize),
                    center + new Vector3(offset, 0, halfSize));
            }

            Gizmos.color = prevColor;
        }

        /// <summary>
        /// Draw a cross at position.
        /// </summary>
        public static void DrawCross(Vector3 position, float size, Color color)
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = color;

            float half = size * 0.5f;
            Gizmos.DrawLine(position - Vector3.right * half, position + Vector3.right * half);
            Gizmos.DrawLine(position - Vector3.up * half, position + Vector3.up * half);
            Gizmos.DrawLine(position - Vector3.forward * half, position + Vector3.forward * half);

            Gizmos.color = prevColor;
        }

        /// <summary>
        /// Draw coordinate axes.
        /// </summary>
        public static void DrawAxes(Vector3 position, float size)
        {
            DrawArrow(position, position + Vector3.right * size, Color.red);
            DrawArrow(position, position + Vector3.up * size, Color.green);
            DrawArrow(position, position + Vector3.forward * size, Color.blue);
        }

        /// <summary>
        /// Draw coordinate axes with rotation.
        /// </summary>
        public static void DrawAxes(Vector3 position, Quaternion rotation, float size)
        {
            DrawArrow(position, position + rotation * Vector3.right * size, Color.red);
            DrawArrow(position, position + rotation * Vector3.up * size, Color.green);
            DrawArrow(position, position + rotation * Vector3.forward * size, Color.blue);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Draw a label in scene view (Editor only).
        /// </summary>
        public static void DrawLabel(Vector3 position, string text, Color color)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = color;
            style.fontSize = 12;
            style.fontStyle = FontStyle.Bold;
            UnityEditor.Handles.Label(position, text, style);
        }

        /// <summary>
        /// Draw a label in scene view (Editor only).
        /// </summary>
        public static void DrawLabel(Vector3 position, string text)
        {
            DrawLabel(position, text, Color.white);
        }
#endif
    }
}
