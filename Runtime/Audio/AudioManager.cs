using UnityEngine;
using UnityEngine.Audio;

namespace LWT.UnityWorkbench.Audio
{
    /// <summary>
    /// Central audio management singleton.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<AudioManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[AudioManager]");
                        _instance = go.AddComponent<AudioManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Configuration")]
        [SerializeField] private AudioMixer _masterMixer;
        [SerializeField] private SoundBank _defaultSoundBank;
        [SerializeField] private MusicBank _defaultMusicBank;

        [Header("Volume Settings")]
        [SerializeField, Range(0f, 1f)] private float _masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float _musicVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float _sfxVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float _uiVolume = 1f;

        [Header("Mixer Parameters")]
        [SerializeField] private string _masterVolumeParam = "MasterVolume";
        [SerializeField] private string _musicVolumeParam = "MusicVolume";
        [SerializeField] private string _sfxVolumeParam = "SFXVolume";
        [SerializeField] private string _uiVolumeParam = "UIVolume";

        private MusicPlayer _musicPlayer;
        private SFXPlayer _sfxPlayer;

        public SoundBank DefaultSoundBank => _defaultSoundBank;
        public MusicBank DefaultMusicBank => _defaultMusicBank;
        public MusicPlayer Music => _musicPlayer;
        public SFXPlayer SFX => _sfxPlayer;

        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Mathf.Clamp01(value);
                ApplyMixerVolume(_masterVolumeParam, _masterVolume);
                PlayerPrefs.SetFloat("Audio_Master", _masterVolume);
            }
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp01(value);
                ApplyMixerVolume(_musicVolumeParam, _musicVolume);
                PlayerPrefs.SetFloat("Audio_Music", _musicVolume);
            }
        }

        public float SFXVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
                ApplyMixerVolume(_sfxVolumeParam, _sfxVolume);
                PlayerPrefs.SetFloat("Audio_SFX", _sfxVolume);
            }
        }

        public float UIVolume
        {
            get => _uiVolume;
            set
            {
                _uiVolume = Mathf.Clamp01(value);
                ApplyMixerVolume(_uiVolumeParam, _uiVolume);
                PlayerPrefs.SetFloat("Audio_UI", _uiVolume);
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeComponents();
            LoadVolumeSettings();
        }

        private void InitializeComponents()
        {
            // Create or get MusicPlayer
            _musicPlayer = GetComponentInChildren<MusicPlayer>();
            if (_musicPlayer == null)
            {
                var musicGO = new GameObject("MusicPlayer");
                musicGO.transform.SetParent(transform);
                _musicPlayer = musicGO.AddComponent<MusicPlayer>();
            }

            // Create or get SFXPlayer
            _sfxPlayer = GetComponentInChildren<SFXPlayer>();
            if (_sfxPlayer == null)
            {
                var sfxGO = new GameObject("SFXPlayer");
                sfxGO.transform.SetParent(transform);
                _sfxPlayer = sfxGO.AddComponent<SFXPlayer>();
            }

            // Set default banks
            if (_defaultMusicBank != null)
            {
                _musicPlayer.SetBank(_defaultMusicBank);
            }

            if (_defaultSoundBank != null)
            {
                _sfxPlayer.SetBank(_defaultSoundBank);
            }
        }

        private void LoadVolumeSettings()
        {
            _masterVolume = PlayerPrefs.GetFloat("Audio_Master", 1f);
            _musicVolume = PlayerPrefs.GetFloat("Audio_Music", 1f);
            _sfxVolume = PlayerPrefs.GetFloat("Audio_SFX", 1f);
            _uiVolume = PlayerPrefs.GetFloat("Audio_UI", 1f);

            ApplyAllVolumes();
        }

        private void ApplyAllVolumes()
        {
            ApplyMixerVolume(_masterVolumeParam, _masterVolume);
            ApplyMixerVolume(_musicVolumeParam, _musicVolume);
            ApplyMixerVolume(_sfxVolumeParam, _sfxVolume);
            ApplyMixerVolume(_uiVolumeParam, _uiVolume);
        }

        private void ApplyMixerVolume(string parameter, float linearVolume)
        {
            if (_masterMixer == null || string.IsNullOrEmpty(parameter)) return;

            // Convert linear (0-1) to decibels (-80 to 0)
            float dB = linearVolume > 0.0001f ? Mathf.Log10(linearVolume) * 20f : -80f;
            _masterMixer.SetFloat(parameter, dB);
        }

        /// <summary>
        /// Gets a mixer volume in linear scale.
        /// </summary>
        public float GetMixerVolume(string parameter)
        {
            if (_masterMixer == null) return 1f;

            if (_masterMixer.GetFloat(parameter, out float dB))
            {
                return Mathf.Pow(10f, dB / 20f);
            }
            return 1f;
        }

        /// <summary>
        /// Plays a sound by ID from the default bank.
        /// </summary>
        public AudioSource PlaySound(string id)
        {
            return _sfxPlayer?.Play(id);
        }

        /// <summary>
        /// Plays a sound at a position.
        /// </summary>
        public AudioSource PlaySoundAt(string id, Vector3 position)
        {
            return _sfxPlayer?.PlayAt(id, position);
        }

        /// <summary>
        /// Plays music by ID from the default bank.
        /// </summary>
        public void PlayMusic(string id, float fadeTime = 1f)
        {
            _musicPlayer?.Play(id, fadeTime);
        }

        /// <summary>
        /// Stops the current music.
        /// </summary>
        public void StopMusic(float fadeTime = 1f)
        {
            _musicPlayer?.Stop(fadeTime);
        }

        /// <summary>
        /// Pauses all audio.
        /// </summary>
        public void PauseAll()
        {
            _musicPlayer?.Pause();
            _sfxPlayer?.PauseAll();
        }

        /// <summary>
        /// Resumes all audio.
        /// </summary>
        public void ResumeAll()
        {
            _musicPlayer?.Resume();
            _sfxPlayer?.ResumeAll();
        }

        /// <summary>
        /// Stops all audio immediately.
        /// </summary>
        public void StopAll()
        {
            _musicPlayer?.Stop(0f);
            _sfxPlayer?.StopAll();
        }

        /// <summary>
        /// Mutes all audio.
        /// </summary>
        public void MuteAll(bool mute)
        {
            if (_masterMixer != null)
            {
                ApplyMixerVolume(_masterVolumeParam, mute ? 0f : _masterVolume);
            }
        }

        /// <summary>
        /// Resets all volumes to default.
        /// </summary>
        public void ResetVolumes()
        {
            MasterVolume = 1f;
            MusicVolume = 1f;
            SFXVolume = 1f;
            UIVolume = 1f;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                ApplyAllVolumes();
            }
        }
#endif
    }
}
