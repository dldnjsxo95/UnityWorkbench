using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// ScriptableObject for level progression configuration.
    /// Defines experience requirements and stat bonuses per level.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "UnityWorkbench/Gameplay/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [Serializable]
        public class LevelData
        {
            public int Level;
            public int RequiredExperience;
            public List<StatBonus> StatBonuses = new List<StatBonus>();
        }

        [Serializable]
        public class StatBonus
        {
            public StatType StatType;
            public float BonusValue;
        }

        [Header("Level Settings")]
        [SerializeField] private int _maxLevel = 100;
        [SerializeField] private int _startingLevel = 1;

        [Header("Experience Curve")]
        [SerializeField] private ExperienceCurveType _curveType = ExperienceCurveType.Polynomial;
        [SerializeField] private float _baseExperience = 100f;
        [SerializeField] private float _experienceMultiplier = 1.5f;
        [SerializeField] private AnimationCurve _customCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Header("Level Data (Optional Override)")]
        [SerializeField] private List<LevelData> _levelDataOverrides = new List<LevelData>();

        [Header("Default Stat Bonuses Per Level")]
        [SerializeField] private List<StatBonus> _defaultStatBonusesPerLevel = new List<StatBonus>
        {
            new StatBonus { StatType = StatType.MaxHealth, BonusValue = 10 },
            new StatBonus { StatType = StatType.Attack, BonusValue = 2 },
            new StatBonus { StatType = StatType.Defense, BonusValue = 1 }
        };

        public int MaxLevel => _maxLevel;
        public int StartingLevel => _startingLevel;

        /// <summary>
        /// Get experience required to reach the next level.
        /// </summary>
        public int GetRequiredExperience(int currentLevel)
        {
            if (currentLevel >= _maxLevel) return int.MaxValue;
            if (currentLevel < 1) return 0;

            // Check for override
            var override_ = _levelDataOverrides.Find(d => d.Level == currentLevel);
            if (override_ != null)
            {
                return override_.RequiredExperience;
            }

            // Calculate based on curve type
            return CalculateExperience(currentLevel);
        }

        /// <summary>
        /// Get total experience required to reach a specific level from level 1.
        /// </summary>
        public int GetTotalExperienceForLevel(int targetLevel)
        {
            int total = 0;
            for (int i = 1; i < targetLevel; i++)
            {
                total += GetRequiredExperience(i);
            }
            return total;
        }

        /// <summary>
        /// Get stat bonuses for leveling up.
        /// </summary>
        public IReadOnlyList<StatBonus> GetStatBonuses(int newLevel)
        {
            // Check for override
            var override_ = _levelDataOverrides.Find(d => d.Level == newLevel);
            if (override_ != null && override_.StatBonuses.Count > 0)
            {
                return override_.StatBonuses;
            }

            return _defaultStatBonusesPerLevel;
        }

        private int CalculateExperience(int level)
        {
            switch (_curveType)
            {
                case ExperienceCurveType.Linear:
                    return Mathf.RoundToInt(_baseExperience * level);

                case ExperienceCurveType.Polynomial:
                    return Mathf.RoundToInt(_baseExperience * Mathf.Pow(level, _experienceMultiplier));

                case ExperienceCurveType.Exponential:
                    return Mathf.RoundToInt(_baseExperience * Mathf.Pow(_experienceMultiplier, level - 1));

                case ExperienceCurveType.Custom:
                    float t = (float)(level - 1) / (_maxLevel - 1);
                    return Mathf.RoundToInt(_baseExperience * _customCurve.Evaluate(t) * _maxLevel);

                default:
                    return Mathf.RoundToInt(_baseExperience * level);
            }
        }
    }

    public enum ExperienceCurveType
    {
        Linear,       // baseExp * level
        Polynomial,   // baseExp * level^multiplier
        Exponential,  // baseExp * multiplier^(level-1)
        Custom        // Use animation curve
    }
}
