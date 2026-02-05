using System;
using System.Collections.Generic;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// Interface for inventory systems.
    /// </summary>
    public interface IInventory
    {
        /// <summary>
        /// Total number of slots.
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// Number of slots with items.
        /// </summary>
        int UsedSlots { get; }

        /// <summary>
        /// Number of empty slots.
        /// </summary>
        int FreeSlots { get; }

        /// <summary>
        /// Whether the inventory is full.
        /// </summary>
        bool IsFull { get; }

        /// <summary>
        /// Get slot at index.
        /// </summary>
        InventorySlot GetSlot(int index);

        /// <summary>
        /// Try to add an item to the inventory.
        /// Returns the amount that couldn't be added.
        /// </summary>
        int AddItem(ItemInstance item);

        /// <summary>
        /// Try to add an item by ID.
        /// Returns the amount that couldn't be added.
        /// </summary>
        int AddItem(string itemId, int amount = 1);

        /// <summary>
        /// Remove a specific amount of an item.
        /// Returns true if successful.
        /// </summary>
        bool RemoveItem(string itemId, int amount = 1);

        /// <summary>
        /// Check if the inventory contains an item.
        /// </summary>
        bool HasItem(string itemId, int amount = 1);

        /// <summary>
        /// Get total count of an item.
        /// </summary>
        int GetItemCount(string itemId);

        /// <summary>
        /// Find all slots containing an item.
        /// </summary>
        IEnumerable<InventorySlot> FindSlots(string itemId);

        /// <summary>
        /// Find first empty slot.
        /// </summary>
        InventorySlot FindEmptySlot();

        /// <summary>
        /// Clear all items from inventory.
        /// </summary>
        void Clear();

        /// <summary>
        /// Sort inventory by specified criteria.
        /// </summary>
        void Sort(InventorySortMode mode);

        /// <summary>
        /// Event fired when inventory changes.
        /// </summary>
        event Action<IInventory> OnInventoryChanged;
    }

    /// <summary>
    /// Sorting modes for inventory.
    /// </summary>
    public enum InventorySortMode
    {
        ByName,
        ByType,
        ByRarity,
        ByAmount,
        ByValue
    }
}
