namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// Defines all available stat types for characters.
    /// </summary>
    public enum StatType
    {
        // Vitals
        Health,
        MaxHealth,
        Mana,
        MaxMana,
        Stamina,
        MaxStamina,

        // Offensive
        Attack,
        MagicAttack,
        CriticalRate,
        CriticalDamage,

        // Defensive
        Defense,
        MagicDefense,
        Evasion,
        BlockRate,

        // Speed
        MoveSpeed,
        AttackSpeed,

        // Utility
        Luck,
        HealthRegen,
        ManaRegen,

        // Progression (typically not modified by equipment)
        Experience,
        Level
    }
}
