using UnityEngine;

namespace LWT.UnityWorkbench.CameraSystem
{
    /// <summary>
    /// 2D camera controller with bounds, deadzone, and smoothing.
    /// </summary>
    public class Camera2D : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _target;
        [SerializeField] private Vector2 _targetOffset = Vector2.zero;

        [Header("Follow")]
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private bool _useFixedUpdate = false;

        [Header("Deadzone")]
        [SerializeField] private bool _useDeadzone = true;
        [SerializeField] private Vector2 _deadzoneSize = new Vector2(2f, 1f);

        [Header("Look Ahead")]
        [SerializeField] private bool _useLookAhead = false;
        [SerializeField] private float _lookAheadDistance = 3f;
        [SerializeField] private float _lookAheadSpeed = 2f;

        [Header("Bounds")]
        [SerializeField] private bool _useBounds = false;
        [SerializeField] private Vector2 _minBounds = new Vector2(-20f, -10f);
        [SerializeField] private Vector2 _maxBounds = new Vector2(20f, 10f);

        [Header("Zoom")]
        [SerializeField] private float _defaultSize = 5f;
        [SerializeField] private float _minSize = 2f;
        [SerializeField] private float _maxSize = 15f;
        [SerializeField] private float _zoomSpeed = 5f;

        private Camera _camera;
        private Vector3 _velocity;
        private Vector3 _lookAheadPos;
        private Vector3 _lastTargetPosition;
        private float _targetSize;

        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        public float OrthographicSize
        {
            get => _camera != null ? _camera.orthographicSize : _defaultSize;
            set
            {
                _targetSize = Mathf.Clamp(value, _minSize, _maxSize);
            }
        }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            _targetSize = _defaultSize;
            if (_camera != null)
            {
                _camera.orthographicSize = _defaultSize;
            }
        }

        private void Start()
        {
            if (_target != null)
            {
                _lastTargetPosition = _target.position;
                SnapToTarget();
            }
        }

        private void LateUpdate()
        {
            if (!_useFixedUpdate)
            {
                UpdateCamera(Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (_useFixedUpdate)
            {
                UpdateCamera(Time.fixedDeltaTime);
            }
        }

        private void UpdateCamera(float deltaTime)
        {
            if (_target == null) return;

            UpdatePosition(deltaTime);
            UpdateZoom(deltaTime);
        }

        private void UpdatePosition(float deltaTime)
        {
            Vector3 targetPosition = GetTargetPosition();

            // Look ahead
            if (_useLookAhead)
            {
                Vector3 moveDir = (_target.position - _lastTargetPosition).normalized;
                _lookAheadPos = Vector3.Lerp(
                    _lookAheadPos,
                    moveDir * _lookAheadDistance,
                    _lookAheadSpeed * deltaTime
                );
                targetPosition += _lookAheadPos;
                _lastTargetPosition = _target.position;
            }

            // Deadzone check
            if (_useDeadzone)
            {
                Vector3 currentPos = transform.position;
                Vector3 diff = targetPosition - currentPos;

                if (Mathf.Abs(diff.x) > _deadzoneSize.x / 2f)
                {
                    float sign = Mathf.Sign(diff.x);
                    targetPosition.x = currentPos.x + sign * (Mathf.Abs(diff.x) - _deadzoneSize.x / 2f);
                }
                else
                {
                    targetPosition.x = currentPos.x;
                }

                if (Mathf.Abs(diff.y) > _deadzoneSize.y / 2f)
                {
                    float sign = Mathf.Sign(diff.y);
                    targetPosition.y = currentPos.y + sign * (Mathf.Abs(diff.y) - _deadzoneSize.y / 2f);
                }
                else
                {
                    targetPosition.y = currentPos.y;
                }
            }

            // Smooth follow
            Vector3 smoothedPosition = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref _velocity,
                1f / _smoothSpeed,
                Mathf.Infinity,
                deltaTime
            );

            // Apply bounds
            if (_useBounds)
            {
                smoothedPosition = ClampToBounds(smoothedPosition);
            }

            transform.position = smoothedPosition;
        }

        private void UpdateZoom(float deltaTime)
        {
            if (_camera == null || !_camera.orthographic) return;

            _camera.orthographicSize = Mathf.Lerp(
                _camera.orthographicSize,
                _targetSize,
                _zoomSpeed * deltaTime
            );
        }

        private Vector3 GetTargetPosition()
        {
            Vector3 pos = _target.position;
            pos.x += _targetOffset.x;
            pos.y += _targetOffset.y;
            pos.z = transform.position.z; // Keep camera Z
            return pos;
        }

        private Vector3 ClampToBounds(Vector3 position)
        {
            if (_camera == null) return position;

            float vertExtent = _camera.orthographicSize;
            float horizExtent = vertExtent * _camera.aspect;

            position.x = Mathf.Clamp(position.x, _minBounds.x + horizExtent, _maxBounds.x - horizExtent);
            position.y = Mathf.Clamp(position.y, _minBounds.y + vertExtent, _maxBounds.y - vertExtent);

            return position;
        }

        /// <summary>
        /// Instantly moves camera to target.
        /// </summary>
        public void SnapToTarget()
        {
            if (_target == null) return;

            Vector3 pos = GetTargetPosition();
            if (_useBounds)
            {
                pos = ClampToBounds(pos);
            }
            transform.position = pos;
            _velocity = Vector3.zero;
        }

        /// <summary>
        /// Zooms in by amount.
        /// </summary>
        public void ZoomIn(float amount = 1f)
        {
            _targetSize = Mathf.Clamp(_targetSize - amount, _minSize, _maxSize);
        }

        /// <summary>
        /// Zooms out by amount.
        /// </summary>
        public void ZoomOut(float amount = 1f)
        {
            _targetSize = Mathf.Clamp(_targetSize + amount, _minSize, _maxSize);
        }

        /// <summary>
        /// Resets zoom to default.
        /// </summary>
        public void ResetZoom()
        {
            _targetSize = _defaultSize;
        }

        /// <summary>
        /// Sets camera bounds.
        /// </summary>
        public void SetBounds(Vector2 min, Vector2 max)
        {
            _minBounds = min;
            _maxBounds = max;
            _useBounds = true;
        }

        /// <summary>
        /// Sets bounds from a collider.
        /// </summary>
        public void SetBoundsFromCollider(Collider2D collider)
        {
            if (collider == null) return;

            Bounds bounds = collider.bounds;
            _minBounds = bounds.min;
            _maxBounds = bounds.max;
            _useBounds = true;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Draw deadzone
            if (_useDeadzone)
            {
                Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
                Gizmos.DrawWireCube(transform.position, new Vector3(_deadzoneSize.x, _deadzoneSize.y, 0.1f));
            }

            // Draw bounds
            if (_useBounds)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
                Vector3 center = new Vector3(
                    (_minBounds.x + _maxBounds.x) / 2f,
                    (_minBounds.y + _maxBounds.y) / 2f,
                    transform.position.z
                );
                Vector3 size = new Vector3(
                    _maxBounds.x - _minBounds.x,
                    _maxBounds.y - _minBounds.y,
                    0.1f
                );
                Gizmos.DrawWireCube(center, size);
            }

            // Draw target
            if (_target != null)
            {
                Gizmos.color = Color.cyan;
                Vector3 targetPos = _target.position + (Vector3)_targetOffset;
                Gizmos.DrawWireSphere(targetPos, 0.3f);
                Gizmos.DrawLine(transform.position, targetPos);
            }
        }
#endif
    }
}
