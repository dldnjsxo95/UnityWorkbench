using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LWT.UnityWorkbench.InputHandling
{
    /// <summary>
    /// Runtime key rebinding system.
    /// </summary>
    public class InputRebinding : MonoBehaviour
    {
        private const string REBIND_PREFS_KEY = "InputRebindings";

        [Header("Settings")]
        [SerializeField] private InputActionAsset _inputActionAsset;
        [SerializeField] private bool _saveToPlayerPrefs = true;
        [SerializeField] private bool _loadOnAwake = true;

        [Header("Rebinding")]
        [SerializeField] private string[] _excludeBindings = { "<Mouse>/position", "<Mouse>/delta" };

        private InputActionRebindingExtensions.RebindingOperation _rebindOperation;
        private Dictionary<string, string> _overrides = new Dictionary<string, string>();

        public bool IsRebinding => _rebindOperation != null;
        public InputActionAsset ActionAsset => _inputActionAsset;

        public event Action<InputAction, int> OnRebindStarted;
        public event Action<InputAction, int> OnRebindComplete;
        public event Action<InputAction, int> OnRebindCanceled;

        private void Awake()
        {
            if (_loadOnAwake && _inputActionAsset != null)
            {
                LoadRebindings();
            }
        }

        private void OnDestroy()
        {
            _rebindOperation?.Dispose();
        }

        /// <summary>
        /// Starts an interactive rebind for an action.
        /// </summary>
        public void StartRebind(InputAction action, int bindingIndex = 0, Action onComplete = null)
        {
            if (action == null || IsRebinding) return;

            // Disable action during rebind
            action.Disable();

            _rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("<Mouse>/position")
                .WithControlsExcluding("<Mouse>/delta")
                .WithControlsExcluding("<Gamepad>/Start")
                .WithControlsExcluding("<Keyboard>/escape")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation =>
                {
                    CompleteRebind(action, bindingIndex);
                    onComplete?.Invoke();
                })
                .OnCancel(operation =>
                {
                    CancelRebind(action, bindingIndex);
                });

            foreach (var exclude in _excludeBindings)
            {
                _rebindOperation.WithControlsExcluding(exclude);
            }

            OnRebindStarted?.Invoke(action, bindingIndex);
            _rebindOperation.Start();
        }

        /// <summary>
        /// Starts rebind for an action by name.
        /// </summary>
        public void StartRebind(string actionName, int bindingIndex = 0, Action onComplete = null)
        {
            if (_inputActionAsset == null) return;

            var action = _inputActionAsset.FindAction(actionName);
            if (action != null)
            {
                StartRebind(action, bindingIndex, onComplete);
            }
        }

        /// <summary>
        /// Cancels the current rebind operation.
        /// </summary>
        public void CancelCurrentRebind()
        {
            _rebindOperation?.Cancel();
        }

        private void CompleteRebind(InputAction action, int bindingIndex)
        {
            string key = GetBindingKey(action, bindingIndex);
            _overrides[key] = action.bindings[bindingIndex].overridePath;

            _rebindOperation?.Dispose();
            _rebindOperation = null;

            action.Enable();

            if (_saveToPlayerPrefs)
            {
                SaveRebindings();
            }

            OnRebindComplete?.Invoke(action, bindingIndex);
        }

        private void CancelRebind(InputAction action, int bindingIndex)
        {
            _rebindOperation?.Dispose();
            _rebindOperation = null;

            action.Enable();

            OnRebindCanceled?.Invoke(action, bindingIndex);
        }

        /// <summary>
        /// Resets a specific binding to default.
        /// </summary>
        public void ResetBinding(InputAction action, int bindingIndex = 0)
        {
            if (action == null) return;

            action.RemoveBindingOverride(bindingIndex);

            string key = GetBindingKey(action, bindingIndex);
            _overrides.Remove(key);

            if (_saveToPlayerPrefs)
            {
                SaveRebindings();
            }
        }

        /// <summary>
        /// Resets all bindings to default.
        /// </summary>
        public void ResetAllBindings()
        {
            if (_inputActionAsset == null) return;

            foreach (var map in _inputActionAsset.actionMaps)
            {
                map.RemoveAllBindingOverrides();
            }

            _overrides.Clear();

            if (_saveToPlayerPrefs)
            {
                PlayerPrefs.DeleteKey(REBIND_PREFS_KEY);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Saves current rebindings to PlayerPrefs.
        /// </summary>
        public void SaveRebindings()
        {
            if (_inputActionAsset == null) return;

            string json = _inputActionAsset.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(REBIND_PREFS_KEY, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Loads rebindings from PlayerPrefs.
        /// </summary>
        public void LoadRebindings()
        {
            if (_inputActionAsset == null) return;

            string json = PlayerPrefs.GetString(REBIND_PREFS_KEY, string.Empty);
            if (!string.IsNullOrEmpty(json))
            {
                _inputActionAsset.LoadBindingOverridesFromJson(json);
            }
        }

        /// <summary>
        /// Gets the display string for a binding.
        /// </summary>
        public string GetBindingDisplayString(InputAction action, int bindingIndex = 0,
            InputBinding.DisplayStringOptions options = InputBinding.DisplayStringOptions.DontUseShortDisplayNames)
        {
            if (action == null || bindingIndex >= action.bindings.Count)
                return string.Empty;

            return action.GetBindingDisplayString(bindingIndex, options);
        }

        /// <summary>
        /// Gets the display string for a binding by action name.
        /// </summary>
        public string GetBindingDisplayString(string actionName, int bindingIndex = 0)
        {
            if (_inputActionAsset == null) return string.Empty;

            var action = _inputActionAsset.FindAction(actionName);
            return GetBindingDisplayString(action, bindingIndex);
        }

        /// <summary>
        /// Gets the binding index for a specific control scheme.
        /// </summary>
        public int GetBindingIndex(InputAction action, string controlScheme)
        {
            if (action == null) return -1;

            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                if (binding.groups.Contains(controlScheme))
                {
                    return i;
                }
            }

            return 0;
        }

        /// <summary>
        /// Checks if a binding has been overridden.
        /// </summary>
        public bool HasOverride(InputAction action, int bindingIndex = 0)
        {
            if (action == null || bindingIndex >= action.bindings.Count)
                return false;

            return !string.IsNullOrEmpty(action.bindings[bindingIndex].overridePath);
        }

        private string GetBindingKey(InputAction action, int bindingIndex)
        {
            return $"{action.actionMap.name}/{action.name}/{bindingIndex}";
        }

        /// <summary>
        /// Gets all actions that can be rebound.
        /// </summary>
        public IEnumerable<InputAction> GetRebindableActions()
        {
            if (_inputActionAsset == null) yield break;

            foreach (var map in _inputActionAsset.actionMaps)
            {
                foreach (var action in map.actions)
                {
                    yield return action;
                }
            }
        }
    }

    /// <summary>
    /// UI component for displaying and rebinding a single action.
    /// </summary>
    public class RebindUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InputRebinding _rebinding;
        [SerializeField] private string _actionName;
        [SerializeField] private int _bindingIndex = 0;

        [Header("UI")]
        [SerializeField] private UnityEngine.UI.Text _bindingText;
        [SerializeField] private UnityEngine.UI.Button _rebindButton;
        [SerializeField] private UnityEngine.UI.Button _resetButton;
        [SerializeField] private GameObject _waitingForInput;

        private InputAction _action;

        private void Start()
        {
            if (_rebinding == null)
            {
                _rebinding = FindFirstObjectByType<InputRebinding>();
            }

            if (_rebinding != null && _rebinding.ActionAsset != null)
            {
                _action = _rebinding.ActionAsset.FindAction(_actionName);
            }

            SetupButtons();
            UpdateDisplay();

            if (_rebinding != null)
            {
                _rebinding.OnRebindStarted += OnRebindStarted;
                _rebinding.OnRebindComplete += OnRebindComplete;
                _rebinding.OnRebindCanceled += OnRebindCanceled;
            }
        }

        private void OnDestroy()
        {
            if (_rebinding != null)
            {
                _rebinding.OnRebindStarted -= OnRebindStarted;
                _rebinding.OnRebindComplete -= OnRebindComplete;
                _rebinding.OnRebindCanceled -= OnRebindCanceled;
            }
        }

        private void SetupButtons()
        {
            if (_rebindButton != null)
            {
                _rebindButton.onClick.AddListener(StartRebind);
            }

            if (_resetButton != null)
            {
                _resetButton.onClick.AddListener(ResetBinding);
            }
        }

        public void StartRebind()
        {
            if (_rebinding == null || _action == null) return;

            _rebinding.StartRebind(_action, _bindingIndex, UpdateDisplay);
        }

        public void ResetBinding()
        {
            if (_rebinding == null || _action == null) return;

            _rebinding.ResetBinding(_action, _bindingIndex);
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_bindingText != null && _rebinding != null)
            {
                _bindingText.text = _rebinding.GetBindingDisplayString(_action, _bindingIndex);
            }

            if (_waitingForInput != null)
            {
                _waitingForInput.SetActive(false);
            }
        }

        private void OnRebindStarted(InputAction action, int index)
        {
            if (action != _action || index != _bindingIndex) return;

            if (_bindingText != null)
            {
                _bindingText.text = "Press any key...";
            }

            if (_waitingForInput != null)
            {
                _waitingForInput.SetActive(true);
            }
        }

        private void OnRebindComplete(InputAction action, int index)
        {
            if (action != _action || index != _bindingIndex) return;
            UpdateDisplay();
        }

        private void OnRebindCanceled(InputAction action, int index)
        {
            if (action != _action || index != _bindingIndex) return;
            UpdateDisplay();
        }
    }
}
