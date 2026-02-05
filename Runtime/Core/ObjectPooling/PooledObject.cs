using UnityEngine;

namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// Attach to pooled objects for automatic despawn functionality.
    /// Provides convenience methods and auto-return options.
    /// </summary>
    public class PooledObject : MonoBehaviour, IPoolable
    {
        [Header("Auto Despawn")]
        [SerializeField] private bool _autoDespawn;
        [SerializeField] private float _lifetime = 5f;

        [Header("Reset Options")]
        [SerializeField] private bool _resetVelocity = true;
        [SerializeField] private bool _resetScale = true;

        private float _spawnTime;
        private Vector3 _originalScale;
        private Rigidbody _rigidbody;
        private Rigidbody2D _rigidbody2D;

        private void Awake()
        {
            _originalScale = transform.localScale;
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (_autoDespawn && Time.time - _spawnTime >= _lifetime)
            {
                Despawn();
            }
        }

        public void OnSpawn()
        {
            _spawnTime = Time.time;

            if (_resetVelocity)
            {
                if (_rigidbody != null)
                {
#if UNITY_6000_0_OR_NEWER
                    _rigidbody.linearVelocity = Vector3.zero;
#else
                    _rigidbody.velocity = Vector3.zero;
#endif
                    _rigidbody.angularVelocity = Vector3.zero;
                }
                if (_rigidbody2D != null)
                {
#if UNITY_6000_0_OR_NEWER
                    _rigidbody2D.linearVelocity = Vector2.zero;
#else
                    _rigidbody2D.velocity = Vector2.zero;
#endif
                    _rigidbody2D.angularVelocity = 0f;
                }
            }

            if (_resetScale)
            {
                transform.localScale = _originalScale;
            }
        }

        public void OnDespawn()
        {
            // Override in child classes for custom cleanup
        }

        /// <summary>
        /// Return this object to pool.
        /// </summary>
        public void Despawn()
        {
            PoolManager.Instance.Despawn(gameObject);
        }

        /// <summary>
        /// Return this object to pool after delay.
        /// </summary>
        public void Despawn(float delay)
        {
            PoolManager.Instance.Despawn(gameObject, delay);
        }

        /// <summary>
        /// Set auto despawn lifetime.
        /// </summary>
        public void SetLifetime(float lifetime)
        {
            _lifetime = lifetime;
            _autoDespawn = true;
            _spawnTime = Time.time;
        }

        /// <summary>
        /// Disable auto despawn.
        /// </summary>
        public void DisableAutoDespawn()
        {
            _autoDespawn = false;
        }
    }
}
