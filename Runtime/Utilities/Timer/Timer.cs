using System;
using UnityEngine;

namespace LWT.UnityWorkbench.Utilities
{
    /// <summary>
    /// A simple timer that can be used for countdowns or delays.
    /// </summary>
    [Serializable]
    public class Timer
    {
        [SerializeField] private float _duration;
        [SerializeField] private bool _autoReset;
        [SerializeField] private bool _unscaledTime;

        private float _elapsed;
        private bool _isRunning;
        private bool _isPaused;

        public float Duration
        {
            get => _duration;
            set => _duration = Mathf.Max(0f, value);
        }

        public float Elapsed => _elapsed;
        public float Remaining => Mathf.Max(0f, _duration - _elapsed);
        public float Progress => _duration > 0f ? Mathf.Clamp01(_elapsed / _duration) : 1f;
        public bool IsRunning => _isRunning && !_isPaused;
        public bool IsPaused => _isPaused;
        public bool IsCompleted => _elapsed >= _duration;
        public bool AutoReset { get => _autoReset; set => _autoReset = value; }
        public bool UnscaledTime { get => _unscaledTime; set => _unscaledTime = value; }

        public event Action OnCompleted;
        public event Action<float> OnTick;

        public Timer(float duration, bool autoReset = false, bool unscaledTime = false)
        {
            _duration = Mathf.Max(0f, duration);
            _autoReset = autoReset;
            _unscaledTime = unscaledTime;
        }

        /// <summary>
        /// Starts or restarts the timer.
        /// </summary>
        public void Start()
        {
            _elapsed = 0f;
            _isRunning = true;
            _isPaused = false;
        }

        /// <summary>
        /// Starts the timer with a new duration.
        /// </summary>
        public void Start(float duration)
        {
            _duration = Mathf.Max(0f, duration);
            Start();
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _isPaused = false;
        }

        /// <summary>
        /// Pauses the timer.
        /// </summary>
        public void Pause()
        {
            if (_isRunning)
            {
                _isPaused = true;
            }
        }

        /// <summary>
        /// Resumes the timer.
        /// </summary>
        public void Resume()
        {
            _isPaused = false;
        }

        /// <summary>
        /// Resets the timer without stopping it.
        /// </summary>
        public void Reset()
        {
            _elapsed = 0f;
        }

        /// <summary>
        /// Updates the timer. Call this in Update().
        /// </summary>
        public void Update()
        {
            if (!_isRunning || _isPaused) return;

            float deltaTime = _unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _elapsed += deltaTime;

            OnTick?.Invoke(Progress);

            if (_elapsed >= _duration)
            {
                OnCompleted?.Invoke();

                if (_autoReset)
                {
                    _elapsed -= _duration;
                }
                else
                {
                    _isRunning = false;
                }
            }
        }

        /// <summary>
        /// Adds time to the timer.
        /// </summary>
        public void AddTime(float time)
        {
            _elapsed += time;
        }

        /// <summary>
        /// Subtracts time from the timer.
        /// </summary>
        public void SubtractTime(float time)
        {
            _elapsed = Mathf.Max(0f, _elapsed - time);
        }

        /// <summary>
        /// Sets the elapsed time directly.
        /// </summary>
        public void SetElapsed(float time)
        {
            _elapsed = Mathf.Clamp(time, 0f, _duration);
        }

        /// <summary>
        /// Creates a simple one-shot timer.
        /// </summary>
        public static Timer OneShot(float duration)
        {
            return new Timer(duration, false);
        }

        /// <summary>
        /// Creates a repeating timer.
        /// </summary>
        public static Timer Repeating(float duration)
        {
            return new Timer(duration, true);
        }
    }
}
