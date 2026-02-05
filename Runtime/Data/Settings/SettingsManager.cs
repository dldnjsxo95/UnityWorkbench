using UnityEngine;
using LWT.UnityWorkbench.Core;

namespace LWT.UnityWorkbench.Data
{
    /// <summary>
    /// Central manager for game settings.
    /// Auto-loads on start, saves on change.
    /// </summary>
    public class SettingsManager : PersistentMonoSingleton<SettingsManager>
    {
        private const string SETTINGS_KEY = "game_settings";

        [SerializeField] private bool _autoSaveOnChange = true;
        [SerializeField] private bool _loadOnStart = true;

        private GameSettings _settings;
        private ISaveSystem _saveSystem;

        public GameSettings Settings => _settings ??= new GameSettings();

        // Quick access
        public GraphicsSettings Graphics => Settings.Graphics;
        public AudioSettings Audio => Settings.Audio;
        public ControlSettings Controls => Settings.Controls;
        public GameplaySettings Gameplay => Settings.Gameplay;

        public event System.Action OnSettingsChanged;

        protected override void OnSingletonAwake()
        {
            _saveSystem = new JsonSaveSystem("Settings", ".cfg", false);

            if (_loadOnStart)
            {
                Load();
            }
        }

        /// <summary>
        /// Save current settings.
        /// </summary>
        public void Save()
        {
            _saveSystem.Save(SETTINGS_KEY, _settings);
            Debug.Log("[SettingsManager] Settings saved");
        }

        /// <summary>
        /// Load settings from disk.
        /// </summary>
        public void Load()
        {
            _settings = _saveSystem.Load<GameSettings>(SETTINGS_KEY);

            if (_settings == null)
            {
                _settings = new GameSettings();
                Debug.Log("[SettingsManager] Created default settings");
            }
            else
            {
                Debug.Log("[SettingsManager] Settings loaded");
            }

            _settings.Apply();
        }

        /// <summary>
        /// Reset to defaults and save.
        /// </summary>
        public void ResetToDefaults()
        {
            _settings = new GameSettings();
            _settings.Apply();
            Save();
            OnSettingsChanged?.Invoke();
            Debug.Log("[SettingsManager] Reset to defaults");
        }

        /// <summary>
        /// Apply and optionally save changes.
        /// </summary>
        public void ApplyChanges()
        {
            _settings.Apply();
            OnSettingsChanged?.Invoke();

            if (_autoSaveOnChange)
            {
                Save();
            }
        }

        // ===== Convenience Methods =====

        public void SetMasterVolume(float volume)
        {
            Audio.MasterVolume = Mathf.Clamp01(volume);
            Audio.Apply();
            if (_autoSaveOnChange) Save();
        }

        public void SetMusicVolume(float volume)
        {
            Audio.MusicVolume = Mathf.Clamp01(volume);
            Audio.Apply();
            if (_autoSaveOnChange) Save();
        }

        public void SetSFXVolume(float volume)
        {
            Audio.SFXVolume = Mathf.Clamp01(volume);
            Audio.Apply();
            if (_autoSaveOnChange) Save();
        }

        public void SetQualityLevel(int level)
        {
            Graphics.QualityLevel = level;
            Graphics.Apply();
            if (_autoSaveOnChange) Save();
        }

        public void SetFullscreen(bool fullscreen)
        {
            Graphics.Fullscreen = fullscreen;
            Graphics.Apply();
            if (_autoSaveOnChange) Save();
        }

        public void SetMouseSensitivity(float sensitivity)
        {
            Controls.MouseSensitivity = sensitivity;
            Controls.Apply();
            if (_autoSaveOnChange) Save();
        }

        public void SetDifficulty(int difficulty)
        {
            Gameplay.DifficultyLevel = difficulty;
            Gameplay.Apply();
            if (_autoSaveOnChange) Save();
        }

        public void ToggleMute()
        {
            Audio.MuteAll = !Audio.MuteAll;
            Audio.Apply();
            if (_autoSaveOnChange) Save();
        }
    }
}
