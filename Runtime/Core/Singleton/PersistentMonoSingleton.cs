using UnityEngine;

namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// MonoBehaviour Singleton that persists across scene loads (DontDestroyOnLoad).
    /// Ideal for managers like GameManager, AudioManager, etc.
    /// </summary>
    /// <typeparam name="T">Type of the singleton MonoBehaviour</typeparam>
    public abstract class PersistentMonoSingleton<T> : MonoBehaviour where T : PersistentMonoSingleton<T>
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _isQuitting;

        public static T Instance
        {
            get
            {
                if (_isQuitting)
                {
                    Debug.LogWarning($"[PersistentMonoSingleton] Instance of {typeof(T)} already destroyed on application quit.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindAnyObjectByType<T>();

                        if (_instance == null)
                        {
                            var go = new GameObject($"[{typeof(T).Name}]");
                            _instance = go.AddComponent<T>();
                        }
                    }

                    return _instance;
                }
            }
        }

        public static bool HasInstance => _instance != null;

        /// <summary>
        /// Returns instance only if it already exists. Does not create new instance.
        /// </summary>
        public static T InstanceOrNull => _instance;

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
                OnSingletonAwake();
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[PersistentMonoSingleton] Duplicate instance of {typeof(T).Name} destroyed.");
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                OnSingletonDestroy();
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        /// <summary>
        /// Called once when singleton is first initialized.
        /// </summary>
        protected virtual void OnSingletonAwake() { }

        /// <summary>
        /// Called when singleton is destroyed.
        /// </summary>
        protected virtual void OnSingletonDestroy() { }
    }
}
