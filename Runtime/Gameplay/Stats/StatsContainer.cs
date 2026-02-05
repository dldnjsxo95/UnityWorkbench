using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// Container for managing multiple stats.
    /// Provides convenient access and modification methods.
    /// </summary>
    [Serializable]
    public class StatsContainer
    {
        [SerializeField] private List<Stat> _statsList = new List<Stat>();

        private readonly Dictionary<StatType, Stat> _stats = new Dictionary<StatType, Stat>();
        private bool _initialized;

        /// <summary>
        /// Event fired when any stat value changes.
        /// </summary>
        public event Action<StatType, float, float> OnStatChanged;  // type, oldValue, newValue

        /// <summary>
        /// Initialize the stats dictionary from the serialized list.
        /// </summary>
        public void Initialize()
        {
            if (_initialized) return;

            _stats.Clear();
            foreach (var stat in _statsList)
            {
                if (!_stats.ContainsKey(stat.Type))
                {
                    _stats[stat.Type] = stat;
                    stat.OnValueChanged += OnStatValueChanged;
                }
            }
            _initialized = true;
        }

        /// <summary>
        /// Get a stat by type. Returns null if not found.
        /// </summary>
        public Stat GetStat(StatType type)
        {
            if (!_initialized) Initialize();
            return _stats.TryGetValue(type, out var stat) ? stat : null;
        }

        /// <summary>
        /// Get the current value of a stat.
        /// </summary>
        public float GetValue(StatType type)
        {
            var stat = GetStat(type);
            return stat?.Value ?? 0f;
        }

        /// <summary>
        /// Get the base value of a stat.
        /// </summary>
        public float GetBaseValue(StatType type)
        {
            var stat = GetStat(type);
            return stat?.BaseValue ?? 0f;
        }

        /// <summary>
        /// Set the base value of a stat.
        /// </summary>
        public void SetBaseValue(StatType type, float value)
        {
            var stat = GetStat(type);
            if (stat != null)
            {
                stat.BaseValue = value;
            }
        }

        /// <summary>
        /// Add a modifier to a stat.
        /// </summary>
        public void AddModifier(StatType type, StatModifier modifier)
        {
            var stat = GetStat(type);
            stat?.AddModifier(modifier);
        }

        /// <summary>
        /// Remove a modifier from a stat.
        /// </summary>
        public bool RemoveModifier(StatType type, StatModifier modifier)
        {
            var stat = GetStat(type);
            return stat?.RemoveModifier(modifier) ?? false;
        }

        /// <summary>
        /// Remove a modifier by ID from a stat.
        /// </summary>
        public bool RemoveModifier(StatType type, string modifierId)
        {
            var stat = GetStat(type);
            return stat?.RemoveModifier(modifierId) ?? false;
        }

        /// <summary>
        /// Remove all modifiers from a source across all stats.
        /// </summary>
        public int RemoveAllModifiersFromSource(object source)
        {
            if (!_initialized) Initialize();

            int totalRemoved = 0;
            foreach (var stat in _stats.Values)
            {
                totalRemoved += stat.RemoveAllModifiersFromSource(source);
            }
            return totalRemoved;
        }

        /// <summary>
        /// Add or update a stat.
        /// </summary>
        public void SetStat(StatType type, float baseValue)
        {
            if (!_initialized) Initialize();

            if (_stats.TryGetValue(type, out var existingStat))
            {
                existingStat.BaseValue = baseValue;
            }
            else
            {
                var newStat = new Stat(type, baseValue);
                newStat.OnValueChanged += OnStatValueChanged;
                _stats[type] = newStat;
                _statsList.Add(newStat);
            }
        }

        /// <summary>
        /// Update all timed modifiers.
        /// </summary>
        public void UpdateTimedModifiers(float deltaTime)
        {
            if (!_initialized) Initialize();

            foreach (var stat in _stats.Values)
            {
                stat.UpdateTimedModifiers(deltaTime);
            }
        }

        /// <summary>
        /// Clear all modifiers from all stats.
        /// </summary>
        public void ClearAllModifiers()
        {
            if (!_initialized) Initialize();

            foreach (var stat in _stats.Values)
            {
                stat.ClearAllModifiers();
            }
        }

        /// <summary>
        /// Check if a stat exists.
        /// </summary>
        public bool HasStat(StatType type)
        {
            if (!_initialized) Initialize();
            return _stats.ContainsKey(type);
        }

        /// <summary>
        /// Get all stat types in this container.
        /// </summary>
        public IEnumerable<StatType> GetAllStatTypes()
        {
            if (!_initialized) Initialize();
            return _stats.Keys;
        }

        private void OnStatValueChanged(Stat stat, float oldValue, float newValue)
        {
            OnStatChanged?.Invoke(stat.Type, oldValue, newValue);
        }

        /// <summary>
        /// Create a copy of this container (without modifiers).
        /// </summary>
        public StatsContainer Clone()
        {
            var clone = new StatsContainer();
            foreach (var stat in _statsList)
            {
                clone._statsList.Add(new Stat(stat.Type, stat.BaseValue));
            }
            return clone;
        }
    }
}
