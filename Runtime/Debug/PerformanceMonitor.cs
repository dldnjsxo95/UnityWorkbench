using UnityEngine;
using LWT.UnityWorkbench.Core;

namespace LWT.UnityWorkbench.DebugTools
{
    /// <summary>
    /// Displays performance metrics on screen.
    /// </summary>
    public class PerformanceMonitor : PersistentMonoSingleton<PerformanceMonitor>
    {
        [Header("Settings")]
        [SerializeField] private bool _showOnStart = true;
        [SerializeField] private KeyCode _toggleKey = KeyCode.F1;
        [SerializeField] private DisplayMode _displayMode = DisplayMode.Compact;

        [Header("Position")]
        [SerializeField] private Corner _corner = Corner.TopRight;
        [SerializeField] private Vector2 _offset = new Vector2(10, 10);

        [Header("Visual")]
        [SerializeField] private int _fontSize = 14;
        [SerializeField] private Color _goodColor = Color.green;
        [SerializeField] private Color _warningColor = Color.yellow;
        [SerializeField] private Color _badColor = Color.red;
        [SerializeField] private Color _backgroundColor = new Color(0, 0, 0, 0.7f);

        private bool _isVisible;
        private GUIStyle _labelStyle;
        private GUIStyle _boxStyle;
        private bool _stylesInitialized;

        // FPS tracking
        private float _deltaTime;
        private float _fps;
        private float _minFps = float.MaxValue;
        private float _maxFps;
        private float _avgFps;
        private int _frameCount;
        private float _fpsTimer;

        // Memory tracking
        private long _totalMemory;
        private long _usedMemory;
        private float _memoryTimer;

        public bool IsVisible => _isVisible;
        public float FPS => _fps;
        public float MinFPS => _minFps;
        public float MaxFPS => _maxFps;

        protected override void Awake()
        {
            base.Awake();
            _isVisible = _showOnStart;
        }

        private void Update()
        {
            if (Input.GetKeyDown(_toggleKey))
            {
                Toggle();
            }

            UpdateFPS();
            UpdateMemory();
        }

        private void UpdateFPS()
        {
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
            _fps = 1f / _deltaTime;

            _frameCount++;
            _fpsTimer += Time.unscaledDeltaTime;

            if (_fpsTimer >= 1f)
            {
                _avgFps = _frameCount / _fpsTimer;
                _minFps = Mathf.Min(_minFps, _fps);
                _maxFps = Mathf.Max(_maxFps, _fps);

                _frameCount = 0;
                _fpsTimer = 0f;
            }
        }

        private void UpdateMemory()
        {
            _memoryTimer += Time.unscaledDeltaTime;
            if (_memoryTimer >= 0.5f)
            {
                _totalMemory = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();
                _usedMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
                _memoryTimer = 0f;
            }
        }

        private void OnGUI()
        {
            if (!_isVisible) return;

            InitStyles();

            Rect rect = GetDisplayRect();

            switch (_displayMode)
            {
                case DisplayMode.Compact:
                    DrawCompact(rect);
                    break;
                case DisplayMode.Detailed:
                    DrawDetailed(rect);
                    break;
                case DisplayMode.Minimal:
                    DrawMinimal(rect);
                    break;
            }
        }

        private void InitStyles()
        {
            if (_stylesInitialized) return;

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = _fontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft
            };

            _boxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = MakeTexture(1, 1, _backgroundColor) }
            };

            _stylesInitialized = true;
        }

        private Rect GetDisplayRect()
        {
            float width = _displayMode == DisplayMode.Detailed ? 200f : (_displayMode == DisplayMode.Compact ? 150f : 80f);
            float height = _displayMode == DisplayMode.Detailed ? 120f : (_displayMode == DisplayMode.Compact ? 60f : 25f);

            float x = _corner switch
            {
                Corner.TopLeft => _offset.x,
                Corner.TopRight => Screen.width - width - _offset.x,
                Corner.BottomLeft => _offset.x,
                Corner.BottomRight => Screen.width - width - _offset.x,
                _ => _offset.x
            };

            float y = _corner switch
            {
                Corner.TopLeft => _offset.y,
                Corner.TopRight => _offset.y,
                Corner.BottomLeft => Screen.height - height - _offset.y,
                Corner.BottomRight => Screen.height - height - _offset.y,
                _ => _offset.y
            };

            return new Rect(x, y, width, height);
        }

        private void DrawMinimal(Rect rect)
        {
            GUI.Box(rect, "", _boxStyle);

            _labelStyle.normal.textColor = GetFPSColor();
            GUI.Label(new Rect(rect.x + 5, rect.y + 3, rect.width - 10, 20),
                $"{_fps:F0} FPS", _labelStyle);
        }

        private void DrawCompact(Rect rect)
        {
            GUI.Box(rect, "", _boxStyle);

            float y = rect.y + 5;
            float lineHeight = _fontSize + 4;

            // FPS
            _labelStyle.normal.textColor = GetFPSColor();
            GUI.Label(new Rect(rect.x + 5, y, rect.width - 10, lineHeight),
                $"FPS: {_fps:F0} ({_minFps:F0}-{_maxFps:F0})", _labelStyle);

            y += lineHeight;

            // Memory
            _labelStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rect.x + 5, y, rect.width - 10, lineHeight),
                $"Mem: {FormatBytes(_usedMemory)}", _labelStyle);
        }

        private void DrawDetailed(Rect rect)
        {
            GUI.Box(rect, "", _boxStyle);

            float y = rect.y + 5;
            float lineHeight = _fontSize + 4;

            // FPS
            _labelStyle.normal.textColor = GetFPSColor();
            GUI.Label(new Rect(rect.x + 5, y, rect.width - 10, lineHeight),
                $"FPS: {_fps:F1}", _labelStyle);
            y += lineHeight;

            _labelStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rect.x + 5, y, rect.width - 10, lineHeight),
                $"  Min: {_minFps:F0} / Max: {_maxFps:F0}", _labelStyle);
            y += lineHeight;

            GUI.Label(new Rect(rect.x + 5, y, rect.width - 10, lineHeight),
                $"  Avg: {_avgFps:F0} / Frame: {_deltaTime * 1000f:F1}ms", _labelStyle);
            y += lineHeight + 5;

            // Memory
            GUI.Label(new Rect(rect.x + 5, y, rect.width - 10, lineHeight),
                $"Memory:", _labelStyle);
            y += lineHeight;

            GUI.Label(new Rect(rect.x + 5, y, rect.width - 10, lineHeight),
                $"  Used: {FormatBytes(_usedMemory)}", _labelStyle);
            y += lineHeight;

            GUI.Label(new Rect(rect.x + 5, y, rect.width - 10, lineHeight),
                $"  Total: {FormatBytes(_totalMemory)}", _labelStyle);
        }

        private Color GetFPSColor()
        {
            if (_fps >= 60) return _goodColor;
            if (_fps >= 30) return _warningColor;
            return _badColor;
        }

        private string FormatBytes(long bytes)
        {
            if (bytes >= 1073741824) return $"{bytes / 1073741824f:F1} GB";
            if (bytes >= 1048576) return $"{bytes / 1048576f:F1} MB";
            if (bytes >= 1024) return $"{bytes / 1024f:F1} KB";
            return $"{bytes} B";
        }

        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            Texture2D tex = new Texture2D(width, height);
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        #region Public API

        public void Toggle()
        {
            _isVisible = !_isVisible;
        }

        public void Show()
        {
            _isVisible = true;
        }

        public void Hide()
        {
            _isVisible = false;
        }

        public void SetDisplayMode(DisplayMode mode)
        {
            _displayMode = mode;
        }

        public void ResetStats()
        {
            _minFps = float.MaxValue;
            _maxFps = 0;
            _avgFps = 0;
            _frameCount = 0;
            _fpsTimer = 0;
        }

        #endregion

        public enum DisplayMode
        {
            Minimal,
            Compact,
            Detailed
        }

        public enum Corner
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }
    }
}
