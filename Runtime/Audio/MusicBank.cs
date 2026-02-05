using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Audio
{
    /// <summary>
    /// ScriptableObject database for music tracks.
    /// </summary>
    [CreateAssetMenu(fileName = "NewMusicBank", menuName = "UnityWorkbench/Audio/Music Bank")]
    public class MusicBank : ScriptableObject
    {
        [SerializeField] private List<MusicData> _tracks = new List<MusicData>();

        private Dictionary<string, MusicData> _lookup;

        public IReadOnlyList<MusicData> Tracks => _tracks;
        public int Count => _tracks.Count;

        private void OnEnable()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            _lookup = new Dictionary<string, MusicData>();
            foreach (var track in _tracks)
            {
                if (track != null && !string.IsNullOrEmpty(track.Id))
                {
                    if (!_lookup.ContainsKey(track.Id))
                    {
                        _lookup[track.Id] = track;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a music track by ID.
        /// </summary>
        public MusicData GetTrack(string id)
        {
            if (_lookup == null) BuildLookup();

            if (_lookup.TryGetValue(id, out var track))
            {
                return track;
            }

            Debug.LogWarning($"[MusicBank] Track not found: {id}");
            return null;
        }

        /// <summary>
        /// Tries to get a music track by ID.
        /// </summary>
        public bool TryGetTrack(string id, out MusicData track)
        {
            if (_lookup == null) BuildLookup();
            return _lookup.TryGetValue(id, out track);
        }

        /// <summary>
        /// Gets a random music track.
        /// </summary>
        public MusicData GetRandomTrack()
        {
            if (_tracks.Count == 0) return null;
            return _tracks[Random.Range(0, _tracks.Count)];
        }

        /// <summary>
        /// Gets all track IDs.
        /// </summary>
        public IEnumerable<string> GetAllIds()
        {
            if (_lookup == null) BuildLookup();
            return _lookup.Keys;
        }
    }
}
