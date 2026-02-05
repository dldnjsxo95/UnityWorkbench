using UnityEngine;

namespace LWT.UnityWorkbench.CameraSystem
{
    /// <summary>
    /// Versatile camera controller with follow, look-at, and offset capabilities.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        public enum UpdateMode
        {
            Update,
            LateUpdate,
            FixedUpdate
        }

        public enum FollowMode
        {
            None,
            Instant,
            Smooth,
            SmoothDamp
        }

        [Header("Target")]
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _targetOffset = Vector3.zero;

        [Header("Position")]
        [SerializeField] private FollowMode _followMode = FollowMode.Smooth;
        [SerializeField] private Vector3 _positionOffset = new Vector3(0f, 5f, -10f);
        [SerializeField] private float _followSpeed = 5f;
        [SerializeField] private float _smoothTime = 0.3f;

        [Header("Rotation")]
        [SerializeField] private bool _lookAtTarget = true;
        [SerializeField] private float _rotationSpeed = 5f;
        [SerializeField] private Vector3 _rotationOffset = Vector3.zero;

        [Header("Boundaries")]
        [SerializeField] private bool _useBoundaries = false;
        [SerializeField] private Vector3 _minBounds = new Vector3(-50f, 0f, -50f);
        [SerializeField] private Vector3 _maxBounds = new Vector3(50f, 30f, 50f);

        [Header("Settings")]
        [SerializeField] private UpdateMode _updateMode = UpdateMode.LateUpdate;
        [SerializeField] private bool _useUnscaledTime = false;

        private Vector3 _velocity;
        private Vector3 _currentOffset;
        private Camera _camera;

        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        public Vector3 PositionOffset
        {
            get => _positionOffset;
            set => _positionOffset = value;
        }

        public Vector3 TargetOffset
        {
            get => _targetOffset;
            set => _targetOffset = value;
        }

        public Camera Camera => _camera;

        private float DeltaTime => _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_camera == null)
            {
                _camera = GetComponentInChildren<Camera>();
            }
            _currentOffset = _positionOffset;
        }

        private void Start()
        {
            if (_target != null && _followMode == FollowMode.Instant)
            {
                SnapToTarget();
            }
        }

        private void Update()
        {
            if (_updateMode == UpdateMode.Update)
            {
                UpdateCamera();
            }
        }

        private void LateUpdate()
        {
            if (_updateMode == UpdateMode.LateUpdate)
            {
                UpdateCamera();
            }
        }

        private void FixedUpdate()
        {
            if (_updateMode == UpdateMode.FixedUpdate)
            {
                UpdateCamera();
            }
        }

        private void UpdateCamera()
        {
            if (_target == null) return;

            UpdatePosition();
            UpdateRotation();
        }

        private void UpdatePosition()
        {
            Vector3 targetPosition = GetTargetPosition();

            switch (_followMode)
            {
                case FollowMode.Instant:
                    transform.position = targetPosition;
                    break;

                case FollowMode.Smooth:
                    transform.position = Vector3.Lerp(
                        transform.position,
                        targetPosition,
                        _followSpeed * DeltaTime
                    );
                    break;

                case FollowMode.SmoothDamp:
                    transform.position = Vector3.SmoothDamp(
                        transform.position,
                        targetPosition,
                        ref _velocity,
                        _smoothTime,
                        Mathf.Infinity,
                        DeltaTime
                    );
                    break;
            }

            if (_useBoundaries)
            {
                transform.position = ClampPosition(transform.position);
            }
        }

        private void UpdateRotation()
        {
            if (!_lookAtTarget) return;

            Vector3 lookTarget = _target.position + _targetOffset;
            Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position);
            targetRotation *= Quaternion.Euler(_rotationOffset);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                _rotationSpeed * DeltaTime
            );
        }

        private Vector3 GetTargetPosition()
        {
            Vector3 targetPos = _target.position + _targetOffset;

            // Apply offset in world or local space
            return targetPos + _target.TransformDirection(_currentOffset);
        }

        private Vector3 ClampPosition(Vector3 position)
        {
            return new Vector3(
                Mathf.Clamp(position.x, _minBounds.x, _maxBounds.x),
                Mathf.Clamp(position.y, _minBounds.y, _maxBounds.y),
                Mathf.Clamp(position.z, _minBounds.z, _maxBounds.z)
            );
        }

        /// <summary>
        /// Instantly moves camera to target position.
        /// </summary>
        public void SnapToTarget()
        {
            if (_target == null) return;

            transform.position = GetTargetPosition();

            if (_lookAtTarget)
            {
                Vector3 lookTarget = _target.position + _targetOffset;
                transform.LookAt(lookTarget);
                transform.rotation *= Quaternion.Euler(_rotationOffset);
            }

            _velocity = Vector3.zero;
        }

        /// <summary>
        /// Sets a new target and optionally snaps to it.
        /// </summary>
        public void SetTarget(Transform newTarget, bool snap = false)
        {
            _target = newTarget;
            if (snap)
            {
                SnapToTarget();
            }
        }

        /// <summary>
        /// Smoothly transitions to a new offset.
        /// </summary>
        public void SetOffset(Vector3 newOffset, float duration = 0f)
        {
            if (duration <= 0f)
            {
                _currentOffset = newOffset;
                _positionOffset = newOffset;
            }
            else
            {
                StartCoroutine(TransitionOffset(newOffset, duration));
            }
        }

        private System.Collections.IEnumerator TransitionOffset(Vector3 newOffset, float duration)
        {
            Vector3 startOffset = _currentOffset;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += DeltaTime;
                float t = elapsed / duration;
                _currentOffset = Vector3.Lerp(startOffset, newOffset, t);
                yield return null;
            }

            _currentOffset = newOffset;
            _positionOffset = newOffset;
        }

        /// <summary>
        /// Gets the world position the camera is looking at.
        /// </summary>
        public Vector3 GetLookPoint()
        {
            if (_target != null)
            {
                return _target.position + _targetOffset;
            }
            return transform.position + transform.forward * 10f;
        }

        /// <summary>
        /// Converts screen point to world position on a plane at target height.
        /// </summary>
        public Vector3 ScreenToWorldOnPlane(Vector2 screenPosition, float planeHeight = 0f)
        {
            if (_camera == null) return Vector3.zero;

            Ray ray = _camera.ScreenPointToRay(screenPosition);
            Plane plane = new Plane(Vector3.up, new Vector3(0f, planeHeight, 0f));

            if (plane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_useBoundaries)
            {
                Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
                Vector3 center = (_minBounds + _maxBounds) / 2f;
                Vector3 size = _maxBounds - _minBounds;
                Gizmos.DrawWireCube(center, size);
            }

            if (_target != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, _target.position + _targetOffset);
                Gizmos.DrawWireSphere(_target.position + _targetOffset, 0.3f);
            }
        }
#endif
    }
}
