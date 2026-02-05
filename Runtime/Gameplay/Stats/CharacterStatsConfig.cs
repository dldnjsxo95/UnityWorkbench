using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// ScriptableObject for defining base stats of a character type.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterStatsConfig", menuName = "UnityWorkbench/Gameplay/Character Stats Config")]
    public class CharacterStatsConfig : ScriptableObject
    {
        [Serializable]
        public class StatEntry
        {
            public StatType Type;
            public float BaseValue;
        }

        [Header("Base Stats")]
        [SerializeField] private List<StatEntry> _baseStats = new List<StatEntry>
        {
            new StatEntry { Type = StatType.MaxHealth, BaseValue = 100 },
            new StatEntry { Type = StatType.MaxMana, BaseValue = 50 },
            new StatEntry { Type = StatType.Attack, BaseValue = 10 },
            new StatEntry { Type = StatType.Defense, BaseValue = 5 },
            new StatEntry { Type = StatType.MoveSpeed, BaseValue = 5 }
        };

        [Header("Stat Limits")]
        [SerializeField] private float _minHealth = 0f;
        [SerializeField] private float _minMana = 0f;

        /// <summary>
        /// Get base stats as a list.
        /// </summary>
        public IReadOnlyList<StatEntry> BaseStats => _baseStats;

        /// <summary>
        /// Minimum health value.
        /// </summary>
        public float MinHealth => _minHealth;

        /// <summary>
        /// Minimum mana value.
        /// </summary>
        public float MinMana => _minMana;

        /// <summary>
        /// Create a StatsContainer from this config.
        /// </summary>
        public StatsContainer CreateStatsContainer()
        {
            var container = new StatsContainer();
            foreach (var entry in _baseStats)
            {
                container.SetStat(entry.Type, entry.BaseValue);
            }
            container.Initialize();
            return container;
        }

        /// <summary>
        /// Get base value for a specific stat type.
        /// </summary>
        public float GetBaseValue(StatType type)
        {
            var entry = _baseStats.Find(e => e.Type == type);
            return entry?.BaseValue ?? 0f;
        }

        private void OnValidate()
        {
            // Ensure essential stats exist
            EnsureStatExists(StatType.MaxHealth, 100);
            EnsureStatExists(StatType.Health, 100);
        }

        private void EnsureStatExists(StatType type, float defaultValue)
        {
            if (!_baseStats.Exists(e => e.Type == type))
            {
                _baseStats.Add(new StatEntry { Type = type, BaseValue = defaultValue });
            }
        }
    }
}
