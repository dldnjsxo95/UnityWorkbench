using System;
using UnityEngine;
using LWT.UnityWorkbench.Core;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// Component for managing character stats.
    /// Handles health, mana, damage, healing, and stat modifiers.
    /// </summary>
    public class CharacterStats : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private CharacterStatsConfig _config;
        [SerializeField] private bool _initializeOnAwake = true;

        [Header("Current Values")]
        [SerializeField] private float _currentHealth;
        [SerializeField] private float _currentMana;
        [SerializeField] private float _currentStamina;

        private StatsContainer _stats;
        private bool _isInitialized;

        /// <summary>
        /// Stats container with all stat values.
        /// </summary>
        public StatsContainer Stats
        {
            get
            {
                if (!_isInitialized) Initialize();
                return _stats;
            }
        }

        #region Convenience Properties

        public float Health
        {
            get => _currentHealth;
            set => _currentHealth = Mathf.Clamp(value, 0, MaxHealth);
        }

        public float MaxHealth => Stats.GetValue(StatType.MaxHealth);
        public float HealthPercent => MaxHealth > 0 ? _currentHealth / MaxHealth : 0;

        public float Mana
        {
            get => _currentMana;
            set => _currentMana = Mathf.Clamp(value, 0, MaxMana);
        }

        public float MaxMana => Stats.GetValue(StatType.MaxMana);
        public float ManaPercent => MaxMana > 0 ? _currentMana / MaxMana : 0;

        public float Stamina
        {
            get => _currentStamina;
            set => _currentStamina = Mathf.Clamp(value, 0, MaxStamina);
        }

        public float MaxStamina => Stats.GetValue(StatType.MaxStamina);
        public float StaminaPercent => MaxStamina > 0 ? _currentStamina / MaxStamina : 0;

        public float Attack => Stats.GetValue(StatType.Attack);
        public float Defense => Stats.GetValue(StatType.Defense);
        public float MoveSpeed => Stats.GetValue(StatType.MoveSpeed);

        public bool IsAlive => _currentHealth > 0;
        public bool IsDead => !IsAlive;

        #endregion

        #region Events

        public event Action<float, float> OnHealthChanged;    // current, max
        public event Action<float, float> OnManaChanged;      // current, max
        public event Action<float, float> OnStaminaChanged;   // current, max
        public event Action<DamageInfo> OnDamaged;
        public event Action<float> OnHealed;
        public event Action OnDeath;
        public event Action OnRevive;

        #endregion

        private void Awake()
        {
            if (_initializeOnAwake)
            {
                Initialize();
            }
        }

        private void Update()
        {
            if (_isInitialized)
            {
                _stats.UpdateTimedModifiers(Time.deltaTime);
            }
        }

        /// <summary>
        /// Initialize stats from config.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;  // Set early to prevent recursive calls

            if (_config != null)
            {
                _stats = _config.CreateStatsContainer();
            }
            else
            {
                _stats = new StatsContainer();
                _stats.SetStat(StatType.MaxHealth, 100);
                _stats.SetStat(StatType.MaxMana, 50);
                _stats.SetStat(StatType.MaxStamina, 100);
                _stats.SetStat(StatType.Attack, 10);
                _stats.SetStat(StatType.Defense, 5);
                _stats.SetStat(StatType.MoveSpeed, 5);
                _stats.Initialize();
            }

            _stats.OnStatChanged += OnStatValueChanged;

            // Set current values to max
            _currentHealth = _stats.GetValue(StatType.MaxHealth);
            _currentMana = _stats.GetValue(StatType.MaxMana);
            _currentStamina = _stats.GetValue(StatType.MaxStamina);
        }

        /// <summary>
        /// Initialize with a specific config.
        /// </summary>
        public void Initialize(CharacterStatsConfig config)
        {
            _config = config;
            _isInitialized = false;
            Initialize();
        }

        #region Health/Damage/Healing

        /// <summary>
        /// Deal damage to this character.
        /// </summary>
        public void TakeDamage(float damage, GameObject source = null, DamageType damageType = DamageType.Physical)
        {
            if (IsDead || damage <= 0) return;

            // Calculate final damage after defense
            float finalDamage = CalculateDamage(damage, damageType);

            float previousHealth = _currentHealth;
            _currentHealth = Mathf.Max(0, _currentHealth - finalDamage);

            var damageInfo = new DamageInfo
            {
                RawDamage = damage,
                FinalDamage = finalDamage,
                DamageType = damageType,
                Source = source,
                Target = gameObject
            };

            OnDamaged?.Invoke(damageInfo);
            OnHealthChanged?.Invoke(_currentHealth, MaxHealth);

            // Publish event
            EventBus<DamageEvent>.Publish(new DamageEvent
            {
                Amount = finalDamage,
                Source = source,
                Target = gameObject
            });

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Heal this character.
        /// </summary>
        public void Heal(float amount, GameObject source = null)
        {
            if (IsDead || amount <= 0) return;

            float previousHealth = _currentHealth;
            _currentHealth = Mathf.Min(MaxHealth, _currentHealth + amount);

            float actualHealing = _currentHealth - previousHealth;

            if (actualHealing > 0)
            {
                OnHealed?.Invoke(actualHealing);
                OnHealthChanged?.Invoke(_currentHealth, MaxHealth);

                EventBus<HealEvent>.Publish(new HealEvent
                {
                    Amount = actualHealing,
                    Source = source,
                    Target = gameObject
                });
            }
        }

        /// <summary>
        /// Set health directly (clamped).
        /// </summary>
        public void SetHealth(float value)
        {
            _currentHealth = Mathf.Clamp(value, 0, MaxHealth);
            OnHealthChanged?.Invoke(_currentHealth, MaxHealth);

            if (_currentHealth <= 0 && IsAlive)
            {
                Die();
            }
        }

        /// <summary>
        /// Restore health to full.
        /// </summary>
        public void FullHeal()
        {
            Heal(MaxHealth - _currentHealth);
        }

        #endregion

        #region Mana

        /// <summary>
        /// Use mana. Returns true if enough mana was available.
        /// </summary>
        public bool UseMana(float amount)
        {
            if (_currentMana < amount) return false;

            _currentMana -= amount;
            OnManaChanged?.Invoke(_currentMana, MaxMana);
            return true;
        }

        /// <summary>
        /// Restore mana.
        /// </summary>
        public void RestoreMana(float amount)
        {
            _currentMana = Mathf.Min(MaxMana, _currentMana + amount);
            OnManaChanged?.Invoke(_currentMana, MaxMana);
        }

        /// <summary>
        /// Use stamina. Returns true if enough stamina was available.
        /// </summary>
        public bool UseStamina(float amount)
        {
            if (_currentStamina < amount) return false;

            _currentStamina -= amount;
            OnStaminaChanged?.Invoke(_currentStamina, MaxStamina);
            return true;
        }

        /// <summary>
        /// Restore stamina.
        /// </summary>
        public void RestoreStamina(float amount)
        {
            _currentStamina = Mathf.Min(MaxStamina, _currentStamina + amount);
            OnStaminaChanged?.Invoke(_currentStamina, MaxStamina);
        }

        #endregion

        #region Stat Modifiers

        /// <summary>
        /// Apply a buff/debuff to a stat.
        /// </summary>
        public void ApplyModifier(StatType statType, StatModifier modifier)
        {
            Stats.AddModifier(statType, modifier);
        }

        /// <summary>
        /// Remove a modifier by ID.
        /// </summary>
        public bool RemoveModifier(StatType statType, string modifierId)
        {
            return Stats.RemoveModifier(statType, modifierId);
        }

        /// <summary>
        /// Remove all modifiers from a source.
        /// </summary>
        public int RemoveAllModifiersFromSource(object source)
        {
            return Stats.RemoveAllModifiersFromSource(source);
        }

        #endregion

        #region Death/Revive

        /// <summary>
        /// Kill this character.
        /// </summary>
        public void Die()
        {
            if (IsDead) return;

            _currentHealth = 0;

            OnDeath?.Invoke();
            EventBus<DeathEvent>.Publish(new DeathEvent { Deceased = gameObject });
        }

        /// <summary>
        /// Revive this character with specified health percentage.
        /// </summary>
        public void Revive(float healthPercent = 1f)
        {
            if (IsAlive) return;

            _currentHealth = MaxHealth * Mathf.Clamp01(healthPercent);
            OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
            OnRevive?.Invoke();
        }

        #endregion

        #region Utility

        private float CalculateDamage(float rawDamage, DamageType damageType)
        {
            float defense = damageType == DamageType.Physical
                ? Stats.GetValue(StatType.Defense)
                : Stats.GetValue(StatType.MagicDefense);

            // Simple damage formula: damage * (100 / (100 + defense))
            float damageReduction = 100f / (100f + defense);
            return rawDamage * damageReduction;
        }

        private void OnStatValueChanged(StatType type, float oldValue, float newValue)
        {
            // Clamp current values when max changes
            if (type == StatType.MaxHealth)
            {
                _currentHealth = Mathf.Min(_currentHealth, newValue);
                OnHealthChanged?.Invoke(_currentHealth, newValue);
            }
            else if (type == StatType.MaxMana)
            {
                _currentMana = Mathf.Min(_currentMana, newValue);
                OnManaChanged?.Invoke(_currentMana, newValue);
            }

            EventBus<StatChangedEvent>.Publish(new StatChangedEvent
            {
                Type = type,
                OldValue = oldValue,
                NewValue = newValue
            });
        }

        #endregion
    }

    #region Supporting Types

    public enum DamageType
    {
        Physical,
        Magical,
        True,  // Ignores defense
        Pure   // Fixed damage
    }

    public struct DamageInfo
    {
        public float RawDamage;
        public float FinalDamage;
        public DamageType DamageType;
        public GameObject Source;
        public GameObject Target;
        public bool IsCritical;
    }

    #endregion

    #region Events

    public struct DamageEvent : IEvent
    {
        public float Amount;
        public GameObject Source;
        public GameObject Target;
    }

    public struct HealEvent : IEvent
    {
        public float Amount;
        public GameObject Source;
        public GameObject Target;
    }

    public struct DeathEvent : IEvent
    {
        public GameObject Deceased;
    }

    public struct StatChangedEvent : IEvent
    {
        public StatType Type;
        public float OldValue;
        public float NewValue;
    }

    #endregion
}
