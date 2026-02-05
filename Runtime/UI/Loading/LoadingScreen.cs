using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LWT.UnityWorkbench.UI
{
    /// <summary>
    /// Base implementation of a loading screen.
    /// Shows progress bar, percentage, message, and optional loading spinner.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class LoadingScreen : MonoBehaviour, ILoadingScreen
    {
        [Header("UI References")]
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TMP_Text _progressText;
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private Image _loadingIcon;
        [SerializeField] private Image _backgroundImage;

        [Header("Progress Settings")]
        [SerializeField] private bool _showPercentage = true;
        [SerializeField] private string _percentageFormat = "{0:0}%";
        [SerializeField] private bool _smoothProgress = true;
        [SerializeField] private float _progressSmoothSpeed = 5f;

        [Header("Spinner Animation")]
        [SerializeField] private bool _rotateLoadingIcon = true;
        [SerializeField] private float _rotationSpeed = 360f;

        [Header("Fade Animation")]
        [SerializeField] private float _fadeInDuration = 0.3f;
        [SerializeField] private float _fadeOutDuration = 0.3f;
        [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private CanvasGroup _canvasGroup;
        private float _currentProgress;
        private float _targetProgress;
        private Coroutine _fadeCoroutine;

        public bool IsVisible { get; private set; }

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // Initialize hidden
            SetVisibleImmediate(false);
        }

        protected virtual void Update()
        {
            // Rotate loading icon
            if (_rotateLoadingIcon && _loadingIcon != null && IsVisible)
            {
                _loadingIcon.transform.Rotate(0, 0, -_rotationSpeed * Time.unscaledDeltaTime);
            }

            // Smooth progress
            if (_smoothProgress && IsVisible)
            {
                if (!Mathf.Approximately(_currentProgress, _targetProgress))
                {
                    _currentProgress = Mathf.MoveTowards(_currentProgress, _targetProgress,
                        _progressSmoothSpeed * Time.unscaledDeltaTime);
                    UpdateProgressUI(_currentProgress);
                }
            }
        }

        /// <summary>
        /// Show the loading screen with fade in.
        /// </summary>
        public virtual void Show()
        {
            gameObject.SetActive(true);

            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);

            if (_fadeInDuration > 0)
            {
                _fadeCoroutine = StartCoroutine(FadeIn());
            }
            else
            {
                SetVisibleImmediate(true);
            }
        }

        /// <summary>
        /// Hide the loading screen with fade out.
        /// </summary>
        public virtual void Hide()
        {
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);

            if (_fadeOutDuration > 0)
            {
                _fadeCoroutine = StartCoroutine(FadeOut());
            }
            else
            {
                SetVisibleImmediate(false);
            }
        }

        /// <summary>
        /// Set the loading progress (0-1).
        /// </summary>
        public virtual void SetProgress(float progress)
        {
            _targetProgress = Mathf.Clamp01(progress);

            if (!_smoothProgress)
            {
                _currentProgress = _targetProgress;
                UpdateProgressUI(_currentProgress);
            }
        }

        /// <summary>
        /// Set the loading message.
        /// </summary>
        public virtual void SetMessage(string message)
        {
            if (_messageText != null)
            {
                _messageText.text = message;
            }
        }

        /// <summary>
        /// Set both progress and message.
        /// </summary>
        public virtual void SetStatus(float progress, string message)
        {
            SetProgress(progress);
            SetMessage(message);
        }

        /// <summary>
        /// Reset the loading screen to initial state.
        /// </summary>
        public virtual void Reset()
        {
            _currentProgress = 0f;
            _targetProgress = 0f;
            UpdateProgressUI(0f);
            SetMessage(string.Empty);
        }

        protected void SetVisibleImmediate(bool visible)
        {
            IsVisible = visible;
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;

            if (!visible)
            {
                gameObject.SetActive(false);
            }
        }

        protected void UpdateProgressUI(float progress)
        {
            if (_progressBar != null)
            {
                _progressBar.value = progress;
            }

            if (_progressText != null && _showPercentage)
            {
                _progressText.text = string.Format(_percentageFormat, progress * 100f);
            }
        }

        private IEnumerator FadeIn()
        {
            IsVisible = true;
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = true;

            float elapsed = 0f;
            while (elapsed < _fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _fadeCurve.Evaluate(elapsed / _fadeInDuration);
                _canvasGroup.alpha = t;
                yield return null;
            }

            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
        }

        private IEnumerator FadeOut()
        {
            _canvasGroup.interactable = false;

            float elapsed = 0f;
            while (elapsed < _fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _fadeCurve.Evaluate(elapsed / _fadeOutDuration);
                _canvasGroup.alpha = 1f - t;
                yield return null;
            }

            SetVisibleImmediate(false);
        }
    }
}
