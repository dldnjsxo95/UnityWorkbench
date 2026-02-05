using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Data
{
    /// <summary>
    /// ScriptableObject-based runtime set.
    /// Objects can register/unregister themselves at runtime.
    /// Useful for tracking all enemies, collectibles, etc.
    /// </summary>
    public abstract class RuntimeSet<T> : ScriptableObject
    {
        [SerializeField] private List<T> _items = new List<T>();

        public IReadOnlyList<T> Items => _items;
        public int Count => _items.Count;

        public event System.Action<T> OnItemAdded;
        public event System.Action<T> OnItemRemoved;
        public event System.Action OnSetChanged;

        private void OnEnable()
        {
            // Clear on play mode enter
#if UNITY_EDITOR
            _items.Clear();
#endif
        }

        public void Add(T item)
        {
            if (item == null) return;

            if (!_items.Contains(item))
            {
                _items.Add(item);
                OnItemAdded?.Invoke(item);
                OnSetChanged?.Invoke();
            }
        }

        public void Remove(T item)
        {
            if (item == null) return;

            if (_items.Remove(item))
            {
                OnItemRemoved?.Invoke(item);
                OnSetChanged?.Invoke();
            }
        }

        public bool Contains(T item) => _items.Contains(item);

        public void Clear()
        {
            _items.Clear();
            OnSetChanged?.Invoke();
        }

        public T GetRandom()
        {
            if (_items.Count == 0) return default;
            return _items[Random.Range(0, _items.Count)];
        }

        public T GetFirst() => _items.Count > 0 ? _items[0] : default;
        public T GetLast() => _items.Count > 0 ? _items[_items.Count - 1] : default;
    }

    /// <summary>
    /// Set of GameObjects.
    /// </summary>
    [CreateAssetMenu(fileName = "GameObjectSet", menuName = "LWT/Data/Sets/GameObject Set")]
    public class GameObjectSet : RuntimeSet<GameObject>
    {
        /// <summary>
        /// Get closest item to a position.
        /// </summary>
        public GameObject GetClosest(Vector3 position)
        {
            if (Count == 0) return null;

            GameObject closest = null;
            float closestDist = float.MaxValue;

            foreach (var item in Items)
            {
                if (item == null) continue;

                float dist = Vector3.Distance(position, item.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = item;
                }
            }

            return closest;
        }

        /// <summary>
        /// Get all items within radius.
        /// </summary>
        public List<GameObject> GetInRadius(Vector3 center, float radius)
        {
            var result = new List<GameObject>();
            float sqrRadius = radius * radius;

            foreach (var item in Items)
            {
                if (item == null) continue;

                if ((item.transform.position - center).sqrMagnitude <= sqrRadius)
                {
                    result.Add(item);
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Set of Transforms.
    /// </summary>
    [CreateAssetMenu(fileName = "TransformSet", menuName = "LWT/Data/Sets/Transform Set")]
    public class TransformSet : RuntimeSet<Transform>
    {
        public Transform GetClosest(Vector3 position)
        {
            if (Count == 0) return null;

            Transform closest = null;
            float closestDist = float.MaxValue;

            foreach (var item in Items)
            {
                if (item == null) continue;

                float dist = Vector3.Distance(position, item.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = item;
                }
            }

            return closest;
        }
    }
}
