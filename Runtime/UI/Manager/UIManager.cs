using System;
using System.Collections.Generic;
using UnityEngine;
using LWT.UnityWorkbench.Core;

namespace LWT.UnityWorkbench.UI
{
    /// <summary>
    /// Central manager for UI screens.
    /// Provides stack-based navigation and screen lifecycle management.
    /// </summary>
    public class UIManager : PersistentMonoSingleton<UIManager>, IUIManager
    {
        [Header("References")]
        [SerializeField] private Transform _screenContainer;
        [SerializeField] private Canvas _rootCanvas;

        [Header("Screen Configs")]
        [SerializeField] private List<UIScreenConfig> _screenConfigs = new List<UIScreenConfig>();

        [Header("Settings")]
        [SerializeField] private bool _handleEscapeKey = true;
        [SerializeField] private bool _createCanvasIfMissing = true;

        private readonly Stack<UIScreen> _screenStack = new Stack<UIScreen>();
        private readonly Dictionary<string, UIScreen> _screenPool = new Dictionary<string, UIScreen>();
        private readonly Dictionary<string, UIScreenConfig> _configLookup = new Dictionary<string, UIScreenConfig>();
        private readonly Dictionary<Type, string> _typeLookup = new Dictionary<Type, string>();

        public UIScreen CurrentScreen => _screenStack.Count > 0 ? _screenStack.Peek() : null;
        public int StackCount => _screenStack.Count;
        public bool CanGoBack => _screenStack.Count > 1;

        public event Action<UIScreen> OnScreenShown;
        public event Action<UIScreen> OnScreenHidden;

        protected override void OnSingletonAwake()
        {
            InitializeConfigLookup();
            EnsureCanvas();
        }

        private void Update()
        {
            if (_handleEscapeKey && Input.GetKeyDown(KeyCode.Escape))
            {
                HandleEscapePressed();
            }

            CurrentScreen?.OnScreenUpdate();
        }

        private void InitializeConfigLookup()
        {
            _configLookup.Clear();
            _typeLookup.Clear();

            foreach (var config in _screenConfigs)
            {
                if (config != null && config.IsValid)
                {
                    _configLookup[config.ScreenName] = config;

                    // Cache type lookup
                    var screen = config.ScreenPrefab.GetComponent<UIScreen>();
                    if (screen != null)
                    {
                        _typeLookup[screen.GetType()] = config.ScreenName;
                    }
                }
            }
        }

        private void EnsureCanvas()
        {
            if (_rootCanvas == null && _createCanvasIfMissing)
            {
                var canvasGo = new GameObject("[UICanvas]");
                canvasGo.transform.SetParent(transform);

                _rootCanvas = canvasGo.AddComponent<Canvas>();
                _rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _rootCanvas.sortingOrder = 100;

                canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            if (_screenContainer == null && _rootCanvas != null)
            {
                var containerGo = new GameObject("ScreenContainer");
                containerGo.transform.SetParent(_rootCanvas.transform);

                var rect = containerGo.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                _screenContainer = containerGo.transform;
            }
        }

        #region Public Methods

        /// <summary>
        /// Show a screen by type.
        /// </summary>
        public void ShowScreen<T>(bool hideCurrentScreen = true) where T : UIScreen
        {
            if (_typeLookup.TryGetValue(typeof(T), out string screenName))
            {
                ShowScreen(screenName, hideCurrentScreen);
            }
            else
            {
                Debug.LogError($"[UIManager] Screen type {typeof(T).Name} not found in configs.");
            }
        }

        /// <summary>
        /// Show a screen by name.
        /// </summary>
        public void ShowScreen(string screenName, bool hideCurrentScreen = true)
        {
            if (string.IsNullOrEmpty(screenName))
            {
                Debug.LogError("[UIManager] Screen name cannot be null or empty.");
                return;
            }

            if (!_configLookup.TryGetValue(screenName, out UIScreenConfig config))
            {
                Debug.LogError($"[UIManager] Screen config not found: {screenName}");
                return;
            }

            // Hide current screen if needed
            if (hideCurrentScreen && CurrentScreen != null)
            {
                CurrentScreen.OnLoseFocus();
                CurrentScreen.Hide();
            }

            // Get or create screen instance
            UIScreen screen = GetOrCreateScreen(screenName, config);
            if (screen == null) return;

            // Push to stack and show
            _screenStack.Push(screen);
            screen.transform.SetAsLastSibling();
            screen.Show();
            screen.OnFocus();

            OnScreenShown?.Invoke(screen);

            // Fire event
            EventBus<ScreenShownEvent>.Publish(new ScreenShownEvent { ScreenName = screenName });
        }

        /// <summary>
        /// Hide the current screen without popping from stack.
        /// </summary>
        public void HideCurrentScreen()
        {
            if (CurrentScreen != null)
            {
                CurrentScreen.Hide();
            }
        }

        /// <summary>
        /// Go back to previous screen.
        /// </summary>
        public void GoBack()
        {
            if (!CanGoBack) return;

            // Pop and hide current screen
            var currentScreen = _screenStack.Pop();
            currentScreen.OnLoseFocus();
            currentScreen.Hide();

            OnScreenHidden?.Invoke(currentScreen);
            EventBus<ScreenHiddenEvent>.Publish(new ScreenHiddenEvent { ScreenName = currentScreen.ScreenName });

            // Show previous screen
            if (CurrentScreen != null)
            {
                CurrentScreen.Show();
                CurrentScreen.OnFocus();
            }
        }

        /// <summary>
        /// Clear all screens except the bottom one.
        /// </summary>
        public void ClearToRoot()
        {
            while (_screenStack.Count > 1)
            {
                var screen = _screenStack.Pop();
                screen.OnLoseFocus();
                screen.Hide(instant: true);
                OnScreenHidden?.Invoke(screen);
            }

            if (CurrentScreen != null)
            {
                CurrentScreen.Show();
                CurrentScreen.OnFocus();
            }
        }

        /// <summary>
        /// Clear all screens.
        /// </summary>
        public void ClearAll()
        {
            while (_screenStack.Count > 0)
            {
                var screen = _screenStack.Pop();
                screen.OnLoseFocus();
                screen.Hide(instant: true);
                OnScreenHidden?.Invoke(screen);
            }
        }

        /// <summary>
        /// Register a screen config at runtime.
        /// </summary>
        public void RegisterScreenConfig(UIScreenConfig config)
        {
            if (config == null || !config.IsValid) return;

            _configLookup[config.ScreenName] = config;

            var screen = config.ScreenPrefab.GetComponent<UIScreen>();
            if (screen != null)
            {
                _typeLookup[screen.GetType()] = config.ScreenName;
            }
        }

        /// <summary>
        /// Check if a screen is currently showing.
        /// </summary>
        public bool IsScreenShowing(string screenName)
        {
            foreach (var screen in _screenStack)
            {
                if (screen.ScreenName == screenName)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get a screen instance if it exists in pool.
        /// </summary>
        public T GetScreen<T>() where T : UIScreen
        {
            if (_typeLookup.TryGetValue(typeof(T), out string screenName))
            {
                if (_screenPool.TryGetValue(screenName, out UIScreen screen))
                {
                    return screen as T;
                }
            }
            return null;
        }

        #endregion

        #region Private Methods

        private UIScreen GetOrCreateScreen(string screenName, UIScreenConfig config)
        {
            // Check pool first
            if (_screenPool.TryGetValue(screenName, out UIScreen pooledScreen))
            {
                return pooledScreen;
            }

            // Create new instance
            if (config.ScreenPrefab == null)
            {
                Debug.LogError($"[UIManager] Screen prefab is null for: {screenName}");
                return null;
            }

            var instance = Instantiate(config.ScreenPrefab, _screenContainer);
            var screen = instance.GetComponent<UIScreen>();

            if (screen == null)
            {
                Debug.LogError($"[UIManager] Prefab does not have UIScreen component: {screenName}");
                Destroy(instance);
                return null;
            }

            screen.Initialize();

            // Add to pool if configured
            if (config.PoolOnHide)
            {
                _screenPool[screenName] = screen;
            }

            return screen;
        }

        private void HandleEscapePressed()
        {
            if (CurrentScreen == null) return;

            // Let the screen handle it first
            if (CurrentScreen.OnBackPressed()) return;

            // If screen wants to close on escape and we can go back
            if (CurrentScreen.CloseOnEscape && CanGoBack)
            {
                GoBack();
            }
        }

        #endregion
    }

    #region Events

    /// <summary>
    /// Event fired when a screen is shown.
    /// </summary>
    public struct ScreenShownEvent : IEvent
    {
        public string ScreenName;
    }

    /// <summary>
    /// Event fired when a screen is hidden.
    /// </summary>
    public struct ScreenHiddenEvent : IEvent
    {
        public string ScreenName;
    }

    #endregion
}
