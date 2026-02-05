using System;
using System.Collections;
using UnityEngine;

namespace LWT.UnityWorkbench.UI
{
    /// <summary>
    /// Base class for all popup windows.
    /// Provides common functionality for modal/modeless popups.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PopupBase : MonoBehaviour, IPopup
    {
        [Header("Popup Settings")]
        [SerializeField] protected string _popupId;
        [SerializeField] protected bool _isModal = true;
        [SerializeField] protected int _priority = 0;
        [SerializeField] protected bool _closeOnBackgroundClick = true;
        [SerializeField] protected bool _closeOnEscape = true;

        [Header("Transition")]
        [SerializeField] protected UITransitionType _showTransition = UITransitionType.Scale;
        [SerializeField] protected UITransitionType _hideTransition = UITransitionType.Scale;
        [SerializeField] protected float _transitionDuration = 0.2f;
        [SerializeField] protected AnimationCurve _transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        protected CanvasGroup _canvasGroup;
        protected RectTransform _rectTransform;
        private Coroutine _transitionCoroutine;

        public string PopupId => string.IsNullOrEmpty(_popupId) ? GetType().Name : _popupId;
        public bool IsModal => _isModal;
        public int Priority => _priority;
        public bool IsVisible { get; protected set; }
        public bool CloseOnBackgroundClick => _closeOnBackgroundClick;
        public bool CloseOnEscape => _closeOnEscape;

        public event Action<IPopup> OnClosed;

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
        /// Initialize the popup. Called once when created.
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// Show the popup with transition.
        /// </summary>
        public virtual void Show()
        {
            gameObject.SetActive(true);

            if (_transitionDuration <= 0)
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
        /// Hide the popup with transition.
        /// </summary>
        public virtual void Hide()
        {
            if (_transitionDuration <= 0)
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
        /// Close and destroy the popup.
        /// </summary>
        public virtual void Close()
        {
            Hide();
            OnClosed?.Invoke(this);

            // PopupManager will handle destruction
        }

        /// <summary>
        /// Called when the popup is shown (after transition).
        /// </summary>
        protected virtual void OnShowComplete()
        {
            IsVisible = true;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// Called when the popup is hidden (after transition).
        /// </summary>
        protected virtual void OnHideComplete()
        {
            IsVisible = false;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        protected void SetVisibleImmediate(bool visible)
        {
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
            transform.localScale = Vector3.one;
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
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

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
                    transform.localScale = Vector3.one;
                    break;

                case UITransitionType.Scale:
                    _canvasGroup.alpha = 1f;
                    transform.localScale = showing ? Vector3.one : Vector3.zero;
                    break;

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

                default:
                    _canvasGroup.alpha = showing ? 1f : 0f;
                    break;
            }
        }
    }
}
