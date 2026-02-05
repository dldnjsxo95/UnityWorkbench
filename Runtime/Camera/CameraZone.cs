using UnityEngine;
using UnityEngine.Events;

namespace LWT.UnityWorkbench.CameraSystem
{
    /// <summary>
    /// Trigger zone that modifies camera settings when entered.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CameraZone : MonoBehaviour
    {
        public enum TransitionType
        {
            Instant,
            Smooth,
            Animated
        }

        [System.Serializable]
        public class CameraSettings
        {
            [Header("Position")]
            public Vector3 PositionOffset = new Vector3(0f, 5f, -10f);
            public bool UseWorldOffset = false;

            [Header("Field of View")]
            public bool ChangeFOV = false;
            public float TargetFOV = 60f;

            [Header("Rotation")]
            public bool OverrideRotation = false;
            public Vector3 TargetRotation = Vector3.zero;

            [Header("Look At")]
            public bool UseLookAtPoint = false;
            public Transform LookAtTarget;
        }

        [Header("Zone Settings")]
        [SerializeField] private string _zoneId = "Zone";
        [SerializeField] private int _priority = 0;
        [SerializeField] private string _targetTag = "Player";

        [Header("Camera Settings")]
        [SerializeField] private CameraSettings _settings = new CameraSettings();

        [Header("Transition")]
        [SerializeField] private TransitionType _transitionType = TransitionType.Smooth;
        [SerializeField] private float _transitionDuration = 1f;
        [SerializeField] private AnimationCurve _transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Events")]
        public UnityEvent OnZoneEnter;
        public UnityEvent OnZoneExit;

        private CameraController _cameraController;
        private Camera _mainCamera;
        private bool _isActive;

        // Store original settings for restoration
        private Vector3 _originalOffset;
        private float _originalFOV;
        private Quaternion _originalRotation;

        public string ZoneId => _zoneId;
        public int Priority => _priority;
        public bool IsActive => _isActive;
        public CameraSettings Settings => _settings;

        private void Awake()
        {
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(_targetTag)) return;

            FindCameraComponents();
            if (_cameraController == null && _mainCamera == null) return;

            StoreOriginalSettings();
            ActivateZone();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(_targetTag)) return;
            if (!_isActive) return;

            DeactivateZone();
        }

        private void FindCameraComponents()
        {
            if (_cameraController == null)
            {
                _cameraController = Camera.main?.GetComponent<CameraController>();
            }

            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
        }

        private void StoreOriginalSettings()
        {
            if (_cameraController != null)
            {
                _originalOffset = _cameraController.PositionOffset;
            }

            if (_mainCamera != null)
            {
                _originalFOV = _mainCamera.fieldOfView;
                _originalRotation = _mainCamera.transform.rotation;
            }
        }

        private void ActivateZone()
        {
            _isActive = true;

            switch (_transitionType)
            {
                case TransitionType.Instant:
                    ApplySettingsInstant();
                    break;

                case TransitionType.Smooth:
                case TransitionType.Animated:
                    StartCoroutine(TransitionToSettings());
                    break;
            }

            OnZoneEnter?.Invoke();
        }

        private void DeactivateZone()
        {
            _isActive = false;

            switch (_transitionType)
            {
                case TransitionType.Instant:
                    RestoreSettingsInstant();
                    break;

                case TransitionType.Smooth:
                case TransitionType.Animated:
                    StartCoroutine(TransitionFromSettings());
                    break;
            }

            OnZoneExit?.Invoke();
        }

        private void ApplySettingsInstant()
        {
            if (_cameraController != null)
            {
                _cameraController.SetOffset(_settings.PositionOffset);
            }

            if (_mainCamera != null && _settings.ChangeFOV)
            {
                _mainCamera.fieldOfView = _settings.TargetFOV;
            }
        }

        private void RestoreSettingsInstant()
        {
            if (_cameraController != null)
            {
                _cameraController.SetOffset(_originalOffset);
            }

            if (_mainCamera != null && _settings.ChangeFOV)
            {
                _mainCamera.fieldOfView = _originalFOV;
            }
        }

        private System.Collections.IEnumerator TransitionToSettings()
        {
            float elapsed = 0f;
            Vector3 startOffset = _cameraController?.PositionOffset ?? Vector3.zero;
            float startFOV = _mainCamera?.fieldOfView ?? 60f;

            while (elapsed < _transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = _transitionCurve.Evaluate(elapsed / _transitionDuration);

                if (_cameraController != null)
                {
                    Vector3 newOffset = Vector3.Lerp(startOffset, _settings.PositionOffset, t);
                    _cameraController.SetOffset(newOffset);
                }

                if (_mainCamera != null && _settings.ChangeFOV)
                {
                    _mainCamera.fieldOfView = Mathf.Lerp(startFOV, _settings.TargetFOV, t);
                }

                yield return null;
            }

            ApplySettingsInstant();
        }

        private System.Collections.IEnumerator TransitionFromSettings()
        {
            float elapsed = 0f;
            Vector3 startOffset = _cameraController?.PositionOffset ?? Vector3.zero;
            float startFOV = _mainCamera?.fieldOfView ?? 60f;

            while (elapsed < _transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = _transitionCurve.Evaluate(elapsed / _transitionDuration);

                if (_cameraController != null)
                {
                    Vector3 newOffset = Vector3.Lerp(startOffset, _originalOffset, t);
                    _cameraController.SetOffset(newOffset);
                }

                if (_mainCamera != null && _settings.ChangeFOV)
                {
                    _mainCamera.fieldOfView = Mathf.Lerp(startFOV, _originalFOV, t);
                }

                yield return null;
            }

            RestoreSettingsInstant();
        }

        /// <summary>
        /// Manually activates this zone.
        /// </summary>
        public void ForceActivate()
        {
            FindCameraComponents();
            StoreOriginalSettings();
            ActivateZone();
        }

        /// <summary>
        /// Manually deactivates this zone.
        /// </summary>
        public void ForceDeactivate()
        {
            if (_isActive)
            {
                DeactivateZone();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var collider = GetComponent<Collider>();
            if (collider == null) return;

            Gizmos.color = _isActive ? new Color(0f, 1f, 0f, 0.3f) : new Color(0f, 0.5f, 1f, 0.3f);

            if (collider is BoxCollider box)
            {
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                Gizmos.DrawCube(box.center, box.size);
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (collider is SphereCollider sphere)
            {
                Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius * transform.lossyScale.x);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw camera offset preview
            Gizmos.color = Color.yellow;
            Vector3 previewPos = transform.position + _settings.PositionOffset;
            Gizmos.DrawWireSphere(previewPos, 0.5f);
            Gizmos.DrawLine(transform.position, previewPos);
        }
#endif
    }
}
