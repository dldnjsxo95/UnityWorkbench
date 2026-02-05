using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// Central manager for multiple object pools.
    /// Provides global access to spawn/despawn pooled objects.
    /// </summary>
    public class PoolManager : PersistentMonoSingleton<PoolManager>
    {
        [System.Serializable]
        public class PoolConfig
        {
            public GameObject Prefab;
            public int InitialSize = 10;
            public int MaxSize = 100;
            public bool AutoExpand = true;
        }

        [SerializeField] private List<PoolConfig> _poolConfigs = new List<PoolConfig>();
        [SerializeField] private bool _createContainerPerPool = true;

        private readonly Dictionary<GameObject, GameObjectPool> _pools = new Dictionary<GameObject, GameObjectPool>();
        private readonly Dictionary<GameObject, GameObjectPool> _instanceToPool = new Dictionary<GameObject, GameObjectPool>();

        protected override void OnSingletonAwake()
        {
            // Initialize pre-configured pools
            foreach (var config in _poolConfigs)
            {
                if (config.Prefab != null)
                {
                    CreatePool(config.Prefab, config.InitialSize, config.MaxSize, config.AutoExpand);
                }
            }
        }

        /// <summary>
        /// Create a new pool for a prefab.
        /// </summary>
        public GameObjectPool CreatePool(GameObject prefab, int initialSize = 10, int maxSize = 100, bool autoExpand = true)
        {
            if (_pools.ContainsKey(prefab))
            {
                Debug.LogWarning($"[PoolManager] Pool already exists for {prefab.name}");
                return _pools[prefab];
            }

            Transform parent = transform;
            if (_createContainerPerPool)
            {
                var container = new GameObject($"Pool_{prefab.name}");
                container.transform.SetParent(transform);
                parent = container.transform;
            }

            var pool = new GameObjectPool(prefab, parent, initialSize, maxSize, autoExpand);
            _pools[prefab] = pool;

            return pool;
        }

        /// <summary>
        /// Get pool for a prefab. Creates one if it doesn't exist.
        /// </summary>
        public GameObjectPool GetPool(GameObject prefab)
        {
            if (!_pools.TryGetValue(prefab, out var pool))
            {
                pool = CreatePool(prefab);
            }
            return pool;
        }

        /// <summary>
        /// Spawn an object from pool.
        /// </summary>
        public GameObject Spawn(GameObject prefab)
        {
            return Spawn(prefab, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Spawn an object from pool at position.
        /// </summary>
        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var pool = GetPool(prefab);
            var obj = pool.Get(position, rotation);

            if (obj != null)
            {
                _instanceToPool[obj] = pool;
            }

            return obj;
        }

        /// <summary>
        /// Spawn and get component.
        /// </summary>
        public T Spawn<T>(GameObject prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            var obj = Spawn(prefab, position, rotation);
            return obj != null ? obj.GetComponent<T>() : null;
        }

        /// <summary>
        /// Return object to its pool.
        /// </summary>
        public void Despawn(GameObject obj)
        {
            if (obj == null) return;

            if (_instanceToPool.TryGetValue(obj, out var pool))
            {
                pool.Release(obj);
                _instanceToPool.Remove(obj);
            }
            else
            {
                Debug.LogWarning($"[PoolManager] Object {obj.name} was not spawned from pool. Destroying.");
                Destroy(obj);
            }
        }

        /// <summary>
        /// Return object to pool after delay.
        /// </summary>
        public void Despawn(GameObject obj, float delay)
        {
            if (obj == null) return;
            StartCoroutine(DespawnAfterDelay(obj, delay));
        }

        private System.Collections.IEnumerator DespawnAfterDelay(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            Despawn(obj);
        }

        /// <summary>
        /// Clear all pools.
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }
            _instanceToPool.Clear();
        }

        /// <summary>
        /// Get statistics for a pool.
        /// </summary>
        public (int active, int inactive, int total) GetPoolStats(GameObject prefab)
        {
            if (_pools.TryGetValue(prefab, out var pool))
            {
                return (pool.CountActive, pool.CountInactive, pool.CountTotal);
            }
            return (0, 0, 0);
        }

        /// <summary>
        /// Get all pool statistics.
        /// </summary>
        public Dictionary<string, (int active, int inactive, int total)> GetAllPoolStats()
        {
            var stats = new Dictionary<string, (int, int, int)>();
            foreach (var kvp in _pools)
            {
                stats[kvp.Key.name] = (kvp.Value.CountActive, kvp.Value.CountInactive, kvp.Value.CountTotal);
            }
            return stats;
        }
    }
}
