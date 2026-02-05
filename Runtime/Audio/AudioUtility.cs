using UnityEngine;

namespace LWT.UnityWorkbench.Audio
{
    /// <summary>
    /// Audio helper utilities.
    /// </summary>
    public static class AudioUtility
    {
        /// <summary>
        /// Converts linear volume (0-1) to decibels.
        /// </summary>
        public static float LinearToDecibels(float linear)
        {
            return linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;
        }

        /// <summary>
        /// Converts decibels to linear volume (0-1).
        /// </summary>
        public static float DecibelsToLinear(float dB)
        {
            return Mathf.Pow(10f, dB / 20f);
        }

        /// <summary>
        /// Calculates attenuation based on distance.
        /// </summary>
        public static float CalculateAttenuation(float distance, float minDistance, float maxDistance, AudioRolloffMode rolloff)
        {
            if (distance <= minDistance) return 1f;
            if (distance >= maxDistance) return 0f;

            float normalizedDistance = (distance - minDistance) / (maxDistance - minDistance);

            switch (rolloff)
            {
                case AudioRolloffMode.Logarithmic:
                    return 1f - Mathf.Log10(1f + 9f * normalizedDistance);
                case AudioRolloffMode.Linear:
                    return 1f - normalizedDistance;
                default:
                    return 1f;
            }
        }

        /// <summary>
        /// Creates a simple audio source with common settings.
        /// </summary>
        public static AudioSource CreateAudioSource(GameObject target, bool is3D = false)
        {
            var source = target.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = is3D ? 1f : 0f;
            return source;
        }

        /// <summary>
        /// Plays a clip at a point in world space (one-shot).
        /// </summary>
        public static void PlayClipAtPoint(AudioClip clip, Vector3 position, float volume = 1f)
        {
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }

        /// <summary>
        /// Fades an audio source volume over time.
        /// </summary>
        public static System.Collections.IEnumerator FadeVolume(AudioSource source, float targetVolume, float duration)
        {
            if (source == null) yield break;

            float startVolume = source.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                yield return null;
            }

            source.volume = targetVolume;

            if (targetVolume <= 0f)
            {
                source.Stop();
            }
        }

        /// <summary>
        /// Crossfades between two audio sources.
        /// </summary>
        public static System.Collections.IEnumerator Crossfade(AudioSource fadeOut, AudioSource fadeIn, float duration)
        {
            float startVolumeOut = fadeOut != null ? fadeOut.volume : 0f;
            float targetVolumeIn = fadeIn != null ? 1f : 0f;
            float elapsed = 0f;

            if (fadeIn != null && !fadeIn.isPlaying)
            {
                fadeIn.volume = 0f;
                fadeIn.Play();
            }

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;

                if (fadeOut != null)
                    fadeOut.volume = Mathf.Lerp(startVolumeOut, 0f, t);

                if (fadeIn != null)
                    fadeIn.volume = Mathf.Lerp(0f, targetVolumeIn, t);

                yield return null;
            }

            if (fadeOut != null)
            {
                fadeOut.volume = 0f;
                fadeOut.Stop();
            }

            if (fadeIn != null)
            {
                fadeIn.volume = targetVolumeIn;
            }
        }

        /// <summary>
        /// Gets a semi-random pitch variation.
        /// </summary>
        public static float GetPitchVariation(float basePitch, float variation)
        {
            return basePitch + Random.Range(-variation, variation);
        }

        /// <summary>
        /// Gets a semi-random volume variation.
        /// </summary>
        public static float GetVolumeVariation(float baseVolume, float variation)
        {
            return Mathf.Clamp01(baseVolume + Random.Range(-variation, variation));
        }

        /// <summary>
        /// Checks if an AudioSource is currently playing.
        /// </summary>
        public static bool IsPlaying(AudioSource source)
        {
            return source != null && source.isPlaying;
        }

        /// <summary>
        /// Gets the normalized playback position (0-1).
        /// </summary>
        public static float GetNormalizedTime(AudioSource source)
        {
            if (source == null || source.clip == null) return 0f;
            return source.time / source.clip.length;
        }
    }
}
