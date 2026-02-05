using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// Represents a single stat with base value and modifiers.
    /// Automatically recalculates value when modifiers change.
    /// </summary>
    [Serializable]
    public class Stat
    {
        [SerializeField] private StatType _type;
        [SerializeField] private float _baseValue;

        private readonly List<StatModifier> _modifiers = new List<StatModifier>();
        private float _value;
        private bool _isDirty = true;

        /// <summary>
        /// Type of this stat.
        /// </summary>
        public StatType Type => _type;

        /// <summary>
        /// Base value before modifiers.
        /// </summary>
        public float BaseValue
        {
            get => _baseValue;
            set
            {
                if (Math.Abs(_baseValue - value) > 0.0001f)
                {
                    _baseValue = value;
                    _isDirty = true;
                }
            }
        }

        /// <summary>
        /// Final calculated value after all modifiers.
        /// </summary>
        public float Value
        {
            get
            {
                if (_isDirty)
                {
                    RecalculateValue();
                }
                return _value;
            }
        }

        /// <summary>
        /// All active modifiers on this stat.
        /// </summary>
        public IReadOnlyList<StatModifier> Modifiers => _modifiers;

        /// <summary>
        /// Event fired when the stat value changes.
        /// </summary>
        public event Action<Stat, float, float> OnValueChanged;  // stat, oldValue, newValue

        public Stat() { }

        public Stat(StatType type, float baseValue)
        {
            _type = type;
            _baseValue = baseValue;
            _isDirty = true;
        }

        /// <summary>
        /// Add a modifier to this stat.
        /// </summary>
        public void AddModifier(StatModifier modifier)
        {
            if (modifier == null) return;

            _modifiers.Add(modifier);
            _modifiers.Sort((a, b) =>
            {
                int typeCompare = a.Type.CompareTo(b.Type);
                return typeCompare != 0 ? typeCompare : a.Order.CompareTo(b.Order);
            });

            _isDirty = true;
            RecalculateValue();
        }

        /// <summary>
        /// Remove a specific modifier.
        /// </summary>
        public bool RemoveModifier(StatModifier modifier)
        {
            if (_modifiers.Remove(modifier))
            {
                _isDirty = true;
                RecalculateValue();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove modifier by ID.
        /// </summary>
        public bool RemoveModifier(string modifierId)
        {
            var modifier = _modifiers.Find(m => m.ModifierId == modifierId);
            return modifier != null && RemoveModifier(modifier);
        }

        /// <summary>
        /// Remove all modifiers from a specific source.
        /// </summary>
        public int RemoveAllModifiersFromSource(object source)
        {
            int count = _modifiers.RemoveAll(m => m.Source == source);
            if (count > 0)
            {
                _isDirty = true;
                RecalculateValue();
            }
            return count;
        }

        /// <summary>
        /// Remove all modifiers.
        /// </summary>
        public void ClearAllModifiers()
        {
            if (_modifiers.Count > 0)
            {
                _modifiers.Clear();
                _isDirty = true;
                RecalculateValue();
            }
        }

        /// <summary>
        /// Update timed modifiers. Call this each frame.
        /// </summary>
        public void UpdateTimedModifiers(float deltaTime)
        {
            bool anyExpired = false;

            for (int i = _modifiers.Count - 1; i >= 0; i--)
            {
                if (_modifiers[i].UpdateTime(deltaTime))
                {
                    _modifiers.RemoveAt(i);
                    anyExpired = true;
                }
            }

            if (anyExpired)
            {
                _isDirty = true;
                RecalculateValue();
            }
        }

        /// <summary>
        /// Check if a modifier with the given ID exists.
        /// </summary>
        public bool HasModifier(string modifierId)
        {
            return _modifiers.Exists(m => m.ModifierId == modifierId);
        }

        /// <summary>
        /// Get a modifier by ID.
        /// </summary>
        public StatModifier GetModifier(string modifierId)
        {
            return _modifiers.Find(m => m.ModifierId == modifierId);
        }

        private void RecalculateValue()
        {
            float oldValue = _value;
            float finalValue = _baseValue;
            float percentAddSum = 0f;

            // Apply modifiers in order: Flat -> PercentAdd -> PercentMult
            foreach (var modifier in _modifiers)
            {
                switch (modifier.Type)
                {
                    case StatModifierType.Flat:
                        finalValue += modifier.Value;
                        break;

                    case StatModifierType.PercentAdd:
                        percentAddSum += modifier.Value;
                        break;

                    case StatModifierType.PercentMult:
                        // Apply accumulated PercentAdd before PercentMult
                        if (percentAddSum != 0)
                        {
                            finalValue *= 1 + percentAddSum;
                            percentAddSum = 0;
                        }
                        finalValue *= 1 + modifier.Value;
                        break;
                }
            }

            // Apply any remaining PercentAdd
            if (percentAddSum != 0)
            {
                finalValue *= 1 + percentAddSum;
            }

            _value = finalValue;
            _isDirty = false;

            // Notify if value changed
            if (Math.Abs(oldValue - _value) > 0.0001f)
            {
                OnValueChanged?.Invoke(this, oldValue, _value);
            }
        }

        public override string ToString()
        {
            return $"{_type}: {Value:F1} (Base: {_baseValue:F1}, Modifiers: {_modifiers.Count})";
        }
    }
}
