using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// Runtime instance of an item in inventory.
    /// Contains item ID, amount, and optional custom data.
    /// </summary>
    [Serializable]
    public class ItemInstance
    {
        [SerializeField] private string _itemId;
        [SerializeField] private int _amount;
        [SerializeField] private List<CustomDataEntry> _customDataList = new List<CustomDataEntry>();

        // Transient cache
        [NonSerialized] private ItemBase _cachedItemData;
        [NonSerialized] private Dictionary<string, object> _customDataCache;

        /// <summary>
        /// Item identifier.
        /// </summary>
        public string ItemId
        {
            get => _itemId;
            set
            {
                _itemId = value;
                _cachedItemData = null;  // Invalidate cache
            }
        }

        /// <summary>
        /// Stack amount.
        /// </summary>
        public int Amount
        {
            get => _amount;
            set => _amount = Mathf.Max(0, value);
        }

        /// <summary>
        /// Whether this instance is empty (no items).
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(_itemId) || _amount <= 0;

        /// <summary>
        /// Get the item data from database.
        /// </summary>
        public ItemBase GetItemData()
        {
            if (_cachedItemData == null && !string.IsNullOrEmpty(_itemId))
            {
                _cachedItemData = ItemDatabase.Instance?.GetItem(_itemId);
            }
            return _cachedItemData;
        }

        public ItemInstance() { }

        public ItemInstance(string itemId, int amount = 1)
        {
            _itemId = itemId;
            _amount = amount;
        }

        /// <summary>
        /// Check if this item can stack with another.
        /// </summary>
        public bool CanStackWith(ItemInstance other)
        {
            if (other == null || other.IsEmpty) return false;
            if (_itemId != other._itemId) return false;

            var itemData = GetItemData();
            return itemData != null && itemData.IsStackable;
        }

        /// <summary>
        /// Try to merge another item into this one.
        /// Returns the amount that couldn't be merged.
        /// </summary>
        public int TryMerge(ItemInstance other)
        {
            if (!CanStackWith(other)) return other.Amount;

            var itemData = GetItemData();
            int maxStack = itemData?.MaxStackSize ?? 99;
            int canAdd = maxStack - _amount;

            if (canAdd <= 0) return other.Amount;

            int toAdd = Mathf.Min(canAdd, other.Amount);
            _amount += toAdd;
            other.Amount -= toAdd;

            return other.Amount;
        }

        /// <summary>
        /// Split this stack into two.
        /// </summary>
        public ItemInstance Split(int amount)
        {
            if (amount <= 0 || amount >= _amount) return null;

            var split = new ItemInstance(_itemId, amount);
            _amount -= amount;

            // Copy custom data
            foreach (var entry in _customDataList)
            {
                split._customDataList.Add(new CustomDataEntry
                {
                    Key = entry.Key,
                    Value = entry.Value
                });
            }

            return split;
        }

        /// <summary>
        /// Create a copy of this instance.
        /// </summary>
        public ItemInstance Clone()
        {
            var clone = new ItemInstance(_itemId, _amount);
            foreach (var entry in _customDataList)
            {
                clone._customDataList.Add(new CustomDataEntry
                {
                    Key = entry.Key,
                    Value = entry.Value
                });
            }
            return clone;
        }

        #region Custom Data

        /// <summary>
        /// Get custom data value.
        /// </summary>
        public T GetCustomData<T>(string key, T defaultValue = default)
        {
            var entry = _customDataList.Find(e => e.Key == key);
            if (entry != null)
            {
                try
                {
                    return (T)Convert.ChangeType(entry.Value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Set custom data value.
        /// </summary>
        public void SetCustomData(string key, object value)
        {
            var entry = _customDataList.Find(e => e.Key == key);
            if (entry != null)
            {
                entry.Value = value?.ToString() ?? "";
            }
            else
            {
                _customDataList.Add(new CustomDataEntry
                {
                    Key = key,
                    Value = value?.ToString() ?? ""
                });
            }
        }

        /// <summary>
        /// Remove custom data.
        /// </summary>
        public bool RemoveCustomData(string key)
        {
            return _customDataList.RemoveAll(e => e.Key == key) > 0;
        }

        /// <summary>
        /// Check if custom data exists.
        /// </summary>
        public bool HasCustomData(string key)
        {
            return _customDataList.Exists(e => e.Key == key);
        }

        #endregion

        public override string ToString()
        {
            return $"{_itemId} x{_amount}";
        }

        [Serializable]
        private class CustomDataEntry
        {
            public string Key;
            public string Value;
        }
    }
}
