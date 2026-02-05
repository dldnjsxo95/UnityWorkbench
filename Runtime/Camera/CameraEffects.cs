using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LWT.UnityWorkbench.CameraSystem
{
    /// <summary>
    /// Camera effects including fade, zoom, and post-processing transitions.
    /// </summary>
    public class CameraEffects : MonoBehaviour
    {
        [Header("Fade Settings")]
        [SerializeField] private Image _fadeImage;
        [SerializeField] private Color _fadeColor = Color.black;
        [SerializeField] private float _defaultFadeDuration = 0.5f;

        [Header("Zoom Settings")]
        [SerializeField] private float _defaultFOV = 60f;
        [SerializeField] private float _zoomSpeed = 5f;

        [Header("Letterbox")]
        [SerializeField] private RectTransform _letterboxTop;
        [SerializeField] private RectTransform _letterboxBottom;
        [SerializeField] private float _letterboxSize = 100f;

        private Camera _camera;
        private Coroutine _fadeCoroutine;
        private Coroutine _zoomCoroutine;
        private Coroutine _letterboxCoroutine;

        public bool IsFading => _fadeCoroutine != null;
        public bool IsZooming => _zoomCoroutine != null;
        public float CurrentFOV => _camera != null ? _camera.fieldOfView : _defaultFOV;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            InitializeFadeImage();
        }

        private void InitializeFadeImage()
        {
            if (_fadeImage == null)
            {
                // Try to find or create fade canvas
                var canvas = Object.FindFirstObjectByType<Canvas>();
                if (canvas == null)
                {
                    var canvasGO = new GameObject("FadeCanvas");
                    canvas = canvasGO.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.sortingOrder = 999;
                    canvasGO.AddComponent<CanvasScaler>();
                }

                var fadeGO = new GameObject("FadeImage");
                fadeGO.transform.SetParent(canvas.transform, false);
                _fadeImage = fadeGO.AddComponent<Image>();
                _fadeImage.color = new Color(_fadeColor.r, _fadeColor.g, _fadeColor.b, 0f);
                _fadeImage.raycastTarget = false;

                var rect = _fadeImage.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = Vector2.zero;
            }
        }

        #region Fade Effects

        /// <summary>
        /// Fades the screen to a color.
        /// </summary>
        public Coroutine FadeIn(float duration = -1f, Color? color = null)
        {
            if (duration < 0f) duration = _defaultFadeDuration;
            Color targetColor = color ?? _fadeColor;

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeRoutine(1f, duration, targetColor));
            return _fadeCoroutine;
        }

        /// <summary>
        /// Fades the screen from a color to clear.
        /// </summary>
        public Coroutine FadeOut(float duration = -1f)
        {
            if (duration < 0f) duration = _defaultFadeDuration;

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeRoutine(0f, duration, _fadeColor));
            return _fadeCoroutine;
        }

        /// <summary>
        /// Performs a fade in then fade out.
        /// </summary>
        public Coroutine FadeInOut(float holdDuration = 0.5f, float fadeDuration = -1f)
        {
            if (fadeDuration < 0f) fadeDuration = _defaultFadeDuration;

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeInOutRoutine(holdDuration, fadeDuration));
            return _fadeCoroutine;
        }

        /// <summary>
        /// Sets fade alpha immediately.
        /// </summary>
        public void SetFadeAlpha(float alpha)
        {
            if (_fadeImage != null)
            {
                var c = _fadeImage.color;
                c.a = alpha;
                _fadeImage.color = c;
            }
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration, Color color)
        {
            if (_fadeImage == null) yield break;

            float startAlpha = _fadeImage.color.a;
            _fadeImage.color = new Color(color.r, color.g, color.b, startAlpha);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                _fadeImage.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }

            _fadeImage.color = new Color(color.r, color.g, color.b, targetAlpha);
            _fadeCoroutine = null;
        }

        private IEnumerator FadeInOutRoutine(float holdDuration, float fadeDuration)
        {
            yield return FadeRoutine(1f, fadeDuration, _fadeColor);
            yield return new WaitForSecondsRealtime(holdDuration);
            yield return FadeRoutine(0f, fadeDuration, _fadeColor);
            _fadeCoroutine = null;
        }

        #endregion

        #region Zoom Effects

        /// <summary>
        /// Zooms to a target FOV.
        /// </summary>
        public Coroutine ZoomTo(float targetFOV, float duration = -1f)
        {
            if (duration < 0f) duration = 1f / _zoomSpeed;

            if (_zoomCoroutine != null) StopCoroutine(_zoomCoroutine);
            _zoomCoroutine = StartCoroutine(ZoomRoutine(targetFOV, duration));
            return _zoomCoroutine;
        }

        /// <summary>
        /// Resets FOV to default.
        /// </summary>
        public Coroutine ResetZoom(float duration = -1f)
        {
            return ZoomTo(_defaultFOV, duration);
        }

        /// <summary>
        /// Performs a quick zoom punch effect.
        /// </summary>
        public Coroutine ZoomPunch(float amount = 10f, float duration = 0.2f)
        {
            if (_zoomCoroutine != null) StopCoroutine(_zoomCoroutine);
            _zoomCoroutine = StartCoroutine(ZoomPunchRoutine(amount, duration));
            return _zoomCoroutine;
        }

        /// <summary>
        /// Sets FOV immediately.
        /// </summary>
        public void SetFOV(float fov)
        {
            if (_camera != null)
            {
                _camera.fieldOfView = fov;
            }
        }

        private IEnumerator ZoomRoutine(float targetFOV, float duration)
        {
            if (_camera == null) yield break;

            float startFOV = _camera.fieldOfView;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _camera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, elapsed / duration);
                yield return null;
            }

            _camera.fieldOfView = targetFOV;
            _zoomCoroutine = null;
        }

        private IEnumerator ZoomPunchRoutine(float amount, float duration)
        {
            if (_camera == null) yield break;

            float originalFOV = _camera.fieldOfView;
            float targetFOV = originalFOV - amount;
            float halfDuration = duration / 2f;

            // Zoom in
            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _camera.fieldOfView = Mathf.Lerp(originalFOV, targetFOV, elapsed / halfDuration);
                yield return null;
            }

            // Zoom out
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _camera.fieldOfView = Mathf.Lerp(targetFOV, originalFOV, elapsed / halfDuration);
                yield return null;
            }

            _camera.fieldOfView = originalFOV;
            _zoomCoroutine = null;
        }

        #endregion

        #region Letterbox

        /// <summary>
        /// Shows cinematic letterbox bars.
        /// </summary>
        public Coroutine ShowLetterbox(float duration = 0.5f)
        {
            if (_letterboxCoroutine != null) StopCoroutine(_letterboxCoroutine);
            _letterboxCoroutine = StartCoroutine(LetterboxRoutine(true, duration));
            return _letterboxCoroutine;
        }

        /// <summary>
        /// Hides cinematic letterbox bars.
        /// </summary>
        public Coroutine HideLetterbox(float duration = 0.5f)
        {
            if (_letterboxCoroutine != null) StopCoroutine(_letterboxCoroutine);
            _letterboxCoroutine = StartCoroutine(LetterboxRoutine(false, duration));
            return _letterboxCoroutine;
        }

        private IEnumerator LetterboxRoutine(bool show, float duration)
        {
            if (_letterboxTop == null || _letterboxBottom == null) yield break;

            float startSize = show ? 0f : _letterboxSize;
            float targetSize = show ? _letterboxSize : 0f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float size = Mathf.Lerp(startSize, targetSize, elapsed / duration);

                _letterboxTop.sizeDelta = new Vector2(_letterboxTop.sizeDelta.x, size);
                _letterboxBottom.sizeDelta = new Vector2(_letterboxBottom.sizeDelta.x, size);

                yield return null;
            }

            _letterboxTop.sizeDelta = new Vector2(_letterboxTop.sizeDelta.x, targetSize);
            _letterboxBottom.sizeDelta = new Vector2(_letterboxBottom.sizeDelta.x, targetSize);
            _letterboxCoroutine = null;
        }

        #endregion

        #region Flash

        /// <summary>
        /// Flashes the screen with a color.
        /// </summary>
        public Coroutine Flash(Color color, float duration = 0.1f)
        {
            return StartCoroutine(FlashRoutine(color, duration));
        }

        /// <summary>
        /// White flash effect.
        /// </summary>
        public Coroutine FlashWhite(float duration = 0.1f)
        {
            return Flash(Color.white, duration);
        }

        /// <summary>
        /// Red flash effect (damage indicator).
        /// </summary>
        public Coroutine FlashRed(float duration = 0.15f)
        {
            return Flash(new Color(1f, 0f, 0f, 0.5f), duration);
        }

        private IEnumerator FlashRoutine(Color color, float duration)
        {
            if (_fadeImage == null) yield break;

            Color originalColor = _fadeImage.color;
            _fadeImage.color = color;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(color.a, 0f, elapsed / duration);
                _fadeImage.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }

            _fadeImage.color = originalColor;
        }

        #endregion
    }
}
