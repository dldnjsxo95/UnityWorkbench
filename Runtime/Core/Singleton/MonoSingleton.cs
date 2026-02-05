using UnityEngine;

namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// MonoBehaviour-based Singleton. Destroyed when scene changes.
    /// Use PersistentMonoSingleton for cross-scene persistence.
    /// </summary>
    /// <typeparam name="T">Type of the singleton MonoBehaviour</typeparam>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
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
                    Debug.LogWarning($"[MonoSingleton] Instance of {typeof(T)} already destroyed on application quit.");
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

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                OnSingletonAwake();
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[MonoSingleton] Duplicate instance of {typeof(T).Name} destroyed.");
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
