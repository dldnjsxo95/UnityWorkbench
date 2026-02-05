using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Audio
{
    /// <summary>
    /// ScriptableObject database for sound effects.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSoundBank", menuName = "UnityWorkbench/Audio/Sound Bank")]
    public class SoundBank : ScriptableObject
    {
        [SerializeField] private List<SoundData> _sounds = new List<SoundData>();

        private Dictionary<string, SoundData> _lookup;

        public IReadOnlyList<SoundData> Sounds => _sounds;
        public int Count => _sounds.Count;

        private void OnEnable()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            _lookup = new Dictionary<string, SoundData>();
            foreach (var sound in _sounds)
            {
                if (sound != null && !string.IsNullOrEmpty(sound.Id))
                {
                    if (!_lookup.ContainsKey(sound.Id))
                    {
                        _lookup[sound.Id] = sound;
                    }
                    else
                    {
                        Debug.LogWarning($"[SoundBank] Duplicate sound ID: {sound.Id}");
                    }
                }
            }
        }

        /// <summary>
        /// Gets a sound by ID.
        /// </summary>
        public SoundData GetSound(string id)
        {
            if (_lookup == null) BuildLookup();

            if (_lookup.TryGetValue(id, out var sound))
            {
                return sound;
            }

            Debug.LogWarning($"[SoundBank] Sound not found: {id}");
            return null;
        }

        /// <summary>
        /// Tries to get a sound by ID.
        /// </summary>
        public bool TryGetSound(string id, out SoundData sound)
        {
            if (_lookup == null) BuildLookup();
            return _lookup.TryGetValue(id, out sound);
        }

        /// <summary>
        /// Checks if a sound exists.
        /// </summary>
        public bool HasSound(string id)
        {
            if (_lookup == null) BuildLookup();
            return _lookup.ContainsKey(id);
        }

        /// <summary>
        /// Gets a random sound from the bank.
        /// </summary>
        public SoundData GetRandomSound()
        {
            if (_sounds.Count == 0) return null;
            return _sounds[Random.Range(0, _sounds.Count)];
        }

        /// <summary>
        /// Gets all sound IDs.
        /// </summary>
        public IEnumerable<string> GetAllIds()
        {
            if (_lookup == null) BuildLookup();
            return _lookup.Keys;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Adds a sound to the bank (Editor only).
        /// </summary>
        public void AddSound(SoundData sound)
        {
            if (sound == null) return;
            _sounds.Add(sound);
            BuildLookup();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Removes a sound from the bank (Editor only).
        /// </summary>
        public bool RemoveSound(string id)
        {
            int index = _sounds.FindIndex(s => s.Id == id);
            if (index >= 0)
            {
                _sounds.RemoveAt(index);
                BuildLookup();
                UnityEditor.EditorUtility.SetDirty(this);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Validates all sounds in the bank (Editor only).
        /// </summary>
        [ContextMenu("Validate Bank")]
        private void ValidateBank()
        {
            int issues = 0;
            foreach (var sound in _sounds)
            {
                if (sound == null)
                {
                    Debug.LogError("[SoundBank] Null sound entry found");
                    issues++;
                    continue;
                }

                if (string.IsNullOrEmpty(sound.Id))
                {
                    Debug.LogError($"[SoundBank] Sound with missing ID: {sound.DisplayName}");
                    issues++;
                }

                if (sound.Clip == null)
                {
                    Debug.LogWarning($"[SoundBank] Sound with missing clip: {sound.Id}");
                    issues++;
                }
            }

            if (issues == 0)
            {
                Debug.Log($"[SoundBank] Validation passed. {_sounds.Count} sounds.");
            }
            else
            {
                Debug.LogWarning($"[SoundBank] Validation found {issues} issues.");
            }
        }

        /// <summary>
        /// Auto-generates IDs from clip names (Editor only).
        /// </summary>
        [ContextMenu("Auto-Generate IDs from Clips")]
        private void AutoGenerateIds()
        {
            foreach (var sound in _sounds)
            {
                if (sound.Clip != null && string.IsNullOrEmpty(sound.Id))
                {
                    sound.Id = sound.Clip.name.ToLower().Replace(" ", "_");
                    sound.DisplayName = sound.Clip.name;
                }
            }
            BuildLookup();
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log("[SoundBank] Auto-generated IDs from clip names.");
        }
#endif
    }
}
