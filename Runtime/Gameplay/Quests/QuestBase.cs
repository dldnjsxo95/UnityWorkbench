using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// ScriptableObject base class for quest definitions.
    /// </summary>
    [CreateAssetMenu(fileName = "Quest", menuName = "UnityWorkbench/Gameplay/Quest")]
    public class QuestBase : ScriptableObject, IQuest
    {
        [Header("Basic Info")]
        [SerializeField] private string _questId;
        [SerializeField] private string _title;
        [SerializeField, TextArea(3, 6)] private string _description;
        [SerializeField] private Sprite _icon;

        [Header("Classification")]
        [SerializeField] private QuestType _questType = QuestType.Side;
        [SerializeField] private int _requiredLevel = 1;
        [SerializeField] private bool _isRepeatable;

        [Header("Prerequisites")]
        [SerializeField] private string[] _requiredQuestIds;
        [SerializeField] private string[] _requiredItemIds;

        [Header("Objectives")]
        [SerializeField] private List<QuestObjective> _objectives = new List<QuestObjective>();

        [Header("Rewards")]
        [SerializeField] private List<QuestReward> _rewards = new List<QuestReward>();

        [Header("Optional Rewards (Choose One)")]
        [SerializeField] private List<QuestReward> _optionalRewards = new List<QuestReward>();

        [Header("Settings")]
        [SerializeField] private float _timeLimit = -1f;  // -1 = no limit
        [SerializeField] private bool _autoComplete = true;
        [SerializeField] private bool _trackOnAccept = true;

        #region IQuest Implementation

        public string QuestId => string.IsNullOrEmpty(_questId) ? name : _questId;
        public string Title => string.IsNullOrEmpty(_title) ? name : _title;
        public string Description => _description;
        public QuestType QuestType => _questType;
        public int RequiredLevel => _requiredLevel;
        public bool IsRepeatable => _isRepeatable;

        #endregion

        /// <summary>
        /// Quest icon.
        /// </summary>
        public Sprite Icon => _icon;

        /// <summary>
        /// Required quests to be completed first.
        /// </summary>
        public string[] RequiredQuestIds => _requiredQuestIds;

        /// <summary>
        /// Required items to have.
        /// </summary>
        public string[] RequiredItemIds => _requiredItemIds;

        /// <summary>
        /// Quest objectives.
        /// </summary>
        public IReadOnlyList<QuestObjective> Objectives => _objectives;

        /// <summary>
        /// Guaranteed rewards.
        /// </summary>
        public IReadOnlyList<QuestReward> Rewards => _rewards;

        /// <summary>
        /// Optional rewards (player chooses one).
        /// </summary>
        public IReadOnlyList<QuestReward> OptionalRewards => _optionalRewards;

        /// <summary>
        /// Time limit in seconds (-1 = no limit).
        /// </summary>
        public float TimeLimit => _timeLimit;

        /// <summary>
        /// Whether quest auto-completes when objectives are done.
        /// </summary>
        public bool AutoComplete => _autoComplete;

        /// <summary>
        /// Whether to start tracking on accept.
        /// </summary>
        public bool TrackOnAccept => _trackOnAccept;

        /// <summary>
        /// Create a runtime instance of this quest.
        /// </summary>
        public virtual QuestInstance CreateInstance()
        {
            return new QuestInstance(this);
        }

        /// <summary>
        /// Check if prerequisites are met.
        /// </summary>
        public virtual bool CheckPrerequisites(QuestManager questManager, int playerLevel)
        {
            // Check level
            if (playerLevel < _requiredLevel)
                return false;

            // Check required quests
            if (_requiredQuestIds != null)
            {
                foreach (var questId in _requiredQuestIds)
                {
                    if (!questManager.IsQuestCompleted(questId))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get the completion summary text.
        /// </summary>
        public virtual string GetCompletionText()
        {
            return $"Quest Complete: {Title}";
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_questId))
            {
                _questId = name;
            }

            _requiredLevel = Mathf.Max(1, _requiredLevel);

            // Ensure objectives have IDs
            for (int i = 0; i < _objectives.Count; i++)
            {
                if (string.IsNullOrEmpty(_objectives[i].ObjectiveId))
                {
                    // Can't modify in OnValidate, but log warning
                    Debug.LogWarning($"[Quest] {name}: Objective {i} is missing an ID");
                }
            }
        }
    }
}
