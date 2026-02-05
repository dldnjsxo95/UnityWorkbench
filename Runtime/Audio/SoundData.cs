using System;
using UnityEngine;
using UnityEngine.Audio;

namespace LWT.UnityWorkbench.Audio
{
    /// <summary>
    /// Data container for a single sound effect or music track.
    /// </summary>
    [Serializable]
    public class SoundData
    {
        [Header("Identification")]
        public string Id;
        public string DisplayName;

        [Header("Audio")]
        public AudioClip Clip;
        [Range(0f, 1f)]
        public float Volume = 1f;
        [Range(0.1f, 3f)]
        public float Pitch = 1f;

        [Header("Variation")]
        [Range(0f, 0.5f)]
        public float VolumeVariation = 0f;
        [Range(0f, 0.5f)]
        public float PitchVariation = 0f;

        [Header("Playback")]
        public bool Loop = false;
        public bool PlayOnAwake = false;
        [Range(0f, 1f)]
        public float SpatialBlend = 0f; // 0 = 2D, 1 = 3D

        [Header("3D Settings")]
        public float MinDistance = 1f;
        public float MaxDistance = 500f;
        public AudioRolloffMode RolloffMode = AudioRolloffMode.Logarithmic;

        [Header("Mixer")]
        public AudioMixerGroup MixerGroup;

        [Header("Priority")]
        [Range(0, 256)]
        public int Priority = 128;

        /// <summary>
        /// Gets volume with random variation applied.
        /// </summary>
        public float GetVolume()
        {
            if (VolumeVariation <= 0f) return Volume;
            return Volume + UnityEngine.Random.Range(-VolumeVariation, VolumeVariation);
        }

        /// <summary>
        /// Gets pitch with random variation applied.
        /// </summary>
        public float GetPitch()
        {
            if (PitchVariation <= 0f) return Pitch;
            return Pitch + UnityEngine.Random.Range(-PitchVariation, PitchVariation);
        }

        /// <summary>
        /// Applies settings to an AudioSource.
        /// </summary>
        public void ApplyTo(AudioSource source)
        {
            source.clip = Clip;
            source.volume = GetVolume();
            source.pitch = GetPitch();
            source.loop = Loop;
            source.spatialBlend = SpatialBlend;
            source.minDistance = MinDistance;
            source.maxDistance = MaxDistance;
            source.rolloffMode = RolloffMode;
            source.priority = Priority;

            if (MixerGroup != null)
            {
                source.outputAudioMixerGroup = MixerGroup;
            }
        }

        /// <summary>
        /// Gets the duration of the clip.
        /// </summary>
        public float Duration => Clip != null ? Clip.length : 0f;

        /// <summary>
        /// Checks if the sound data is valid.
        /// </summary>
        public bool IsValid => Clip != null && !string.IsNullOrEmpty(Id);
    }

    /// <summary>
    /// Extended sound data with multiple clip variations.
    /// </summary>
    [Serializable]
    public class SoundDataVariant : SoundData
    {
        [Header("Clip Variants")]
        public AudioClip[] ClipVariants;

        private int _lastClipIndex = -1;

        /// <summary>
        /// Gets a random clip from variants (avoids repeating last clip).
        /// </summary>
        public AudioClip GetRandomClip()
        {
            if (ClipVariants == null || ClipVariants.Length == 0)
                return Clip;

            if (ClipVariants.Length == 1)
                return ClipVariants[0];

            int index;
            do
            {
                index = UnityEngine.Random.Range(0, ClipVariants.Length);
            }
            while (index == _lastClipIndex && ClipVariants.Length > 1);

            _lastClipIndex = index;
            return ClipVariants[index];
        }

        /// <summary>
        /// Applies settings with random clip variant.
        /// </summary>
        public void ApplyWithVariant(AudioSource source)
        {
            base.ApplyTo(source);
            source.clip = GetRandomClip();
        }
    }

    /// <summary>
    /// Music track data with additional music-specific settings.
    /// </summary>
    [Serializable]
    public class MusicData : SoundData
    {
        [Header("Music Settings")]
        public float BPM = 120f;
        public float IntroLength = 0f;
        public float LoopStartTime = 0f;
        public float LoopEndTime = 0f; // 0 = end of clip

        [Header("Transitions")]
        public float FadeInDuration = 1f;
        public float FadeOutDuration = 1f;

        public MusicData()
        {
            Loop = true;
            SpatialBlend = 0f;
            Priority = 0; // Highest priority for music
        }

        /// <summary>
        /// Gets the actual loop end time.
        /// </summary>
        public float ActualLoopEndTime => LoopEndTime > 0f ? LoopEndTime : Duration;
    }
}
