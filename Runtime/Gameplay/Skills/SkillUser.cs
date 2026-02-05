using System;
using System.Collections.Generic;
using UnityEngine;
using LWT.UnityWorkbench.Core;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// Component for characters that can use skills.
    /// </summary>
    public class SkillUser : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private CharacterStats _characterStats;
        [SerializeField] private LevelSystem _levelSystem;
        [SerializeField] private bool _autoFindComponents = true;
        [SerializeField] private int _maxSkillSlots = 8;

        [Header("Skills")]
        [SerializeField] private List<SkillInstance> _skills = new List<SkillInstance>();

        // Runtime state
        private bool _isCasting;
        private float _castTimer;
        private SkillInstance _castingSkill;
        private GameObject _castTarget;

        /// <summary>
        /// All learned skills.
        /// </summary>
        public IReadOnlyList<SkillInstance> Skills => _skills;

        /// <summary>
        /// Maximum skill slots.
        /// </summary>
        public int MaxSkillSlots => _maxSkillSlots;

        /// <summary>
        /// Number of learned skills.
        /// </summary>
        public int SkillCount => _skills.Count;

        /// <summary>
        /// Whether currently casting a skill.
        /// </summary>
        public bool IsCasting => _isCasting;

        /// <summary>
        /// Current cast progress (0-1).
        /// </summary>
        public float CastProgress
        {
            get
            {
                if (!_isCasting || _castingSkill == null) return 0;
                var skillData = _castingSkill.GetSkillData();
                if (skillData == null || skillData.CastTime <= 0) return 1;
                return 1f - (_castTimer / skillData.CastTime);
            }
        }

        #region Events

        public event Action<SkillInstance, GameObject> OnSkillUsed;
        public event Action<SkillInstance> OnSkillStartCast;
        public event Action<SkillInstance> OnSkillInterrupted;
        public event Action<SkillInstance> OnSkillLearned;
        public event Action<SkillInstance> OnSkillLevelUp;

        #endregion

        private void Awake()
        {
            if (_autoFindComponents)
            {
                if (_characterStats == null)
                    _characterStats = GetComponent<CharacterStats>();
                if (_levelSystem == null)
                    _levelSystem = GetComponent<LevelSystem>();
            }
        }

        private void Update()
        {
            // Update cooldowns
            foreach (var skill in _skills)
            {
                skill.UpdateCooldown(Time.deltaTime);
            }

            // Handle casting
            if (_isCasting)
            {
                _castTimer -= Time.deltaTime;
                if (_castTimer <= 0)
                {
                    CompleteCast();
                }
            }
        }

        /// <summary>
        /// Learn a new skill.
        /// </summary>
        public bool LearnSkill(string skillId, int level = 1)
        {
            if (HasSkill(skillId))
            {
                Debug.LogWarning($"[SkillUser] Skill already learned: {skillId}");
                return false;
            }

            if (_skills.Count >= _maxSkillSlots)
            {
                Debug.LogWarning($"[SkillUser] Max skill slots reached");
                return false;
            }

            var skillData = SkillDatabase.Instance?.GetSkill(skillId);
            if (skillData == null)
            {
                Debug.LogWarning($"[SkillUser] Skill not found: {skillId}");
                return false;
            }

            var instance = new SkillInstance(skillId, level);
            _skills.Add(instance);

            OnSkillLearned?.Invoke(instance);

            EventBus<SkillLearnedEvent>.Publish(new SkillLearnedEvent
            {
                SkillId = skillId,
                User = gameObject
            });

            Debug.Log($"[SkillUser] {gameObject.name} learned {skillData.DisplayName}");
            return true;
        }

        /// <summary>
        /// Forget a skill.
        /// </summary>
        public bool ForgetSkill(string skillId)
        {
            var skill = GetSkill(skillId);
            if (skill == null) return false;

            _skills.Remove(skill);
            return true;
        }

        /// <summary>
        /// Check if a skill is learned.
        /// </summary>
        public bool HasSkill(string skillId)
        {
            return _skills.Exists(s => s.SkillId == skillId);
        }

        /// <summary>
        /// Get a skill instance.
        /// </summary>
        public SkillInstance GetSkill(string skillId)
        {
            return _skills.Find(s => s.SkillId == skillId);
        }

        /// <summary>
        /// Get skill at slot index.
        /// </summary>
        public SkillInstance GetSkillAtSlot(int index)
        {
            if (index < 0 || index >= _skills.Count)
                return null;
            return _skills[index];
        }

        /// <summary>
        /// Use a skill by ID.
        /// </summary>
        public SkillResult UseSkill(string skillId, GameObject target = null)
        {
            var skill = GetSkill(skillId);
            if (skill == null)
                return SkillResult.Failed;

            return UseSkillInstance(skill, target);
        }

        /// <summary>
        /// Use a skill by slot index.
        /// </summary>
        public SkillResult UseSkillAtSlot(int slotIndex, GameObject target = null)
        {
            var skill = GetSkillAtSlot(slotIndex);
            if (skill == null)
                return SkillResult.Failed;

            return UseSkillInstance(skill, target);
        }

        /// <summary>
        /// Use a skill instance.
        /// </summary>
        public SkillResult UseSkillInstance(SkillInstance skill, GameObject target = null)
        {
            if (_isCasting)
                return SkillResult.Failed;

            var skillData = skill.GetSkillData();
            if (skillData == null)
                return SkillResult.Failed;

            // Check if can use
            int userLevel = _levelSystem?.Level ?? 1;
            var canUse = skill.CanUse(_characterStats, userLevel);
            if (canUse != SkillResult.Success)
                return canUse;

            // Check target for self-targeting skills
            if (skillData.TargetType == TargetType.Self)
            {
                target = gameObject;
            }

            // Validate target
            if (!ValidateTarget(skillData, target))
                return SkillResult.InvalidTarget;

            // Check range
            if (!CheckRange(skillData, target))
                return SkillResult.OutOfRange;

            // Consume resources
            if (_characterStats != null)
            {
                if (skillData.ManaCost > 0)
                    _characterStats.UseMana(skillData.ManaCost);
                if (skillData.StaminaCost > 0)
                    _characterStats.UseStamina(skillData.StaminaCost);
            }

            // Handle cast time
            if (skillData.CastTime > 0)
            {
                StartCasting(skill, target);
                return SkillResult.Success;
            }

            // Instant cast
            return ExecuteSkill(skill, target);
        }

        private bool ValidateTarget(SkillBase skillData, GameObject target)
        {
            switch (skillData.TargetType)
            {
                case TargetType.Self:
                case TargetType.None:
                    return true;

                case TargetType.SingleEnemy:
                case TargetType.SingleAlly:
                case TargetType.Area:
                case TargetType.Direction:
                    return target != null;

                default:
                    return true;
            }
        }

        private bool CheckRange(SkillBase skillData, GameObject target)
        {
            if (skillData.Range <= 0 || target == null || skillData.TargetType == TargetType.Self)
                return true;

            float distance = Vector3.Distance(transform.position, target.transform.position);
            return distance <= skillData.Range;
        }

        private void StartCasting(SkillInstance skill, GameObject target)
        {
            var skillData = skill.GetSkillData();

            _isCasting = true;
            _castTimer = skillData.CastTime;
            _castingSkill = skill;
            _castTarget = target;

            OnSkillStartCast?.Invoke(skill);

            Debug.Log($"[SkillUser] {gameObject.name} started casting {skillData.DisplayName}");
        }

        private void CompleteCast()
        {
            if (_castingSkill != null)
            {
                ExecuteSkill(_castingSkill, _castTarget);
            }

            _isCasting = false;
            _castingSkill = null;
            _castTarget = null;
        }

        /// <summary>
        /// Interrupt current casting.
        /// </summary>
        public void InterruptCast()
        {
            if (!_isCasting) return;

            OnSkillInterrupted?.Invoke(_castingSkill);

            Debug.Log($"[SkillUser] {gameObject.name} casting interrupted");

            _isCasting = false;
            _castingSkill = null;
            _castTarget = null;
        }

        private SkillResult ExecuteSkill(SkillInstance skill, GameObject target)
        {
            var skillData = skill.GetSkillData();
            if (skillData == null)
                return SkillResult.Failed;

            // Execute the skill
            var result = skillData.Execute(gameObject, target);

            if (result == SkillResult.Success)
            {
                // Start cooldown
                skill.StartCooldown();

                // Add experience
                if (skill.AddExperience(10))
                {
                    OnSkillLevelUp?.Invoke(skill);
                }

                // Fire events
                OnSkillUsed?.Invoke(skill, target);

                EventBus<SkillUsedEvent>.Publish(new SkillUsedEvent
                {
                    SkillId = skill.SkillId,
                    User = gameObject,
                    Target = target
                });
            }

            return result;
        }

        /// <summary>
        /// Reset all cooldowns.
        /// </summary>
        public void ResetAllCooldowns()
        {
            foreach (var skill in _skills)
            {
                skill.ResetCooldown();
            }
        }

        /// <summary>
        /// Level up a skill.
        /// </summary>
        public bool LevelUpSkill(string skillId)
        {
            var skill = GetSkill(skillId);
            if (skill == null) return false;

            skill.Level++;
            OnSkillLevelUp?.Invoke(skill);
            return true;
        }
    }

    #region Events

    public struct SkillUsedEvent : IEvent
    {
        public string SkillId;
        public GameObject User;
        public GameObject Target;
    }

    public struct SkillLearnedEvent : IEvent
    {
        public string SkillId;
        public GameObject User;
    }

    #endregion
}
