using System.Collections;
using UnityEngine;

namespace LWT.UnityWorkbench.CameraSystem
{
    /// <summary>
    /// Camera shake effect system with multiple shake types.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        public enum ShakeType
        {
            Perlin,
            Random,
            Sine
        }

        [System.Serializable]
        public class ShakePreset
        {
            public string Name = "Default";
            public ShakeType Type = ShakeType.Perlin;
            public float Duration = 0.5f;
            public float Magnitude = 0.5f;
            public float Frequency = 25f;
            public AnimationCurve FalloffCurve = AnimationCurve.Linear(1f, 1f, 0f, 0f);
            public bool AffectPosition = true;
            public bool AffectRotation = false;
            public float RotationMagnitude = 5f;
        }

        [Header("Settings")]
        [SerializeField] private bool _useUnscaledTime = true;
        [SerializeField] private float _maxShakeMagnitude = 2f;

        [Header("Presets")]
        [SerializeField] private ShakePreset[] _presets = new ShakePreset[]
        {
            new ShakePreset { Name = "Light", Magnitude = 0.1f, Duration = 0.2f },
            new ShakePreset { Name = "Medium", Magnitude = 0.3f, Duration = 0.3f },
            new ShakePreset { Name = "Heavy", Magnitude = 0.5f, Duration = 0.5f },
            new ShakePreset { Name = "Explosion", Magnitude = 1f, Duration = 0.8f, AffectRotation = true }
        };

        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private float _currentMagnitude;
        private float _traumaAmount;
        private Coroutine _shakeCoroutine;
        private Transform _cameraTransform;

        public bool IsShaking => _traumaAmount > 0f || _shakeCoroutine != null;
        public float TraumaAmount => _traumaAmount;

        private float DeltaTime => _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        private float CurrentTime => _useUnscaledTime ? Time.unscaledTime : Time.time;

        private void Awake()
        {
            _cameraTransform = transform;
            _originalPosition = _cameraTransform.localPosition;
            _originalRotation = _cameraTransform.localRotation;
        }

        private void LateUpdate()
        {
            if (_traumaAmount > 0f)
            {
                ApplyTraumaShake();
                _traumaAmount = Mathf.Max(0f, _traumaAmount - DeltaTime);
            }
        }

        /// <summary>
        /// Triggers a shake with specified parameters.
        /// </summary>
        public void Shake(float duration, float magnitude, ShakeType type = ShakeType.Perlin)
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
            }

            var preset = new ShakePreset
            {
                Duration = duration,
                Magnitude = magnitude,
                Type = type
            };

            _shakeCoroutine = StartCoroutine(ShakeRoutine(preset));
        }

        /// <summary>
        /// Triggers a shake using a preset by name.
        /// </summary>
        public void ShakeByPreset(string presetName)
        {
            var preset = GetPreset(presetName);
            if (preset != null)
            {
                Shake(preset);
            }
        }

        /// <summary>
        /// Triggers a shake using a preset.
        /// </summary>
        public void Shake(ShakePreset preset)
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
            }

            _shakeCoroutine = StartCoroutine(ShakeRoutine(preset));
        }

        /// <summary>
        /// Adds trauma for trauma-based shake (accumulative).
        /// </summary>
        public void AddTrauma(float amount)
        {
            _traumaAmount = Mathf.Clamp01(_traumaAmount + amount);
        }

        /// <summary>
        /// Stops any active shake.
        /// </summary>
        public void StopShake()
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                _shakeCoroutine = null;
            }

            _traumaAmount = 0f;
            _cameraTransform.localPosition = _originalPosition;
            _cameraTransform.localRotation = _originalRotation;
        }

        private IEnumerator ShakeRoutine(ShakePreset preset)
        {
            float elapsed = 0f;
            float seed = Random.value * 1000f;

            while (elapsed < preset.Duration)
            {
                float progress = elapsed / preset.Duration;
                float falloff = preset.FalloffCurve.Evaluate(progress);
                float currentMag = Mathf.Min(preset.Magnitude * falloff, _maxShakeMagnitude);

                Vector3 offset = GetShakeOffset(preset.Type, currentMag, preset.Frequency, seed);

                if (preset.AffectPosition)
                {
                    _cameraTransform.localPosition = _originalPosition + offset;
                }

                if (preset.AffectRotation)
                {
                    Vector3 rotOffset = GetShakeOffset(preset.Type, preset.RotationMagnitude * falloff, preset.Frequency * 0.5f, seed + 100f);
                    _cameraTransform.localRotation = _originalRotation * Quaternion.Euler(rotOffset);
                }

                elapsed += DeltaTime;
                yield return null;
            }

            _cameraTransform.localPosition = _originalPosition;
            _cameraTransform.localRotation = _originalRotation;
            _shakeCoroutine = null;
        }

        private void ApplyTraumaShake()
        {
            float shake = _traumaAmount * _traumaAmount; // Quadratic for smoother feel
            float magnitude = Mathf.Min(shake * _maxShakeMagnitude, _maxShakeMagnitude);

            float time = CurrentTime;
            Vector3 offset = new Vector3(
                (Mathf.PerlinNoise(time * 25f, 0f) - 0.5f) * 2f * magnitude,
                (Mathf.PerlinNoise(0f, time * 25f) - 0.5f) * 2f * magnitude,
                0f
            );

            _cameraTransform.localPosition = _originalPosition + offset;
        }

        private Vector3 GetShakeOffset(ShakeType type, float magnitude, float frequency, float seed)
        {
            float time = CurrentTime;

            switch (type)
            {
                case ShakeType.Perlin:
                    return new Vector3(
                        (Mathf.PerlinNoise(seed, time * frequency) - 0.5f) * 2f * magnitude,
                        (Mathf.PerlinNoise(seed + 100f, time * frequency) - 0.5f) * 2f * magnitude,
                        (Mathf.PerlinNoise(seed + 200f, time * frequency) - 0.5f) * 2f * magnitude
                    );

                case ShakeType.Random:
                    return new Vector3(
                        Random.Range(-1f, 1f) * magnitude,
                        Random.Range(-1f, 1f) * magnitude,
                        Random.Range(-1f, 1f) * magnitude
                    );

                case ShakeType.Sine:
                    return new Vector3(
                        Mathf.Sin(time * frequency) * magnitude,
                        Mathf.Cos(time * frequency * 1.1f) * magnitude,
                        Mathf.Sin(time * frequency * 0.9f) * magnitude
                    );

                default:
                    return Vector3.zero;
            }
        }

        private ShakePreset GetPreset(string name)
        {
            foreach (var preset in _presets)
            {
                if (preset.Name == name)
                {
                    return preset;
                }
            }
            Debug.LogWarning($"[CameraShake] Preset not found: {name}");
            return null;
        }

        /// <summary>
        /// Updates the original position (call after camera moves).
        /// </summary>
        public void UpdateOriginalPosition()
        {
            if (!IsShaking)
            {
                _originalPosition = _cameraTransform.localPosition;
                _originalRotation = _cameraTransform.localRotation;
            }
        }

        // Convenience methods
        public void ShakeLight() => ShakeByPreset("Light");
        public void ShakeMedium() => ShakeByPreset("Medium");
        public void ShakeHeavy() => ShakeByPreset("Heavy");
        public void ShakeExplosion() => ShakeByPreset("Explosion");
    }
}
