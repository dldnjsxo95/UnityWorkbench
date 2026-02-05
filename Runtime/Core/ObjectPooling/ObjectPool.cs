using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// Generic object pool for any type.
    /// For GameObjects, use GameObjectPool instead.
    /// </summary>
    public class ObjectPool<T> where T : class, new()
    {
        private readonly Stack<T> _pool;
        private readonly Func<T> _createFunc;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly int _maxSize;

        public int CountInactive => _pool.Count;
        public int CountActive { get; private set; }
        public int CountTotal => CountActive + CountInactive;

        public ObjectPool(
            Func<T> createFunc = null,
            Action<T> onGet = null,
            Action<T> onRelease = null,
            int initialSize = 0,
            int maxSize = 10000)
        {
            _pool = new Stack<T>(initialSize);
            _createFunc = createFunc ?? (() => new T());
            _onGet = onGet;
            _onRelease = onRelease;
            _maxSize = maxSize;

            // Pre-warm pool
            for (int i = 0; i < initialSize; i++)
            {
                _pool.Push(_createFunc());
            }
        }

        public T Get()
        {
            T item;
            if (_pool.Count > 0)
            {
                item = _pool.Pop();
            }
            else
            {
                item = _createFunc();
            }

            CountActive++;
            _onGet?.Invoke(item);

            if (item is IPoolable poolable)
            {
                poolable.OnSpawn();
            }

            return item;
        }

        public void Release(T item)
        {
            if (item == null) return;

            if (item is IPoolable poolable)
            {
                poolable.OnDespawn();
            }

            _onRelease?.Invoke(item);
            CountActive--;

            if (_pool.Count < _maxSize)
            {
                _pool.Push(item);
            }
        }

        public void Clear()
        {
            _pool.Clear();
            CountActive = 0;
        }
    }
}
