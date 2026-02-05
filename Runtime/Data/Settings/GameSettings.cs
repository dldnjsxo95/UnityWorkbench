using System;
using UnityEngine;

namespace LWT.UnityWorkbench.Data
{
    /// <summary>
    /// Game settings data container.
    /// Handles graphics, audio, and control settings.
    /// </summary>
    [Serializable]
    public class GameSettings
    {
        // Graphics
        public GraphicsSettings Graphics = new GraphicsSettings();

        // Audio
        public AudioSettings Audio = new AudioSettings();

        // Controls
        public ControlSettings Controls = new ControlSettings();

        // Gameplay
        public GameplaySettings Gameplay = new GameplaySettings();

        /// <summary>
        /// Apply all settings.
        /// </summary>
        public void Apply()
        {
            Graphics.Apply();
            Audio.Apply();
            Controls.Apply();
            Gameplay.Apply();
        }

        /// <summary>
        /// Reset all to defaults.
        /// </summary>
        public void ResetToDefaults()
        {
            Graphics = new GraphicsSettings();
            Audio = new AudioSettings();
            Controls = new ControlSettings();
            Gameplay = new GameplaySettings();
        }
    }

    [Serializable]
    public class GraphicsSettings
    {
        public int ResolutionIndex = -1; // -1 = native
        public bool Fullscreen = true;
        public int QualityLevel = 2; // Medium
        public bool VSync = true;
        public int TargetFrameRate = 60;
        public float Brightness = 1f;

        public void Apply()
        {
            // Resolution
            if (ResolutionIndex >= 0 && ResolutionIndex < Screen.resolutions.Length)
            {
                var res = Screen.resolutions[ResolutionIndex];
                Screen.SetResolution(res.width, res.height, Fullscreen);
            }
            else
            {
                Screen.fullScreen = Fullscreen;
            }

            // Quality
            QualitySettings.SetQualityLevel(QualityLevel);

            // VSync & Frame Rate
            QualitySettings.vSyncCount = VSync ? 1 : 0;
            Application.targetFrameRate = VSync ? -1 : TargetFrameRate;

            Debug.Log($"[Graphics] Applied: Quality={QualityLevel}, Fullscreen={Fullscreen}, VSync={VSync}");
        }
    }

    [Serializable]
    public class AudioSettings
    {
        [Range(0f, 1f)] public float MasterVolume = 1f;
        [Range(0f, 1f)] public float MusicVolume = 0.8f;
        [Range(0f, 1f)] public float SFXVolume = 1f;
        [Range(0f, 1f)] public float VoiceVolume = 1f;
        [Range(0f, 1f)] public float UIVolume = 0.7f;

        public bool MuteAll = false;

        public void Apply()
        {
            // Apply to AudioMixer if available
            // AudioMixer.SetFloat("MasterVolume", MuteAll ? -80f : Mathf.Log10(MasterVolume) * 20f);

            AudioListener.volume = MuteAll ? 0f : MasterVolume;

            Debug.Log($"[Audio] Applied: Master={MasterVolume}, Music={MusicVolume}, SFX={SFXVolume}");
        }

        public float GetEffectiveVolume(VolumeType type)
        {
            if (MuteAll) return 0f;

            return type switch
            {
                VolumeType.Music => MasterVolume * MusicVolume,
                VolumeType.SFX => MasterVolume * SFXVolume,
                VolumeType.Voice => MasterVolume * VoiceVolume,
                VolumeType.UI => MasterVolume * UIVolume,
                _ => MasterVolume
            };
        }

        public enum VolumeType { Master, Music, SFX, Voice, UI }
    }

    [Serializable]
    public class ControlSettings
    {
        // Mouse
        public float MouseSensitivity = 1f;
        public bool InvertMouseY = false;

        // Gamepad
        public float GamepadSensitivity = 1f;
        public bool InvertGamepadY = false;
        public float DeadZone = 0.1f;
        public bool Vibration = true;

        // General
        public bool HoldToSprint = false;
        public bool HoldToCrouch = true;
        public bool AutoAim = false;

        public void Apply()
        {
            // Apply to input system
            Debug.Log($"[Controls] Applied: MouseSens={MouseSensitivity}, InvertY={InvertMouseY}");
        }
    }

    [Serializable]
    public class GameplaySettings
    {
        // Difficulty
        public int DifficultyLevel = 1; // 0=Easy, 1=Normal, 2=Hard

        // UI
        public bool ShowDamageNumbers = true;
        public bool ShowMinimap = true;
        public bool ShowTutorials = true;
        public float UIScale = 1f;

        // Camera
        public float FieldOfView = 60f;
        public bool CameraShake = true;
        public bool MotionBlur = false;

        // Accessibility
        public bool Subtitles = true;
        public float SubtitleSize = 1f;
        public bool ScreenFlashEffects = true;
        public bool ColorBlindMode = false;
        public int ColorBlindType = 0; // 0=None, 1=Protanopia, 2=Deuteranopia, 3=Tritanopia

        public void Apply()
        {
            // Apply camera settings
            if (Camera.main != null)
            {
                Camera.main.fieldOfView = FieldOfView;
            }

            Debug.Log($"[Gameplay] Applied: Difficulty={DifficultyLevel}, FOV={FieldOfView}");
        }
    }
}
