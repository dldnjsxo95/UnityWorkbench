using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// Runtime instance of a quest.
    /// </summary>
    [Serializable]
    public class QuestInstance
    {
        [SerializeField] private string _questId;
        [SerializeField] private QuestStatus _status = QuestStatus.InProgress;
        [SerializeField] private List<ObjectiveProgress> _objectiveProgress = new List<ObjectiveProgress>();
        [SerializeField] private float _timeRemaining = -1f;
        [SerializeField] private bool _isTracked;

        // Transient cache
        [NonSerialized] private QuestBase _cachedQuestData;

        /// <summary>
        /// Quest identifier.
        /// </summary>
        public string QuestId => _questId;

        /// <summary>
        /// Current status.
        /// </summary>
        public QuestStatus Status
        {
            get => _status;
            set => _status = value;
        }

        /// <summary>
        /// Time remaining (-1 = no limit).
        /// </summary>
        public float TimeRemaining => _timeRemaining;

        /// <summary>
        /// Whether this quest has a time limit.
        /// </summary>
        public bool HasTimeLimit => _timeRemaining >= 0;

        /// <summary>
        /// Whether this quest is being tracked.
        /// </summary>
        public bool IsTracked
        {
            get => _isTracked;
            set => _isTracked = value;
        }

        /// <summary>
        /// Progress for each objective.
        /// </summary>
        public IReadOnlyList<ObjectiveProgress> ObjectiveProgress => _objectiveProgress;

        /// <summary>
        /// Get the quest data from database.
        /// </summary>
        public QuestBase GetQuestData()
        {
            if (_cachedQuestData == null && !string.IsNullOrEmpty(_questId))
            {
                _cachedQuestData = QuestDatabase.Instance?.GetQuest(_questId);
            }
            return _cachedQuestData;
        }

        public QuestInstance() { }

        public QuestInstance(QuestBase questData)
        {
            _questId = questData.QuestId;
            _cachedQuestData = questData;
            _status = QuestStatus.InProgress;
            _isTracked = questData.TrackOnAccept;

            // Initialize time limit
            if (questData.TimeLimit > 0)
            {
                _timeRemaining = questData.TimeLimit;
            }

            // Initialize objective progress
            foreach (var objective in questData.Objectives)
            {
                var progress = new ObjectiveProgress(objective.ObjectiveId);
                progress.IsDiscovered = !objective.IsHidden;
                _objectiveProgress.Add(progress);
            }
        }

        /// <summary>
        /// Update time limit (call from Update).
        /// </summary>
        public bool UpdateTime(float deltaTime)
        {
            if (!HasTimeLimit || _status != QuestStatus.InProgress)
                return false;

            _timeRemaining -= deltaTime;
            if (_timeRemaining <= 0)
            {
                _timeRemaining = 0;
                _status = QuestStatus.Failed;
                return true; // Quest failed
            }

            return false;
        }

        /// <summary>
        /// Report progress on an objective.
        /// </summary>
        public bool ReportProgress(string objectiveId, int amount = 1)
        {
            if (_status != QuestStatus.InProgress)
                return false;

            var questData = GetQuestData();
            if (questData == null) return false;

            // Find objective
            QuestObjective objective = null;
            ObjectiveProgress progress = null;

            for (int i = 0; i < questData.Objectives.Count; i++)
            {
                if (questData.Objectives[i].ObjectiveId == objectiveId)
                {
                    objective = questData.Objectives[i];
                    progress = _objectiveProgress[i];
                    break;
                }
            }

            if (objective == null || progress == null)
                return false;

            // Discover hidden objective
            if (!progress.IsDiscovered)
            {
                progress.IsDiscovered = true;
            }

            // Add progress
            bool justCompleted = progress.AddProgress(amount, objective.RequiredAmount);

            // Check if all required objectives are complete
            if (justCompleted)
            {
                CheckCompletion();
            }

            return justCompleted;
        }

        /// <summary>
        /// Report progress by target ID (for kill/collect objectives).
        /// </summary>
        public bool ReportProgressByTarget(ObjectiveType type, string targetId, int amount = 1)
        {
            if (_status != QuestStatus.InProgress)
                return false;

            var questData = GetQuestData();
            if (questData == null) return false;

            bool anyCompleted = false;

            for (int i = 0; i < questData.Objectives.Count; i++)
            {
                var objective = questData.Objectives[i];
                if (objective.Type == type && objective.TargetId == targetId)
                {
                    var progress = _objectiveProgress[i];

                    if (!progress.IsDiscovered)
                    {
                        progress.IsDiscovered = true;
                    }

                    if (progress.AddProgress(amount, objective.RequiredAmount))
                    {
                        anyCompleted = true;
                    }
                }
            }

            if (anyCompleted)
            {
                CheckCompletion();
            }

            return anyCompleted;
        }

        /// <summary>
        /// Check if all required objectives are complete.
        /// </summary>
        public void CheckCompletion()
        {
            var questData = GetQuestData();
            if (questData == null) return;

            bool allComplete = true;

            for (int i = 0; i < questData.Objectives.Count; i++)
            {
                var objective = questData.Objectives[i];
                var progress = _objectiveProgress[i];

                // Skip optional objectives
                if (objective.IsOptional) continue;

                if (!progress.IsCompleted)
                {
                    allComplete = false;
                    break;
                }
            }

            if (allComplete && questData.AutoComplete)
            {
                _status = QuestStatus.Completed;
            }
        }

        /// <summary>
        /// Get overall progress (0-1).
        /// </summary>
        public float GetOverallProgress()
        {
            var questData = GetQuestData();
            if (questData == null || questData.Objectives.Count == 0)
                return 0;

            float total = 0;
            int count = 0;

            for (int i = 0; i < questData.Objectives.Count; i++)
            {
                var objective = questData.Objectives[i];
                if (objective.IsOptional) continue;

                var progress = _objectiveProgress[i];
                total += progress.GetProgress(objective.RequiredAmount);
                count++;
            }

            return count > 0 ? total / count : 1f;
        }

        /// <summary>
        /// Get progress for specific objective.
        /// </summary>
        public ObjectiveProgress GetObjectiveProgress(string objectiveId)
        {
            var questData = GetQuestData();
            if (questData == null) return null;

            for (int i = 0; i < questData.Objectives.Count; i++)
            {
                if (questData.Objectives[i].ObjectiveId == objectiveId)
                {
                    return _objectiveProgress[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Fail the quest.
        /// </summary>
        public void Fail()
        {
            if (_status == QuestStatus.InProgress)
            {
                _status = QuestStatus.Failed;
            }
        }

        /// <summary>
        /// Abandon the quest (reset progress).
        /// </summary>
        public void Abandon()
        {
            _status = QuestStatus.Available;
            foreach (var progress in _objectiveProgress)
            {
                progress.CurrentAmount = 0;
                progress.IsCompleted = false;
            }

            var questData = GetQuestData();
            if (questData != null && questData.TimeLimit > 0)
            {
                _timeRemaining = questData.TimeLimit;
            }
        }

        public override string ToString()
        {
            var questData = GetQuestData();
            return questData != null ? $"{questData.Title} ({_status})" : $"[{_questId}] ({_status})";
        }
    }
}
