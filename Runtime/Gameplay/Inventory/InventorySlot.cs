using System;
using UnityEngine;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// Represents a single slot in an inventory.
    /// </summary>
    [Serializable]
    public class InventorySlot
    {
        [SerializeField] private ItemInstance _item;
        [SerializeField] private bool _isLocked;
        [SerializeField] private ItemType[] _allowedTypes;

        /// <summary>
        /// Item in this slot.
        /// </summary>
        public ItemInstance Item
        {
            get => _item;
            set => _item = value;
        }

        /// <summary>
        /// Whether this slot is empty.
        /// </summary>
        public bool IsEmpty => _item == null || _item.IsEmpty;

        /// <summary>
        /// Whether this slot is locked (cannot be modified).
        /// </summary>
        public bool IsLocked
        {
            get => _isLocked;
            set => _isLocked = value;
        }

        /// <summary>
        /// Allowed item types for this slot. Empty means all types allowed.
        /// </summary>
        public ItemType[] AllowedTypes => _allowedTypes;

        /// <summary>
        /// Event fired when slot contents change.
        /// </summary>
        public event Action<InventorySlot> OnSlotChanged;

        public InventorySlot()
        {
            _item = null;
            _isLocked = false;
            _allowedTypes = Array.Empty<ItemType>();
        }

        public InventorySlot(ItemType[] allowedTypes)
        {
            _item = null;
            _isLocked = false;
            _allowedTypes = allowedTypes ?? Array.Empty<ItemType>();
        }

        /// <summary>
        /// Check if an item type is allowed in this slot.
        /// </summary>
        public bool IsTypeAllowed(ItemType type)
        {
            if (_allowedTypes == null || _allowedTypes.Length == 0)
                return true;

            return Array.IndexOf(_allowedTypes, type) >= 0;
        }

        /// <summary>
        /// Check if an item can be placed in this slot.
        /// </summary>
        public bool CanAccept(ItemInstance item)
        {
            if (_isLocked) return false;
            if (item == null || item.IsEmpty) return true;

            var itemData = item.GetItemData();
            if (itemData == null) return false;

            return IsTypeAllowed(itemData.ItemType);
        }

        /// <summary>
        /// Try to add an item to this slot.
        /// Returns the amount that couldn't be added.
        /// </summary>
        public int TryAddItem(ItemInstance item)
        {
            if (_isLocked || item == null || item.IsEmpty)
                return item?.Amount ?? 0;

            if (!CanAccept(item))
                return item.Amount;

            // If slot is empty, just place the item
            if (IsEmpty)
            {
                _item = item.Clone();
                item.Amount = 0;
                OnSlotChanged?.Invoke(this);
                return 0;
            }

            // Try to stack
            if (_item.CanStackWith(item))
            {
                int remaining = _item.TryMerge(item);
                OnSlotChanged?.Invoke(this);
                return remaining;
            }

            return item.Amount;
        }

        /// <summary>
        /// Set the item directly.
        /// </summary>
        public bool SetItem(ItemInstance item)
        {
            if (_isLocked) return false;

            if (item != null && !CanAccept(item))
                return false;

            _item = item;
            OnSlotChanged?.Invoke(this);
            return true;
        }

        /// <summary>
        /// Take items from this slot.
        /// </summary>
        public ItemInstance TakeItem(int amount = -1)
        {
            if (_isLocked || IsEmpty) return null;

            if (amount < 0 || amount >= _item.Amount)
            {
                // Take all
                var taken = _item;
                _item = null;
                OnSlotChanged?.Invoke(this);
                return taken;
            }

            // Take partial
            var split = _item.Split(amount);
            OnSlotChanged?.Invoke(this);
            return split;
        }

        /// <summary>
        /// Remove a specific amount from this slot.
        /// </summary>
        public bool RemoveAmount(int amount)
        {
            if (_isLocked || IsEmpty || amount <= 0)
                return false;

            if (amount >= _item.Amount)
            {
                Clear();
                return true;
            }

            _item.Amount -= amount;
            OnSlotChanged?.Invoke(this);
            return true;
        }

        /// <summary>
        /// Clear this slot.
        /// </summary>
        public void Clear()
        {
            if (_isLocked) return;

            _item = null;
            OnSlotChanged?.Invoke(this);
        }

        /// <summary>
        /// Swap contents with another slot.
        /// </summary>
        public bool SwapWith(InventorySlot other)
        {
            if (_isLocked || other._isLocked) return false;

            // Check type restrictions
            if (_item != null && !other.CanAccept(_item)) return false;
            if (other._item != null && !CanAccept(other._item)) return false;

            var temp = _item;
            _item = other._item;
            other._item = temp;

            OnSlotChanged?.Invoke(this);
            other.OnSlotChanged?.Invoke(other);

            return true;
        }

        public override string ToString()
        {
            return IsEmpty ? "[Empty]" : $"[{_item}]";
        }
    }
}
