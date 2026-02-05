using System.Collections;
using UnityEngine;

namespace LWT.UnityWorkbench.Audio
{
    /// <summary>
    /// Handles background music playback with crossfade support.
    /// </summary>
    public class MusicPlayer : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private MusicBank _musicBank;
        [SerializeField] private float _defaultFadeTime = 1f;
        [SerializeField] private bool _playOnAwake = false;
        [SerializeField] private string _defaultTrackId;

        private AudioSource _sourceA;
        private AudioSource _sourceB;
        private AudioSource _activeSource;
        private MusicData _currentTrack;
        private Coroutine _fadeCoroutine;
        private bool _isPaused;

        public MusicData CurrentTrack => _currentTrack;
        public bool IsPlaying => _activeSource != null && _activeSource.isPlaying && !_isPaused;
        public bool IsPaused => _isPaused;
        public float CurrentTime => _activeSource != null ? _activeSource.time : 0f;

        private void Awake()
        {
            CreateAudioSources();
        }

        private void Start()
        {
            if (_playOnAwake && !string.IsNullOrEmpty(_defaultTrackId))
            {
                Play(_defaultTrackId, 0f);
            }
        }

        private void CreateAudioSources()
        {
            _sourceA = CreateSource("MusicSource_A");
            _sourceB = CreateSource("MusicSource_B");
            _activeSource = _sourceA;
        }

        private AudioSource CreateSource(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.priority = 0;
            return source;
        }

        /// <summary>
        /// Sets the music bank.
        /// </summary>
        public void SetBank(MusicBank bank)
        {
            _musicBank = bank;
        }

        /// <summary>
        /// Plays a music track by ID.
        /// </summary>
        public void Play(string trackId, float fadeTime = -1f)
        {
            if (_musicBank == null)
            {
                Debug.LogWarning("[MusicPlayer] No music bank assigned.");
                return;
            }

            var track = _musicBank.GetTrack(trackId);
            if (track == null) return;

            Play(track, fadeTime);
        }

        /// <summary>
        /// Plays a MusicData directly.
        /// </summary>
        public void Play(MusicData track, float fadeTime = -1f)
        {
            if (track == null || track.Clip == null)
            {
                Debug.LogWarning("[MusicPlayer] Invalid track or missing clip.");
                return;
            }

            if (fadeTime < 0f) fadeTime = _defaultFadeTime;

            // If same track is already playing, don't restart
            if (_currentTrack == track && IsPlaying) return;

            _currentTrack = track;
            _isPaused = false;

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(CrossfadeTo(track, fadeTime));
        }

        /// <summary>
        /// Stops the current music.
        /// </summary>
        public void Stop(float fadeTime = -1f)
        {
            if (fadeTime < 0f) fadeTime = _defaultFadeTime;

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            if (fadeTime <= 0f)
            {
                _sourceA.Stop();
                _sourceB.Stop();
                _currentTrack = null;
            }
            else
            {
                _fadeCoroutine = StartCoroutine(FadeOut(fadeTime));
            }
        }

        /// <summary>
        /// Pauses the current music.
        /// </summary>
        public void Pause()
        {
            if (!IsPlaying) return;

            _isPaused = true;
            _activeSource?.Pause();
        }

        /// <summary>
        /// Resumes the paused music.
        /// </summary>
        public void Resume()
        {
            if (!_isPaused) return;

            _isPaused = false;
            _activeSource?.UnPause();
        }

        /// <summary>
        /// Sets the playback time.
        /// </summary>
        public void SetTime(float time)
        {
            if (_activeSource != null && _activeSource.clip != null)
            {
                _activeSource.time = Mathf.Clamp(time, 0f, _activeSource.clip.length);
            }
        }

        /// <summary>
        /// Sets the volume multiplier for music.
        /// </summary>
        public void SetVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            _sourceA.volume = volume;
            _sourceB.volume = volume;
        }

        private IEnumerator CrossfadeTo(MusicData track, float fadeTime)
        {
            var inactiveSource = _activeSource == _sourceA ? _sourceB : _sourceA;

            // Setup new source
            track.ApplyTo(inactiveSource);
            inactiveSource.volume = 0f;
            inactiveSource.Play();

            float elapsed = 0f;
            float startVolumeActive = _activeSource.volume;
            float targetVolume = track.Volume;

            if (fadeTime > 0f)
            {
                while (elapsed < fadeTime)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = elapsed / fadeTime;

                    // Fade out old
                    _activeSource.volume = Mathf.Lerp(startVolumeActive, 0f, t);
                    // Fade in new
                    inactiveSource.volume = Mathf.Lerp(0f, targetVolume, t);

                    yield return null;
                }
            }

            // Finalize
            _activeSource.Stop();
            _activeSource.volume = 0f;

            inactiveSource.volume = targetVolume;
            _activeSource = inactiveSource;

            _fadeCoroutine = null;
        }

        private IEnumerator FadeOut(float fadeTime)
        {
            float startVolume = _activeSource.volume;
            float elapsed = 0f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.unscaledDeltaTime;
                _activeSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeTime);
                yield return null;
            }

            _activeSource.Stop();
            _activeSource.volume = 0f;
            _currentTrack = null;
            _fadeCoroutine = null;
        }

        /// <summary>
        /// Plays a random track from the bank.
        /// </summary>
        public void PlayRandom(float fadeTime = -1f)
        {
            if (_musicBank == null) return;

            var track = _musicBank.GetRandomTrack();
            if (track != null)
            {
                Play(track, fadeTime);
            }
        }

        /// <summary>
        /// Crossfades to the next track (if using a playlist).
        /// </summary>
        public void Next(float fadeTime = -1f)
        {
            PlayRandom(fadeTime);
        }
    }
}
