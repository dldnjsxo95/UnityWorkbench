using UnityEngine;

namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// ScriptableObject-based Singleton.
    /// Must be placed in Resources folder with matching type name.
    /// </summary>
    /// <typeparam name="T">Type of the singleton ScriptableObject</typeparam>
    public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableSingleton<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Try to load from Resources
                    _instance = Resources.Load<T>(typeof(T).Name);

                    if (_instance == null)
                    {
                        // Try to find in loaded assets
                        var instances = Resources.FindObjectsOfTypeAll<T>();
                        if (instances.Length > 0)
                        {
                            _instance = instances[0];
                        }
                    }

                    if (_instance == null)
                    {
                        Debug.LogError($"[ScriptableSingleton] {typeof(T).Name} not found in Resources folder.");
                    }
                }

                return _instance;
            }
        }

        public static bool HasInstance => _instance != null;

        protected virtual void OnEnable()
        {
            if (_instance == null)
            {
                _instance = this as T;
            }
        }

        protected virtual void OnDisable()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
