using UnityEngine;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// Interface for all items.
    /// </summary>
    public interface IItem
    {
        string ItemId { get; }
        string DisplayName { get; }
        string Description { get; }
        Sprite Icon { get; }
        ItemType ItemType { get; }
        ItemRarity Rarity { get; }
        bool IsStackable { get; }
        int MaxStackSize { get; }
        float Weight { get; }
        int BuyPrice { get; }
        int SellPrice { get; }
    }

    /// <summary>
    /// Item type categories.
    /// </summary>
    public enum ItemType
    {
        Consumable,     // Potions, food, etc.
        Equipment,      // Weapons, armor
        Material,       // Crafting materials
        Quest,          // Quest items
        Key,            // Keys and special items
        Currency,       // Gold, gems
        Misc            // Miscellaneous
    }

    /// <summary>
    /// Item rarity levels.
    /// </summary>
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythic
    }
}
