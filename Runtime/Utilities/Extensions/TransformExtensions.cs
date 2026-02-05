using UnityEngine;

namespace LWT.UnityWorkbench.Utilities
{
    /// <summary>
    /// Extension methods for Transform.
    /// </summary>
    public static class TransformExtensions
    {
        #region Position

        /// <summary>
        /// Sets the X position.
        /// </summary>
        public static void SetPositionX(this Transform t, float x)
        {
            t.position = t.position.WithX(x);
        }

        /// <summary>
        /// Sets the Y position.
        /// </summary>
        public static void SetPositionY(this Transform t, float y)
        {
            t.position = t.position.WithY(y);
        }

        /// <summary>
        /// Sets the Z position.
        /// </summary>
        public static void SetPositionZ(this Transform t, float z)
        {
            t.position = t.position.WithZ(z);
        }

        /// <summary>
        /// Sets the local X position.
        /// </summary>
        public static void SetLocalPositionX(this Transform t, float x)
        {
            t.localPosition = t.localPosition.WithX(x);
        }

        /// <summary>
        /// Sets the local Y position.
        /// </summary>
        public static void SetLocalPositionY(this Transform t, float y)
        {
            t.localPosition = t.localPosition.WithY(y);
        }

        /// <summary>
        /// Sets the local Z position.
        /// </summary>
        public static void SetLocalPositionZ(this Transform t, float z)
        {
            t.localPosition = t.localPosition.WithZ(z);
        }

        /// <summary>
        /// Resets position to zero.
        /// </summary>
        public static void ResetPosition(this Transform t)
        {
            t.position = Vector3.zero;
        }

        /// <summary>
        /// Resets local position to zero.
        /// </summary>
        public static void ResetLocalPosition(this Transform t)
        {
            t.localPosition = Vector3.zero;
        }

        #endregion

        #region Rotation

        /// <summary>
        /// Sets the euler angle X.
        /// </summary>
        public static void SetEulerX(this Transform t, float x)
        {
            t.eulerAngles = t.eulerAngles.WithX(x);
        }

        /// <summary>
        /// Sets the euler angle Y.
        /// </summary>
        public static void SetEulerY(this Transform t, float y)
        {
            t.eulerAngles = t.eulerAngles.WithY(y);
        }

        /// <summary>
        /// Sets the euler angle Z.
        /// </summary>
        public static void SetEulerZ(this Transform t, float z)
        {
            t.eulerAngles = t.eulerAngles.WithZ(z);
        }

        /// <summary>
        /// Resets rotation to identity.
        /// </summary>
        public static void ResetRotation(this Transform t)
        {
            t.rotation = Quaternion.identity;
        }

        /// <summary>
        /// Resets local rotation to identity.
        /// </summary>
        public static void ResetLocalRotation(this Transform t)
        {
            t.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Looks at target on the Y axis only.
        /// </summary>
        public static void LookAtY(this Transform t, Vector3 target)
        {
            Vector3 direction = target - t.position;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                t.rotation = Quaternion.LookRotation(direction);
            }
        }

        /// <summary>
        /// Looks at target transform on the Y axis only.
        /// </summary>
        public static void LookAtY(this Transform t, Transform target)
        {
            t.LookAtY(target.position);
        }

        #endregion

        #region Scale

        /// <summary>
        /// Sets uniform local scale.
        /// </summary>
        public static void SetLocalScale(this Transform t, float scale)
        {
            t.localScale = Vector3.one * scale;
        }

        /// <summary>
        /// Sets the local scale X.
        /// </summary>
        public static void SetLocalScaleX(this Transform t, float x)
        {
            t.localScale = t.localScale.WithX(x);
        }

        /// <summary>
        /// Sets the local scale Y.
        /// </summary>
        public static void SetLocalScaleY(this Transform t, float y)
        {
            t.localScale = t.localScale.WithY(y);
        }

        /// <summary>
        /// Sets the local scale Z.
        /// </summary>
        public static void SetLocalScaleZ(this Transform t, float z)
        {
            t.localScale = t.localScale.WithZ(z);
        }

        /// <summary>
        /// Resets local scale to one.
        /// </summary>
        public static void ResetLocalScale(this Transform t)
        {
            t.localScale = Vector3.one;
        }

        #endregion

        #region Reset

        /// <summary>
        /// Resets position, rotation, and scale.
        /// </summary>
        public static void Reset(this Transform t)
        {
            t.position = Vector3.zero;
            t.rotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        /// <summary>
        /// Resets local position, rotation, and scale.
        /// </summary>
        public static void ResetLocal(this Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        #endregion

        #region Children

        /// <summary>
        /// Destroys all children.
        /// </summary>
        public static void DestroyAllChildren(this Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(t.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Destroys all children immediately (Editor only).
        /// </summary>
        public static void DestroyAllChildrenImmediate(this Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(t.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Gets all children as an array.
        /// </summary>
        public static Transform[] GetChildren(this Transform t)
        {
            var children = new Transform[t.childCount];
            for (int i = 0; i < t.childCount; i++)
            {
                children[i] = t.GetChild(i);
            }
            return children;
        }

        /// <summary>
        /// Finds a child by name recursively.
        /// </summary>
        public static Transform FindChildRecursive(this Transform t, string name)
        {
            foreach (Transform child in t)
            {
                if (child.name == name)
                    return child;

                var result = child.FindChildRecursive(name);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// Sets the layer for this transform and all children recursively.
        /// </summary>
        public static void SetLayerRecursive(this Transform t, int layer)
        {
            t.gameObject.layer = layer;
            foreach (Transform child in t)
            {
                child.SetLayerRecursive(layer);
            }
        }

        #endregion

        #region Distance & Direction

        /// <summary>
        /// Returns the direction to another transform.
        /// </summary>
        public static Vector3 DirectionTo(this Transform t, Transform target)
        {
            return t.position.DirectionTo(target.position);
        }

        /// <summary>
        /// Returns the direction to a point.
        /// </summary>
        public static Vector3 DirectionTo(this Transform t, Vector3 target)
        {
            return t.position.DirectionTo(target);
        }

        /// <summary>
        /// Returns the distance to another transform.
        /// </summary>
        public static float DistanceTo(this Transform t, Transform target)
        {
            return t.position.DistanceTo(target.position);
        }

        /// <summary>
        /// Returns the distance to a point.
        /// </summary>
        public static float DistanceTo(this Transform t, Vector3 target)
        {
            return t.position.DistanceTo(target);
        }

        /// <summary>
        /// Checks if within range of another transform.
        /// </summary>
        public static bool IsWithinRange(this Transform t, Transform target, float range)
        {
            return t.position.IsWithinRange(target.position, range);
        }

        /// <summary>
        /// Checks if within range of a point.
        /// </summary>
        public static bool IsWithinRange(this Transform t, Vector3 target, float range)
        {
            return t.position.IsWithinRange(target, range);
        }

        #endregion

        #region Hierarchy

        /// <summary>
        /// Gets the full hierarchy path.
        /// </summary>
        public static string GetPath(this Transform t)
        {
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }

        /// <summary>
        /// Gets the sibling index from the end.
        /// </summary>
        public static int GetSiblingIndexFromEnd(this Transform t)
        {
            if (t.parent == null) return 0;
            return t.parent.childCount - 1 - t.GetSiblingIndex();
        }

        /// <summary>
        /// Checks if this transform is a descendant of another.
        /// </summary>
        public static bool IsDescendantOf(this Transform t, Transform ancestor)
        {
            Transform current = t.parent;
            while (current != null)
            {
                if (current == ancestor) return true;
                current = current.parent;
            }
            return false;
        }

        #endregion
    }
}
