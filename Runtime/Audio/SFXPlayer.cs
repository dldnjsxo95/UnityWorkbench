using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Audio
{
    /// <summary>
    /// Handles sound effect playback with object pooling.
    /// </summary>
    public class SFXPlayer : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private SoundBank _soundBank;
        [SerializeField] private int _initialPoolSize = 10;
        [SerializeField] private int _maxPoolSize = 50;
        [SerializeField] private bool _allowPoolGrowth = true;

        private List<AudioSource> _pool = new List<AudioSource>();
        private List<AudioSource> _activeSources = new List<AudioSource>();
        private Transform _poolContainer;

        private void Awake()
        {
            CreatePool();
        }

        private void CreatePool()
        {
            _poolContainer = new GameObject("SFX_Pool").transform;
            _poolContainer.SetParent(transform);

            for (int i = 0; i < _initialPoolSize; i++)
            {
                CreatePooledSource();
            }
        }

        private AudioSource CreatePooledSource()
        {
            var go = new GameObject("SFX_Source");
            go.transform.SetParent(_poolContainer);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            go.SetActive(false);
            _pool.Add(source);
            return source;
        }

        private AudioSource GetSource()
        {
            // Find available source in pool
            foreach (var source in _pool)
            {
                if (!source.gameObject.activeSelf)
                {
                    return source;
                }
            }

            // Grow pool if allowed
            if (_allowPoolGrowth && _pool.Count < _maxPoolSize)
            {
                return CreatePooledSource();
            }

            // Steal oldest active source
            if (_activeSources.Count > 0)
            {
                var oldest = _activeSources[0];
                _activeSources.RemoveAt(0);
                oldest.Stop();
                return oldest;
            }

            return null;
        }

        private void ReturnToPool(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
            _activeSources.Remove(source);
        }

        /// <summary>
        /// Sets the sound bank.
        /// </summary>
        public void SetBank(SoundBank bank)
        {
            _soundBank = bank;
        }

        /// <summary>
        /// Plays a sound by ID.
        /// </summary>
        public AudioSource Play(string soundId)
        {
            if (_soundBank == null)
            {
                Debug.LogWarning("[SFXPlayer] No sound bank assigned.");
                return null;
            }

            var sound = _soundBank.GetSound(soundId);
            if (sound == null) return null;

            return Play(sound);
        }

        /// <summary>
        /// Plays a SoundData directly.
        /// </summary>
        public AudioSource Play(SoundData sound)
        {
            if (sound == null || sound.Clip == null) return null;

            var source = GetSource();
            if (source == null)
            {
                Debug.LogWarning("[SFXPlayer] No available audio sources.");
                return null;
            }

            source.gameObject.SetActive(true);
            source.transform.localPosition = Vector3.zero;

            // Handle variants
            if (sound is SoundDataVariant variant)
            {
                variant.ApplyWithVariant(source);
            }
            else
            {
                sound.ApplyTo(source);
            }

            source.Play();
            _activeSources.Add(source);

            // Auto-return to pool when done (if not looping)
            if (!sound.Loop)
            {
                StartCoroutine(ReturnWhenDone(source, sound.Duration));
            }

            return source;
        }

        /// <summary>
        /// Plays a sound at a world position.
        /// </summary>
        public AudioSource PlayAt(string soundId, Vector3 position)
        {
            if (_soundBank == null) return null;

            var sound = _soundBank.GetSound(soundId);
            if (sound == null) return null;

            return PlayAt(sound, position);
        }

        /// <summary>
        /// Plays a SoundData at a world position.
        /// </summary>
        public AudioSource PlayAt(SoundData sound, Vector3 position)
        {
            var source = Play(sound);
            if (source != null)
            {
                source.transform.position = position;
            }
            return source;
        }

        /// <summary>
        /// Plays a sound attached to a transform.
        /// </summary>
        public AudioSource PlayAttached(string soundId, Transform parent)
        {
            var source = Play(soundId);
            if (source != null)
            {
                source.transform.SetParent(parent);
                source.transform.localPosition = Vector3.zero;
            }
            return source;
        }

        /// <summary>
        /// Plays a one-shot sound (fire and forget, no control).
        /// </summary>
        public void PlayOneShot(string soundId)
        {
            if (_soundBank == null) return;

            var sound = _soundBank.GetSound(soundId);
            if (sound == null || sound.Clip == null) return;

            var source = GetSource();
            if (source == null) return;

            source.gameObject.SetActive(true);
            sound.ApplyTo(source);
            source.PlayOneShot(sound.Clip, sound.GetVolume());

            _activeSources.Add(source);
            StartCoroutine(ReturnWhenDone(source, sound.Duration));
        }

        /// <summary>
        /// Plays an AudioClip directly.
        /// </summary>
        public AudioSource PlayClip(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return null;

            var source = GetSource();
            if (source == null) return null;

            source.gameObject.SetActive(true);
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.loop = false;
            source.spatialBlend = 0f;
            source.Play();

            _activeSources.Add(source);
            StartCoroutine(ReturnWhenDone(source, clip.length / pitch));

            return source;
        }

        private System.Collections.IEnumerator ReturnWhenDone(AudioSource source, float duration)
        {
            yield return new WaitForSeconds(duration + 0.1f);

            if (source != null && _activeSources.Contains(source))
            {
                ReturnToPool(source);
            }
        }

        /// <summary>
        /// Stops a specific audio source.
        /// </summary>
        public void Stop(AudioSource source)
        {
            if (source != null && _activeSources.Contains(source))
            {
                ReturnToPool(source);
            }
        }

        /// <summary>
        /// Stops all sounds.
        /// </summary>
        public void StopAll()
        {
            for (int i = _activeSources.Count - 1; i >= 0; i--)
            {
                ReturnToPool(_activeSources[i]);
            }
            _activeSources.Clear();
        }

        /// <summary>
        /// Pauses all active sounds.
        /// </summary>
        public void PauseAll()
        {
            foreach (var source in _activeSources)
            {
                if (source.isPlaying)
                {
                    source.Pause();
                }
            }
        }

        /// <summary>
        /// Resumes all paused sounds.
        /// </summary>
        public void ResumeAll()
        {
            foreach (var source in _activeSources)
            {
                source.UnPause();
            }
        }

        /// <summary>
        /// Gets the number of active sound sources.
        /// </summary>
        public int ActiveCount => _activeSources.Count;

        /// <summary>
        /// Gets the current pool size.
        /// </summary>
        public int PoolSize => _pool.Count;

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
