using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Utilities
{
    /// <summary>
    /// Chains multiple actions together with delays and conditions.
    /// </summary>
    public class ActionSequence
    {
        private readonly List<ISequenceStep> _steps = new List<ISequenceStep>();
        private Coroutine _coroutine;
        private bool _isRunning;
        private bool _isPaused;
        private int _currentStep;
        private int _loopCount;
        private int _maxLoops;

        public bool IsRunning => _isRunning;
        public bool IsPaused => _isPaused;
        public int CurrentStepIndex => _currentStep;
        public int TotalSteps => _steps.Count;

        public event Action OnStart;
        public event Action OnComplete;
        public event Action OnCancel;
        public event Action<int> OnStepComplete;

        /// <summary>
        /// Adds an immediate action.
        /// </summary>
        public ActionSequence Then(Action action)
        {
            _steps.Add(new ActionStep(action));
            return this;
        }

        /// <summary>
        /// Adds a delay.
        /// </summary>
        public ActionSequence Wait(float seconds)
        {
            _steps.Add(new WaitStep(seconds, false));
            return this;
        }

        /// <summary>
        /// Adds an unscaled delay.
        /// </summary>
        public ActionSequence WaitUnscaled(float seconds)
        {
            _steps.Add(new WaitStep(seconds, true));
            return this;
        }

        /// <summary>
        /// Adds a frame wait.
        /// </summary>
        public ActionSequence WaitFrame()
        {
            _steps.Add(new WaitFrameStep());
            return this;
        }

        /// <summary>
        /// Waits until a condition is true.
        /// </summary>
        public ActionSequence WaitUntil(Func<bool> condition)
        {
            _steps.Add(new WaitUntilStep(condition));
            return this;
        }

        /// <summary>
        /// Waits while a condition is true.
        /// </summary>
        public ActionSequence WaitWhile(Func<bool> condition)
        {
            _steps.Add(new WaitWhileStep(condition));
            return this;
        }

        /// <summary>
        /// Adds a tween/lerp action.
        /// </summary>
        public ActionSequence Tween(float duration, Action<float> onUpdate, bool unscaledTime = false)
        {
            _steps.Add(new TweenStep(duration, onUpdate, unscaledTime));
            return this;
        }

        /// <summary>
        /// Adds a tween with easing.
        /// </summary>
        public ActionSequence Tween(float duration, Action<float> onUpdate, Easing.Type easingType, bool unscaledTime = false)
        {
            _steps.Add(new TweenStep(duration, t => onUpdate(Easing.Evaluate(easingType, t)), unscaledTime));
            return this;
        }

        /// <summary>
        /// Adds a coroutine step.
        /// </summary>
        public ActionSequence Coroutine(Func<IEnumerator> coroutineFactory)
        {
            _steps.Add(new CoroutineStep(coroutineFactory));
            return this;
        }

        /// <summary>
        /// Sets the sequence to loop.
        /// </summary>
        public ActionSequence Loop(int times = -1)
        {
            _maxLoops = times;
            return this;
        }

        /// <summary>
        /// Starts the sequence.
        /// </summary>
        public ActionSequence Start()
        {
            if (_isRunning) return this;

            _isRunning = true;
            _isPaused = false;
            _currentStep = 0;
            _loopCount = 0;

            OnStart?.Invoke();
            _coroutine = CoroutineRunner.Run(RunSequence());
            return this;
        }

        /// <summary>
        /// Pauses the sequence.
        /// </summary>
        public void Pause()
        {
            _isPaused = true;
        }

        /// <summary>
        /// Resumes the sequence.
        /// </summary>
        public void Resume()
        {
            _isPaused = false;
        }

        /// <summary>
        /// Cancels the sequence.
        /// </summary>
        public void Cancel()
        {
            if (!_isRunning) return;

            if (_coroutine != null)
            {
                CoroutineRunner.Stop(_coroutine);
                _coroutine = null;
            }

            _isRunning = false;
            OnCancel?.Invoke();
        }

        /// <summary>
        /// Resets the sequence for reuse.
        /// </summary>
        public ActionSequence Reset()
        {
            Cancel();
            _currentStep = 0;
            _loopCount = 0;
            return this;
        }

        /// <summary>
        /// Clears all steps.
        /// </summary>
        public ActionSequence Clear()
        {
            Cancel();
            _steps.Clear();
            return this;
        }

        private IEnumerator RunSequence()
        {
            do
            {
                _currentStep = 0;

                while (_currentStep < _steps.Count)
                {
                    // Handle pause
                    while (_isPaused)
                    {
                        yield return null;
                    }

                    var step = _steps[_currentStep];
                    yield return step.Execute();

                    OnStepComplete?.Invoke(_currentStep);
                    _currentStep++;
                }

                _loopCount++;
            }
            while (_maxLoops < 0 || _loopCount < _maxLoops);

            _isRunning = false;
            OnComplete?.Invoke();
        }

        /// <summary>
        /// Creates a new sequence.
        /// </summary>
        public static ActionSequence Create()
        {
            return new ActionSequence();
        }

        #region Step Implementations

        private interface ISequenceStep
        {
            IEnumerator Execute();
        }

        private class ActionStep : ISequenceStep
        {
            private readonly Action _action;
            public ActionStep(Action action) => _action = action;
            public IEnumerator Execute()
            {
                _action?.Invoke();
                yield break;
            }
        }

        private class WaitStep : ISequenceStep
        {
            private readonly float _seconds;
            private readonly bool _unscaled;
            public WaitStep(float seconds, bool unscaled) { _seconds = seconds; _unscaled = unscaled; }
            public IEnumerator Execute()
            {
                if (_unscaled)
                    yield return new WaitForSecondsRealtime(_seconds);
                else
                    yield return new WaitForSeconds(_seconds);
            }
        }

        private class WaitFrameStep : ISequenceStep
        {
            public IEnumerator Execute()
            {
                yield return null;
            }
        }

        private class WaitUntilStep : ISequenceStep
        {
            private readonly Func<bool> _condition;
            public WaitUntilStep(Func<bool> condition) => _condition = condition;
            public IEnumerator Execute()
            {
                yield return new WaitUntil(_condition);
            }
        }

        private class WaitWhileStep : ISequenceStep
        {
            private readonly Func<bool> _condition;
            public WaitWhileStep(Func<bool> condition) => _condition = condition;
            public IEnumerator Execute()
            {
                yield return new WaitWhile(_condition);
            }
        }

        private class TweenStep : ISequenceStep
        {
            private readonly float _duration;
            private readonly Action<float> _onUpdate;
            private readonly bool _unscaled;

            public TweenStep(float duration, Action<float> onUpdate, bool unscaled)
            {
                _duration = duration;
                _onUpdate = onUpdate;
                _unscaled = unscaled;
            }

            public IEnumerator Execute()
            {
                float elapsed = 0f;
                while (elapsed < _duration)
                {
                    _onUpdate?.Invoke(elapsed / _duration);
                    yield return null;
                    elapsed += _unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
                }
                _onUpdate?.Invoke(1f);
            }
        }

        private class CoroutineStep : ISequenceStep
        {
            private readonly Func<IEnumerator> _factory;
            public CoroutineStep(Func<IEnumerator> factory) => _factory = factory;
            public IEnumerator Execute()
            {
                yield return _factory();
            }
        }

        #endregion
    }
}
