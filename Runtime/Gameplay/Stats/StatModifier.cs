using System;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// Types of stat modifications.
    /// Calculation order: BaseValue + Flat -> * (1 + PercentAdd sum) -> * PercentMult product
    /// </summary>
    public enum StatModifierType
    {
        /// <summary>
        /// Adds a flat value. Example: +10 Attack
        /// </summary>
        Flat = 100,

        /// <summary>
        /// Adds a percentage (additive). Example: +10% Attack (stacks additively)
        /// </summary>
        PercentAdd = 200,

        /// <summary>
        /// Multiplies by a percentage (multiplicative). Example: *1.1 Attack
        /// </summary>
        PercentMult = 300
    }

    /// <summary>
    /// Represents a modification to a stat value (buff, debuff, equipment bonus, etc.)
    /// </summary>
    [Serializable]
    public class StatModifier
    {
        /// <summary>
        /// Type of modification (Flat, PercentAdd, PercentMult).
        /// </summary>
        public StatModifierType Type;

        /// <summary>
        /// Value of the modification.
        /// For Flat: actual value to add (e.g., 10 for +10)
        /// For Percent: decimal value (e.g., 0.1 for +10%)
        /// </summary>
        public float Value;

        /// <summary>
        /// Order within the same type. Lower values are applied first.
        /// </summary>
        public int Order;

        /// <summary>
        /// Source of this modifier (equipment, buff skill, etc.)
        /// Used for removing all modifiers from a specific source.
        /// </summary>
        public object Source;

        /// <summary>
        /// Duration in seconds. -1 means permanent.
        /// </summary>
        public float Duration = -1f;

        /// <summary>
        /// Unique identifier for this modifier instance.
        /// </summary>
        public string ModifierId;

        /// <summary>
        /// Time remaining for timed modifiers.
        /// </summary>
        public float RemainingTime { get; set; }

        /// <summary>
        /// Whether this modifier has expired.
        /// </summary>
        public bool IsExpired => Duration > 0 && RemainingTime <= 0;

        /// <summary>
        /// Whether this modifier is permanent (no duration).
        /// </summary>
        public bool IsPermanent => Duration < 0;

        public StatModifier() { }

        public StatModifier(float value, StatModifierType type, int order = 0, object source = null)
        {
            Value = value;
            Type = type;
            Order = order;
            Source = source;
            ModifierId = Guid.NewGuid().ToString();
        }

        public StatModifier(float value, StatModifierType type, float duration, object source = null)
            : this(value, type, 0, source)
        {
            Duration = duration;
            RemainingTime = duration;
        }

        /// <summary>
        /// Update the remaining time. Returns true if expired.
        /// </summary>
        public bool UpdateTime(float deltaTime)
        {
            if (IsPermanent) return false;

            RemainingTime -= deltaTime;
            return IsExpired;
        }

        /// <summary>
        /// Reset the duration timer.
        /// </summary>
        public void ResetDuration()
        {
            if (!IsPermanent)
            {
                RemainingTime = Duration;
            }
        }
    }
}
