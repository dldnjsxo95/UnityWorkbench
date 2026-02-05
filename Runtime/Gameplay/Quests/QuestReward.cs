using System;
using UnityEngine;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// Reward types.
    /// </summary>
    public enum RewardType
    {
        Experience,
        Currency,
        Item,
        Skill,
        Reputation,
        Custom
    }

    /// <summary>
    /// Definition of a quest reward.
    /// </summary>
    [Serializable]
    public class QuestReward
    {
        [SerializeField] private RewardType _type = RewardType.Experience;
        [SerializeField] private string _rewardId;
        [SerializeField] private int _amount = 1;
        [SerializeField] private bool _isOptional;
        [SerializeField] private Sprite _icon;

        /// <summary>
        /// Type of reward.
        /// </summary>
        public RewardType Type => _type;

        /// <summary>
        /// Reward identifier (item ID, skill ID, etc.)
        /// </summary>
        public string RewardId => _rewardId;

        /// <summary>
        /// Amount of reward.
        /// </summary>
        public int Amount => _amount;

        /// <summary>
        /// Whether this is an optional/choice reward.
        /// </summary>
        public bool IsOptional => _isOptional;

        /// <summary>
        /// Icon to display.
        /// </summary>
        public Sprite Icon => _icon;

        public QuestReward() { }

        public QuestReward(RewardType type, int amount, string rewardId = null)
        {
            _type = type;
            _amount = amount;
            _rewardId = rewardId;
        }

        /// <summary>
        /// Create an experience reward.
        /// </summary>
        public static QuestReward Experience(int amount)
        {
            return new QuestReward(RewardType.Experience, amount);
        }

        /// <summary>
        /// Create a currency reward.
        /// </summary>
        public static QuestReward Currency(int amount, string currencyId = "gold")
        {
            return new QuestReward(RewardType.Currency, amount, currencyId);
        }

        /// <summary>
        /// Create an item reward.
        /// </summary>
        public static QuestReward Item(string itemId, int amount = 1)
        {
            return new QuestReward(RewardType.Item, amount, itemId);
        }

        /// <summary>
        /// Get display text for this reward.
        /// </summary>
        public string GetDisplayText()
        {
            return _type switch
            {
                RewardType.Experience => $"{_amount} XP",
                RewardType.Currency => $"{_amount} {_rewardId ?? "Gold"}",
                RewardType.Item => $"{_rewardId} x{_amount}",
                RewardType.Skill => $"Skill: {_rewardId}",
                RewardType.Reputation => $"{_amount} Rep ({_rewardId})",
                _ => $"{_rewardId} x{_amount}"
            };
        }
    }
}
