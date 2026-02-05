using System.Collections;
using UnityEngine;
using TMPro;

namespace LWT.UnityWorkbench.UI
{
    /// <summary>
    /// Toast notification that appears briefly and fades out.
    /// Non-modal and non-blocking.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ToastNotification : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private RectTransform _container;

        [Header("Animation")]
        [SerializeField] private float _fadeInDuration = 0.2f;
        [SerializeField] private float _fadeOutDuration = 0.3f;
        [SerializeField] private float _slideDistance = 50f;
        [SerializeField] private AnimationCurve _animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Position")]
        [SerializeField] private ToastPosition _position = ToastPosition.Bottom;
        [SerializeField] private float _verticalOffset = 100f;

        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private Coroutine _activeCoroutine;
        private Vector2 _targetPosition;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();

            if (_container == null)
                _container = _rectTransform;

            SetupPosition();
            SetVisibleImmediate(false);
        }

        /// <summary>
        /// Show the toast with a message for specified duration.
        /// </summary>
        public void Show(string message, float duration = 2f)
        {
            if (_messageText != null)
                _messageText.text = message;

            if (_activeCoroutine != null)
                StopCoroutine(_activeCoroutine);

            _activeCoroutine = StartCoroutine(ToastSequence(duration));
        }

        private void SetupPosition()
        {
            switch (_position)
            {
                case ToastPosition.Top:
                    _rectTransform.anchorMin = new Vector2(0.5f, 1f);
                    _rectTransform.anchorMax = new Vector2(0.5f, 1f);
                    _rectTransform.pivot = new Vector2(0.5f, 1f);
                    _targetPosition = new Vector2(0, -_verticalOffset);
                    break;

                case ToastPosition.Center:
                    _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    _rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    _targetPosition = Vector2.zero;
                    break;

                case ToastPosition.Bottom:
                default:
                    _rectTransform.anchorMin = new Vector2(0.5f, 0f);
                    _rectTransform.anchorMax = new Vector2(0.5f, 0f);
                    _rectTransform.pivot = new Vector2(0.5f, 0f);
                    _targetPosition = new Vector2(0, _verticalOffset);
                    break;
            }

            _rectTransform.anchoredPosition = _targetPosition;
        }

        private void SetVisibleImmediate(bool visible)
        {
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private IEnumerator ToastSequence(float displayDuration)
        {
            // Fade in with slide
            Vector2 startPos = _targetPosition + GetSlideOffset(true);
            _rectTransform.anchoredPosition = startPos;
            _canvasGroup.alpha = 0f;

            float elapsed = 0f;
            while (elapsed < _fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _animationCurve.Evaluate(elapsed / _fadeInDuration);

                _canvasGroup.alpha = t;
                _rectTransform.anchoredPosition = Vector2.Lerp(startPos, _targetPosition, t);

                yield return null;
            }

            _canvasGroup.alpha = 1f;
            _rectTransform.anchoredPosition = _targetPosition;

            // Wait for display duration
            yield return new WaitForSecondsRealtime(displayDuration);

            // Fade out with slide
            Vector2 endPos = _targetPosition + GetSlideOffset(false);
            startPos = _targetPosition;

            elapsed = 0f;
            while (elapsed < _fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _animationCurve.Evaluate(elapsed / _fadeOutDuration);

                _canvasGroup.alpha = 1f - t;
                _rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

                yield return null;
            }

            SetVisibleImmediate(false);

            // Self-destroy
            Destroy(gameObject);
        }

        private Vector2 GetSlideOffset(bool entering)
        {
            switch (_position)
            {
                case ToastPosition.Top:
                    return new Vector2(0, entering ? _slideDistance : _slideDistance);

                case ToastPosition.Bottom:
                    return new Vector2(0, entering ? -_slideDistance : -_slideDistance);

                case ToastPosition.Center:
                default:
                    return new Vector2(0, entering ? -_slideDistance : _slideDistance);
            }
        }
    }

    /// <summary>
    /// Toast notification position on screen.
    /// </summary>
    public enum ToastPosition
    {
        Top,
        Center,
        Bottom
    }
}
