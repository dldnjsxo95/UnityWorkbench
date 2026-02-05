using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LWT.UnityWorkbench.UI
{
    /// <summary>
    /// Static utility class for scene loading operations.
    /// Provides convenient methods for common scene operations.
    /// </summary>
    public static class SceneLoader
    {
        /// <summary>
        /// Load a scene with loading screen via LoadingManager.
        /// </summary>
        public static void Load(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (LoadingManager.HasInstance)
            {
                LoadingManager.Instance.LoadScene(sceneName, mode);
            }
            else
            {
                SceneManager.LoadScene(sceneName, mode);
            }
        }

        /// <summary>
        /// Load a scene by build index with loading screen.
        /// </summary>
        public static void Load(int buildIndex, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (LoadingManager.HasInstance)
            {
                LoadingManager.Instance.LoadScene(buildIndex, mode);
            }
            else
            {
                SceneManager.LoadScene(buildIndex, mode);
            }
        }

        /// <summary>
        /// Load a scene additively.
        /// </summary>
        public static void LoadAdditive(string sceneName)
        {
            Load(sceneName, LoadSceneMode.Additive);
        }

        /// <summary>
        /// Unload a scene.
        /// </summary>
        public static void Unload(string sceneName)
        {
            if (LoadingManager.HasInstance)
            {
                LoadingManager.Instance.UnloadScene(sceneName);
            }
            else
            {
                SceneManager.UnloadSceneAsync(sceneName);
            }
        }

        /// <summary>
        /// Reload the current active scene.
        /// </summary>
        public static void ReloadCurrent()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            Load(currentScene);
        }

        /// <summary>
        /// Get the name of the currently active scene.
        /// </summary>
        public static string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// Get the build index of the currently active scene.
        /// </summary>
        public static int GetCurrentSceneBuildIndex()
        {
            return SceneManager.GetActiveScene().buildIndex;
        }

        /// <summary>
        /// Check if a scene is loaded.
        /// </summary>
        public static bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == sceneName)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get the total number of scenes in build settings.
        /// </summary>
        public static int GetSceneCountInBuildSettings()
        {
            return SceneManager.sceneCountInBuildSettings;
        }

        /// <summary>
        /// Load the next scene in build order.
        /// </summary>
        public static void LoadNext()
        {
            int currentIndex = GetCurrentSceneBuildIndex();
            int nextIndex = (currentIndex + 1) % GetSceneCountInBuildSettings();
            Load(nextIndex);
        }

        /// <summary>
        /// Load the previous scene in build order.
        /// </summary>
        public static void LoadPrevious()
        {
            int currentIndex = GetCurrentSceneBuildIndex();
            int prevIndex = currentIndex - 1;
            if (prevIndex < 0) prevIndex = GetSceneCountInBuildSettings() - 1;
            Load(prevIndex);
        }

        /// <summary>
        /// Load a scene directly without loading screen (synchronous).
        /// </summary>
        public static void LoadDirect(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            SceneManager.LoadScene(sceneName, mode);
        }

        /// <summary>
        /// Start an async scene load operation manually.
        /// Returns the AsyncOperation for custom handling.
        /// </summary>
        public static AsyncOperation LoadAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single,
            bool allowSceneActivation = true)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName, mode);
            operation.allowSceneActivation = allowSceneActivation;
            return operation;
        }

        /// <summary>
        /// Perform a scene transition with a custom action between unload and load.
        /// </summary>
        public static void TransitionWithAction(string targetScene, Action betweenAction,
            MonoBehaviour runner)
        {
            if (runner != null)
            {
                runner.StartCoroutine(TransitionCoroutine(targetScene, betweenAction));
            }
        }

        private static IEnumerator TransitionCoroutine(string targetScene, Action betweenAction)
        {
            // Show loading screen
            if (LoadingManager.HasInstance)
            {
                LoadingManager.Instance.ShowLoadingScreen("Loading...");
            }

            yield return new WaitForSecondsRealtime(0.1f);

            // Perform action
            betweenAction?.Invoke();

            yield return new WaitForSecondsRealtime(0.1f);

            // Load scene
            Load(targetScene);
        }
    }
}
