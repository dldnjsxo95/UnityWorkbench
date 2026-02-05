using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LWT.UnityWorkbench.SceneManagement
{
    /// <summary>
    /// Loading screen controller with progress display.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider _progressBar;
        [SerializeField] private Text _progressText;
        [SerializeField] private Text _tipText;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private GameObject _pressAnyKeyPrompt;

        [Header("Settings")]
        [SerializeField] private bool _smoothProgress = true;
        [SerializeField] private float _progressSmoothSpeed = 3f;
        [SerializeField] private bool _waitForInput = false;
        [SerializeField] private float _minimumDisplayTime = 1f;

        [Header("Tips")]
        [SerializeField] private string[] _tips;
        [SerializeField] private float _tipChangeInterval = 3f;

        [Header("Backgrounds")]
        [SerializeField] private Sprite[] _backgrounds;
        [SerializeField] private bool _randomBackground = true;

        private float _displayedProgress;
        private float _targetProgress;
        private bool _loadingComplete;
        private bool _inputReceived;
        private Coroutine _tipCoroutine;

        public bool IsComplete => _loadingComplete && (!_waitForInput || _inputReceived);
        public float Progress => _targetProgress;
        public float MinimumDisplayTime => _minimumDisplayTime;

        private void Start()
        {
            Initialize();
            SubscribeToLoader();
        }

        private void OnDestroy()
        {
            UnsubscribeFromLoader();
        }

        private void Initialize()
        {
            _displayedProgress = 0f;
            _targetProgress = 0f;
            _loadingComplete = false;
            _inputReceived = false;

            if (_pressAnyKeyPrompt != null)
            {
                _pressAnyKeyPrompt.SetActive(false);
            }

            // Set random background
            if (_randomBackground && _backgrounds != null && _backgrounds.Length > 0 && _backgroundImage != null)
            {
                _backgroundImage.sprite = _backgrounds[Random.Range(0, _backgrounds.Length)];
            }

            // Start tip rotation
            if (_tips != null && _tips.Length > 0)
            {
                _tipCoroutine = StartCoroutine(RotateTips());
            }

            UpdateUI();
        }

        private void SubscribeToLoader()
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.OnLoadProgressChanged += OnProgressChanged;
                SceneLoader.Instance.OnSceneLoadCompleted += OnLoadCompleted;
            }
        }

        private void UnsubscribeFromLoader()
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.OnLoadProgressChanged -= OnProgressChanged;
                SceneLoader.Instance.OnSceneLoadCompleted -= OnLoadCompleted;
            }
        }

        private void Update()
        {
            // Smooth progress bar
            if (_smoothProgress)
            {
                _displayedProgress = Mathf.MoveTowards(
                    _displayedProgress,
                    _targetProgress,
                    _progressSmoothSpeed * Time.unscaledDeltaTime
                );
            }
            else
            {
                _displayedProgress = _targetProgress;
            }

            UpdateUI();

            // Check for input
            if (_loadingComplete && _waitForInput && !_inputReceived)
            {
                if (Input.anyKeyDown)
                {
                    _inputReceived = true;
                }
            }
        }

        private void OnProgressChanged(float progress)
        {
            _targetProgress = progress;
        }

        private void OnLoadCompleted(string sceneName)
        {
            _loadingComplete = true;
            _targetProgress = 1f;

            if (_waitForInput && _pressAnyKeyPrompt != null)
            {
                _pressAnyKeyPrompt.SetActive(true);
            }
        }

        private void UpdateUI()
        {
            if (_progressBar != null)
            {
                _progressBar.value = _displayedProgress;
            }

            if (_progressText != null)
            {
                _progressText.text = $"{Mathf.RoundToInt(_displayedProgress * 100)}%";
            }
        }

        private IEnumerator RotateTips()
        {
            int currentIndex = 0;

            while (true)
            {
                if (_tipText != null && _tips.Length > 0)
                {
                    _tipText.text = _tips[currentIndex];
                    currentIndex = (currentIndex + 1) % _tips.Length;
                }

                yield return new WaitForSecondsRealtime(_tipChangeInterval);
            }
        }

        /// <summary>
        /// Sets the progress manually.
        /// </summary>
        public void SetProgress(float progress)
        {
            _targetProgress = Mathf.Clamp01(progress);
        }

        /// <summary>
        /// Sets the tip text.
        /// </summary>
        public void SetTip(string tip)
        {
            if (_tipText != null)
            {
                _tipText.text = tip;
            }
        }

        /// <summary>
        /// Marks loading as complete.
        /// </summary>
        public void MarkComplete()
        {
            _loadingComplete = true;
            _targetProgress = 1f;

            if (_waitForInput && _pressAnyKeyPrompt != null)
            {
                _pressAnyKeyPrompt.SetActive(true);
            }
        }

        /// <summary>
        /// Skips waiting for input.
        /// </summary>
        public void SkipWait()
        {
            _inputReceived = true;
        }
    }

    /// <summary>
    /// Simple loading spinner animation.
    /// </summary>
    public class LoadingSpinner : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeed = 360f;
        [SerializeField] private bool _clockwise = true;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            float direction = _clockwise ? -1f : 1f;
            _rectTransform.Rotate(0f, 0f, direction * _rotationSpeed * Time.unscaledDeltaTime);
        }
    }

    /// <summary>
    /// Animated loading dots (...)
    /// </summary>
    public class LoadingDots : MonoBehaviour
    {
        [SerializeField] private Text _text;
        [SerializeField] private string _baseText = "Loading";
        [SerializeField] private float _dotInterval = 0.5f;
        [SerializeField] private int _maxDots = 3;

        private int _currentDots;
        private float _timer;

        private void Update()
        {
            _timer += Time.unscaledDeltaTime;

            if (_timer >= _dotInterval)
            {
                _timer = 0f;
                _currentDots = (_currentDots + 1) % (_maxDots + 1);

                if (_text != null)
                {
                    _text.text = _baseText + new string('.', _currentDots);
                }
            }
        }
    }
}
