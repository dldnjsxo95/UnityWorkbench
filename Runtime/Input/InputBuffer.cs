using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.InputHandling
{
    /// <summary>
    /// Input buffering system for combos and pre-input.
    /// </summary>
    public class InputBuffer : MonoBehaviour
    {
        [System.Serializable]
        public class BufferedInput
        {
            public string ActionName;
            public float Timestamp;
            public float Value;
            public bool Consumed;

            public float Age => Time.unscaledTime - Timestamp;
        }

        [Header("Settings")]
        [SerializeField] private float _bufferDuration = 0.2f;
        [SerializeField] private int _maxBufferSize = 10;
        [SerializeField] private List<string> _bufferedActions = new List<string>();

        private List<BufferedInput> _buffer = new List<BufferedInput>();
        private InputManager _inputManager;

        public float BufferDuration
        {
            get => _bufferDuration;
            set => _bufferDuration = Mathf.Max(0f, value);
        }

        public IReadOnlyList<BufferedInput> Buffer => _buffer;

        private void Start()
        {
            _inputManager = InputManager.Instance;
            if (_inputManager != null)
            {
                _inputManager.OnAnyAction += OnAction;
            }
        }

        private void OnDestroy()
        {
            if (_inputManager != null)
            {
                _inputManager.OnAnyAction -= OnAction;
            }
        }

        private void Update()
        {
            CleanupBuffer();
        }

        private void OnAction(string actionName, UnityEngine.InputSystem.InputActionPhase phase)
        {
            // Only buffer started phase (press)
            if (phase != UnityEngine.InputSystem.InputActionPhase.Started) return;

            // Only buffer specified actions
            if (_bufferedActions.Count > 0 && !_bufferedActions.Contains(actionName)) return;

            AddToBuffer(actionName);
        }

        private void AddToBuffer(string actionName, float value = 1f)
        {
            // Remove oldest if at capacity
            while (_buffer.Count >= _maxBufferSize)
            {
                _buffer.RemoveAt(0);
            }

            _buffer.Add(new BufferedInput
            {
                ActionName = actionName,
                Timestamp = Time.unscaledTime,
                Value = value,
                Consumed = false
            });
        }

        private void CleanupBuffer()
        {
            float currentTime = Time.unscaledTime;
            _buffer.RemoveAll(input =>
                input.Consumed || (currentTime - input.Timestamp) > _bufferDuration
            );
        }

        /// <summary>
        /// Checks if an action is in the buffer (unconsumed).
        /// </summary>
        public bool HasBuffered(string actionName)
        {
            foreach (var input in _buffer)
            {
                if (!input.Consumed && input.ActionName == actionName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Consumes a buffered action if available.
        /// </summary>
        public bool ConsumeBuffered(string actionName)
        {
            for (int i = _buffer.Count - 1; i >= 0; i--)
            {
                var input = _buffer[i];
                if (!input.Consumed && input.ActionName == actionName)
                {
                    input.Consumed = true;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets and consumes the oldest buffered action of a type.
        /// </summary>
        public BufferedInput PopBuffered(string actionName)
        {
            for (int i = 0; i < _buffer.Count; i++)
            {
                var input = _buffer[i];
                if (!input.Consumed && input.ActionName == actionName)
                {
                    input.Consumed = true;
                    return input;
                }
            }
            return null;
        }

        /// <summary>
        /// Clears all buffered inputs.
        /// </summary>
        public void ClearBuffer()
        {
            _buffer.Clear();
        }

        /// <summary>
        /// Clears buffered inputs for a specific action.
        /// </summary>
        public void ClearBuffered(string actionName)
        {
            _buffer.RemoveAll(input => input.ActionName == actionName);
        }

        /// <summary>
        /// Manually adds an input to the buffer.
        /// </summary>
        public void BufferInput(string actionName)
        {
            AddToBuffer(actionName);
        }

        /// <summary>
        /// Adds an action to the list of buffered actions.
        /// </summary>
        public void AddBufferedAction(string actionName)
        {
            if (!_bufferedActions.Contains(actionName))
            {
                _bufferedActions.Add(actionName);
            }
        }

        /// <summary>
        /// Removes an action from the list of buffered actions.
        /// </summary>
        public void RemoveBufferedAction(string actionName)
        {
            _bufferedActions.Remove(actionName);
        }
    }

    /// <summary>
    /// Combo input detection system.
    /// </summary>
    public class ComboDetector : MonoBehaviour
    {
        [System.Serializable]
        public class ComboDefinition
        {
            public string ComboName;
            public string[] Sequence;
            public float MaxTimeBetweenInputs = 0.5f;
            public UnityEngine.Events.UnityEvent OnComboExecuted;
        }

        [Header("Combos")]
        [SerializeField] private List<ComboDefinition> _combos = new List<ComboDefinition>();

        [Header("Settings")]
        [SerializeField] private float _inputTimeout = 1f;

        private List<string> _inputHistory = new List<string>();
        private float _lastInputTime;
        private InputManager _inputManager;

        public event System.Action<string> OnComboDetected;

        private void Start()
        {
            _inputManager = InputManager.Instance;
            if (_inputManager != null)
            {
                _inputManager.OnAnyAction += OnAction;
            }
        }

        private void OnDestroy()
        {
            if (_inputManager != null)
            {
                _inputManager.OnAnyAction -= OnAction;
            }
        }

        private void Update()
        {
            // Clear history if timeout
            if (_inputHistory.Count > 0 && Time.unscaledTime - _lastInputTime > _inputTimeout)
            {
                _inputHistory.Clear();
            }
        }

        private void OnAction(string actionName, UnityEngine.InputSystem.InputActionPhase phase)
        {
            if (phase != UnityEngine.InputSystem.InputActionPhase.Started) return;

            float currentTime = Time.unscaledTime;

            // Check if input is within timing window
            if (_inputHistory.Count > 0)
            {
                float timeSinceLastInput = currentTime - _lastInputTime;

                // Check against all combos for timing
                bool withinWindow = false;
                foreach (var combo in _combos)
                {
                    if (timeSinceLastInput <= combo.MaxTimeBetweenInputs)
                    {
                        withinWindow = true;
                        break;
                    }
                }

                if (!withinWindow)
                {
                    _inputHistory.Clear();
                }
            }

            // Add to history
            _inputHistory.Add(actionName);
            _lastInputTime = currentTime;

            // Check for combo matches
            CheckCombos();
        }

        private void CheckCombos()
        {
            foreach (var combo in _combos)
            {
                if (MatchesCombo(combo))
                {
                    ExecuteCombo(combo);
                    _inputHistory.Clear();
                    break;
                }
            }

            // Limit history size
            while (_inputHistory.Count > 20)
            {
                _inputHistory.RemoveAt(0);
            }
        }

        private bool MatchesCombo(ComboDefinition combo)
        {
            if (combo.Sequence == null || combo.Sequence.Length == 0) return false;
            if (_inputHistory.Count < combo.Sequence.Length) return false;

            int startIndex = _inputHistory.Count - combo.Sequence.Length;

            for (int i = 0; i < combo.Sequence.Length; i++)
            {
                if (_inputHistory[startIndex + i] != combo.Sequence[i])
                {
                    return false;
                }
            }

            return true;
        }

        private void ExecuteCombo(ComboDefinition combo)
        {
            combo.OnComboExecuted?.Invoke();
            OnComboDetected?.Invoke(combo.ComboName);
        }

        /// <summary>
        /// Adds a combo definition at runtime.
        /// </summary>
        public void AddCombo(string name, string[] sequence, float maxTime = 0.5f)
        {
            _combos.Add(new ComboDefinition
            {
                ComboName = name,
                Sequence = sequence,
                MaxTimeBetweenInputs = maxTime,
                OnComboExecuted = new UnityEngine.Events.UnityEvent()
            });
        }

        /// <summary>
        /// Removes a combo by name.
        /// </summary>
        public void RemoveCombo(string name)
        {
            _combos.RemoveAll(c => c.ComboName == name);
        }

        /// <summary>
        /// Clears input history.
        /// </summary>
        public void ResetHistory()
        {
            _inputHistory.Clear();
        }
    }
}
