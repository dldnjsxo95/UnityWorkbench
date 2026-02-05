using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LWT.UnityWorkbench.SceneManagement
{
    /// <summary>
    /// Base class for scene transitions.
    /// </summary>
    public abstract class SceneTransition : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] protected float _transitionDuration = 0.5f;
        [SerializeField] protected AnimationCurve _transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public float Duration => _transitionDuration;
        public bool IsTransitioning { get; protected set; }

        public abstract IEnumerator TransitionOut();
        public abstract IEnumerator TransitionIn();
    }

    /// <summary>
    /// Simple fade transition.
    /// </summary>
    public class FadeTransition : SceneTransition
    {
        [Header("Fade Settings")]
        [SerializeField] private Image _fadeImage;
        [SerializeField] private Color _fadeColor = Color.black;
        [SerializeField] private bool _createImageIfMissing = true;

        private Canvas _canvas;

        private void Awake()
        {
            EnsureFadeImage();
        }

        private void EnsureFadeImage()
        {
            if (_fadeImage != null) return;
            if (!_createImageIfMissing) return;

            // Create canvas
            var canvasGO = new GameObject("TransitionCanvas");
            canvasGO.transform.SetParent(transform);
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 9999;
            canvasGO.AddComponent<CanvasScaler>();

            // Create fade image
            var imageGO = new GameObject("FadeImage");
            imageGO.transform.SetParent(canvasGO.transform);
            _fadeImage = imageGO.AddComponent<Image>();
            _fadeImage.color = new Color(_fadeColor.r, _fadeColor.g, _fadeColor.b, 0f);
            _fadeImage.raycastTarget = true;

            var rect = _fadeImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }

        public override IEnumerator TransitionOut()
        {
            EnsureFadeImage();
            IsTransitioning = true;

            float elapsed = 0f;
            while (elapsed < _transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _transitionCurve.Evaluate(elapsed / _transitionDuration);
                _fadeImage.color = new Color(_fadeColor.r, _fadeColor.g, _fadeColor.b, t);
                yield return null;
            }

            _fadeImage.color = new Color(_fadeColor.r, _fadeColor.g, _fadeColor.b, 1f);
            IsTransitioning = false;
        }

        public override IEnumerator TransitionIn()
        {
            EnsureFadeImage();
            IsTransitioning = true;

            float elapsed = 0f;
            while (elapsed < _transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _transitionCurve.Evaluate(elapsed / _transitionDuration);
                _fadeImage.color = new Color(_fadeColor.r, _fadeColor.g, _fadeColor.b, 1f - t);
                yield return null;
            }

            _fadeImage.color = new Color(_fadeColor.r, _fadeColor.g, _fadeColor.b, 0f);
            IsTransitioning = false;
        }

        public void SetColor(Color color)
        {
            _fadeColor = color;
        }
    }

    /// <summary>
    /// Crossfade transition using two cameras.
    /// </summary>
    public class CrossfadeTransition : SceneTransition
    {
        [Header("Crossfade Settings")]
        [SerializeField] private RenderTexture _captureTexture;
        [SerializeField] private RawImage _displayImage;

        public override IEnumerator TransitionOut()
        {
            IsTransitioning = true;

            // Capture current frame
            if (Camera.main != null && _captureTexture != null)
            {
                Camera.main.targetTexture = _captureTexture;
                Camera.main.Render();
                Camera.main.targetTexture = null;
            }

            // Show captured frame
            if (_displayImage != null)
            {
                _displayImage.gameObject.SetActive(true);
                _displayImage.color = Color.white;
            }

            IsTransitioning = false;
            yield break;
        }

        public override IEnumerator TransitionIn()
        {
            IsTransitioning = true;

            if (_displayImage != null)
            {
                float elapsed = 0f;
                while (elapsed < _transitionDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = _transitionCurve.Evaluate(elapsed / _transitionDuration);
                    _displayImage.color = new Color(1f, 1f, 1f, 1f - t);
                    yield return null;
                }

                _displayImage.gameObject.SetActive(false);
            }

            IsTransitioning = false;
        }
    }

    /// <summary>
    /// Wipe transition effect.
    /// </summary>
    public class WipeTransition : SceneTransition
    {
        public enum WipeDirection
        {
            Left,
            Right,
            Up,
            Down
        }

        [Header("Wipe Settings")]
        [SerializeField] private Image _wipeImage;
        [SerializeField] private WipeDirection _direction = WipeDirection.Left;
        [SerializeField] private Color _wipeColor = Color.black;

        private RectTransform _rect;

        private void Awake()
        {
            if (_wipeImage != null)
            {
                _rect = _wipeImage.rectTransform;
                _wipeImage.color = _wipeColor;
            }
        }

        public override IEnumerator TransitionOut()
        {
            if (_wipeImage == null) yield break;

            IsTransitioning = true;
            _wipeImage.gameObject.SetActive(true);

            Vector2 startPos = GetStartPosition();
            Vector2 endPos = Vector2.zero;

            float elapsed = 0f;
            while (elapsed < _transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _transitionCurve.Evaluate(elapsed / _transitionDuration);
                _rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            _rect.anchoredPosition = endPos;
            IsTransitioning = false;
        }

        public override IEnumerator TransitionIn()
        {
            if (_wipeImage == null) yield break;

            IsTransitioning = true;

            Vector2 startPos = Vector2.zero;
            Vector2 endPos = GetEndPosition();

            float elapsed = 0f;
            while (elapsed < _transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _transitionCurve.Evaluate(elapsed / _transitionDuration);
                _rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            _rect.anchoredPosition = endPos;
            _wipeImage.gameObject.SetActive(false);
            IsTransitioning = false;
        }

        private Vector2 GetStartPosition()
        {
            float width = Screen.width;
            float height = Screen.height;

            return _direction switch
            {
                WipeDirection.Left => new Vector2(-width, 0),
                WipeDirection.Right => new Vector2(width, 0),
                WipeDirection.Up => new Vector2(0, -height),
                WipeDirection.Down => new Vector2(0, height),
                _ => Vector2.zero
            };
        }

        private Vector2 GetEndPosition()
        {
            float width = Screen.width;
            float height = Screen.height;

            return _direction switch
            {
                WipeDirection.Left => new Vector2(width, 0),
                WipeDirection.Right => new Vector2(-width, 0),
                WipeDirection.Up => new Vector2(0, height),
                WipeDirection.Down => new Vector2(0, -height),
                _ => Vector2.zero
            };
        }
    }

    /// <summary>
    /// Circle/Iris transition effect.
    /// </summary>
    public class CircleTransition : SceneTransition
    {
        [Header("Circle Settings")]
        [SerializeField] private Image _circleImage;
        [SerializeField] private Material _circleMaterial;
        [SerializeField] private float _maxRadius = 2f;

        private static readonly int RadiusProperty = Shader.PropertyToID("_Radius");
        private static readonly int CenterProperty = Shader.PropertyToID("_Center");

        public override IEnumerator TransitionOut()
        {
            if (_circleMaterial == null) yield break;

            IsTransitioning = true;
            _circleImage?.gameObject.SetActive(true);

            // Circle closes
            float elapsed = 0f;
            while (elapsed < _transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _transitionCurve.Evaluate(elapsed / _transitionDuration);
                float radius = Mathf.Lerp(_maxRadius, 0f, t);
                _circleMaterial.SetFloat(RadiusProperty, radius);
                yield return null;
            }

            _circleMaterial.SetFloat(RadiusProperty, 0f);
            IsTransitioning = false;
        }

        public override IEnumerator TransitionIn()
        {
            if (_circleMaterial == null) yield break;

            IsTransitioning = true;

            // Circle opens
            float elapsed = 0f;
            while (elapsed < _transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _transitionCurve.Evaluate(elapsed / _transitionDuration);
                float radius = Mathf.Lerp(0f, _maxRadius, t);
                _circleMaterial.SetFloat(RadiusProperty, radius);
                yield return null;
            }

            _circleMaterial.SetFloat(RadiusProperty, _maxRadius);
            _circleImage?.gameObject.SetActive(false);
            IsTransitioning = false;
        }

        public void SetCenter(Vector2 center)
        {
            _circleMaterial?.SetVector(CenterProperty, center);
        }
    }
}
