using System;
using UnityEngine;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// Runtime instance of a skill.
    /// Tracks cooldown and level progression.
    /// </summary>
    [Serializable]
    public class SkillInstance
    {
        [SerializeField] private string _skillId;
        [SerializeField] private int _level = 1;
        [SerializeField] private int _experiencePoints;

        // Runtime state
        [NonSerialized] private float _currentCooldown;
        [NonSerialized] private bool _isActive;
        [NonSerialized] private SkillBase _cachedSkillData;

        /// <summary>
        /// Skill identifier.
        /// </summary>
        public string SkillId
        {
            get => _skillId;
            set
            {
                _skillId = value;
                _cachedSkillData = null;
            }
        }

        /// <summary>
        /// Current skill level.
        /// </summary>
        public int Level
        {
            get => _level;
            set => _level = Mathf.Max(1, value);
        }

        /// <summary>
        /// Experience points for this skill.
        /// </summary>
        public int ExperiencePoints
        {
            get => _experiencePoints;
            set => _experiencePoints = Mathf.Max(0, value);
        }

        /// <summary>
        /// Current cooldown remaining.
        /// </summary>
        public float CurrentCooldown => _currentCooldown;

        /// <summary>
        /// Whether skill is on cooldown.
        /// </summary>
        public bool IsOnCooldown => _currentCooldown > 0;

        /// <summary>
        /// Cooldown progress (0-1, 0 = ready).
        /// </summary>
        public float CooldownProgress
        {
            get
            {
                var skillData = GetSkillData();
                if (skillData == null || skillData.Cooldown <= 0)
                    return 0;
                return _currentCooldown / skillData.Cooldown;
            }
        }

        /// <summary>
        /// Whether toggle skill is active.
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }

        /// <summary>
        /// Get the skill data from database.
        /// </summary>
        public SkillBase GetSkillData()
        {
            if (_cachedSkillData == null && !string.IsNullOrEmpty(_skillId))
            {
                _cachedSkillData = SkillDatabase.Instance?.GetSkill(_skillId);
            }
            return _cachedSkillData;
        }

        public SkillInstance() { }

        public SkillInstance(string skillId, int level = 1)
        {
            _skillId = skillId;
            _level = level;
        }

        /// <summary>
        /// Start the cooldown.
        /// </summary>
        public void StartCooldown()
        {
            var skillData = GetSkillData();
            if (skillData != null)
            {
                _currentCooldown = skillData.Cooldown;
            }
        }

        /// <summary>
        /// Update cooldown (call from Update).
        /// </summary>
        public void UpdateCooldown(float deltaTime)
        {
            if (_currentCooldown > 0)
            {
                _currentCooldown = Mathf.Max(0, _currentCooldown - deltaTime);
            }
        }

        /// <summary>
        /// Reset cooldown to ready.
        /// </summary>
        public void ResetCooldown()
        {
            _currentCooldown = 0;
        }

        /// <summary>
        /// Check if skill can be used.
        /// </summary>
        public SkillResult CanUse(CharacterStats userStats, int userLevel)
        {
            var skillData = GetSkillData();
            if (skillData == null)
                return SkillResult.Failed;

            if (IsOnCooldown)
                return SkillResult.OnCooldown;

            if (userLevel < skillData.RequiredLevel)
                return SkillResult.LevelTooLow;

            if (userStats != null)
            {
                if (skillData.ManaCost > 0 && userStats.Mana < skillData.ManaCost)
                    return SkillResult.InsufficientMana;

                if (skillData.StaminaCost > 0 && userStats.Stamina < skillData.StaminaCost)
                    return SkillResult.InsufficientStamina;
            }

            return SkillResult.Success;
        }

        /// <summary>
        /// Get scaled damage based on level.
        /// </summary>
        public float GetScaledDamage()
        {
            var skillData = GetSkillData();
            if (skillData == null) return 0;

            // 10% damage increase per level
            return skillData.BaseDamage * (1f + (_level - 1) * 0.1f);
        }

        /// <summary>
        /// Get scaled healing based on level.
        /// </summary>
        public float GetScaledHealing()
        {
            var skillData = GetSkillData();
            if (skillData == null) return 0;

            // 10% healing increase per level
            return skillData.BaseHealing * (1f + (_level - 1) * 0.1f);
        }

        /// <summary>
        /// Add experience to this skill.
        /// </summary>
        public bool AddExperience(int amount)
        {
            _experiencePoints += amount;

            // Check for level up (100 XP per level)
            int requiredXP = _level * 100;
            if (_experiencePoints >= requiredXP)
            {
                _experiencePoints -= requiredXP;
                _level++;
                return true; // Leveled up
            }

            return false;
        }

        public override string ToString()
        {
            var skillData = GetSkillData();
            return skillData != null ? $"{skillData.DisplayName} Lv.{_level}" : $"[{_skillId}] Lv.{_level}";
        }
    }
}
