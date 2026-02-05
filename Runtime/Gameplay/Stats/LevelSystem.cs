using System;
using UnityEngine;
using LWT.UnityWorkbench.Core;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// Component for managing character level and experience.
    /// Works with CharacterStats to apply level-up bonuses.
    /// </summary>
    public class LevelSystem : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private LevelConfig _config;
        [SerializeField] private CharacterStats _characterStats;
        [SerializeField] private bool _autoFindCharacterStats = true;

        [Header("Current State")]
        [SerializeField] private int _currentLevel = 1;
        [SerializeField] private int _currentExperience;
        [SerializeField] private int _totalExperience;

        /// <summary>
        /// Current level.
        /// </summary>
        public int Level => _currentLevel;

        /// <summary>
        /// Experience in current level.
        /// </summary>
        public int Experience => _currentExperience;

        /// <summary>
        /// Total experience earned.
        /// </summary>
        public int TotalExperience => _totalExperience;

        /// <summary>
        /// Experience required to reach next level.
        /// </summary>
        public int ExperienceToNextLevel => _config != null
            ? _config.GetRequiredExperience(_currentLevel)
            : 100 * _currentLevel;

        /// <summary>
        /// Progress towards next level (0-1).
        /// </summary>
        public float LevelProgress => ExperienceToNextLevel > 0
            ? (float)_currentExperience / ExperienceToNextLevel
            : 0;

        /// <summary>
        /// Maximum level.
        /// </summary>
        public int MaxLevel => _config != null ? _config.MaxLevel : 100;

        /// <summary>
        /// Whether at max level.
        /// </summary>
        public bool IsMaxLevel => _currentLevel >= MaxLevel;

        #region Events

        public event Action<int> OnLevelUp;           // newLevel
        public event Action<int, int> OnExperienceGained;  // amount, total
        public event Action<int> OnLevelChanged;      // newLevel

        #endregion

        private void Awake()
        {
            if (_autoFindCharacterStats && _characterStats == null)
            {
                _characterStats = GetComponent<CharacterStats>();
            }
        }

        private void Start()
        {
            if (_config != null)
            {
                _currentLevel = Mathf.Max(_config.StartingLevel, _currentLevel);
            }
        }

        /// <summary>
        /// Add experience and check for level up.
        /// </summary>
        public void AddExperience(int amount)
        {
            if (amount <= 0 || IsMaxLevel) return;

            _currentExperience += amount;
            _totalExperience += amount;

            OnExperienceGained?.Invoke(amount, _totalExperience);

            // Check for level ups
            while (_currentExperience >= ExperienceToNextLevel && !IsMaxLevel)
            {
                LevelUp();
            }
        }

        /// <summary>
        /// Set level directly.
        /// </summary>
        public void SetLevel(int level)
        {
            int newLevel = Mathf.Clamp(level, 1, MaxLevel);
            if (newLevel == _currentLevel) return;

            int oldLevel = _currentLevel;
            _currentLevel = newLevel;
            _currentExperience = 0;

            // Apply stat bonuses for each level gained
            if (_characterStats != null && newLevel > oldLevel)
            {
                for (int l = oldLevel + 1; l <= newLevel; l++)
                {
                    ApplyLevelUpBonuses(l);
                }
            }

            OnLevelChanged?.Invoke(_currentLevel);

            if (newLevel > oldLevel)
            {
                EventBus<LevelUpEvent>.Publish(new LevelUpEvent
                {
                    NewLevel = _currentLevel,
                    Character = gameObject
                });
            }
        }

        /// <summary>
        /// Reset to starting level with no experience.
        /// </summary>
        public void Reset()
        {
            _currentLevel = _config != null ? _config.StartingLevel : 1;
            _currentExperience = 0;
            _totalExperience = 0;
            OnLevelChanged?.Invoke(_currentLevel);
        }

        private void LevelUp()
        {
            int expRequired = ExperienceToNextLevel;
            _currentExperience -= expRequired;
            _currentLevel++;

            ApplyLevelUpBonuses(_currentLevel);

            OnLevelUp?.Invoke(_currentLevel);
            OnLevelChanged?.Invoke(_currentLevel);

            EventBus<LevelUpEvent>.Publish(new LevelUpEvent
            {
                NewLevel = _currentLevel,
                Character = gameObject
            });

            Debug.Log($"[LevelSystem] {gameObject.name} leveled up to {_currentLevel}!");
        }

        private void ApplyLevelUpBonuses(int level)
        {
            if (_characterStats == null || _config == null) return;

            var bonuses = _config.GetStatBonuses(level);
            foreach (var bonus in bonuses)
            {
                var stat = _characterStats.Stats.GetStat(bonus.StatType);
                if (stat != null)
                {
                    stat.BaseValue += bonus.BonusValue;
                }
            }

            // Restore health/mana on level up
            _characterStats.FullHeal();
            _characterStats.RestoreMana(_characterStats.MaxMana);
        }

        /// <summary>
        /// Get experience needed to reach a specific level from current.
        /// </summary>
        public int GetExperienceToLevel(int targetLevel)
        {
            if (targetLevel <= _currentLevel) return 0;

            int total = ExperienceToNextLevel - _currentExperience;
            for (int l = _currentLevel + 1; l < targetLevel; l++)
            {
                total += _config.GetRequiredExperience(l);
            }
            return total;
        }
    }

    #region Events

    public struct LevelUpEvent : IEvent
    {
        public int NewLevel;
        public GameObject Character;
    }

    #endregion
}
