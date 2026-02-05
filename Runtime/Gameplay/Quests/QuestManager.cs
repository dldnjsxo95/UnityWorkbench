using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LWT.UnityWorkbench.Core;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// Component for managing player quests.
    /// </summary>
    public class QuestManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private int _maxActiveQuests = 20;
        [SerializeField] private LevelSystem _levelSystem;
        [SerializeField] private Inventory _inventory;
        [SerializeField] private bool _autoFindComponents = true;

        [Header("Quests")]
        [SerializeField] private List<QuestInstance> _activeQuests = new List<QuestInstance>();
        [SerializeField] private List<string> _completedQuestIds = new List<string>();

        /// <summary>
        /// Currently active quests.
        /// </summary>
        public IReadOnlyList<QuestInstance> ActiveQuests => _activeQuests;

        /// <summary>
        /// IDs of completed quests.
        /// </summary>
        public IReadOnlyList<string> CompletedQuestIds => _completedQuestIds;

        /// <summary>
        /// Maximum active quests.
        /// </summary>
        public int MaxActiveQuests => _maxActiveQuests;

        /// <summary>
        /// Current number of active quests.
        /// </summary>
        public int ActiveQuestCount => _activeQuests.Count;

        /// <summary>
        /// Whether quest log is full.
        /// </summary>
        public bool IsQuestLogFull => _activeQuests.Count >= _maxActiveQuests;

        #region Events

        public event Action<QuestInstance> OnQuestAccepted;
        public event Action<QuestInstance> OnQuestCompleted;
        public event Action<QuestInstance, QuestReward[]> OnQuestTurnedIn;
        public event Action<QuestInstance> OnQuestFailed;
        public event Action<QuestInstance> OnQuestAbandoned;
        public event Action<QuestInstance, string> OnObjectiveCompleted;
        public event Action<QuestInstance, string, int, int> OnObjectiveProgress;

        #endregion

        private void Awake()
        {
            if (_autoFindComponents)
            {
                if (_levelSystem == null)
                    _levelSystem = GetComponent<LevelSystem>();
                if (_inventory == null)
                    _inventory = GetComponent<Inventory>();
            }
        }

        private void Update()
        {
            // Update timed quests
            for (int i = _activeQuests.Count - 1; i >= 0; i--)
            {
                var quest = _activeQuests[i];
                if (quest.UpdateTime(Time.deltaTime))
                {
                    FailQuest(quest);
                }
            }
        }

        /// <summary>
        /// Accept a quest.
        /// </summary>
        public bool AcceptQuest(string questId)
        {
            if (IsQuestLogFull)
            {
                Debug.LogWarning($"[QuestManager] Quest log is full");
                return false;
            }

            if (HasActiveQuest(questId))
            {
                Debug.LogWarning($"[QuestManager] Quest already active: {questId}");
                return false;
            }

            var questData = QuestDatabase.Instance?.GetQuest(questId);
            if (questData == null)
            {
                Debug.LogWarning($"[QuestManager] Quest not found: {questId}");
                return false;
            }

            // Check prerequisites
            int playerLevel = _levelSystem?.Level ?? 1;
            if (!questData.CheckPrerequisites(this, playerLevel))
            {
                Debug.LogWarning($"[QuestManager] Prerequisites not met for: {questId}");
                return false;
            }

            // Check repeatable
            if (!questData.IsRepeatable && IsQuestCompleted(questId))
            {
                Debug.LogWarning($"[QuestManager] Quest already completed and not repeatable: {questId}");
                return false;
            }

            // Create instance
            var instance = questData.CreateInstance();
            _activeQuests.Add(instance);

            OnQuestAccepted?.Invoke(instance);

            EventBus<QuestAcceptedEvent>.Publish(new QuestAcceptedEvent
            {
                QuestId = questId,
                QuestManager = this
            });

            Debug.Log($"[QuestManager] Accepted quest: {questData.Title}");
            return true;
        }

        /// <summary>
        /// Turn in a completed quest and claim rewards.
        /// </summary>
        public bool TurnInQuest(string questId, int optionalRewardIndex = -1)
        {
            var quest = GetActiveQuest(questId);
            if (quest == null || quest.Status != QuestStatus.Completed)
            {
                Debug.LogWarning($"[QuestManager] Quest not ready for turn-in: {questId}");
                return false;
            }

            var questData = quest.GetQuestData();
            if (questData == null) return false;

            // Collect rewards
            var rewards = new List<QuestReward>(questData.Rewards);

            // Add optional reward if selected
            if (optionalRewardIndex >= 0 && optionalRewardIndex < questData.OptionalRewards.Count)
            {
                rewards.Add(questData.OptionalRewards[optionalRewardIndex]);
            }

            // Grant rewards
            GrantRewards(rewards);

            // Update status
            quest.Status = QuestStatus.TurnedIn;
            _activeQuests.Remove(quest);

            if (!questData.IsRepeatable && !_completedQuestIds.Contains(questId))
            {
                _completedQuestIds.Add(questId);
            }

            OnQuestTurnedIn?.Invoke(quest, rewards.ToArray());

            EventBus<QuestCompletedEvent>.Publish(new QuestCompletedEvent
            {
                QuestId = questId,
                QuestManager = this,
                Rewards = rewards.ToArray()
            });

            Debug.Log($"[QuestManager] Turned in quest: {questData.Title}");
            return true;
        }

        /// <summary>
        /// Abandon an active quest.
        /// </summary>
        public bool AbandonQuest(string questId)
        {
            var quest = GetActiveQuest(questId);
            if (quest == null)
                return false;

            _activeQuests.Remove(quest);
            OnQuestAbandoned?.Invoke(quest);

            Debug.Log($"[QuestManager] Abandoned quest: {questId}");
            return true;
        }

        /// <summary>
        /// Report progress on quest objectives.
        /// </summary>
        public void ReportProgress(ObjectiveType type, string targetId, int amount = 1)
        {
            foreach (var quest in _activeQuests)
            {
                if (quest.Status != QuestStatus.InProgress) continue;

                var questData = quest.GetQuestData();
                if (questData == null) continue;

                for (int i = 0; i < questData.Objectives.Count; i++)
                {
                    var objective = questData.Objectives[i];
                    if (objective.Type != type || objective.TargetId != targetId)
                        continue;

                    var progress = quest.ObjectiveProgress[i];
                    int prevAmount = progress.CurrentAmount;

                    if (progress.AddProgress(amount, objective.RequiredAmount))
                    {
                        // Objective completed
                        OnObjectiveCompleted?.Invoke(quest, objective.ObjectiveId);

                        EventBus<QuestObjectiveCompletedEvent>.Publish(new QuestObjectiveCompletedEvent
                        {
                            QuestId = quest.QuestId,
                            ObjectiveId = objective.ObjectiveId
                        });
                    }

                    // Report progress change
                    if (progress.CurrentAmount != prevAmount)
                    {
                        OnObjectiveProgress?.Invoke(quest, objective.ObjectiveId,
                            progress.CurrentAmount, objective.RequiredAmount);
                    }
                }

                // Check quest completion
                quest.CheckCompletion();

                if (quest.Status == QuestStatus.Completed)
                {
                    OnQuestCompleted?.Invoke(quest);
                }
            }
        }

        /// <summary>
        /// Report a kill.
        /// </summary>
        public void ReportKill(string enemyId, int count = 1)
        {
            ReportProgress(ObjectiveType.Kill, enemyId, count);
        }

        /// <summary>
        /// Report item collection.
        /// </summary>
        public void ReportCollect(string itemId, int count = 1)
        {
            ReportProgress(ObjectiveType.Collect, itemId, count);
        }

        /// <summary>
        /// Report talking to NPC.
        /// </summary>
        public void ReportTalk(string npcId)
        {
            ReportProgress(ObjectiveType.Talk, npcId, 1);
        }

        /// <summary>
        /// Report visiting location.
        /// </summary>
        public void ReportExplore(string locationId)
        {
            ReportProgress(ObjectiveType.Explore, locationId, 1);
        }

        /// <summary>
        /// Check if a quest is active.
        /// </summary>
        public bool HasActiveQuest(string questId)
        {
            return _activeQuests.Exists(q => q.QuestId == questId);
        }

        /// <summary>
        /// Check if a quest is completed.
        /// </summary>
        public bool IsQuestCompleted(string questId)
        {
            return _completedQuestIds.Contains(questId);
        }

        /// <summary>
        /// Get an active quest.
        /// </summary>
        public QuestInstance GetActiveQuest(string questId)
        {
            return _activeQuests.Find(q => q.QuestId == questId);
        }

        /// <summary>
        /// Get tracked quests.
        /// </summary>
        public List<QuestInstance> GetTrackedQuests()
        {
            return _activeQuests.Where(q => q.IsTracked).ToList();
        }

        /// <summary>
        /// Get quests by type.
        /// </summary>
        public List<QuestInstance> GetQuestsByType(QuestType type)
        {
            return _activeQuests.Where(q =>
            {
                var data = q.GetQuestData();
                return data != null && data.QuestType == type;
            }).ToList();
        }

        /// <summary>
        /// Track a quest.
        /// </summary>
        public void TrackQuest(string questId, bool track = true)
        {
            var quest = GetActiveQuest(questId);
            if (quest != null)
            {
                quest.IsTracked = track;
            }
        }

        private void FailQuest(QuestInstance quest)
        {
            quest.Status = QuestStatus.Failed;
            OnQuestFailed?.Invoke(quest);

            Debug.Log($"[QuestManager] Quest failed: {quest.QuestId}");
        }

        private void GrantRewards(List<QuestReward> rewards)
        {
            foreach (var reward in rewards)
            {
                switch (reward.Type)
                {
                    case RewardType.Experience:
                        _levelSystem?.AddExperience(reward.Amount);
                        break;

                    case RewardType.Item:
                        _inventory?.AddItem(reward.RewardId, reward.Amount);
                        break;

                    // Currency, Skill, Reputation would be handled by their respective systems
                    default:
                        Debug.Log($"[QuestManager] Reward: {reward.GetDisplayText()}");
                        break;
                }
            }
        }
    }

    #region Events

    public struct QuestAcceptedEvent : IEvent
    {
        public string QuestId;
        public QuestManager QuestManager;
    }

    public struct QuestCompletedEvent : IEvent
    {
        public string QuestId;
        public QuestManager QuestManager;
        public QuestReward[] Rewards;
    }

    public struct QuestObjectiveCompletedEvent : IEvent
    {
        public string QuestId;
        public string ObjectiveId;
    }

    #endregion
}
