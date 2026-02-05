using System;
using UnityEngine;

namespace LWT.UnityWorkbench.Utilities
{
    /// <summary>
    /// A stopwatch for measuring elapsed time.
    /// </summary>
    [Serializable]
    public class Stopwatch
    {
        [SerializeField] private bool _unscaledTime;

        private float _startTime;
        private float _pausedTime;
        private float _totalPausedDuration;
        private bool _isRunning;
        private bool _isPaused;

        public bool UnscaledTime
        {
            get => _unscaledTime;
            set => _unscaledTime = value;
        }

        public float Elapsed
        {
            get
            {
                if (!_isRunning) return 0f;
                if (_isPaused) return _pausedTime - _startTime - _totalPausedDuration;
                return CurrentTime - _startTime - _totalPausedDuration;
            }
        }

        public float ElapsedMilliseconds => Elapsed * 1000f;
        public bool IsRunning => _isRunning && !_isPaused;
        public bool IsPaused => _isPaused;

        private float CurrentTime => _unscaledTime ? Time.unscaledTime : Time.time;

        public Stopwatch(bool unscaledTime = false)
        {
            _unscaledTime = unscaledTime;
        }

        /// <summary>
        /// Starts or restarts the stopwatch.
        /// </summary>
        public void Start()
        {
            _startTime = CurrentTime;
            _totalPausedDuration = 0f;
            _isRunning = true;
            _isPaused = false;
        }

        /// <summary>
        /// Stops the stopwatch.
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _isPaused = false;
        }

        /// <summary>
        /// Pauses the stopwatch.
        /// </summary>
        public void Pause()
        {
            if (_isRunning && !_isPaused)
            {
                _pausedTime = CurrentTime;
                _isPaused = true;
            }
        }

        /// <summary>
        /// Resumes the stopwatch.
        /// </summary>
        public void Resume()
        {
            if (_isPaused)
            {
                _totalPausedDuration += CurrentTime - _pausedTime;
                _isPaused = false;
            }
        }

        /// <summary>
        /// Resets the stopwatch.
        /// </summary>
        public void Reset()
        {
            _startTime = CurrentTime;
            _totalPausedDuration = 0f;
            _isPaused = false;
        }

        /// <summary>
        /// Restarts the stopwatch (reset + start).
        /// </summary>
        public void Restart()
        {
            Start();
        }

        /// <summary>
        /// Gets the elapsed time and resets the stopwatch (lap time).
        /// </summary>
        public float Lap()
        {
            float elapsed = Elapsed;
            Reset();
            return elapsed;
        }

        /// <summary>
        /// Formats the elapsed time as a string.
        /// </summary>
        public string Format(string format = @"mm\:ss\.ff")
        {
            var timeSpan = TimeSpan.FromSeconds(Elapsed);
            return timeSpan.ToString(format);
        }

        /// <summary>
        /// Creates and starts a new stopwatch.
        /// </summary>
        public static Stopwatch StartNew(bool unscaledTime = false)
        {
            var sw = new Stopwatch(unscaledTime);
            sw.Start();
            return sw;
        }
    }
}
