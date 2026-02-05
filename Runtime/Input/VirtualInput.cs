using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LWT.UnityWorkbench.InputHandling
{
    /// <summary>
    /// Virtual joystick for touch/mobile input.
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public enum JoystickType
        {
            Fixed,
            Floating,
            Dynamic
        }

        [Header("Type")]
        [SerializeField] private JoystickType _joystickType = JoystickType.Fixed;

        [Header("References")]
        [SerializeField] private RectTransform _background;
        [SerializeField] private RectTransform _handle;

        [Header("Settings")]
        [SerializeField] private float _handleRange = 1f;
        [SerializeField] private float _deadzone = 0.1f;
        [SerializeField] private bool _snapX = false;
        [SerializeField] private bool _snapY = false;

        private Canvas _canvas;
        private Camera _cam;
        private Vector2 _input = Vector2.zero;
        private Vector2 _startPosition;
        private bool _isActive;

        public Vector2 Direction => _input;
        public float Horizontal => _input.x;
        public float Vertical => _input.y;
        public bool IsActive => _isActive;

        private void Start()
        {
            _canvas = GetComponentInParent<Canvas>();
            if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                _cam = _canvas.worldCamera;
            }

            _startPosition = _background.anchoredPosition;

            if (_joystickType == JoystickType.Floating || _joystickType == JoystickType.Dynamic)
            {
                _background.gameObject.SetActive(false);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isActive = true;

            if (_joystickType == JoystickType.Floating || _joystickType == JoystickType.Dynamic)
            {
                _background.gameObject.SetActive(true);
                _background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
            }

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _background,
                eventData.position,
                _cam,
                out position
            );

            Vector2 radius = _background.sizeDelta / 2f;
            position = new Vector2(
                position.x / radius.x,
                position.y / radius.y
            );

            _input = position.magnitude > 1f ? position.normalized : position;

            // Apply deadzone
            if (_input.magnitude < _deadzone)
            {
                _input = Vector2.zero;
            }
            else
            {
                _input = (_input.magnitude - _deadzone) / (1f - _deadzone) * _input.normalized;
            }

            // Snap axes
            if (_snapX) _input.x = Mathf.Round(_input.x);
            if (_snapY) _input.y = Mathf.Round(_input.y);

            // Update handle position
            _handle.anchoredPosition = _input * radius.x * _handleRange;

            // Dynamic joystick follows
            if (_joystickType == JoystickType.Dynamic && _input.magnitude > 0)
            {
                Vector2 diff = eventData.position - (Vector2)_background.position;
                if (diff.magnitude > radius.x)
                {
                    _background.position = (Vector2)_background.position + (diff - diff.normalized * radius.x);
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isActive = false;
            _input = Vector2.zero;
            _handle.anchoredPosition = Vector2.zero;

            if (_joystickType == JoystickType.Floating || _joystickType == JoystickType.Dynamic)
            {
                _background.gameObject.SetActive(false);
                _background.anchoredPosition = _startPosition;
            }
        }

        private Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _background.parent as RectTransform,
                screenPosition,
                _cam,
                out localPoint
            );
            return localPoint;
        }

        /// <summary>
        /// Sets the joystick type at runtime.
        /// </summary>
        public void SetJoystickType(JoystickType type)
        {
            _joystickType = type;

            if (type == JoystickType.Fixed)
            {
                _background.gameObject.SetActive(true);
                _background.anchoredPosition = _startPosition;
            }
            else if (!_isActive)
            {
                _background.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Virtual button for touch/mobile input.
    /// </summary>
    public class VirtualButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Settings")]
        [SerializeField] private string _buttonName = "Button";
        [SerializeField] private bool _useHoldTimer = false;
        [SerializeField] private float _holdDuration = 0.5f;

        [Header("Visual Feedback")]
        [SerializeField] private float _pressedScale = 0.9f;
        [SerializeField] private Color _pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);

        private bool _isPressed;
        private bool _wasPressed;
        private bool _wasReleased;
        private float _pressTime;
        private bool _holdTriggered;
        private RectTransform _rectTransform;
        private Image _image;
        private Color _originalColor;
        private Vector3 _originalScale;

        public string ButtonName => _buttonName;
        public bool IsPressed => _isPressed;
        public bool WasPressed => _wasPressed;
        public bool WasReleased => _wasReleased;
        public float HoldTime => _isPressed ? Time.unscaledTime - _pressTime : 0f;

        public event System.Action OnPress;
        public event System.Action OnRelease;
        public event System.Action OnHold;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _image = GetComponent<Image>();

            if (_rectTransform != null)
            {
                _originalScale = _rectTransform.localScale;
            }

            if (_image != null)
            {
                _originalColor = _image.color;
            }
        }

        private void LateUpdate()
        {
            _wasPressed = false;
            _wasReleased = false;

            // Check for hold
            if (_useHoldTimer && _isPressed && !_holdTriggered)
            {
                if (Time.unscaledTime - _pressTime >= _holdDuration)
                {
                    _holdTriggered = true;
                    OnHold?.Invoke();
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            _wasPressed = true;
            _pressTime = Time.unscaledTime;
            _holdTriggered = false;

            ApplyPressedVisual(true);
            OnPress?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            _wasReleased = true;

            ApplyPressedVisual(false);
            OnRelease?.Invoke();
        }

        private void ApplyPressedVisual(bool pressed)
        {
            if (_rectTransform != null)
            {
                _rectTransform.localScale = pressed ? _originalScale * _pressedScale : _originalScale;
            }

            if (_image != null)
            {
                _image.color = pressed ? _pressedColor : _originalColor;
            }
        }

        /// <summary>
        /// Simulates a button press.
        /// </summary>
        public void SimulatePress()
        {
            OnPointerDown(null);
        }

        /// <summary>
        /// Simulates a button release.
        /// </summary>
        public void SimulateRelease()
        {
            OnPointerUp(null);
        }
    }

    /// <summary>
    /// Touch area for swipe detection.
    /// </summary>
    public class SwipeDetector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public enum SwipeDirection
        {
            None,
            Up,
            Down,
            Left,
            Right
        }

        [Header("Settings")]
        [SerializeField] private float _minSwipeDistance = 50f;
        [SerializeField] private float _maxSwipeTime = 0.5f;
        [SerializeField] private bool _detectDiagonals = false;

        private Vector2 _startPosition;
        private float _startTime;
        private bool _swiping;

        public event System.Action<SwipeDirection> OnSwipe;
        public event System.Action<Vector2> OnSwipeVector;

        public void OnPointerDown(PointerEventData eventData)
        {
            _startPosition = eventData.position;
            _startTime = Time.unscaledTime;
            _swiping = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Optional: detect swipe during drag
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_swiping) return;
            _swiping = false;

            float elapsed = Time.unscaledTime - _startTime;
            if (elapsed > _maxSwipeTime) return;

            Vector2 delta = eventData.position - _startPosition;
            float distance = delta.magnitude;

            if (distance < _minSwipeDistance) return;

            SwipeDirection direction = GetSwipeDirection(delta);
            OnSwipe?.Invoke(direction);
            OnSwipeVector?.Invoke(delta.normalized);
        }

        private SwipeDirection GetSwipeDirection(Vector2 delta)
        {
            if (_detectDiagonals)
            {
                // 8-direction detection could be added here
            }

            // 4-direction detection
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                return delta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
            }
            else
            {
                return delta.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
            }
        }
    }
}
