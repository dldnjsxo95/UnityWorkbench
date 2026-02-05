using UnityEngine;

namespace LWT.UnityWorkbench.UI
{
    /// <summary>
    /// ScriptableObject configuration for UI screens.
    /// Define screen prefabs and their settings.
    /// </summary>
    [CreateAssetMenu(fileName = "ScreenConfig", menuName = "UnityWorkbench/UI/Screen Config")]
    public class UIScreenConfig : ScriptableObject
    {
        [Header("Screen Info")]
        [Tooltip("Unique identifier for this screen")]
        public string ScreenName;

        [Tooltip("Prefab containing the UIScreen component")]
        public GameObject ScreenPrefab;

        [Header("Behavior")]
        [Tooltip("Keep screen in pool when hidden (better performance)")]
        public bool PoolOnHide = true;

        [Tooltip("Destroy this screen when scene changes")]
        public bool DestroyOnSceneChange = false;

        [Tooltip("Whether this screen blocks input to screens below")]
        public bool BlocksInput = true;

        [Header("Transitions")]
        [Tooltip("Transition animation when showing")]
        public UITransitionType ShowTransition = UITransitionType.Fade;

        [Tooltip("Transition animation when hiding")]
        public UITransitionType HideTransition = UITransitionType.Fade;

        [Tooltip("Duration of transition animations")]
        [Range(0f, 2f)]
        public float TransitionDuration = 0.3f;

        [Tooltip("Easing curve for transitions")]
        public AnimationCurve TransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        /// <summary>
        /// Validate the configuration.
        /// </summary>
        public bool IsValid => !string.IsNullOrEmpty(ScreenName) && ScreenPrefab != null;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(ScreenName) && ScreenPrefab != null)
            {
                ScreenName = ScreenPrefab.name;
            }
        }
    }
}
