using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// Database for storing and retrieving quest definitions.
    /// </summary>
    [CreateAssetMenu(fileName = "QuestDatabase", menuName = "UnityWorkbench/Gameplay/Quest Database")]
    public class QuestDatabase : ScriptableObject
    {
        [SerializeField] private List<QuestBase> _quests = new List<QuestBase>();

        // Runtime lookup
        private Dictionary<string, QuestBase> _questLookup;
        private bool _isInitialized;

        private static QuestDatabase _instance;

        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static QuestDatabase Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<QuestDatabase>("QuestDatabase");
                    if (_instance != null)
                    {
                        _instance.Initialize();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// All quests in database.
        /// </summary>
        public IReadOnlyList<QuestBase> Quests => _quests;

        /// <summary>
        /// Number of quests.
        /// </summary>
        public int Count => _quests.Count;

        private void OnEnable()
        {
            _isInitialized = false;
        }

        /// <summary>
        /// Initialize the lookup dictionary.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            _questLookup = new Dictionary<string, QuestBase>();

            foreach (var quest in _quests)
            {
                if (quest == null) continue;

                if (_questLookup.ContainsKey(quest.QuestId))
                {
                    Debug.LogWarning($"[QuestDatabase] Duplicate quest ID: {quest.QuestId}");
                    continue;
                }

                _questLookup[quest.QuestId] = quest;
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Get a quest by ID.
        /// </summary>
        public QuestBase GetQuest(string questId)
        {
            if (!_isInitialized) Initialize();

            if (string.IsNullOrEmpty(questId))
                return null;

            return _questLookup.TryGetValue(questId, out var quest) ? quest : null;
        }

        /// <summary>
        /// Check if a quest exists.
        /// </summary>
        public bool HasQuest(string questId)
        {
            if (!_isInitialized) Initialize();
            return !string.IsNullOrEmpty(questId) && _questLookup.ContainsKey(questId);
        }

        /// <summary>
        /// Get quests by type.
        /// </summary>
        public List<QuestBase> GetQuestsByType(QuestType type)
        {
            if (!_isInitialized) Initialize();

            var result = new List<QuestBase>();
            foreach (var quest in _quests)
            {
                if (quest != null && quest.QuestType == type)
                {
                    result.Add(quest);
                }
            }
            return result;
        }

        /// <summary>
        /// Get quests available for a level.
        /// </summary>
        public List<QuestBase> GetQuestsForLevel(int level)
        {
            if (!_isInitialized) Initialize();

            var result = new List<QuestBase>();
            foreach (var quest in _quests)
            {
                if (quest != null && quest.RequiredLevel <= level)
                {
                    result.Add(quest);
                }
            }
            return result;
        }

        /// <summary>
        /// Get repeatable quests (dailies, weeklies).
        /// </summary>
        public List<QuestBase> GetRepeatableQuests()
        {
            if (!_isInitialized) Initialize();

            var result = new List<QuestBase>();
            foreach (var quest in _quests)
            {
                if (quest != null && quest.IsRepeatable)
                {
                    result.Add(quest);
                }
            }
            return result;
        }

        /// <summary>
        /// Search quests by title.
        /// </summary>
        public List<QuestBase> SearchQuests(string searchTerm)
        {
            if (!_isInitialized) Initialize();

            if (string.IsNullOrEmpty(searchTerm))
                return new List<QuestBase>(_quests);

            searchTerm = searchTerm.ToLowerInvariant();
            var result = new List<QuestBase>();

            foreach (var quest in _quests)
            {
                if (quest == null) continue;

                if (quest.Title.ToLowerInvariant().Contains(searchTerm) ||
                    quest.QuestId.ToLowerInvariant().Contains(searchTerm))
                {
                    result.Add(quest);
                }
            }

            return result;
        }

        #region Editor Methods

#if UNITY_EDITOR
        /// <summary>
        /// Add a quest to the database (Editor only).
        /// </summary>
        public void AddQuest(QuestBase quest)
        {
            if (quest == null || _quests.Contains(quest)) return;
            _quests.Add(quest);
            _isInitialized = false;
        }

        /// <summary>
        /// Remove a quest from the database (Editor only).
        /// </summary>
        public void RemoveQuest(QuestBase quest)
        {
            _quests.Remove(quest);
            _isInitialized = false;
        }

        /// <summary>
        /// Clear all quests (Editor only).
        /// </summary>
        public void ClearAll()
        {
            _quests.Clear();
            _questLookup?.Clear();
            _isInitialized = false;
        }

        /// <summary>
        /// Refresh database from project quests (Editor only).
        /// </summary>
        public void RefreshFromAssets()
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("t:QuestBase");
            _quests.Clear();

            foreach (var guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var quest = UnityEditor.AssetDatabase.LoadAssetAtPath<QuestBase>(path);
                if (quest != null)
                {
                    _quests.Add(quest);
                }
            }

            _isInitialized = false;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        #endregion

        /// <summary>
        /// Set as the singleton instance.
        /// </summary>
        public void SetAsInstance()
        {
            _instance = this;
            Initialize();
        }
    }
}
