using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace LWT.UnityWorkbench.UI
{
    /// <summary>
    /// Handles keyboard/gamepad navigation for menu systems.
    /// Supports Input System for input handling.
    /// </summary>
    public class MenuNavigator : MonoBehaviour
    {
        [Header("Menu Groups")]
        [SerializeField] private MenuGroup _rootMenuGroup;
        [SerializeField] private bool _autoFindMenuGroup = true;

        [Header("Input Settings")]
        [SerializeField] private bool _useInputSystem = true;
        [SerializeField] private InputActionReference _navigateAction;
        [SerializeField] private InputActionReference _submitAction;
        [SerializeField] private InputActionReference _cancelAction;

        [Header("Legacy Input")]
        [SerializeField] private bool _useLegacyInput = true;
        [SerializeField] private KeyCode _submitKey = KeyCode.Return;
        [SerializeField] private KeyCode _cancelKey = KeyCode.Escape;

        [Header("Input Repeat")]
        [SerializeField] private float _inputRepeatDelay = 0.5f;
        [SerializeField] private float _inputRepeatRate = 0.1f;

        [Header("Events")]
        [SerializeField] private UnityEvent _onNavigate;
        [SerializeField] private UnityEvent _onSubmit;
        [SerializeField] private UnityEvent _onCancel;

        private MenuGroup _currentMenuGroup;
        private Vector2 _lastNavigationInput;
        private float _nextRepeatTime;
        private bool _isRepeating;

        /// <summary>
        /// Current active menu group.
        /// </summary>
        public MenuGroup CurrentMenuGroup => _currentMenuGroup;

        /// <summary>
        /// Currently selected button.
        /// </summary>
        public MenuButton CurrentButton => _currentMenuGroup?.CurrentButton;

        /// <summary>
        /// Event fired when navigation occurs.
        /// </summary>
        public event Action<Vector2> OnNavigateEvent;

        /// <summary>
        /// Event fired when submit is pressed.
        /// </summary>
        public event Action OnSubmitEvent;

        /// <summary>
        /// Event fired when cancel is pressed.
        /// </summary>
        public event Action OnCancelEvent;

        private void Awake()
        {
            if (_autoFindMenuGroup && _rootMenuGroup == null)
            {
                _rootMenuGroup = GetComponentInChildren<MenuGroup>();
            }

            _currentMenuGroup = _rootMenuGroup;
        }

        private void OnEnable()
        {
            // Enable Input System actions
            if (_useInputSystem)
            {
                if (_navigateAction != null && _navigateAction.action != null)
                {
                    _navigateAction.action.Enable();
                    _navigateAction.action.performed += OnNavigatePerformed;
                    _navigateAction.action.canceled += OnNavigateCanceled;
                }

                if (_submitAction != null && _submitAction.action != null)
                {
                    _submitAction.action.Enable();
                    _submitAction.action.performed += OnSubmitPerformed;
                }

                if (_cancelAction != null && _cancelAction.action != null)
                {
                    _cancelAction.action.Enable();
                    _cancelAction.action.performed += OnCancelPerformed;
                }
            }
        }

        private void OnDisable()
        {
            // Disable Input System actions
            if (_useInputSystem)
            {
                if (_navigateAction != null && _navigateAction.action != null)
                {
                    _navigateAction.action.performed -= OnNavigatePerformed;
                    _navigateAction.action.canceled -= OnNavigateCanceled;
                }

                if (_submitAction != null && _submitAction.action != null)
                {
                    _submitAction.action.performed -= OnSubmitPerformed;
                }

                if (_cancelAction != null && _cancelAction.action != null)
                {
                    _cancelAction.action.performed -= OnCancelPerformed;
                }
            }
        }

        private void Update()
        {
            // Handle legacy input
            if (_useLegacyInput)
            {
                HandleLegacyInput();
            }

            // Handle input repeat
            HandleInputRepeat();
        }

        #region Public Methods

        /// <summary>
        /// Navigate in the specified direction.
        /// </summary>
        public void Navigate(Vector2 direction)
        {
            if (_currentMenuGroup == null) return;

            _currentMenuGroup.Navigate(direction);
            _onNavigate?.Invoke();
            OnNavigateEvent?.Invoke(direction);
        }

        /// <summary>
        /// Submit/confirm the current selection.
        /// </summary>
        public void Submit()
        {
            var button = CurrentButton;
            if (button != null && button.IsNavigable)
            {
                button.OnSubmit();
            }

            _onSubmit?.Invoke();
            OnSubmitEvent?.Invoke();
        }

        /// <summary>
        /// Cancel/back action.
        /// </summary>
        public void Cancel()
        {
            // Let current button handle first
            var button = CurrentButton;
            if (button != null && button is IMenuNavigable navigable)
            {
                if (navigable.OnCancel())
                    return;
            }

            _onCancel?.Invoke();
            OnCancelEvent?.Invoke();
        }

        /// <summary>
        /// Set the active menu group.
        /// </summary>
        public void SetMenuGroup(MenuGroup group)
        {
            _currentMenuGroup?.ClearSelection();
            _currentMenuGroup = group;
            _currentMenuGroup?.SelectFirstNavigable();
        }

        /// <summary>
        /// Select a specific button.
        /// </summary>
        public void SelectButton(MenuButton button)
        {
            if (button == null) return;

            // Find the group containing this button
            var group = button.GetComponentInParent<MenuGroup>();
            if (group != null && group != _currentMenuGroup)
            {
                SetMenuGroup(group);
            }

            _currentMenuGroup?.SelectButton(button);
        }

        /// <summary>
        /// Clear current selection.
        /// </summary>
        public void ClearSelection()
        {
            _currentMenuGroup?.ClearSelection();
        }

        /// <summary>
        /// Reset to root menu group.
        /// </summary>
        public void ResetToRoot()
        {
            SetMenuGroup(_rootMenuGroup);
        }

        #endregion

        #region Input Handlers

        private void OnNavigatePerformed(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            ProcessNavigationInput(input);
        }

        private void OnNavigateCanceled(InputAction.CallbackContext context)
        {
            _lastNavigationInput = Vector2.zero;
            _isRepeating = false;
        }

        private void OnSubmitPerformed(InputAction.CallbackContext context)
        {
            Submit();
        }

        private void OnCancelPerformed(InputAction.CallbackContext context)
        {
            Cancel();
        }

        private void HandleLegacyInput()
        {
            // Navigation
            Vector2 input = Vector2.zero;

            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
                input.y = 1f;
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
                input.y = -1f;

            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                input.x = 1f;
            else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                input.x = -1f;

            if (input != Vector2.zero)
            {
                ProcessNavigationInput(input);
            }
            else
            {
                _lastNavigationInput = Vector2.zero;
                _isRepeating = false;
            }

            // Submit
            if (Input.GetKeyDown(_submitKey))
            {
                Submit();
            }

            // Cancel
            if (Input.GetKeyDown(_cancelKey))
            {
                Cancel();
            }
        }

        private void ProcessNavigationInput(Vector2 input)
        {
            // Initial press
            if (_lastNavigationInput == Vector2.zero && input != Vector2.zero)
            {
                Navigate(input);
                _nextRepeatTime = Time.unscaledTime + _inputRepeatDelay;
                _isRepeating = false;
            }

            _lastNavigationInput = input;
        }

        private void HandleInputRepeat()
        {
            if (_lastNavigationInput == Vector2.zero) return;

            if (Time.unscaledTime >= _nextRepeatTime)
            {
                Navigate(_lastNavigationInput);
                _nextRepeatTime = Time.unscaledTime + (_isRepeating ? _inputRepeatRate : _inputRepeatDelay);
                _isRepeating = true;
            }
        }

        #endregion
    }
}
