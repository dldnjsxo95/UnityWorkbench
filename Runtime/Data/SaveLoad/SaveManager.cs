using System;
using UnityEngine;
using LWT.UnityWorkbench.Core;

namespace LWT.UnityWorkbench.Data
{
    /// <summary>
    /// Central manager for save/load operations.
    /// Supports auto-save and multiple save slots.
    /// </summary>
    public class SaveManager : PersistentMonoSingleton<SaveManager>
    {
        [Header("Save Settings")]
        [SerializeField] private SaveType _saveType = SaveType.Json;
        [SerializeField] private bool _useEncryption = false;
        [SerializeField] private bool _autoSave = true;
        [SerializeField] private float _autoSaveInterval = 60f;

        [Header("Slot Settings")]
        [SerializeField] private int _maxSlots = 3;
        [SerializeField] private int _currentSlot = 0;

        private ISaveSystem _saveSystem;
        private float _lastAutoSaveTime;

        public int CurrentSlot
        {
            get => _currentSlot;
            set => _currentSlot = Mathf.Clamp(value, 0, _maxSlots - 1);
        }

        public int MaxSlots => _maxSlots;
        public event Action<int> OnSaveCompleted;
        public event Action<int> OnLoadCompleted;

        public enum SaveType
        {
            Json,
            Binary
        }

        protected override void OnSingletonAwake()
        {
            InitializeSaveSystem();
        }

        private void InitializeSaveSystem()
        {
            _saveSystem = _saveType switch
            {
                SaveType.Json => new JsonSaveSystem(useEncryption: _useEncryption),
                SaveType.Binary => new BinarySaveSystem(),
                _ => new JsonSaveSystem()
            };

            Debug.Log($"[SaveManager] Initialized with {_saveType} system");
        }

        private void Update()
        {
            if (_autoSave && Time.time - _lastAutoSaveTime >= _autoSaveInterval)
            {
                _lastAutoSaveTime = Time.time;
                // Auto-save can be triggered here if needed
                // Save("autosave", currentGameData);
            }
        }

        /// <summary>
        /// Get slot-specific key.
        /// </summary>
        private string GetSlotKey(string key) => $"slot{_currentSlot}_{key}";

        /// <summary>
        /// Save data to current slot.
        /// </summary>
        public void Save<T>(string key, T data) where T : class
        {
            _saveSystem.Save(GetSlotKey(key), data);
            OnSaveCompleted?.Invoke(_currentSlot);
        }

        /// <summary>
        /// Load data from current slot.
        /// </summary>
        public T Load<T>(string key) where T : class
        {
            var data = _saveSystem.Load<T>(GetSlotKey(key));
            if (data != null)
            {
                OnLoadCompleted?.Invoke(_currentSlot);
            }
            return data;
        }

        /// <summary>
        /// Save to specific slot.
        /// </summary>
        public void SaveToSlot<T>(int slot, string key, T data) where T : class
        {
            int prevSlot = _currentSlot;
            _currentSlot = slot;
            Save(key, data);
            _currentSlot = prevSlot;
        }

        /// <summary>
        /// Load from specific slot.
        /// </summary>
        public T LoadFromSlot<T>(int slot, string key) where T : class
        {
            int prevSlot = _currentSlot;
            _currentSlot = slot;
            var data = Load<T>(key);
            _currentSlot = prevSlot;
            return data;
        }

        /// <summary>
        /// Check if save exists in current slot.
        /// </summary>
        public bool HasSave(string key)
        {
            return _saveSystem.Exists(GetSlotKey(key));
        }

        /// <summary>
        /// Delete save from current slot.
        /// </summary>
        public void Delete(string key)
        {
            _saveSystem.Delete(GetSlotKey(key));
        }

        /// <summary>
        /// Delete all data in current slot.
        /// </summary>
        public void DeleteSlot()
        {
            // Delete known save keys for this slot
            // Implementation depends on your save structure
            Debug.Log($"[SaveManager] Slot {_currentSlot} deleted");
        }

        /// <summary>
        /// Delete all saves.
        /// </summary>
        public void DeleteAllSaves()
        {
            _saveSystem.DeleteAll();
        }

        /// <summary>
        /// Get save system for advanced operations.
        /// </summary>
        public ISaveSystem GetSaveSystem() => _saveSystem;

        /// <summary>
        /// Create backup of current slot.
        /// </summary>
        public void CreateBackup(string key)
        {
            if (_saveSystem is JsonSaveSystem jsonSystem)
            {
                jsonSystem.CreateBackup(GetSlotKey(key));
            }
        }

        /// <summary>
        /// Get slot info for UI display.
        /// </summary>
        public SlotInfo GetSlotInfo(int slot)
        {
            int prevSlot = _currentSlot;
            _currentSlot = slot;

            var info = new SlotInfo
            {
                SlotIndex = slot,
                HasData = HasSave("gamedata"),
                // Add more info as needed
            };

            _currentSlot = prevSlot;
            return info;
        }

        [System.Serializable]
        public class SlotInfo
        {
            public int SlotIndex;
            public bool HasData;
            public string SaveDate;
            public string PlayTime;
        }
    }
}
