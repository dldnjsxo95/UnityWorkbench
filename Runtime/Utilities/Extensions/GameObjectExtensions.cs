using UnityEngine;

namespace LWT.UnityWorkbench.Utilities
{
    /// <summary>
    /// Extension methods for GameObject.
    /// </summary>
    public static class GameObjectExtensions
    {
        #region Components

        /// <summary>
        /// Gets or adds a component.
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            var component = go.GetComponent<T>();
            return component != null ? component : go.AddComponent<T>();
        }

        /// <summary>
        /// Checks if the GameObject has a component.
        /// </summary>
        public static bool HasComponent<T>(this GameObject go) where T : Component
        {
            return go.GetComponent<T>() != null;
        }

        /// <summary>
        /// Tries to get a component.
        /// </summary>
        public static bool TryGetComponent<T>(this GameObject go, out T component) where T : Component
        {
            component = go.GetComponent<T>();
            return component != null;
        }

        /// <summary>
        /// Removes a component if it exists.
        /// </summary>
        public static void RemoveComponent<T>(this GameObject go) where T : Component
        {
            var component = go.GetComponent<T>();
            if (component != null)
            {
                Object.Destroy(component);
            }
        }

        /// <summary>
        /// Removes a component immediately if it exists (Editor only).
        /// </summary>
        public static void RemoveComponentImmediate<T>(this GameObject go) where T : Component
        {
            var component = go.GetComponent<T>();
            if (component != null)
            {
                Object.DestroyImmediate(component);
            }
        }

        #endregion

        #region Layer & Tag

        /// <summary>
        /// Sets the layer for this GameObject and all children.
        /// </summary>
        public static void SetLayerRecursive(this GameObject go, int layer)
        {
            go.layer = layer;
            go.transform.SetLayerRecursive(layer);
        }

        /// <summary>
        /// Sets the layer by name.
        /// </summary>
        public static void SetLayer(this GameObject go, string layerName)
        {
            go.layer = LayerMask.NameToLayer(layerName);
        }

        /// <summary>
        /// Checks if the GameObject is in a specific layer.
        /// </summary>
        public static bool IsInLayer(this GameObject go, int layer)
        {
            return go.layer == layer;
        }

        /// <summary>
        /// Checks if the GameObject is in a layer mask.
        /// </summary>
        public static bool IsInLayerMask(this GameObject go, LayerMask layerMask)
        {
            return ((1 << go.layer) & layerMask) != 0;
        }

        /// <summary>
        /// Checks if the GameObject has a specific tag.
        /// </summary>
        public static bool HasTag(this GameObject go, string tag)
        {
            return go.CompareTag(tag);
        }

        #endregion

        #region Hierarchy

        /// <summary>
        /// Gets the full hierarchy path.
        /// </summary>
        public static string GetPath(this GameObject go)
        {
            return go.transform.GetPath();
        }

        /// <summary>
        /// Sets the parent and optionally resets local transform.
        /// </summary>
        public static void SetParent(this GameObject go, Transform parent, bool resetLocal = false)
        {
            go.transform.SetParent(parent);
            if (resetLocal)
            {
                go.transform.ResetLocal();
            }
        }

        /// <summary>
        /// Sets the parent GameObject.
        /// </summary>
        public static void SetParent(this GameObject go, GameObject parent, bool resetLocal = false)
        {
            go.SetParent(parent?.transform, resetLocal);
        }

        /// <summary>
        /// Unparents the GameObject.
        /// </summary>
        public static void Unparent(this GameObject go)
        {
            go.transform.SetParent(null);
        }

        #endregion

        #region Active State

        /// <summary>
        /// Toggles the active state.
        /// </summary>
        public static void ToggleActive(this GameObject go)
        {
            go.SetActive(!go.activeSelf);
        }

        /// <summary>
        /// Sets active with a delay using coroutine.
        /// </summary>
        public static void SetActiveDelayed(this GameObject go, bool active, float delay)
        {
            if (go.TryGetComponent<MonoBehaviour>(out var mb))
            {
                mb.StartCoroutine(SetActiveDelayedCoroutine(go, active, delay));
            }
        }

        private static System.Collections.IEnumerator SetActiveDelayedCoroutine(GameObject go, bool active, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (go != null)
            {
                go.SetActive(active);
            }
        }

        #endregion

        #region Bounds

        /// <summary>
        /// Gets the combined bounds of all renderers.
        /// </summary>
        public static Bounds GetRendererBounds(this GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return new Bounds(go.transform.position, Vector3.zero);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            return bounds;
        }

        /// <summary>
        /// Gets the combined bounds of all colliders.
        /// </summary>
        public static Bounds GetColliderBounds(this GameObject go)
        {
            var colliders = go.GetComponentsInChildren<Collider>();
            if (colliders.Length == 0)
            {
                return new Bounds(go.transform.position, Vector3.zero);
            }

            Bounds bounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
            {
                bounds.Encapsulate(colliders[i].bounds);
            }
            return bounds;
        }

        #endregion

        #region Prefab

        /// <summary>
        /// Instantiates a copy of this GameObject.
        /// </summary>
        public static GameObject Clone(this GameObject go)
        {
            return Object.Instantiate(go);
        }

        /// <summary>
        /// Instantiates a copy at a specific position and rotation.
        /// </summary>
        public static GameObject Clone(this GameObject go, Vector3 position, Quaternion rotation)
        {
            return Object.Instantiate(go, position, rotation);
        }

        /// <summary>
        /// Instantiates a copy as a child of a parent.
        /// </summary>
        public static GameObject Clone(this GameObject go, Transform parent, bool worldPositionStays = false)
        {
            return Object.Instantiate(go, parent, worldPositionStays);
        }

        #endregion

        #region Destroy

        /// <summary>
        /// Destroys this GameObject.
        /// </summary>
        public static void Destroy(this GameObject go)
        {
            Object.Destroy(go);
        }

        /// <summary>
        /// Destroys this GameObject with a delay.
        /// </summary>
        public static void Destroy(this GameObject go, float delay)
        {
            Object.Destroy(go, delay);
        }

        /// <summary>
        /// Destroys this GameObject immediately (Editor only).
        /// </summary>
        public static void DestroyImmediate(this GameObject go)
        {
            Object.DestroyImmediate(go);
        }

        #endregion
    }
}
