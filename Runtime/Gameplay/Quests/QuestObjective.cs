using System;
using UnityEngine;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// Definition of a quest objective.
    /// </summary>
    [Serializable]
    public class QuestObjective
    {
        [SerializeField] private string _objectiveId;
        [SerializeField] private string _description;
        [SerializeField] private ObjectiveType _type = ObjectiveType.Kill;
        [SerializeField] private string _targetId;
        [SerializeField] private int _requiredAmount = 1;
        [SerializeField] private bool _isOptional;
        [SerializeField] private bool _isHidden;

        /// <summary>
        /// Unique identifier for this objective.
        /// </summary>
        public string ObjectiveId => _objectiveId;

        /// <summary>
        /// Description shown to player.
        /// </summary>
        public string Description => _description;

        /// <summary>
        /// Type of objective.
        /// </summary>
        public ObjectiveType Type => _type;

        /// <summary>
        /// Target ID (enemy type, item ID, NPC name, etc.)
        /// </summary>
        public string TargetId => _targetId;

        /// <summary>
        /// Amount required to complete.
        /// </summary>
        public int RequiredAmount => _requiredAmount;

        /// <summary>
        /// Whether this objective is optional.
        /// </summary>
        public bool IsOptional => _isOptional;

        /// <summary>
        /// Whether this objective is hidden until discovered.
        /// </summary>
        public bool IsHidden => _isHidden;

        public QuestObjective() { }

        public QuestObjective(string id, string description, ObjectiveType type, string targetId, int amount = 1)
        {
            _objectiveId = id;
            _description = description;
            _type = type;
            _targetId = targetId;
            _requiredAmount = amount;
        }
    }

    /// <summary>
    /// Runtime state of a quest objective.
    /// </summary>
    [Serializable]
    public class ObjectiveProgress
    {
        [SerializeField] private string _objectiveId;
        [SerializeField] private int _currentAmount;
        [SerializeField] private bool _isCompleted;
        [SerializeField] private bool _isDiscovered = true;

        /// <summary>
        /// Reference to objective definition.
        /// </summary>
        public string ObjectiveId => _objectiveId;

        /// <summary>
        /// Current progress amount.
        /// </summary>
        public int CurrentAmount
        {
            get => _currentAmount;
            set => _currentAmount = Mathf.Max(0, value);
        }

        /// <summary>
        /// Whether this objective is completed.
        /// </summary>
        public bool IsCompleted
        {
            get => _isCompleted;
            set => _isCompleted = value;
        }

        /// <summary>
        /// Whether this hidden objective has been discovered.
        /// </summary>
        public bool IsDiscovered
        {
            get => _isDiscovered;
            set => _isDiscovered = value;
        }

        public ObjectiveProgress() { }

        public ObjectiveProgress(string objectiveId)
        {
            _objectiveId = objectiveId;
            _currentAmount = 0;
            _isCompleted = false;
        }

        /// <summary>
        /// Add progress and check completion.
        /// </summary>
        public bool AddProgress(int amount, int requiredAmount)
        {
            if (_isCompleted) return false;

            _currentAmount = Mathf.Min(_currentAmount + amount, requiredAmount);

            if (_currentAmount >= requiredAmount)
            {
                _isCompleted = true;
                return true; // Just completed
            }

            return false;
        }

        /// <summary>
        /// Get progress as percentage (0-1).
        /// </summary>
        public float GetProgress(int requiredAmount)
        {
            if (requiredAmount <= 0) return 1f;
            return Mathf.Clamp01((float)_currentAmount / requiredAmount);
        }
    }
}
