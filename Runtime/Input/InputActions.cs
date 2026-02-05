using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace LWT.UnityWorkbench.InputHandling
{
    /// <summary>
    /// Component for easily connecting input actions to Unity Events.
    /// </summary>
    public class InputActionEvents : MonoBehaviour
    {
        [Serializable]
        public class ActionBinding
        {
            public string ActionName;
            public UnityEvent OnPressed;
            public UnityEvent OnReleased;
            public UnityEvent OnHeld;
            public UnityEvent<float> OnValue;
            public UnityEvent<Vector2> OnVector2;

            [HideInInspector] public bool wasPressed;
            [HideInInspector] public bool isHeld;
        }

        [SerializeField] private List<ActionBinding> _bindings = new List<ActionBinding>();
        [SerializeField] private bool _activeWhenPaused = false;

        private void Update()
        {
            if (!_activeWhenPaused && Time.timeScale == 0f) return;

            var inputManager = InputManager.Instance;
            if (inputManager == null || !inputManager.InputEnabled) return;

            foreach (var binding in _bindings)
            {
                if (string.IsNullOrEmpty(binding.ActionName)) continue;

                bool pressed = inputManager.WasPressed(binding.ActionName);
                bool released = inputManager.WasReleased(binding.ActionName);
                bool held = inputManager.IsHeld(binding.ActionName);

                if (pressed)
                {
                    binding.OnPressed?.Invoke();
                }

                if (released)
                {
                    binding.OnReleased?.Invoke();
                }

                if (held)
                {
                    binding.OnHeld?.Invoke();

                    float value = inputManager.GetFloat(binding.ActionName);
                    if (value != 0f)
                    {
                        binding.OnValue?.Invoke(value);
                    }

                    Vector2 vector = inputManager.GetVector2(binding.ActionName);
                    if (vector != Vector2.zero)
                    {
                        binding.OnVector2?.Invoke(vector);
                    }
                }

                binding.wasPressed = pressed;
                binding.isHeld = held;
            }
        }

        /// <summary>
        /// Adds a binding at runtime.
        /// </summary>
        public ActionBinding AddBinding(string actionName)
        {
            var binding = new ActionBinding
            {
                ActionName = actionName,
                OnPressed = new UnityEvent(),
                OnReleased = new UnityEvent(),
                OnHeld = new UnityEvent(),
                OnValue = new UnityEvent<float>(),
                OnVector2 = new UnityEvent<Vector2>()
            };
            _bindings.Add(binding);
            return binding;
        }

        /// <summary>
        /// Removes a binding.
        /// </summary>
        public void RemoveBinding(string actionName)
        {
            _bindings.RemoveAll(b => b.ActionName == actionName);
        }
    }

    /// <summary>
    /// Simple input wrapper for common game actions.
    /// </summary>
    public static class GameInput
    {
        // Common action names (customize as needed)
        public const string MOVE = "Move";
        public const string LOOK = "Look";
        public const string JUMP = "Jump";
        public const string ATTACK = "Attack";
        public const string INTERACT = "Interact";
        public const string PAUSE = "Pause";
        public const string SUBMIT = "Submit";
        public const string CANCEL = "Cancel";

        public static Vector2 Movement => InputManager.Instance?.GetVector2(MOVE) ?? Vector2.zero;
        public static Vector2 Look => InputManager.Instance?.GetVector2(LOOK) ?? Vector2.zero;

        public static bool JumpPressed => InputManager.Instance?.WasPressed(JUMP) ?? false;
        public static bool JumpHeld => InputManager.Instance?.IsHeld(JUMP) ?? false;
        public static bool JumpReleased => InputManager.Instance?.WasReleased(JUMP) ?? false;

        public static bool AttackPressed => InputManager.Instance?.WasPressed(ATTACK) ?? false;
        public static bool AttackHeld => InputManager.Instance?.IsHeld(ATTACK) ?? false;

        public static bool InteractPressed => InputManager.Instance?.WasPressed(INTERACT) ?? false;
        public static bool PausePressed => InputManager.Instance?.WasPressed(PAUSE) ?? false;
        public static bool SubmitPressed => InputManager.Instance?.WasPressed(SUBMIT) ?? false;
        public static bool CancelPressed => InputManager.Instance?.WasPressed(CANCEL) ?? false;

        /// <summary>
        /// Gets movement as a normalized Vector3 (XZ plane).
        /// </summary>
        public static Vector3 MovementXZ
        {
            get
            {
                Vector2 move = Movement;
                return new Vector3(move.x, 0f, move.y).normalized;
            }
        }

        /// <summary>
        /// Gets movement relative to a camera.
        /// </summary>
        public static Vector3 GetMovementRelativeToCamera(Camera camera)
        {
            if (camera == null) return MovementXZ;

            Vector2 input = Movement;
            if (input == Vector2.zero) return Vector3.zero;

            Vector3 forward = camera.transform.forward;
            Vector3 right = camera.transform.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return (forward * input.y + right * input.x).normalized;
        }
    }

    /// <summary>
    /// Input action wrapper with callbacks.
    /// </summary>
    public class InputActionWrapper : IDisposable
    {
        private InputAction _action;
        private Action _onPressed;
        private Action _onReleased;
        private Action<float> _onValue;
        private Action<Vector2> _onVector2;

        public string Name => _action?.name;
        public bool IsPressed => _action?.IsPressed() ?? false;

        public InputActionWrapper(InputAction action)
        {
            _action = action;

            if (_action != null)
            {
                _action.started += OnStarted;
                _action.performed += OnPerformed;
                _action.canceled += OnCanceled;
            }
        }

        public InputActionWrapper OnPressed(Action callback)
        {
            _onPressed = callback;
            return this;
        }

        public InputActionWrapper OnReleased(Action callback)
        {
            _onReleased = callback;
            return this;
        }

        public InputActionWrapper OnValue(Action<float> callback)
        {
            _onValue = callback;
            return this;
        }

        public InputActionWrapper OnVector2(Action<Vector2> callback)
        {
            _onVector2 = callback;
            return this;
        }

        private void OnStarted(InputAction.CallbackContext ctx)
        {
            _onPressed?.Invoke();
        }

        private void OnPerformed(InputAction.CallbackContext ctx)
        {
            if (_onValue != null && ctx.valueType == typeof(float))
            {
                _onValue.Invoke(ctx.ReadValue<float>());
            }
            else if (_onVector2 != null && ctx.valueType == typeof(Vector2))
            {
                _onVector2.Invoke(ctx.ReadValue<Vector2>());
            }
        }

        private void OnCanceled(InputAction.CallbackContext ctx)
        {
            _onReleased?.Invoke();
        }

        public void Enable()
        {
            _action?.Enable();
        }

        public void Disable()
        {
            _action?.Disable();
        }

        public void Dispose()
        {
            if (_action != null)
            {
                _action.started -= OnStarted;
                _action.performed -= OnPerformed;
                _action.canceled -= OnCanceled;
            }
        }
    }
}
