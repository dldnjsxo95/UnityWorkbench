using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LWT.UnityWorkbench.SceneManagement
{
    /// <summary>
    /// Central scene loading manager with async loading and transition support.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        private static SceneLoader _instance;
        public static SceneLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SceneLoader>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[SceneLoader]");
                        _instance = go.AddComponent<SceneLoader>();
                    }
                }
                return _instance;
            }
        }

        [Header("Settings")]
        [SerializeField] private float _minimumLoadTime = 0.5f;
        [SerializeField] private bool _allowSceneActivation = true;
        [SerializeField] private string _loadingSceneName = "Loading";

        [Header("Transitions")]
        [SerializeField] private SceneTransition _defaultTransition;

        private bool _isLoading;
        private float _loadProgress;
        private AsyncOperation _currentOperation;
        private string _currentSceneName;
        private Stack<string> _sceneHistory = new Stack<string>();

        public bool IsLoading => _isLoading;
        public float LoadProgress => _loadProgress;
        public string CurrentSceneName => _currentSceneName;
        public SceneTransition DefaultTransition => _defaultTransition;
        public bool AllowSceneActivation => _allowSceneActivation;

        public event Action<string> OnSceneLoadStarted;
        public event Action<float> OnLoadProgressChanged;
        public event Action<string> OnSceneLoadCompleted;
        public event Action<string, string> OnSceneChanged;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            _currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _currentSceneName = scene.name;
        }

        #region Load Methods

        /// <summary>
        /// Loads a scene by name.
        /// </summary>
        public void LoadScene(string sceneName, bool useTransition = true)
        {
            if (_isLoading) return;
            StartCoroutine(LoadSceneRoutine(sceneName, useTransition, false));
        }

        /// <summary>
        /// Loads a scene by build index.
        /// </summary>
        public void LoadScene(int buildIndex, bool useTransition = true)
        {
            string sceneName = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(buildIndex);
            sceneName = System.IO.Path.GetFileNameWithoutExtension(sceneName);
            LoadScene(sceneName, useTransition);
        }

        /// <summary>
        /// Loads a scene additively.
        /// </summary>
        public void LoadSceneAdditive(string sceneName)
        {
            if (_isLoading) return;
            StartCoroutine(LoadSceneAdditiveRoutine(sceneName));
        }

        /// <summary>
        /// Unloads an additive scene.
        /// </summary>
        public void UnloadScene(string sceneName)
        {
            StartCoroutine(UnloadSceneRoutine(sceneName));
        }

        /// <summary>
        /// Reloads the current scene.
        /// </summary>
        public void ReloadCurrentScene(bool useTransition = true)
        {
            LoadScene(_currentSceneName, useTransition);
        }

        /// <summary>
        /// Loads the previous scene from history.
        /// </summary>
        public void LoadPreviousScene(bool useTransition = true)
        {
            if (_sceneHistory.Count > 0)
            {
                string previousScene = _sceneHistory.Pop();
                StartCoroutine(LoadSceneRoutine(previousScene, useTransition, false));
            }
        }

        /// <summary>
        /// Loads a scene with a loading screen.
        /// </summary>
        public void LoadSceneWithLoadingScreen(string sceneName, string loadingSceneName = null)
        {
            if (_isLoading) return;
            StartCoroutine(LoadWithLoadingScreenRoutine(sceneName, loadingSceneName ?? _loadingSceneName));
        }

        #endregion

        #region Load Routines

        private IEnumerator LoadSceneRoutine(string sceneName, bool useTransition, bool skipHistory)
        {
            _isLoading = true;
            _loadProgress = 0f;

            string previousScene = _currentSceneName;
            if (!skipHistory && !string.IsNullOrEmpty(previousScene))
            {
                _sceneHistory.Push(previousScene);
            }

            OnSceneLoadStarted?.Invoke(sceneName);

            // Transition out
            if (useTransition && _defaultTransition != null)
            {
                yield return _defaultTransition.TransitionOut();
            }

            // Start async load
            _currentOperation = SceneManager.LoadSceneAsync(sceneName);
            _currentOperation.allowSceneActivation = false;

            float startTime = Time.unscaledTime;

            // Wait for load (0.9 = ready to activate)
            while (_currentOperation.progress < 0.9f)
            {
                _loadProgress = _currentOperation.progress / 0.9f;
                OnLoadProgressChanged?.Invoke(_loadProgress);
                yield return null;
            }

            _loadProgress = 1f;
            OnLoadProgressChanged?.Invoke(_loadProgress);

            // Ensure minimum load time
            float elapsed = Time.unscaledTime - startTime;
            if (elapsed < _minimumLoadTime)
            {
                yield return new WaitForSecondsRealtime(_minimumLoadTime - elapsed);
            }

            // Activate scene
            _currentOperation.allowSceneActivation = true;

            // Wait for activation
            while (!_currentOperation.isDone)
            {
                yield return null;
            }

            // Transition in
            if (useTransition && _defaultTransition != null)
            {
                yield return _defaultTransition.TransitionIn();
            }

            _isLoading = false;
            _currentOperation = null;

            OnSceneLoadCompleted?.Invoke(sceneName);
            OnSceneChanged?.Invoke(previousScene, sceneName);
        }

        private IEnumerator LoadSceneAdditiveRoutine(string sceneName)
        {
            _isLoading = true;

            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            while (!operation.isDone)
            {
                _loadProgress = operation.progress;
                OnLoadProgressChanged?.Invoke(_loadProgress);
                yield return null;
            }

            _isLoading = false;
            OnSceneLoadCompleted?.Invoke(sceneName);
        }

        private IEnumerator UnloadSceneRoutine(string sceneName)
        {
            var operation = SceneManager.UnloadSceneAsync(sceneName);

            if (operation != null)
            {
                while (!operation.isDone)
                {
                    yield return null;
                }
            }
        }

        private IEnumerator LoadWithLoadingScreenRoutine(string targetScene, string loadingScene)
        {
            _isLoading = true;
            _loadProgress = 0f;

            string previousScene = _currentSceneName;
            _sceneHistory.Push(previousScene);

            OnSceneLoadStarted?.Invoke(targetScene);

            // Transition out
            if (_defaultTransition != null)
            {
                yield return _defaultTransition.TransitionOut();
            }

            // Load loading screen
            yield return SceneManager.LoadSceneAsync(loadingScene);

            // Transition in to loading screen
            if (_defaultTransition != null)
            {
                yield return _defaultTransition.TransitionIn();
            }

            // Start loading target scene in background
            _currentOperation = SceneManager.LoadSceneAsync(targetScene);
            _currentOperation.allowSceneActivation = false;

            // Update progress while loading
            while (_currentOperation.progress < 0.9f)
            {
                _loadProgress = _currentOperation.progress / 0.9f;
                OnLoadProgressChanged?.Invoke(_loadProgress);
                yield return null;
            }

            _loadProgress = 1f;
            OnLoadProgressChanged?.Invoke(_loadProgress);

            // Wait for minimum time or user input
            yield return new WaitForSecondsRealtime(_minimumLoadTime);

            // Transition out of loading screen
            if (_defaultTransition != null)
            {
                yield return _defaultTransition.TransitionOut();
            }

            // Activate target scene
            _currentOperation.allowSceneActivation = true;

            while (!_currentOperation.isDone)
            {
                yield return null;
            }

            // Transition in
            if (_defaultTransition != null)
            {
                yield return _defaultTransition.TransitionIn();
            }

            _isLoading = false;
            _currentOperation = null;

            OnSceneLoadCompleted?.Invoke(targetScene);
            OnSceneChanged?.Invoke(previousScene, targetScene);
        }

        #endregion

        #region Utility

        /// <summary>
        /// Gets the current active scene name.
        /// </summary>
        public string GetActiveSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// Gets all loaded scene names.
        /// </summary>
        public List<string> GetLoadedScenes()
        {
            var scenes = new List<string>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                scenes.Add(SceneManager.GetSceneAt(i).name);
            }
            return scenes;
        }

        /// <summary>
        /// Checks if a scene is loaded.
        /// </summary>
        public bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == sceneName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Sets the active scene.
        /// </summary>
        public void SetActiveScene(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (scene.isLoaded)
            {
                SceneManager.SetActiveScene(scene);
                _currentSceneName = sceneName;
            }
        }

        /// <summary>
        /// Clears scene history.
        /// </summary>
        public void ClearHistory()
        {
            _sceneHistory.Clear();
        }

        /// <summary>
        /// Gets the scene history count.
        /// </summary>
        public int HistoryCount => _sceneHistory.Count;

        /// <summary>
        /// Sets the default transition.
        /// </summary>
        public void SetDefaultTransition(SceneTransition transition)
        {
            _defaultTransition = transition;
        }

        #endregion
    }
}
