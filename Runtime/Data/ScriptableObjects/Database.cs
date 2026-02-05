using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Data
{
    /// <summary>
    /// Base class for ScriptableObject databases.
    /// Stores and manages collections of DataContainers.
    /// </summary>
    public abstract class Database<T> : ScriptableObject where T : DataContainer
    {
        [SerializeField] protected List<T> _items = new List<T>();

        private Dictionary<string, T> _lookup;

        public IReadOnlyList<T> Items => _items;
        public int Count => _items.Count;

        private void OnEnable()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            _lookup = new Dictionary<string, T>();
            foreach (var item in _items)
            {
                if (item != null && !string.IsNullOrEmpty(item.Id))
                {
                    _lookup[item.Id] = item;
                }
            }
        }

        /// <summary>
        /// Get item by ID.
        /// </summary>
        public T GetById(string id)
        {
            if (_lookup == null) BuildLookup();

            if (_lookup.TryGetValue(id, out var item))
            {
                return item;
            }

            Debug.LogWarning($"[Database] Item not found: {id}");
            return null;
        }

        /// <summary>
        /// Try get item by ID.
        /// </summary>
        public bool TryGetById(string id, out T item)
        {
            if (_lookup == null) BuildLookup();
            return _lookup.TryGetValue(id, out item);
        }

        /// <summary>
        /// Check if ID exists.
        /// </summary>
        public bool Contains(string id)
        {
            if (_lookup == null) BuildLookup();
            return _lookup.ContainsKey(id);
        }

        /// <summary>
        /// Get random item.
        /// </summary>
        public T GetRandom()
        {
            if (_items.Count == 0) return null;
            return _items[Random.Range(0, _items.Count)];
        }

        /// <summary>
        /// Get all IDs.
        /// </summary>
        public IEnumerable<string> GetAllIds()
        {
            if (_lookup == null) BuildLookup();
            return _lookup.Keys;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Rebuild lookup (Editor only).
        /// </summary>
        [ContextMenu("Rebuild Lookup")]
        private void EditorRebuildLookup()
        {
            BuildLookup();
            Debug.Log($"[Database] Rebuilt lookup: {_lookup.Count} items");
        }

        /// <summary>
        /// Validate for duplicates (Editor only).
        /// </summary>
        [ContextMenu("Check for Duplicates")]
        private void EditorCheckDuplicates()
        {
            var ids = new HashSet<string>();
            foreach (var item in _items)
            {
                if (item == null) continue;

                if (ids.Contains(item.Id))
                {
                    Debug.LogWarning($"[Database] Duplicate ID: {item.Id}");
                }
                else
                {
                    ids.Add(item.Id);
                }
            }
            Debug.Log($"[Database] Check complete. {ids.Count} unique IDs.");
        }
#endif
    }
}
