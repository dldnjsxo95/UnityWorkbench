using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LWT.UnityWorkbench.SceneManagement
{
    /// <summary>
    /// Scene utility functions.
    /// </summary>
    public static class SceneUtility
    {
        /// <summary>
        /// Gets the current active scene name.
        /// </summary>
        public static string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// Gets the current active scene build index.
        /// </summary>
        public static int GetCurrentSceneBuildIndex()
        {
            return SceneManager.GetActiveScene().buildIndex;
        }

        /// <summary>
        /// Gets a list of all loaded scene names.
        /// </summary>
        public static List<string> GetLoadedSceneNames()
        {
            var scenes = new List<string>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                scenes.Add(SceneManager.GetSceneAt(i).name);
            }
            return scenes;
        }

        /// <summary>
        /// Checks if a scene is currently loaded.
        /// </summary>
        public static bool IsSceneLoaded(string sceneName)
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
        /// Gets the total number of scenes in build settings.
        /// </summary>
        public static int GetSceneCountInBuildSettings()
        {
            return SceneManager.sceneCountInBuildSettings;
        }

        /// <summary>
        /// Gets the scene name from build index.
        /// </summary>
        public static string GetSceneNameFromBuildIndex(int buildIndex)
        {
            string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(buildIndex);
            return System.IO.Path.GetFileNameWithoutExtension(path);
        }

        /// <summary>
        /// Gets the build index from scene name.
        /// </summary>
        public static int GetBuildIndexFromSceneName(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name == sceneName)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Checks if a scene exists in build settings.
        /// </summary>
        public static bool IsSceneInBuildSettings(string sceneName)
        {
            return GetBuildIndexFromSceneName(sceneName) >= 0;
        }

        /// <summary>
        /// Gets all root GameObjects in a scene.
        /// </summary>
        public static GameObject[] GetRootObjects(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (scene.isLoaded)
            {
                return scene.GetRootGameObjects();
            }
            return new GameObject[0];
        }

        /// <summary>
        /// Moves a GameObject to a different scene.
        /// </summary>
        public static void MoveObjectToScene(GameObject obj, string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (scene.isLoaded)
            {
                SceneManager.MoveGameObjectToScene(obj, scene);
            }
        }

        /// <summary>
        /// Finds an object of type in a specific scene.
        /// </summary>
        public static T FindObjectInScene<T>(string sceneName) where T : Component
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.isLoaded) return null;

            foreach (var root in scene.GetRootGameObjects())
            {
                var result = root.GetComponentInChildren<T>(true);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds all objects of type in a specific scene.
        /// </summary>
        public static List<T> FindObjectsInScene<T>(string sceneName) where T : Component
        {
            var results = new List<T>();
            var scene = SceneManager.GetSceneByName(sceneName);

            if (!scene.isLoaded) return results;

            foreach (var root in scene.GetRootGameObjects())
            {
                results.AddRange(root.GetComponentsInChildren<T>(true));
            }
            return results;
        }

        /// <summary>
        /// Creates a simple scene with basic setup.
        /// </summary>
        public static void CreateBasicSceneSetup(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.isLoaded) return;

            // Check if camera exists
            var existingCamera = FindObjectInScene<Camera>(sceneName);
            if (existingCamera == null)
            {
                var cameraGO = new GameObject("Main Camera");
                cameraGO.AddComponent<Camera>();
                cameraGO.AddComponent<AudioListener>();
                cameraGO.tag = "MainCamera";
                cameraGO.transform.position = new Vector3(0f, 1f, -10f);
                SceneManager.MoveGameObjectToScene(cameraGO, scene);
            }

            // Check if light exists
            var existingLight = FindObjectInScene<Light>(sceneName);
            if (existingLight == null)
            {
                var lightGO = new GameObject("Directional Light");
                var light = lightGO.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = Color.white;
                lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                SceneManager.MoveGameObjectToScene(lightGO, scene);
            }
        }
    }

    /// <summary>
    /// Component for persisting objects across scene loads.
    /// </summary>
    public class DontDestroyOnLoadManager : MonoBehaviour
    {
        private static DontDestroyOnLoadManager _instance;
        private static List<GameObject> _persistentObjects = new List<GameObject>();

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Makes an object persistent across scene loads.
        /// </summary>
        public static void MakePersistent(GameObject obj)
        {
            if (obj == null) return;

            if (!_persistentObjects.Contains(obj))
            {
                DontDestroyOnLoad(obj);
                _persistentObjects.Add(obj);
            }
        }

        /// <summary>
        /// Destroys a persistent object.
        /// </summary>
        public static void DestroyPersistent(GameObject obj)
        {
            if (obj == null) return;

            _persistentObjects.Remove(obj);
            Destroy(obj);
        }

        /// <summary>
        /// Gets all persistent objects.
        /// </summary>
        public static IReadOnlyList<GameObject> GetPersistentObjects()
        {
            _persistentObjects.RemoveAll(o => o == null);
            return _persistentObjects;
        }

        /// <summary>
        /// Destroys all persistent objects.
        /// </summary>
        public static void DestroyAllPersistent()
        {
            foreach (var obj in _persistentObjects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            _persistentObjects.Clear();
        }
    }
}
