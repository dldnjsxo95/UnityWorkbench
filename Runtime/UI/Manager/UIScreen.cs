using System;
using System.Collections;
using UnityEngine;

namespace LWT.UnityWorkbench.UI
{
    /// <summary>
    /// Base class for all UI screens.
    /// Provides lifecycle callbacks and transition support.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIScreen : MonoBehaviour
    {
        [Header("Screen Settings")]
        [SerializeField] protected string _screenName;
        [SerializeField] protected bool _closeOnEscape = true;

        [Header("Transition")]
        [SerializeField] protected UITransitionType _showTransition = UITransitionType.Fade;
        [SerializeField] protected UITransitionType _hideTransition = UITransitionType.Fade;
        [SerializeField] protected float _transitionDuration = 0.3f;
        [SerializeField] protected AnimationCurve _transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        protected CanvasGroup _canvasGroup;
        protected RectTransform _rectTransform;
        private Coroutine _transitionCoroutine;

        /// <summary>
        /// Screen identifier name.
        /// </summary>
        public string ScreenName => string.IsNullOrEmpty(_screenName) ? GetType().Name : _screenName;

        /// <summary>
        /// Whether this screen is currently visible.
        /// </summary>
        public bool IsVisible { get; protected set; }

        /// <summary>
        /// Whether this screen is currently transitioning.
        /// </summary>
        public bool IsTransitioning { get; protected set; }

        /// <summary>
        /// Whether pressing Escape/Back should close this screen.
        /// </summary>
        public bool CloseOnEscape => _closeOnEscape;

        /// <summary>
        /// Event fired when screen transition completes.
        /// </summary>
        public event Action<UIScreen> OnTransitionComplete;

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();

            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        /// <summary>
        /// Initialize the screen. Called once when screen is created.
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// Called when the screen starts showing (before transition).
        /// Override this for setup logic.
        /// </summary>
        public virtual void OnShow() { }

        /// <summary>
        /// Called when the screen starts hiding (before transition).
        /// Override this for cleanup logic.
        /// </summary>
        public virtual void OnHide() { }

        /// <summary>
        /// Show the screen with transition.
        /// </summary>
        public virtual void Show(bool instant = false)
        {
            gameObject.SetActive(true);
            OnShow();

            if (instant || _transitionDuration <= 0)
            {
                SetVisibleImmediate(true);
                OnShowComplete();
            }
            else
            {
                StopCurrentTransition();
                _transitionCoroutine = StartCoroutine(TransitionIn());
            }
        }

        /// <summary>
        /// Hide the screen with transition.
        /// </summary>
        public virtual void Hide(bool instant = false)
        {
            OnHide();

            if (instant || _transitionDuration <= 0)
            {
                SetVisibleImmediate(false);
                OnHideComplete();
            }
            else
            {
                StopCurrentTransition();
                _transitionCoroutine = StartCoroutine(TransitionOut());
            }
        }

        /// <summary>
        /// Called when this screen gains focus (becomes top of stack).
        /// </summary>
        public virtual void OnFocus() { }

        /// <summary>
        /// Called when this screen loses focus (another screen pushed on top).
        /// </summary>
        public virtual void OnLoseFocus() { }

        /// <summary>
        /// Called every frame while screen is visible and focused.
        /// </summary>
        public virtual void OnScreenUpdate() { }

        /// <summary>
        /// Called when back/escape is pressed.
        /// Return true to handle the back action, false to pass to UIManager.
        /// </summary>
        public virtual bool OnBackPressed()
        {
            return false;
        }

        protected virtual void OnShowComplete()
        {
            IsVisible = true;
            IsTransitioning = false;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            OnTransitionComplete?.Invoke(this);
        }

        protected virtual void OnHideComplete()
        {
            IsVisible = false;
            IsTransitioning = false;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
            OnTransitionComplete?.Invoke(this);
        }

        protected void SetVisibleImmediate(bool visible)
        {
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
        }

        protected void StopCurrentTransition()
        {
            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
                _transitionCoroutine = null;
            }
        }

        protected virtual IEnumerator TransitionIn()
        {
            IsTransitioning = true;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            // Set initial state based on transition type
            SetTransitionStartState(_showTransition, false);

            float elapsed = 0f;
            while (elapsed < _transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _transitionCurve.Evaluate(elapsed / _transitionDuration);
                ApplyTransition(_showTransition, t, true);
                yield return null;
            }

            SetVisibleImmediate(true);
            OnShowComplete();
        }

        protected virtual IEnumerator TransitionOut()
        {
            IsTransitioning = true;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            float elapsed = 0f;
            while (elapsed < _transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _transitionCurve.Evaluate(elapsed / _transitionDuration);
                ApplyTransition(_hideTransition, t, false);
                yield return null;
            }

            SetVisibleImmediate(false);
            OnHideComplete();
        }

        protected virtual void SetTransitionStartState(UITransitionType transitionType, bool showing)
        {
            switch (transitionType)
            {
                case UITransitionType.Fade:
                    _canvasGroup.alpha = showing ? 1f : 0f;
                    break;

                case UITransitionType.Scale:
                    transform.localScale = showing ? Vector3.one : Vector3.zero;
                    _canvasGroup.alpha = 1f;
                    break;

                case UITransitionType.SlideLeft:
                    _rectTransform.anchoredPosition = showing ? Vector2.zero : new Vector2(-Screen.width, 0);
                    _canvasGroup.alpha = 1f;
                    break;

                case UITransitionType.SlideRight:
                    _rectTransform.anchoredPosition = showing ? Vector2.zero : new Vector2(Screen.width, 0);
                    _canvasGroup.alpha = 1f;
                    break;

                case UITransitionType.SlideUp:
                    _rectTransform.anchoredPosition = showing ? Vector2.zero : new Vector2(0, Screen.height);
                    _canvasGroup.alpha = 1f;
                    break;

                case UITransitionType.SlideDown:
                    _rectTransform.anchoredPosition = showing ? Vector2.zero : new Vector2(0, -Screen.height);
                    _canvasGroup.alpha = 1f;
                    break;

                case UITransitionType.None:
                default:
                    _canvasGroup.alpha = showing ? 1f : 0f;
                    break;
            }
        }

        protected virtual void ApplyTransition(UITransitionType transitionType, float t, bool showing)
        {
            float progress = showing ? t : 1f - t;

            switch (transitionType)
            {
                case UITransitionType.Fade:
                    _canvasGroup.alpha = progress;
                    break;

                case UITransitionType.Scale:
                    transform.localScale = Vector3.one * progress;
                    break;

                case UITransitionType.SlideLeft:
                    float xLeft = Mathf.Lerp(-Screen.width, 0, progress);
                    _rectTransform.anchoredPosition = new Vector2(xLeft, 0);
                    break;

                case UITransitionType.SlideRight:
                    float xRight = Mathf.Lerp(Screen.width, 0, progress);
                    _rectTransform.anchoredPosition = new Vector2(xRight, 0);
                    break;

                case UITransitionType.SlideUp:
                    float yUp = Mathf.Lerp(Screen.height, 0, progress);
                    _rectTransform.anchoredPosition = new Vector2(0, yUp);
                    break;

                case UITransitionType.SlideDown:
                    float yDown = Mathf.Lerp(-Screen.height, 0, progress);
                    _rectTransform.anchoredPosition = new Vector2(0, yDown);
                    break;

                case UITransitionType.None:
                default:
                    _canvasGroup.alpha = showing ? 1f : 0f;
                    break;
            }
        }
    }

    /// <summary>
    /// Types of UI transitions.
    /// </summary>
    public enum UITransitionType
    {
        None,
        Fade,
        Scale,
        SlideLeft,
        SlideRight,
        SlideUp,
        SlideDown
    }
}
