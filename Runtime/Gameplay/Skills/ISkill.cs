using UnityEngine;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// Interface for all skills.
    /// </summary>
    public interface ISkill
    {
        string SkillId { get; }
        string DisplayName { get; }
        string Description { get; }
        Sprite Icon { get; }
        SkillType SkillType { get; }
        TargetType TargetType { get; }
        float Cooldown { get; }
        float CastTime { get; }
        float Range { get; }
        int ManaCost { get; }
        int StaminaCost { get; }
        int RequiredLevel { get; }
    }

    /// <summary>
    /// Skill type categories.
    /// </summary>
    public enum SkillType
    {
        Active,         // Manually activated
        Passive,        // Always active
        Toggle,         // On/off state
        Channeled,      // Continuous effect while held
        Ultimate        // Special powerful skill
    }

    /// <summary>
    /// Target types for skills.
    /// </summary>
    public enum TargetType
    {
        Self,           // Targets self only
        SingleEnemy,    // Single enemy target
        SingleAlly,     // Single ally target
        AllEnemies,     // All enemies in range
        AllAllies,      // All allies in range
        Area,           // Area of effect
        Direction,      // Directional skill
        None            // No target (instant)
    }

    /// <summary>
    /// Result of skill execution.
    /// </summary>
    public enum SkillResult
    {
        Success,
        Failed,
        OnCooldown,
        InsufficientMana,
        InsufficientStamina,
        InvalidTarget,
        OutOfRange,
        Interrupted,
        LevelTooLow
    }

    /// <summary>
    /// Stat modifier applied by skills.
    /// </summary>
    [System.Serializable]
    public class SkillStatModifier
    {
        public StatType StatType;
        public StatModifierType ModifierType;
        public float Value;
    }
}
