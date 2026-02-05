using System;
using UnityEngine;

namespace LWT.UnityWorkbench.Utilities
{
    /// <summary>
    /// A cooldown timer that prevents actions from being performed too frequently.
    /// </summary>
    [Serializable]
    public class Cooldown
    {
        [SerializeField] private float _duration;
        [SerializeField] private bool _unscaledTime;

        private float _lastUseTime = float.NegativeInfinity;

        public float Duration
        {
            get => _duration;
            set => _duration = Mathf.Max(0f, value);
        }

        public bool UnscaledTime
        {
            get => _unscaledTime;
            set => _unscaledTime = value;
        }

        public float Elapsed => CurrentTime - _lastUseTime;
        public float Remaining => Mathf.Max(0f, _duration - Elapsed);
        public float Progress => _duration > 0f ? Mathf.Clamp01(Elapsed / _duration) : 1f;
        public bool IsReady => Elapsed >= _duration;

        private float CurrentTime => _unscaledTime ? Time.unscaledTime : Time.time;

        public event Action OnReady;
        public event Action OnUsed;

        public Cooldown(float duration, bool unscaledTime = false)
        {
            _duration = Mathf.Max(0f, duration);
            _unscaledTime = unscaledTime;
        }

        /// <summary>
        /// Attempts to use the cooldown. Returns true if successful.
        /// </summary>
        public bool TryUse()
        {
            if (!IsReady) return false;

            Use();
            return true;
        }

        /// <summary>
        /// Forces the cooldown to be used regardless of ready state.
        /// </summary>
        public void Use()
        {
            _lastUseTime = CurrentTime;
            OnUsed?.Invoke();
        }

        /// <summary>
        /// Resets the cooldown to ready state.
        /// </summary>
        public void Reset()
        {
            _lastUseTime = float.NegativeInfinity;
            OnReady?.Invoke();
        }

        /// <summary>
        /// Reduces the remaining cooldown time.
        /// </summary>
        public void ReduceCooldown(float amount)
        {
            _lastUseTime -= amount;
            if (IsReady)
            {
                OnReady?.Invoke();
            }
        }

        /// <summary>
        /// Checks if ready and invokes callback if so.
        /// </summary>
        public void CheckReady(Action onReady)
        {
            if (IsReady)
            {
                onReady?.Invoke();
            }
        }

        /// <summary>
        /// Uses the cooldown if ready and performs an action.
        /// </summary>
        public bool TryUse(Action action)
        {
            if (!TryUse()) return false;
            action?.Invoke();
            return true;
        }
    }
}
