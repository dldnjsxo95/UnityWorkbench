using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.SceneManagement
{
    /// <summary>
    /// ScriptableObject containing scene metadata.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSceneData", menuName = "UnityWorkbench/Scene Management/Scene Data")]
    public class SceneData : ScriptableObject
    {
        [Header("Identification")]
        [SerializeField] private string _sceneId;
        [SerializeField] private string _displayName;
        [SerializeField] private string _sceneName;
        [SerializeField] private int _buildIndex = -1;

        [Header("Description")]
        [TextArea(3, 5)]
        [SerializeField] private string _description;
        [SerializeField] private Sprite _thumbnail;

        [Header("Loading")]
        [SerializeField] private string[] _requiredScenes;
        [SerializeField] private string[] _preloadScenes;
        [SerializeField] private bool _useCustomLoadingScreen;
        [SerializeField] private string _customLoadingScene;

        [Header("Music")]
        [SerializeField] private AudioClip _backgroundMusic;
        [SerializeField] private float _musicFadeTime = 1f;

        [Header("Metadata")]
        [SerializeField] private SceneType _sceneType = SceneType.Gameplay;
        [SerializeField] private bool _allowPause = true;
        [SerializeField] private bool _showCursor = false;

        public string SceneId => _sceneId;
        public string DisplayName => _displayName;
        public string SceneName => _sceneName;
        public int BuildIndex => _buildIndex;
        public string Description => _description;
        public Sprite Thumbnail => _thumbnail;
        public string[] RequiredScenes => _requiredScenes;
        public string[] PreloadScenes => _preloadScenes;
        public bool UseCustomLoadingScreen => _useCustomLoadingScreen;
        public string CustomLoadingScene => _customLoadingScene;
        public AudioClip BackgroundMusic => _backgroundMusic;
        public float MusicFadeTime => _musicFadeTime;
        public SceneType Type => _sceneType;
        public bool AllowPause => _allowPause;
        public bool ShowCursor => _showCursor;

        public enum SceneType
        {
            MainMenu,
            Gameplay,
            Cutscene,
            Loading,
            Credits,
            Settings
        }

        /// <summary>
        /// Loads this scene.
        /// </summary>
        public void Load(bool useTransition = true)
        {
            if (!string.IsNullOrEmpty(_sceneName))
            {
                SceneLoader.Instance?.LoadScene(_sceneName, useTransition);
            }
            else if (_buildIndex >= 0)
            {
                SceneLoader.Instance?.LoadScene(_buildIndex, useTransition);
            }
        }

        /// <summary>
        /// Loads this scene with a loading screen.
        /// </summary>
        public void LoadWithLoadingScreen()
        {
            string loadingScene = _useCustomLoadingScreen && !string.IsNullOrEmpty(_customLoadingScene)
                ? _customLoadingScene
                : null;

            SceneLoader.Instance?.LoadSceneWithLoadingScreen(_sceneName, loadingScene);
        }
    }

    /// <summary>
    /// ScriptableObject database of all scenes.
    /// </summary>
    [CreateAssetMenu(fileName = "SceneDatabase", menuName = "UnityWorkbench/Scene Management/Scene Database")]
    public class SceneDatabase : ScriptableObject
    {
        [SerializeField] private List<SceneData> _scenes = new List<SceneData>();
        [SerializeField] private SceneData _mainMenuScene;
        [SerializeField] private SceneData _defaultLoadingScene;

        private Dictionary<string, SceneData> _lookup;

        public IReadOnlyList<SceneData> Scenes => _scenes;
        public SceneData MainMenuScene => _mainMenuScene;
        public SceneData DefaultLoadingScene => _defaultLoadingScene;

        private void OnEnable()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            _lookup = new Dictionary<string, SceneData>();
            foreach (var scene in _scenes)
            {
                if (scene != null && !string.IsNullOrEmpty(scene.SceneId))
                {
                    _lookup[scene.SceneId] = scene;
                }
            }
        }

        /// <summary>
        /// Gets a scene by ID.
        /// </summary>
        public SceneData GetScene(string sceneId)
        {
            if (_lookup == null) BuildLookup();
            return _lookup.TryGetValue(sceneId, out var scene) ? scene : null;
        }

        /// <summary>
        /// Gets a scene by name.
        /// </summary>
        public SceneData GetSceneByName(string sceneName)
        {
            foreach (var scene in _scenes)
            {
                if (scene.SceneName == sceneName)
                {
                    return scene;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets scenes by type.
        /// </summary>
        public List<SceneData> GetScenesByType(SceneData.SceneType type)
        {
            var result = new List<SceneData>();
            foreach (var scene in _scenes)
            {
                if (scene.Type == type)
                {
                    result.Add(scene);
                }
            }
            return result;
        }

        /// <summary>
        /// Loads the main menu scene.
        /// </summary>
        public void LoadMainMenu()
        {
            _mainMenuScene?.Load();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Adds a scene to the database (Editor only).
        /// </summary>
        public void AddScene(SceneData scene)
        {
            if (scene != null && !_scenes.Contains(scene))
            {
                _scenes.Add(scene);
                BuildLookup();
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        /// <summary>
        /// Removes a scene from the database (Editor only).
        /// </summary>
        public void RemoveScene(SceneData scene)
        {
            if (_scenes.Remove(scene))
            {
                BuildLookup();
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }

    /// <summary>
    /// Component that loads a scene on trigger enter.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SceneLoadTrigger : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] private SceneData _targetScene;
        [SerializeField] private string _targetSceneName;

        [Header("Trigger")]
        [SerializeField] private string _triggerTag = "Player";
        [SerializeField] private bool _useLoadingScreen = false;
        [SerializeField] private bool _useTransition = true;

        [Header("Delay")]
        [SerializeField] private float _loadDelay = 0f;

        private bool _triggered;

        private void Awake()
        {
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered) return;
            if (!other.CompareTag(_triggerTag)) return;

            _triggered = true;

            if (_loadDelay > 0f)
            {
                Invoke(nameof(LoadScene), _loadDelay);
            }
            else
            {
                LoadScene();
            }
        }

        private void LoadScene()
        {
            if (_targetScene != null)
            {
                if (_useLoadingScreen)
                {
                    _targetScene.LoadWithLoadingScreen();
                }
                else
                {
                    _targetScene.Load(_useTransition);
                }
            }
            else if (!string.IsNullOrEmpty(_targetSceneName))
            {
                if (_useLoadingScreen)
                {
                    SceneLoader.Instance?.LoadSceneWithLoadingScreen(_targetSceneName);
                }
                else
                {
                    SceneLoader.Instance?.LoadScene(_targetSceneName, _useTransition);
                }
            }
        }

        /// <summary>
        /// Resets the trigger for reuse.
        /// </summary>
        public void ResetTrigger()
        {
            _triggered = false;
        }
    }
}
