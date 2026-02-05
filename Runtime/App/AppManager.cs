using System;
using UnityEngine;

namespace LWT.UnityWorkbench.App
{
    /// <summary>
    /// Central app lifecycle and state manager.
    /// Handles app pause, resume, focus, and quit events.
    /// </summary>
    public class AppManager : MonoBehaviour
    {
        private static AppManager _instance;
        public static AppManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<AppManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[AppManager]");
                        _instance = go.AddComponent<AppManager>();
                    }
                }
                return _instance;
            }
        }

        public static bool HasInstance => _instance != null;

        [Header("Settings")]
        [SerializeField] private bool _pauseOnFocusLost = true;
        [SerializeField] private bool _lowPowerModeOnBackground = true;
        [SerializeField] private int _backgroundTargetFrameRate = 15;

        private int _normalTargetFrameRate;
        private bool _isPaused;
        private bool _hasFocus = true;
        private float _pauseTime;
        private float _totalPausedTime;

        /// <summary>Current app state.</summary>
        public AppState State { get; private set; } = AppState.Active;

        /// <summary>Whether the app is currently paused.</summary>
        public bool IsPaused => _isPaused;

        /// <summary>Whether the app has focus.</summary>
        public bool HasFocus => _hasFocus;

        /// <summary>Time spent in paused state during this session.</summary>
        public float TotalPausedTime => _totalPausedTime;

        /// <summary>App version from PlayerSettings.</summary>
        public string Version => Application.version;

        /// <summary>Platform the app is running on.</summary>
        public RuntimePlatform Platform => Application.platform;

        /// <summary>Whether running on mobile device.</summary>
        public bool IsMobile => Application.isMobilePlatform;

        /// <summary>Whether running in editor.</summary>
        public bool IsEditor => Application.isEditor;

        // Events
        public event Action OnAppPaused;
        public event Action OnAppResumed;
        public event Action OnAppFocusGained;
        public event Action OnAppFocusLost;
        public event Action OnAppQuitting;
        public event Action<AppState, AppState> OnAppStateChanged;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            _normalTargetFrameRate = Application.targetFrameRate;
            if (_normalTargetFrameRate <= 0) _normalTargetFrameRate = 60;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                HandlePause();
            }
            else
            {
                HandleResume();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            _hasFocus = hasFocus;

            if (hasFocus)
            {
                OnAppFocusGained?.Invoke();

                if (_pauseOnFocusLost && _isPaused)
                {
                    HandleResume();
                }
            }
            else
            {
                OnAppFocusLost?.Invoke();

                if (_pauseOnFocusLost && !_isPaused)
                {
                    HandlePause();
                }
            }
        }

        private void OnApplicationQuit()
        {
            SetState(AppState.Quitting);
            OnAppQuitting?.Invoke();
        }

        private void HandlePause()
        {
            if (_isPaused) return;

            _isPaused = true;
            _pauseTime = Time.realtimeSinceStartup;

            SetState(AppState.Paused);

            if (_lowPowerModeOnBackground)
            {
                Application.targetFrameRate = _backgroundTargetFrameRate;
            }

            OnAppPaused?.Invoke();
        }

        private void HandleResume()
        {
            if (!_isPaused) return;

            _totalPausedTime += Time.realtimeSinceStartup - _pauseTime;
            _isPaused = false;

            SetState(AppState.Active);

            if (_lowPowerModeOnBackground)
            {
                Application.targetFrameRate = _normalTargetFrameRate;
            }

            OnAppResumed?.Invoke();
        }

        private void SetState(AppState newState)
        {
            if (State == newState) return;

            var oldState = State;
            State = newState;
            OnAppStateChanged?.Invoke(oldState, newState);
        }

        /// <summary>
        /// Quit the application.
        /// </summary>
        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// Open a URL in the default browser.
        /// </summary>
        public void OpenURL(string url)
        {
            Application.OpenURL(url);
        }

        /// <summary>
        /// Set the target frame rate.
        /// </summary>
        public void SetTargetFrameRate(int frameRate)
        {
            _normalTargetFrameRate = frameRate;
            if (!_isPaused)
            {
                Application.targetFrameRate = frameRate;
            }
        }

        /// <summary>
        /// Prevent the screen from dimming/sleeping.
        /// </summary>
        public void SetScreenSleepTimeout(SleepTimeout timeout)
        {
            Screen.sleepTimeout = (int)timeout;
        }
    }

    public enum AppState
    {
        Active,
        Paused,
        Background,
        Quitting
    }

    public enum SleepTimeout
    {
        NeverSleep = UnityEngine.SleepTimeout.NeverSleep,
        SystemSetting = UnityEngine.SleepTimeout.SystemSetting
    }
}
