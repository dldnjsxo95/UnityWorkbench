using UnityEngine;

namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// Extension methods for easy pooling access.
    /// </summary>
    public static class PoolExtensions
    {
        /// <summary>
        /// Spawn a copy of this prefab from pool.
        /// </summary>
        public static GameObject Spawn(this GameObject prefab)
        {
            return PoolManager.Instance.Spawn(prefab);
        }

        /// <summary>
        /// Spawn a copy of this prefab from pool at position.
        /// </summary>
        public static GameObject Spawn(this GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return PoolManager.Instance.Spawn(prefab, position, rotation);
        }

        /// <summary>
        /// Spawn and get component.
        /// </summary>
        public static T Spawn<T>(this GameObject prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return PoolManager.Instance.Spawn<T>(prefab, position, rotation);
        }

        /// <summary>
        /// Return this object to pool.
        /// </summary>
        public static void Despawn(this GameObject obj)
        {
            PoolManager.Instance.Despawn(obj);
        }

        /// <summary>
        /// Return this object to pool after delay.
        /// </summary>
        public static void Despawn(this GameObject obj, float delay)
        {
            PoolManager.Instance.Despawn(obj, delay);
        }

        /// <summary>
        /// Return this component's gameObject to pool.
        /// </summary>
        public static void Despawn(this Component component)
        {
            PoolManager.Instance.Despawn(component.gameObject);
        }

        /// <summary>
        /// Return this component's gameObject to pool after delay.
        /// </summary>
        public static void Despawn(this Component component, float delay)
        {
            PoolManager.Instance.Despawn(component.gameObject, delay);
        }
    }
}
