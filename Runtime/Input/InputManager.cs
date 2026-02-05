using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LWT.UnityWorkbench.InputHandling
{
    /// <summary>
    /// Central input management system with support for multiple control schemes.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public enum ControlScheme
        {
            KeyboardMouse,
            Gamepad,
            Touch
        }

        private static InputManager _instance;
        public static InputManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<InputManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[InputManager]");
                        _instance = go.AddComponent<InputManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Settings")]
        [SerializeField] private bool _persistAcrossScenes = true;
        [SerializeField] private PlayerInput _playerInput;

        private ControlScheme _currentScheme = ControlScheme.KeyboardMouse;
        private Dictionary<string, InputActionState> _actionStates = new Dictionary<string, InputActionState>();
        private bool _inputEnabled = true;

        public ControlScheme CurrentScheme => _currentScheme;
        public bool InputEnabled => _inputEnabled;
        public PlayerInput PlayerInput => _playerInput;

        public event Action<ControlScheme> OnControlSchemeChanged;
        public event Action<string, InputActionPhase> OnAnyAction;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            if (_persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }

            InitializeInput();
        }

        private void InitializeInput()
        {
            if (_playerInput == null)
            {
                _playerInput = GetComponent<PlayerInput>();
            }

            if (_playerInput != null)
            {
                _playerInput.onControlsChanged += OnControlsChanged;
                _playerInput.onActionTriggered += OnActionTriggered;

                // Detect initial scheme
                DetectControlScheme();
            }

            // Subscribe to device changes
            UnityEngine.InputSystem.InputSystem.onDeviceChange += OnDeviceChange;
        }

        private void OnDestroy()
        {
            if (_playerInput != null)
            {
                _playerInput.onControlsChanged -= OnControlsChanged;
                _playerInput.onActionTriggered -= OnActionTriggered;
            }

            UnityEngine.InputSystem.InputSystem.onDeviceChange -= OnDeviceChange;
        }

        private void Update()
        {
            UpdateActionStates();
        }

        private void UpdateActionStates()
        {
            foreach (var state in _actionStates.Values)
            {
                state.Update();
            }
        }

        private void OnControlsChanged(PlayerInput input)
        {
            DetectControlScheme();
        }

        private void OnActionTriggered(InputAction.CallbackContext context)
        {
            if (!_inputEnabled) return;

            string actionName = context.action.name;

            // Update action state
            if (!_actionStates.ContainsKey(actionName))
            {
                _actionStates[actionName] = new InputActionState(actionName);
            }
            _actionStates[actionName].ProcessCallback(context);

            OnAnyAction?.Invoke(actionName, context.phase);
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Added || change == InputDeviceChange.Removed)
            {
                DetectControlScheme();
            }
        }

        private void DetectControlScheme()
        {
            ControlScheme newScheme = _currentScheme;

            if (_playerInput != null && !string.IsNullOrEmpty(_playerInput.currentControlScheme))
            {
                string schemeName = _playerInput.currentControlScheme.ToLower();

                if (schemeName.Contains("gamepad") || schemeName.Contains("controller"))
                {
                    newScheme = ControlScheme.Gamepad;
                }
                else if (schemeName.Contains("touch"))
                {
                    newScheme = ControlScheme.Touch;
                }
                else
                {
                    newScheme = ControlScheme.KeyboardMouse;
                }
            }
            else
            {
                // Fallback detection
                if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
                {
                    newScheme = ControlScheme.Gamepad;
                }
                else if (Touchscreen.current != null && Touchscreen.current.wasUpdatedThisFrame)
                {
                    newScheme = ControlScheme.Touch;
                }
                else
                {
                    newScheme = ControlScheme.KeyboardMouse;
                }
            }

            if (newScheme != _currentScheme)
            {
                _currentScheme = newScheme;
                OnControlSchemeChanged?.Invoke(_currentScheme);
            }
        }

        /// <summary>
        /// Enables or disables all input.
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;

            if (_playerInput != null)
            {
                if (enabled)
                    _playerInput.ActivateInput();
                else
                    _playerInput.DeactivateInput();
            }
        }

        /// <summary>
        /// Gets whether an action was pressed this frame.
        /// </summary>
        public bool WasPressed(string actionName)
        {
            if (!_inputEnabled) return false;
            return _actionStates.TryGetValue(actionName, out var state) && state.WasPressed;
        }

        /// <summary>
        /// Gets whether an action was released this frame.
        /// </summary>
        public bool WasReleased(string actionName)
        {
            if (!_inputEnabled) return false;
            return _actionStates.TryGetValue(actionName, out var state) && state.WasReleased;
        }

        /// <summary>
        /// Gets whether an action is currently held.
        /// </summary>
        public bool IsHeld(string actionName)
        {
            if (!_inputEnabled) return false;
            return _actionStates.TryGetValue(actionName, out var state) && state.IsHeld;
        }

        /// <summary>
        /// Gets the value of a Vector2 action (e.g., movement).
        /// </summary>
        public Vector2 GetVector2(string actionName)
        {
            if (!_inputEnabled) return Vector2.zero;
            return _actionStates.TryGetValue(actionName, out var state) ? state.Vector2Value : Vector2.zero;
        }

        /// <summary>
        /// Gets the value of a float action (e.g., trigger).
        /// </summary>
        public float GetFloat(string actionName)
        {
            if (!_inputEnabled) return 0f;
            return _actionStates.TryGetValue(actionName, out var state) ? state.FloatValue : 0f;
        }

        /// <summary>
        /// Gets the duration an action has been held.
        /// </summary>
        public float GetHoldDuration(string actionName)
        {
            return _actionStates.TryGetValue(actionName, out var state) ? state.HoldDuration : 0f;
        }

        /// <summary>
        /// Switches to a different action map.
        /// </summary>
        public void SwitchActionMap(string mapName)
        {
            if (_playerInput != null)
            {
                _playerInput.SwitchCurrentActionMap(mapName);
            }
        }

        /// <summary>
        /// Gets the current action map name.
        /// </summary>
        public string GetCurrentActionMap()
        {
            return _playerInput?.currentActionMap?.name;
        }

        /// <summary>
        /// Vibrates the gamepad (if available).
        /// </summary>
        public void Vibrate(float lowFrequency, float highFrequency, float duration)
        {
            if (Gamepad.current != null)
            {
                StartCoroutine(VibrateRoutine(lowFrequency, highFrequency, duration));
            }
        }

        private System.Collections.IEnumerator VibrateRoutine(float low, float high, float duration)
        {
            Gamepad.current?.SetMotorSpeeds(low, high);
            yield return new WaitForSecondsRealtime(duration);
            Gamepad.current?.SetMotorSpeeds(0f, 0f);
        }

        /// <summary>
        /// Stops all gamepad vibration.
        /// </summary>
        public void StopVibration()
        {
            Gamepad.current?.SetMotorSpeeds(0f, 0f);
        }
    }

    /// <summary>
    /// Tracks the state of a single input action.
    /// </summary>
    public class InputActionState
    {
        public string ActionName { get; }
        public bool WasPressed { get; private set; }
        public bool WasReleased { get; private set; }
        public bool IsHeld { get; private set; }
        public Vector2 Vector2Value { get; private set; }
        public float FloatValue { get; private set; }
        public float HoldDuration { get; private set; }

        private float _pressTime;
        private bool _wasHeldLastFrame;

        public InputActionState(string actionName)
        {
            ActionName = actionName;
        }

        public void ProcessCallback(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    WasPressed = true;
                    IsHeld = true;
                    _pressTime = Time.unscaledTime;
                    break;

                case InputActionPhase.Performed:
                    IsHeld = true;
                    ReadValue(context);
                    break;

                case InputActionPhase.Canceled:
                    WasReleased = true;
                    IsHeld = false;
                    Vector2Value = Vector2.zero;
                    FloatValue = 0f;
                    break;
            }
        }

        private void ReadValue(InputAction.CallbackContext context)
        {
            var valueType = context.valueType;

            if (valueType == typeof(Vector2))
            {
                Vector2Value = context.ReadValue<Vector2>();
                FloatValue = Vector2Value.magnitude;
            }
            else if (valueType == typeof(float))
            {
                FloatValue = context.ReadValue<float>();
            }
            else if (valueType == typeof(bool))
            {
                FloatValue = context.ReadValue<bool>() ? 1f : 0f;
            }
        }

        public void Update()
        {
            // Clear frame-specific states
            WasPressed = false;
            WasReleased = false;

            // Update hold duration
            if (IsHeld)
            {
                HoldDuration = Time.unscaledTime - _pressTime;
            }
            else
            {
                HoldDuration = 0f;
            }

            _wasHeldLastFrame = IsHeld;
        }
    }
}
