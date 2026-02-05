using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LWT.UnityWorkbench.Core;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// Component for managing an inventory.
    /// </summary>
    public class Inventory : MonoBehaviour, IInventory
    {
        [Header("Configuration")]
        [SerializeField] private int _capacity = 20;
        [SerializeField] private bool _autoSort = false;
        [SerializeField] private InventorySortMode _defaultSortMode = InventorySortMode.ByType;

        [Header("Slots")]
        [SerializeField] private List<InventorySlot> _slots = new List<InventorySlot>();

        public int Capacity => _capacity;
        public int UsedSlots => _slots.Count(s => !s.IsEmpty);
        public int FreeSlots => _capacity - UsedSlots;
        public bool IsFull => FreeSlots <= 0;

        public event Action<IInventory> OnInventoryChanged;

        private void Awake()
        {
            InitializeSlots();
        }

        private void InitializeSlots()
        {
            // Ensure we have the right number of slots
            while (_slots.Count < _capacity)
            {
                var slot = new InventorySlot();
                slot.OnSlotChanged += OnSlotChanged;
                _slots.Add(slot);
            }

            // Remove excess slots
            while (_slots.Count > _capacity)
            {
                var lastSlot = _slots[_slots.Count - 1];
                lastSlot.OnSlotChanged -= OnSlotChanged;
                _slots.RemoveAt(_slots.Count - 1);
            }
        }

        private void OnSlotChanged(InventorySlot slot)
        {
            OnInventoryChanged?.Invoke(this);
        }

        public InventorySlot GetSlot(int index)
        {
            if (index < 0 || index >= _slots.Count)
                return null;
            return _slots[index];
        }

        public int AddItem(ItemInstance item)
        {
            if (item == null || item.IsEmpty)
                return 0;

            int remaining = item.Amount;

            // First try to stack with existing items
            var itemData = item.GetItemData();
            if (itemData != null && itemData.IsStackable)
            {
                foreach (var slot in _slots)
                {
                    if (!slot.IsEmpty && slot.Item.CanStackWith(item))
                    {
                        remaining = slot.TryAddItem(new ItemInstance(item.ItemId, remaining));
                        if (remaining <= 0) break;
                    }
                }
            }

            // Then try empty slots
            while (remaining > 0)
            {
                var emptySlot = FindEmptySlot();
                if (emptySlot == null) break;

                var newItem = new ItemInstance(item.ItemId, remaining);
                remaining = emptySlot.TryAddItem(newItem);
            }

            if (_autoSort && remaining < item.Amount)
            {
                Sort(_defaultSortMode);
            }

            // Publish event
            if (remaining < item.Amount)
            {
                EventBus<InventoryChangedEvent>.Publish(new InventoryChangedEvent
                {
                    Inventory = this,
                    ItemId = item.ItemId,
                    AmountChanged = item.Amount - remaining,
                    ChangeType = InventoryChangeType.Added
                });
            }

            return remaining;
        }

        public int AddItem(string itemId, int amount = 1)
        {
            return AddItem(new ItemInstance(itemId, amount));
        }

        public bool RemoveItem(string itemId, int amount = 1)
        {
            if (!HasItem(itemId, amount))
                return false;

            int toRemove = amount;

            foreach (var slot in _slots)
            {
                if (slot.IsEmpty || slot.Item.ItemId != itemId)
                    continue;

                int removeFromSlot = Mathf.Min(toRemove, slot.Item.Amount);
                slot.RemoveAmount(removeFromSlot);
                toRemove -= removeFromSlot;

                if (toRemove <= 0) break;
            }

            EventBus<InventoryChangedEvent>.Publish(new InventoryChangedEvent
            {
                Inventory = this,
                ItemId = itemId,
                AmountChanged = amount,
                ChangeType = InventoryChangeType.Removed
            });

            return true;
        }

        public bool HasItem(string itemId, int amount = 1)
        {
            return GetItemCount(itemId) >= amount;
        }

        public int GetItemCount(string itemId)
        {
            return _slots
                .Where(s => !s.IsEmpty && s.Item.ItemId == itemId)
                .Sum(s => s.Item.Amount);
        }

        public IEnumerable<InventorySlot> FindSlots(string itemId)
        {
            return _slots.Where(s => !s.IsEmpty && s.Item.ItemId == itemId);
        }

        public InventorySlot FindEmptySlot()
        {
            return _slots.FirstOrDefault(s => s.IsEmpty && !s.IsLocked);
        }

        public void Clear()
        {
            foreach (var slot in _slots)
            {
                slot.Clear();
            }

            EventBus<InventoryChangedEvent>.Publish(new InventoryChangedEvent
            {
                Inventory = this,
                ItemId = null,
                AmountChanged = 0,
                ChangeType = InventoryChangeType.Cleared
            });
        }

        public void Sort(InventorySortMode mode)
        {
            // Collect all items
            var items = new List<ItemInstance>();
            foreach (var slot in _slots)
            {
                if (!slot.IsEmpty && !slot.IsLocked)
                {
                    items.Add(slot.Item);
                    slot.SetItem(null);
                }
            }

            // Sort items
            items = SortItems(items, mode).ToList();

            // Re-add items
            int slotIndex = 0;
            foreach (var item in items)
            {
                while (slotIndex < _slots.Count && _slots[slotIndex].IsLocked)
                {
                    slotIndex++;
                }

                if (slotIndex >= _slots.Count) break;
                _slots[slotIndex].SetItem(item);
                slotIndex++;
            }

            OnInventoryChanged?.Invoke(this);
        }

        private IEnumerable<ItemInstance> SortItems(List<ItemInstance> items, InventorySortMode mode)
        {
            return mode switch
            {
                InventorySortMode.ByName => items.OrderBy(i => i.GetItemData()?.DisplayName ?? i.ItemId),
                InventorySortMode.ByType => items.OrderBy(i => i.GetItemData()?.ItemType ?? ItemType.Misc)
                                                  .ThenBy(i => i.GetItemData()?.DisplayName ?? i.ItemId),
                InventorySortMode.ByRarity => items.OrderByDescending(i => i.GetItemData()?.Rarity ?? ItemRarity.Common)
                                                    .ThenBy(i => i.GetItemData()?.DisplayName ?? i.ItemId),
                InventorySortMode.ByAmount => items.OrderByDescending(i => i.Amount)
                                                   .ThenBy(i => i.GetItemData()?.DisplayName ?? i.ItemId),
                InventorySortMode.ByValue => items.OrderByDescending(i => i.GetItemData()?.SellPrice ?? 0)
                                                   .ThenBy(i => i.GetItemData()?.DisplayName ?? i.ItemId),
                _ => items
            };
        }

        /// <summary>
        /// Swap items between two slots.
        /// </summary>
        public bool SwapSlots(int indexA, int indexB)
        {
            var slotA = GetSlot(indexA);
            var slotB = GetSlot(indexB);

            if (slotA == null || slotB == null)
                return false;

            return slotA.SwapWith(slotB);
        }

        /// <summary>
        /// Move item from one slot to another.
        /// </summary>
        public bool MoveItem(int fromIndex, int toIndex)
        {
            var fromSlot = GetSlot(fromIndex);
            var toSlot = GetSlot(toIndex);

            if (fromSlot == null || toSlot == null || fromSlot.IsEmpty)
                return false;

            // If target is empty, just move
            if (toSlot.IsEmpty)
            {
                return toSlot.SetItem(fromSlot.TakeItem());
            }

            // Try to stack
            if (toSlot.Item.CanStackWith(fromSlot.Item))
            {
                int remaining = toSlot.TryAddItem(fromSlot.Item);
                if (remaining <= 0)
                {
                    fromSlot.Clear();
                }
                return true;
            }

            // Swap
            return fromSlot.SwapWith(toSlot);
        }

        /// <summary>
        /// Use an item from inventory.
        /// </summary>
        public bool UseItem(int slotIndex, GameObject user = null)
        {
            var slot = GetSlot(slotIndex);
            if (slot == null || slot.IsEmpty)
                return false;

            var itemData = slot.Item.GetItemData();
            if (itemData == null)
                return false;

            if (itemData.Use(user ?? gameObject))
            {
                slot.RemoveAmount(1);

                EventBus<ItemUsedEvent>.Publish(new ItemUsedEvent
                {
                    ItemId = slot.Item.ItemId,
                    User = user ?? gameObject
                });

                return true;
            }

            return false;
        }

        /// <summary>
        /// Get all items as a list.
        /// </summary>
        public List<ItemInstance> GetAllItems()
        {
            return _slots
                .Where(s => !s.IsEmpty)
                .Select(s => s.Item)
                .ToList();
        }

        /// <summary>
        /// Calculate total weight.
        /// </summary>
        public float GetTotalWeight()
        {
            return _slots
                .Where(s => !s.IsEmpty)
                .Sum(s => (s.Item.GetItemData()?.Weight ?? 0) * s.Item.Amount);
        }

        /// <summary>
        /// Calculate total value.
        /// </summary>
        public int GetTotalValue()
        {
            return _slots
                .Where(s => !s.IsEmpty)
                .Sum(s => (s.Item.GetItemData()?.SellPrice ?? 0) * s.Item.Amount);
        }

        /// <summary>
        /// Set inventory capacity.
        /// </summary>
        public void SetCapacity(int newCapacity)
        {
            _capacity = Mathf.Max(1, newCapacity);
            InitializeSlots();
        }

        /// <summary>
        /// Lock or unlock a slot.
        /// </summary>
        public void SetSlotLocked(int index, bool locked)
        {
            var slot = GetSlot(index);
            if (slot != null)
            {
                slot.IsLocked = locked;
            }
        }
    }

    #region Events

    public enum InventoryChangeType
    {
        Added,
        Removed,
        Moved,
        Cleared
    }

    public struct InventoryChangedEvent : IEvent
    {
        public Inventory Inventory;
        public string ItemId;
        public int AmountChanged;
        public InventoryChangeType ChangeType;
    }

    public struct ItemUsedEvent : IEvent
    {
        public string ItemId;
        public GameObject User;
    }

    #endregion
}
