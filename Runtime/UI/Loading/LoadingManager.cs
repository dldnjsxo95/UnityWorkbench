using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using LWT.UnityWorkbench.Core;

namespace LWT.UnityWorkbench.UI
{
    /// <summary>
    /// Central manager for loading screens and async scene loading.
    /// Provides convenient methods for scene transitions with loading feedback.
    /// </summary>
    public class LoadingManager : PersistentMonoSingleton<LoadingManager>
    {
        [Header("Loading Screen")]
        [SerializeField] private LoadingScreen _defaultLoadingScreen;
        [SerializeField] private bool _createDefaultIfMissing = true;

        [Header("Settings")]
        [SerializeField] private float _minimumLoadingTime = 0.5f;
        [SerializeField] private bool _allowSceneActivation = true;
        [SerializeField] private float _progressActivationThreshold = 0.9f;

        private ILoadingScreen _currentLoadingScreen;
        private bool _isLoading;
        private float _currentProgress;

        /// <summary>
        /// Whether a loading operation is in progress.
        /// </summary>
        public bool IsLoading => _isLoading;

        /// <summary>
        /// Current loading progress (0-1).
        /// </summary>
        public float Progress => _currentProgress;

        /// <summary>
        /// Event fired when loading starts.
        /// </summary>
        public event Action<string> OnLoadingStarted;

        /// <summary>
        /// Event fired when progress updates.
        /// </summary>
        public event Action<float> OnProgressChanged;

        /// <summary>
        /// Event fired when loading completes.
        /// </summary>
        public event Action<string> OnLoadingCompleted;

        protected override void OnSingletonAwake()
        {
            if (_defaultLoadingScreen == null && _createDefaultIfMissing)
            {
                CreateDefaultLoadingScreen();
            }

            _currentLoadingScreen = _defaultLoadingScreen;
        }

        #region Public Methods

        /// <summary>
        /// Load a scene asynchronously with loading screen.
        /// </summary>
        public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (_isLoading)
            {
                Debug.LogWarning("[LoadingManager] Already loading a scene.");
                return;
            }

            StartCoroutine(LoadSceneAsync(sceneName, mode));
        }

        /// <summary>
        /// Load a scene by build index asynchronously with loading screen.
        /// </summary>
        public void LoadScene(int buildIndex, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (_isLoading)
            {
                Debug.LogWarning("[LoadingManager] Already loading a scene.");
                return;
            }

            StartCoroutine(LoadSceneByIndexAsync(buildIndex, mode));
        }

        /// <summary>
        /// Unload a scene asynchronously.
        /// </summary>
        public void UnloadScene(string sceneName)
        {
            StartCoroutine(UnloadSceneAsync(sceneName));
        }

        /// <summary>
        /// Show loading screen while performing a custom operation.
        /// </summary>
        public void ShowLoadingWhile(Action<Action<float>> operation, Action onComplete = null)
        {
            if (_isLoading)
            {
                Debug.LogWarning("[LoadingManager] Already loading.");
                return;
            }

            StartCoroutine(CustomLoadingOperation(operation, onComplete));
        }

        /// <summary>
        /// Set a custom loading screen.
        /// </summary>
        public void SetLoadingScreen(ILoadingScreen loadingScreen)
        {
            _currentLoadingScreen = loadingScreen ?? _defaultLoadingScreen;
        }

        /// <summary>
        /// Reset to default loading screen.
        /// </summary>
        public void ResetLoadingScreen()
        {
            _currentLoadingScreen = _defaultLoadingScreen;
        }

        /// <summary>
        /// Manually show the loading screen.
        /// </summary>
        public void ShowLoadingScreen(string message = null)
        {
            _currentLoadingScreen?.Show();

            if (!string.IsNullOrEmpty(message))
            {
                _currentLoadingScreen?.SetMessage(message);
            }
        }

        /// <summary>
        /// Manually hide the loading screen.
        /// </summary>
        public void HideLoadingScreen()
        {
            _currentLoadingScreen?.Hide();
        }

        /// <summary>
        /// Update loading progress manually.
        /// </summary>
        public void UpdateProgress(float progress, string message = null)
        {
            _currentProgress = progress;
            _currentLoadingScreen?.SetProgress(progress);

            if (!string.IsNullOrEmpty(message))
            {
                _currentLoadingScreen?.SetMessage(message);
            }

            OnProgressChanged?.Invoke(progress);
        }

        #endregion

        #region Coroutines

        private IEnumerator LoadSceneAsync(string sceneName, LoadSceneMode mode)
        {
            _isLoading = true;
            _currentProgress = 0f;

            // Show loading screen
            _currentLoadingScreen?.Show();
            _currentLoadingScreen?.SetProgress(0f);
            _currentLoadingScreen?.SetMessage($"Loading {sceneName}...");

            // Fire events
            OnLoadingStarted?.Invoke(sceneName);
            EventBus<LoadingStartedEvent>.Publish(new LoadingStartedEvent { TargetScene = sceneName });

            float startTime = Time.realtimeSinceStartup;

            // Start async loading
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);
            operation.allowSceneActivation = false;

            // Wait for loading
            while (!operation.isDone)
            {
                // Unity async operation progress goes from 0 to 0.9, then jumps to 1 when activated
                float progress = Mathf.Clamp01(operation.progress / _progressActivationThreshold);
                UpdateProgress(progress);

                EventBus<LoadingProgressEvent>.Publish(new LoadingProgressEvent { Progress = progress });

                // Check if loading is complete (0.9 is the max before activation)
                if (operation.progress >= _progressActivationThreshold)
                {
                    // Ensure minimum loading time has passed
                    float elapsed = Time.realtimeSinceStartup - startTime;
                    if (elapsed < _minimumLoadingTime)
                    {
                        yield return new WaitForSecondsRealtime(_minimumLoadingTime - elapsed);
                    }

                    UpdateProgress(1f);

                    if (_allowSceneActivation)
                    {
                        operation.allowSceneActivation = true;
                    }
                }

                yield return null;
            }

            // Complete
            OnLoadingComplete(sceneName);
        }

        private IEnumerator LoadSceneByIndexAsync(int buildIndex, LoadSceneMode mode)
        {
            string sceneName = SceneUtility.GetScenePathByBuildIndex(buildIndex);
            sceneName = System.IO.Path.GetFileNameWithoutExtension(sceneName);

            yield return LoadSceneAsync(sceneName, mode);
        }

        private IEnumerator UnloadSceneAsync(string sceneName)
        {
            AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneName);

            while (!operation.isDone)
            {
                yield return null;
            }
        }

        private IEnumerator CustomLoadingOperation(Action<Action<float>> operation, Action onComplete)
        {
            _isLoading = true;
            _currentProgress = 0f;

            _currentLoadingScreen?.Show();
            _currentLoadingScreen?.SetProgress(0f);

            float startTime = Time.realtimeSinceStartup;
            bool operationComplete = false;

            // Run the operation with progress callback
            operation?.Invoke(progress =>
            {
                _currentProgress = progress;
                _currentLoadingScreen?.SetProgress(progress);
                OnProgressChanged?.Invoke(progress);

                if (progress >= 1f)
                {
                    operationComplete = true;
                }
            });

            // Wait for operation to complete
            while (!operationComplete)
            {
                yield return null;
            }

            // Ensure minimum loading time
            float elapsed = Time.realtimeSinceStartup - startTime;
            if (elapsed < _minimumLoadingTime)
            {
                yield return new WaitForSecondsRealtime(_minimumLoadingTime - elapsed);
            }

            _currentLoadingScreen?.Hide();
            _isLoading = false;

            onComplete?.Invoke();
        }

        #endregion

        #region Private Methods

        private void CreateDefaultLoadingScreen()
        {
            var go = new GameObject("[DefaultLoadingScreen]");
            go.transform.SetParent(transform);

            // Create canvas
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            go.AddComponent<UnityEngine.UI.CanvasScaler>();
            go.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create background
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(go.transform);

            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            var bgImage = bgGo.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);

            // Add loading screen component
            _defaultLoadingScreen = go.AddComponent<LoadingScreen>();
        }

        private void OnLoadingComplete(string sceneName)
        {
            _currentLoadingScreen?.Hide();
            _isLoading = false;

            OnLoadingCompleted?.Invoke(sceneName);
            EventBus<LoadingCompletedEvent>.Publish(new LoadingCompletedEvent { LoadedScene = sceneName });
        }

        #endregion
    }

    #region Events

    /// <summary>
    /// Event fired when loading starts.
    /// </summary>
    public struct LoadingStartedEvent : IEvent
    {
        public string TargetScene;
    }

    /// <summary>
    /// Event fired when loading progress updates.
    /// </summary>
    public struct LoadingProgressEvent : IEvent
    {
        public float Progress;
    }

    /// <summary>
    /// Event fired when loading completes.
    /// </summary>
    public struct LoadingCompletedEvent : IEvent
    {
        public string LoadedScene;
    }

    #endregion
}
