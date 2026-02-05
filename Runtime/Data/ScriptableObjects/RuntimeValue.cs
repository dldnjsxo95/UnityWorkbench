using System;
using UnityEngine;

namespace LWT.UnityWorkbench.Data
{
    /// <summary>
    /// Base class for ScriptableObject-based runtime values.
    /// Supports events and Inspector debugging.
    /// </summary>
    public abstract class RuntimeValue<T> : ScriptableObject
    {
        [Header("Value")]
        [SerializeField] protected T _initialValue;
        [SerializeField] protected T _runtimeValue;

        [Header("Debug")]
        [SerializeField] private bool _logChanges;

#if UNITY_EDITOR
        [TextArea(2, 4)]
        [SerializeField] private string _description;
#endif

        public event Action<T> OnValueChanged;

        public T Value
        {
            get => _runtimeValue;
            set
            {
                if (!Equals(_runtimeValue, value))
                {
                    T oldValue = _runtimeValue;
                    _runtimeValue = value;

                    if (_logChanges)
                    {
                        Debug.Log($"[RuntimeValue] {name}: {oldValue} → {value}");
                    }

                    OnValueChanged?.Invoke(_runtimeValue);
                }
            }
        }

        public T InitialValue => _initialValue;

        private void OnEnable()
        {
            // Reset to initial value when entering play mode
#if UNITY_EDITOR
            _runtimeValue = _initialValue;
#endif
        }

        /// <summary>
        /// Reset to initial value.
        /// </summary>
        public void Reset()
        {
            Value = _initialValue;
        }

        /// <summary>
        /// Set value without triggering event.
        /// </summary>
        public void SetValueSilent(T value)
        {
            _runtimeValue = value;
        }

        public static implicit operator T(RuntimeValue<T> variable) => variable.Value;

        public override string ToString() => _runtimeValue?.ToString() ?? "null";
    }
}
