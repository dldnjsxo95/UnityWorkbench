using UnityEngine;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// ScriptableObject base class for item definitions.
    /// </summary>
    [CreateAssetMenu(fileName = "Item", menuName = "UnityWorkbench/Gameplay/Item")]
    public class ItemBase : ScriptableObject, IItem
    {
        [Header("Basic Info")]
        [SerializeField] private string _itemId;
        [SerializeField] private string _displayName;
        [SerializeField, TextArea(2, 5)] private string _description;
        [SerializeField] private Sprite _icon;

        [Header("Classification")]
        [SerializeField] private ItemType _itemType = ItemType.Misc;
        [SerializeField] private ItemRarity _rarity = ItemRarity.Common;

        [Header("Stack Settings")]
        [SerializeField] private bool _isStackable = true;
        [SerializeField] private int _maxStackSize = 99;

        [Header("Properties")]
        [SerializeField] private float _weight = 0.1f;
        [SerializeField] private int _buyPrice = 10;
        [SerializeField] private int _sellPrice = 5;

        [Header("Visual")]
        [SerializeField] private GameObject _worldPrefab;
        [SerializeField] private Color _rarityColor = Color.white;

        #region IItem Implementation

        public string ItemId => string.IsNullOrEmpty(_itemId) ? name : _itemId;
        public string DisplayName => string.IsNullOrEmpty(_displayName) ? name : _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public ItemType ItemType => _itemType;
        public ItemRarity Rarity => _rarity;
        public bool IsStackable => _isStackable;
        public int MaxStackSize => _isStackable ? _maxStackSize : 1;
        public float Weight => _weight;
        public int BuyPrice => _buyPrice;
        public int SellPrice => _sellPrice;

        #endregion

        /// <summary>
        /// World prefab for dropped items.
        /// </summary>
        public GameObject WorldPrefab => _worldPrefab;

        /// <summary>
        /// Color associated with rarity.
        /// </summary>
        public Color RarityColor => _rarityColor;

        /// <summary>
        /// Create a runtime instance of this item.
        /// </summary>
        public virtual ItemInstance CreateInstance(int amount = 1)
        {
            return new ItemInstance(ItemId, amount);
        }

        /// <summary>
        /// Called when this item is used.
        /// Override in subclasses for consumable behavior.
        /// </summary>
        public virtual bool Use(GameObject user)
        {
            Debug.Log($"[Item] {DisplayName} used by {user.name}");
            return false;
        }

        /// <summary>
        /// Get tooltip text for this item.
        /// </summary>
        public virtual string GetTooltip()
        {
            return $"<b>{DisplayName}</b>\n" +
                   $"<color=#{ColorUtility.ToHtmlStringRGB(GetRarityColor())}>{Rarity}</color>\n\n" +
                   $"{Description}\n\n" +
                   $"Weight: {Weight:F1}\n" +
                   $"Value: {SellPrice}";
        }

        /// <summary>
        /// Get the standard color for a rarity level.
        /// </summary>
        public static Color GetRarityColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => Color.white,
                ItemRarity.Uncommon => Color.green,
                ItemRarity.Rare => Color.blue,
                ItemRarity.Epic => new Color(0.6f, 0.2f, 0.8f),  // Purple
                ItemRarity.Legendary => new Color(1f, 0.5f, 0f),  // Orange
                ItemRarity.Mythic => Color.red,
                _ => Color.white
            };
        }

        public Color GetRarityColor()
        {
            return _rarityColor != Color.white ? _rarityColor : GetRarityColor(_rarity);
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_itemId))
            {
                _itemId = name;
            }

            if (_maxStackSize < 1)
            {
                _maxStackSize = 1;
            }

            if (!_isStackable)
            {
                _maxStackSize = 1;
            }

            // Auto-set rarity color
            if (_rarityColor == Color.white || _rarityColor == default)
            {
                _rarityColor = GetRarityColor(_rarity);
            }
        }
    }
}
