using System.Collections.Generic;
using UnityEngine;
using LWT.UnityWorkbench.Core;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// Database for storing and retrieving item definitions.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "UnityWorkbench/Gameplay/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [SerializeField] private List<ItemBase> _items = new List<ItemBase>();

        // Runtime lookup
        private Dictionary<string, ItemBase> _itemLookup;
        private bool _isInitialized;

        private static ItemDatabase _instance;

        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static ItemDatabase Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<ItemDatabase>("ItemDatabase");
                    if (_instance != null)
                    {
                        _instance.Initialize();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// All items in database.
        /// </summary>
        public IReadOnlyList<ItemBase> Items => _items;

        /// <summary>
        /// Number of items.
        /// </summary>
        public int Count => _items.Count;

        private void OnEnable()
        {
            _isInitialized = false;
        }

        /// <summary>
        /// Initialize the lookup dictionary.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            _itemLookup = new Dictionary<string, ItemBase>();

            foreach (var item in _items)
            {
                if (item == null) continue;

                if (_itemLookup.ContainsKey(item.ItemId))
                {
                    Debug.LogWarning($"[ItemDatabase] Duplicate item ID: {item.ItemId}");
                    continue;
                }

                _itemLookup[item.ItemId] = item;
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Get an item by ID.
        /// </summary>
        public ItemBase GetItem(string itemId)
        {
            if (!_isInitialized) Initialize();

            if (string.IsNullOrEmpty(itemId))
                return null;

            return _itemLookup.TryGetValue(itemId, out var item) ? item : null;
        }

        /// <summary>
        /// Check if an item exists.
        /// </summary>
        public bool HasItem(string itemId)
        {
            if (!_isInitialized) Initialize();
            return !string.IsNullOrEmpty(itemId) && _itemLookup.ContainsKey(itemId);
        }

        /// <summary>
        /// Get items by type.
        /// </summary>
        public List<ItemBase> GetItemsByType(ItemType type)
        {
            if (!_isInitialized) Initialize();

            var result = new List<ItemBase>();
            foreach (var item in _items)
            {
                if (item != null && item.ItemType == type)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        /// <summary>
        /// Get items by rarity.
        /// </summary>
        public List<ItemBase> GetItemsByRarity(ItemRarity rarity)
        {
            if (!_isInitialized) Initialize();

            var result = new List<ItemBase>();
            foreach (var item in _items)
            {
                if (item != null && item.Rarity == rarity)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        /// <summary>
        /// Search items by name.
        /// </summary>
        public List<ItemBase> SearchItems(string searchTerm)
        {
            if (!_isInitialized) Initialize();

            if (string.IsNullOrEmpty(searchTerm))
                return new List<ItemBase>(_items);

            searchTerm = searchTerm.ToLowerInvariant();
            var result = new List<ItemBase>();

            foreach (var item in _items)
            {
                if (item == null) continue;

                if (item.DisplayName.ToLowerInvariant().Contains(searchTerm) ||
                    item.ItemId.ToLowerInvariant().Contains(searchTerm))
                {
                    result.Add(item);
                }
            }

            return result;
        }

        /// <summary>
        /// Create an item instance.
        /// </summary>
        public ItemInstance CreateInstance(string itemId, int amount = 1)
        {
            var itemData = GetItem(itemId);
            if (itemData == null)
            {
                Debug.LogWarning($"[ItemDatabase] Item not found: {itemId}");
                return null;
            }

            return itemData.CreateInstance(amount);
        }

        #region Editor Methods

#if UNITY_EDITOR
        /// <summary>
        /// Add an item to the database (Editor only).
        /// </summary>
        public void AddItem(ItemBase item)
        {
            if (item == null || _items.Contains(item)) return;
            _items.Add(item);
            _isInitialized = false;
        }

        /// <summary>
        /// Remove an item from the database (Editor only).
        /// </summary>
        public void RemoveItem(ItemBase item)
        {
            _items.Remove(item);
            _isInitialized = false;
        }

        /// <summary>
        /// Clear all items (Editor only).
        /// </summary>
        public void ClearAll()
        {
            _items.Clear();
            _itemLookup?.Clear();
            _isInitialized = false;
        }

        /// <summary>
        /// Refresh database from project items (Editor only).
        /// </summary>
        public void RefreshFromAssets()
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("t:ItemBase");
            _items.Clear();

            foreach (var guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var item = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemBase>(path);
                if (item != null)
                {
                    _items.Add(item);
                }
            }

            _isInitialized = false;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        #endregion

        /// <summary>
        /// Set as the singleton instance.
        /// </summary>
        public void SetAsInstance()
        {
            _instance = this;
            Initialize();
        }
    }
}
