using UnityEngine;

namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// Initializes persistent singletons at game start.
    /// Attach to a GameObject in your first scene or use [RuntimeInitializeOnLoadMethod].
    /// </summary>
    public class SingletonInitializer : MonoBehaviour
    {
        [SerializeField] private bool _initializeOnAwake = true;
        [SerializeField] private GameObject[] _singletonPrefabs;

        private void Awake()
        {
            if (_initializeOnAwake)
            {
                InitializeSingletons();
            }
        }

        public void InitializeSingletons()
        {
            if (_singletonPrefabs == null) return;

            foreach (var prefab in _singletonPrefabs)
            {
                if (prefab != null)
                {
                    Instantiate(prefab);
                }
            }
        }

        /// <summary>
        /// Example of auto-initialization at runtime.
        /// Uncomment and modify as needed.
        /// </summary>
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        // private static void AutoInitialize()
        // {
        //     // Initialize your critical singletons here
        //     // var _ = GameManager.Instance;
        //     // var _ = AudioManager.Instance;
        // }
    }
}
