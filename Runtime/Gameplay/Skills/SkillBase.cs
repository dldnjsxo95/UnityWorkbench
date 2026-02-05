using UnityEngine;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// ScriptableObject base class for skill definitions.
    /// </summary>
    [CreateAssetMenu(fileName = "Skill", menuName = "UnityWorkbench/Gameplay/Skill")]
    public class SkillBase : ScriptableObject, ISkill
    {
        [Header("Basic Info")]
        [SerializeField] private string _skillId;
        [SerializeField] private string _displayName;
        [SerializeField, TextArea(2, 5)] private string _description;
        [SerializeField] private Sprite _icon;

        [Header("Classification")]
        [SerializeField] private SkillType _skillType = SkillType.Active;
        [SerializeField] private TargetType _targetType = TargetType.SingleEnemy;

        [Header("Timing")]
        [SerializeField] private float _cooldown = 5f;
        [SerializeField] private float _castTime = 0f;

        [Header("Range & Cost")]
        [SerializeField] private float _range = 5f;
        [SerializeField] private int _manaCost = 10;
        [SerializeField] private int _staminaCost = 0;
        [SerializeField] private int _requiredLevel = 1;

        [Header("Effects")]
        [SerializeField] private float _baseDamage = 0f;
        [SerializeField] private float _baseHealing = 0f;
        [SerializeField] private float _effectDuration = 0f;
        [SerializeField] private SkillStatModifier[] _statModifiers;

        [Header("Visual")]
        [SerializeField] private GameObject _castEffectPrefab;
        [SerializeField] private GameObject _hitEffectPrefab;
        [SerializeField] private AudioClip _castSound;
        [SerializeField] private AudioClip _hitSound;

        #region ISkill Implementation

        public string SkillId => string.IsNullOrEmpty(_skillId) ? name : _skillId;
        public string DisplayName => string.IsNullOrEmpty(_displayName) ? name : _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public SkillType SkillType => _skillType;
        public TargetType TargetType => _targetType;
        public float Cooldown => _cooldown;
        public float CastTime => _castTime;
        public float Range => _range;
        public int ManaCost => _manaCost;
        public int StaminaCost => _staminaCost;
        public int RequiredLevel => _requiredLevel;

        #endregion

        /// <summary>
        /// Base damage of the skill.
        /// </summary>
        public float BaseDamage => _baseDamage;

        /// <summary>
        /// Base healing of the skill.
        /// </summary>
        public float BaseHealing => _baseHealing;

        /// <summary>
        /// Duration of skill effects.
        /// </summary>
        public float EffectDuration => _effectDuration;

        /// <summary>
        /// Stat modifiers applied by this skill.
        /// </summary>
        public SkillStatModifier[] StatModifiers => _statModifiers;

        /// <summary>
        /// Prefab spawned when casting.
        /// </summary>
        public GameObject CastEffectPrefab => _castEffectPrefab;

        /// <summary>
        /// Prefab spawned on hit.
        /// </summary>
        public GameObject HitEffectPrefab => _hitEffectPrefab;

        /// <summary>
        /// Sound played when casting.
        /// </summary>
        public AudioClip CastSound => _castSound;

        /// <summary>
        /// Sound played on hit.
        /// </summary>
        public AudioClip HitSound => _hitSound;

        /// <summary>
        /// Create a runtime instance of this skill.
        /// </summary>
        public virtual SkillInstance CreateInstance()
        {
            return new SkillInstance(SkillId);
        }

        /// <summary>
        /// Execute the skill on a target.
        /// Override in subclasses for custom behavior.
        /// </summary>
        public virtual SkillResult Execute(GameObject user, GameObject target)
        {
            if (target == null && _targetType != TargetType.Self && _targetType != TargetType.None)
                return SkillResult.InvalidTarget;

            // Apply damage
            if (_baseDamage > 0 && target != null)
            {
                var targetStats = target.GetComponent<CharacterStats>();
                if (targetStats != null)
                {
                    float damage = CalculateDamage(user);
                    targetStats.TakeDamage(damage);
                }
            }

            // Apply healing
            if (_baseHealing > 0)
            {
                var healTarget = _targetType == TargetType.Self ? user.GetComponent<CharacterStats>() :
                                 target?.GetComponent<CharacterStats>();
                if (healTarget != null)
                {
                    float healing = CalculateHealing(user);
                    healTarget.Heal(healing);
                }
            }

            // Apply stat modifiers
            if (_statModifiers != null && _statModifiers.Length > 0)
            {
                var modTarget = _targetType == TargetType.Self ? user.GetComponent<CharacterStats>() :
                                target?.GetComponent<CharacterStats>();
                if (modTarget != null)
                {
                    foreach (var mod in _statModifiers)
                    {
                        var modCopy = new StatModifier(mod.Value, mod.ModifierType, _effectDuration, this);
                        modTarget.Stats.GetStat(mod.StatType)?.AddModifier(modCopy);
                    }
                }
            }

            // Spawn effects
            SpawnEffects(user, target);

            Debug.Log($"[Skill] {DisplayName} executed by {user.name}");
            return SkillResult.Success;
        }

        /// <summary>
        /// Calculate actual damage based on user stats.
        /// </summary>
        protected virtual float CalculateDamage(GameObject user)
        {
            float damage = _baseDamage;
            var stats = user.GetComponent<CharacterStats>();
            if (stats != null)
            {
                // Add attack stat bonus
                damage += stats.Stats.GetValue(StatType.Attack);
            }
            return damage;
        }

        /// <summary>
        /// Calculate actual healing based on user stats.
        /// </summary>
        protected virtual float CalculateHealing(GameObject user)
        {
            float healing = _baseHealing;
            var stats = user.GetComponent<CharacterStats>();
            if (stats != null)
            {
                // Add magic attack bonus for healing
                healing += stats.Stats.GetValue(StatType.MagicAttack) * 0.5f;
            }
            return healing;
        }

        /// <summary>
        /// Spawn visual effects.
        /// </summary>
        protected virtual void SpawnEffects(GameObject user, GameObject target)
        {
            if (_castEffectPrefab != null)
            {
                var effect = Instantiate(_castEffectPrefab, user.transform.position, Quaternion.identity);
                Destroy(effect, 3f);
            }

            if (_hitEffectPrefab != null && target != null)
            {
                var effect = Instantiate(_hitEffectPrefab, target.transform.position, Quaternion.identity);
                Destroy(effect, 3f);
            }
        }

        /// <summary>
        /// Get tooltip text for this skill.
        /// </summary>
        public virtual string GetTooltip()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"<b>{DisplayName}</b>");
            sb.AppendLine($"<color=yellow>{SkillType}</color>");
            sb.AppendLine();
            sb.AppendLine(Description);
            sb.AppendLine();

            if (_manaCost > 0) sb.AppendLine($"Mana: {_manaCost}");
            if (_staminaCost > 0) sb.AppendLine($"Stamina: {_staminaCost}");
            if (_cooldown > 0) sb.AppendLine($"Cooldown: {_cooldown:F1}s");
            if (_castTime > 0) sb.AppendLine($"Cast Time: {_castTime:F1}s");
            if (_range > 0) sb.AppendLine($"Range: {_range:F0}");

            if (_baseDamage > 0) sb.AppendLine($"Damage: {_baseDamage:F0}");
            if (_baseHealing > 0) sb.AppendLine($"Healing: {_baseHealing:F0}");

            return sb.ToString();
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_skillId))
            {
                _skillId = name;
            }

            _cooldown = Mathf.Max(0, _cooldown);
            _castTime = Mathf.Max(0, _castTime);
            _range = Mathf.Max(0, _range);
            _manaCost = Mathf.Max(0, _manaCost);
            _staminaCost = Mathf.Max(0, _staminaCost);
            _requiredLevel = Mathf.Max(1, _requiredLevel);
        }
    }
}
