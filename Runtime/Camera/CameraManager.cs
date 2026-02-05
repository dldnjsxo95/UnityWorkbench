using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.CameraSystem
{
    /// <summary>
    /// Central camera management for switching between multiple cameras.
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        [System.Serializable]
        public class CameraEntry
        {
            public string Id;
            public Camera Camera;
            public int Priority;
            public bool StartActive;
        }

        [Header("Cameras")]
        [SerializeField] private List<CameraEntry> _cameras = new List<CameraEntry>();
        [SerializeField] private string _defaultCameraId = "Main";

        [Header("Transition")]
        [SerializeField] private float _transitionDuration = 0.5f;
        [SerializeField] private AnimationCurve _transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private CameraEntry _activeCamera;
        private Dictionary<string, CameraEntry> _cameraLookup = new Dictionary<string, CameraEntry>();
        private Stack<CameraEntry> _cameraStack = new Stack<CameraEntry>();

        public Camera ActiveCamera => _activeCamera?.Camera;
        public string ActiveCameraId => _activeCamera?.Id;

        public event System.Action<Camera, Camera> OnCameraChanged;

        private void Awake()
        {
            BuildLookup();
            InitializeCameras();
        }

        private void BuildLookup()
        {
            _cameraLookup.Clear();
            foreach (var entry in _cameras)
            {
                if (!string.IsNullOrEmpty(entry.Id) && entry.Camera != null)
                {
                    _cameraLookup[entry.Id] = entry;
                }
            }
        }

        private void InitializeCameras()
        {
            // Disable all cameras first
            foreach (var entry in _cameras)
            {
                if (entry.Camera != null)
                {
                    entry.Camera.gameObject.SetActive(false);
                }
            }

            // Find and activate default camera
            if (_cameraLookup.TryGetValue(_defaultCameraId, out var defaultCam))
            {
                ActivateCamera(defaultCam);
            }
            else if (_cameras.Count > 0)
            {
                ActivateCamera(_cameras[0]);
            }
        }

        /// <summary>
        /// Switches to a camera by ID.
        /// </summary>
        public void SwitchCamera(string cameraId, bool instant = false)
        {
            if (!_cameraLookup.TryGetValue(cameraId, out var entry))
            {
                Debug.LogWarning($"[CameraManager] Camera not found: {cameraId}");
                return;
            }

            if (instant)
            {
                ActivateCamera(entry);
            }
            else
            {
                StartCoroutine(TransitionToCamera(entry));
            }
        }

        /// <summary>
        /// Pushes current camera to stack and switches to new one.
        /// </summary>
        public void PushCamera(string cameraId, bool instant = false)
        {
            if (_activeCamera != null)
            {
                _cameraStack.Push(_activeCamera);
            }

            SwitchCamera(cameraId, instant);
        }

        /// <summary>
        /// Pops and returns to previous camera.
        /// </summary>
        public void PopCamera(bool instant = false)
        {
            if (_cameraStack.Count == 0)
            {
                Debug.LogWarning("[CameraManager] Camera stack is empty.");
                return;
            }

            var previousCamera = _cameraStack.Pop();

            if (instant)
            {
                ActivateCamera(previousCamera);
            }
            else
            {
                StartCoroutine(TransitionToCamera(previousCamera));
            }
        }

        /// <summary>
        /// Registers a new camera at runtime.
        /// </summary>
        public void RegisterCamera(string id, Camera camera, int priority = 0)
        {
            var entry = new CameraEntry
            {
                Id = id,
                Camera = camera,
                Priority = priority
            };

            _cameras.Add(entry);
            _cameraLookup[id] = entry;
            camera.gameObject.SetActive(false);
        }

        /// <summary>
        /// Unregisters a camera.
        /// </summary>
        public void UnregisterCamera(string id)
        {
            if (_cameraLookup.TryGetValue(id, out var entry))
            {
                _cameras.Remove(entry);
                _cameraLookup.Remove(id);

                // Switch to default if we removed the active camera
                if (_activeCamera == entry)
                {
                    SwitchCamera(_defaultCameraId, true);
                }
            }
        }

        /// <summary>
        /// Gets a camera by ID.
        /// </summary>
        public Camera GetCamera(string id)
        {
            return _cameraLookup.TryGetValue(id, out var entry) ? entry.Camera : null;
        }

        /// <summary>
        /// Checks if a camera exists.
        /// </summary>
        public bool HasCamera(string id)
        {
            return _cameraLookup.ContainsKey(id);
        }

        private void ActivateCamera(CameraEntry entry)
        {
            if (entry == null || entry.Camera == null) return;

            Camera previousCamera = _activeCamera?.Camera;

            // Deactivate previous
            if (_activeCamera != null && _activeCamera.Camera != null)
            {
                _activeCamera.Camera.gameObject.SetActive(false);
            }

            // Activate new
            _activeCamera = entry;
            _activeCamera.Camera.gameObject.SetActive(true);

            OnCameraChanged?.Invoke(previousCamera, _activeCamera.Camera);
        }

        private System.Collections.IEnumerator TransitionToCamera(CameraEntry entry)
        {
            if (entry == null || entry.Camera == null) yield break;

            Camera previousCamera = _activeCamera?.Camera;
            Camera newCamera = entry.Camera;

            // Enable both cameras temporarily
            newCamera.gameObject.SetActive(true);

            // Get camera effects for fade
            var effects = previousCamera?.GetComponent<CameraEffects>();

            if (effects != null)
            {
                yield return effects.FadeIn(_transitionDuration / 2f);
            }

            // Switch
            ActivateCamera(entry);

            if (effects != null)
            {
                yield return effects.FadeOut(_transitionDuration / 2f);
            }

            OnCameraChanged?.Invoke(previousCamera, newCamera);
        }

        /// <summary>
        /// Gets all registered camera IDs.
        /// </summary>
        public IEnumerable<string> GetAllCameraIds()
        {
            return _cameraLookup.Keys;
        }

        /// <summary>
        /// Clears the camera stack.
        /// </summary>
        public void ClearStack()
        {
            _cameraStack.Clear();
        }
    }
}
