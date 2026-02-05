using UnityEngine;

namespace LWT.UnityWorkbench.CameraSystem
{
    /// <summary>
    /// Third-person free look camera with orbit controls.
    /// </summary>
    public class FreeLookCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _targetOffset = new Vector3(0f, 1.5f, 0f);

        [Header("Distance")]
        [SerializeField] private float _distance = 5f;
        [SerializeField] private float _minDistance = 2f;
        [SerializeField] private float _maxDistance = 15f;
        [SerializeField] private float _zoomSpeed = 2f;
        [SerializeField] private float _zoomSmoothness = 10f;

        [Header("Rotation")]
        [SerializeField] private float _horizontalSpeed = 3f;
        [SerializeField] private float _verticalSpeed = 2f;
        [SerializeField] private float _rotationSmoothness = 10f;
        [SerializeField] private bool _invertY = false;

        [Header("Vertical Limits")]
        [SerializeField] private float _minVerticalAngle = -30f;
        [SerializeField] private float _maxVerticalAngle = 70f;

        [Header("Input")]
        [SerializeField] private bool _useMouseInput = true;
        [SerializeField] private bool _requireMouseButton = true;
        [SerializeField] private int _mouseButton = 1; // Right click
        [SerializeField] private string _horizontalAxis = "Mouse X";
        [SerializeField] private string _verticalAxis = "Mouse Y";
        [SerializeField] private string _zoomAxis = "Mouse ScrollWheel";

        [Header("Collision")]
        [SerializeField] private bool _checkCollision = true;
        [SerializeField] private LayerMask _collisionMask = -1;
        [SerializeField] private float _collisionRadius = 0.3f;
        [SerializeField] private float _collisionSmoothness = 10f;

        private float _currentHorizontalAngle;
        private float _currentVerticalAngle;
        private float _targetHorizontalAngle;
        private float _targetVerticalAngle;
        private float _currentDistance;
        private float _targetDistance;

        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        public float Distance
        {
            get => _targetDistance;
            set => _targetDistance = Mathf.Clamp(value, _minDistance, _maxDistance);
        }

        public Vector2 Angles
        {
            get => new Vector2(_targetHorizontalAngle, _targetVerticalAngle);
            set
            {
                _targetHorizontalAngle = value.x;
                _targetVerticalAngle = Mathf.Clamp(value.y, _minVerticalAngle, _maxVerticalAngle);
            }
        }

        private void Start()
        {
            _targetDistance = _distance;
            _currentDistance = _distance;

            // Initialize angles from current rotation
            Vector3 angles = transform.eulerAngles;
            _targetHorizontalAngle = angles.y;
            _targetVerticalAngle = angles.x;
            _currentHorizontalAngle = _targetHorizontalAngle;
            _currentVerticalAngle = _targetVerticalAngle;

            if (_target != null)
            {
                UpdatePosition(true);
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            HandleInput();
            UpdateAngles();
            UpdateDistance();
            UpdatePosition(false);
        }

        private void HandleInput()
        {
            if (!_useMouseInput) return;

            bool canRotate = !_requireMouseButton || Input.GetMouseButton(_mouseButton);

            if (canRotate)
            {
                float horizontal = Input.GetAxis(_horizontalAxis);
                float vertical = Input.GetAxis(_verticalAxis);

                _targetHorizontalAngle += horizontal * _horizontalSpeed;
                _targetVerticalAngle -= vertical * _verticalSpeed * (_invertY ? -1f : 1f);
                _targetVerticalAngle = Mathf.Clamp(_targetVerticalAngle, _minVerticalAngle, _maxVerticalAngle);
            }

            // Zoom
            float scroll = Input.GetAxis(_zoomAxis);
            if (Mathf.Abs(scroll) > 0.01f)
            {
                _targetDistance -= scroll * _zoomSpeed;
                _targetDistance = Mathf.Clamp(_targetDistance, _minDistance, _maxDistance);
            }
        }

        private void UpdateAngles()
        {
            _currentHorizontalAngle = Mathf.LerpAngle(
                _currentHorizontalAngle,
                _targetHorizontalAngle,
                _rotationSmoothness * Time.deltaTime
            );

            _currentVerticalAngle = Mathf.Lerp(
                _currentVerticalAngle,
                _targetVerticalAngle,
                _rotationSmoothness * Time.deltaTime
            );
        }

        private void UpdateDistance()
        {
            _currentDistance = Mathf.Lerp(
                _currentDistance,
                _targetDistance,
                _zoomSmoothness * Time.deltaTime
            );
        }

        private void UpdatePosition(bool instant)
        {
            Vector3 targetPosition = _target.position + _targetOffset;

            // Calculate direction from angles
            Quaternion rotation = Quaternion.Euler(_currentVerticalAngle, _currentHorizontalAngle, 0f);
            Vector3 direction = rotation * Vector3.back;

            float actualDistance = _currentDistance;

            // Collision check
            if (_checkCollision)
            {
                actualDistance = CheckCollision(targetPosition, direction, _currentDistance);
            }

            // Calculate final position
            Vector3 desiredPosition = targetPosition + direction * actualDistance;

            if (instant)
            {
                transform.position = desiredPosition;
            }
            else if (_checkCollision)
            {
                transform.position = Vector3.Lerp(
                    transform.position,
                    desiredPosition,
                    _collisionSmoothness * Time.deltaTime
                );
            }
            else
            {
                transform.position = desiredPosition;
            }

            // Look at target
            transform.LookAt(targetPosition);
        }

        private float CheckCollision(Vector3 target, Vector3 direction, float desiredDistance)
        {
            RaycastHit hit;
            if (Physics.SphereCast(
                target,
                _collisionRadius,
                direction,
                out hit,
                desiredDistance,
                _collisionMask,
                QueryTriggerInteraction.Ignore))
            {
                return hit.distance - _collisionRadius;
            }

            return desiredDistance;
        }

        /// <summary>
        /// Rotates camera by specified angles.
        /// </summary>
        public void Rotate(float horizontal, float vertical)
        {
            _targetHorizontalAngle += horizontal;
            _targetVerticalAngle = Mathf.Clamp(_targetVerticalAngle + vertical, _minVerticalAngle, _maxVerticalAngle);
        }

        /// <summary>
        /// Sets camera angles immediately.
        /// </summary>
        public void SetAngles(float horizontal, float vertical, bool instant = false)
        {
            _targetHorizontalAngle = horizontal;
            _targetVerticalAngle = Mathf.Clamp(vertical, _minVerticalAngle, _maxVerticalAngle);

            if (instant)
            {
                _currentHorizontalAngle = _targetHorizontalAngle;
                _currentVerticalAngle = _targetVerticalAngle;
                UpdatePosition(true);
            }
        }

        /// <summary>
        /// Resets camera to behind the target.
        /// </summary>
        public void ResetToBehindTarget(bool instant = false)
        {
            if (_target == null) return;

            _targetHorizontalAngle = _target.eulerAngles.y;
            _targetVerticalAngle = 20f;

            if (instant)
            {
                _currentHorizontalAngle = _targetHorizontalAngle;
                _currentVerticalAngle = _targetVerticalAngle;
                UpdatePosition(true);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_target == null) return;

            Vector3 targetPos = _target.position + _targetOffset;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPos, 0.2f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, targetPos);

            // Draw distance range
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(targetPos, _minDistance);
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(targetPos, _maxDistance);
        }
#endif
    }
}
