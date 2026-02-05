using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// Object pool specifically for GameObjects.
    /// Handles instantiation, activation, and parenting.
    /// </summary>
    public class GameObjectPool
    {
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly Stack<GameObject> _pool;
        private readonly int _maxSize;
        private readonly bool _autoExpand;

        public int CountInactive => _pool.Count;
        public int CountActive { get; private set; }
        public int CountTotal => CountActive + CountInactive;
        public GameObject Prefab => _prefab;

        public GameObjectPool(
            GameObject prefab,
            Transform parent = null,
            int initialSize = 10,
            int maxSize = 1000,
            bool autoExpand = true)
        {
            _prefab = prefab;
            _parent = parent;
            _maxSize = maxSize;
            _autoExpand = autoExpand;
            _pool = new Stack<GameObject>(initialSize);

            // Pre-warm pool
            for (int i = 0; i < initialSize; i++)
            {
                var obj = CreateInstance();
                obj.SetActive(false);
                _pool.Push(obj);
            }
        }

        private GameObject CreateInstance()
        {
            var obj = Object.Instantiate(_prefab, _parent);
            obj.name = $"{_prefab.name} (Pooled)";
            return obj;
        }

        public GameObject Get()
        {
            return Get(Vector3.zero, Quaternion.identity);
        }

        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            GameObject obj;

            if (_pool.Count > 0)
            {
                obj = _pool.Pop();
            }
            else if (_autoExpand || CountTotal < _maxSize)
            {
                obj = CreateInstance();
            }
            else
            {
                Debug.LogWarning($"[GameObjectPool] Pool exhausted for {_prefab.name}");
                return null;
            }

            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            CountActive++;

            // Call IPoolable.OnSpawn on all components
            var poolables = obj.GetComponentsInChildren<IPoolable>(true);
            foreach (var poolable in poolables)
            {
                poolable.OnSpawn();
            }

            return obj;
        }

        public T Get<T>(Vector3 position, Quaternion rotation) where T : Component
        {
            var obj = Get(position, rotation);
            return obj != null ? obj.GetComponent<T>() : null;
        }

        public void Release(GameObject obj)
        {
            if (obj == null) return;

            // Call IPoolable.OnDespawn on all components
            var poolables = obj.GetComponentsInChildren<IPoolable>(true);
            foreach (var poolable in poolables)
            {
                poolable.OnDespawn();
            }

            obj.SetActive(false);
            obj.transform.SetParent(_parent);
            CountActive--;

            if (_pool.Count < _maxSize)
            {
                _pool.Push(obj);
            }
            else
            {
                Object.Destroy(obj);
            }
        }

        public void Clear()
        {
            while (_pool.Count > 0)
            {
                var obj = _pool.Pop();
                if (obj != null)
                {
                    Object.Destroy(obj);
                }
            }
            CountActive = 0;
        }

        public void Prewarm(int count)
        {
            for (int i = _pool.Count; i < count; i++)
            {
                var obj = CreateInstance();
                obj.SetActive(false);
                _pool.Push(obj);
            }
        }
    }
}
