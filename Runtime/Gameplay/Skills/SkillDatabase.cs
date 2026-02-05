using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Gameplay
{
    /// <summary>
    /// Database for storing and retrieving skill definitions.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillDatabase", menuName = "UnityWorkbench/Gameplay/Skill Database")]
    public class SkillDatabase : ScriptableObject
    {
        [SerializeField] private List<SkillBase> _skills = new List<SkillBase>();

        // Runtime lookup
        private Dictionary<string, SkillBase> _skillLookup;
        private bool _isInitialized;

        private static SkillDatabase _instance;

        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static SkillDatabase Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<SkillDatabase>("SkillDatabase");
                    if (_instance != null)
                    {
                        _instance.Initialize();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// All skills in database.
        /// </summary>
        public IReadOnlyList<SkillBase> Skills => _skills;

        /// <summary>
        /// Number of skills.
        /// </summary>
        public int Count => _skills.Count;

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

            _skillLookup = new Dictionary<string, SkillBase>();

            foreach (var skill in _skills)
            {
                if (skill == null) continue;

                if (_skillLookup.ContainsKey(skill.SkillId))
                {
                    Debug.LogWarning($"[SkillDatabase] Duplicate skill ID: {skill.SkillId}");
                    continue;
                }

                _skillLookup[skill.SkillId] = skill;
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Get a skill by ID.
        /// </summary>
        public SkillBase GetSkill(string skillId)
        {
            if (!_isInitialized) Initialize();

            if (string.IsNullOrEmpty(skillId))
                return null;

            return _skillLookup.TryGetValue(skillId, out var skill) ? skill : null;
        }

        /// <summary>
        /// Check if a skill exists.
        /// </summary>
        public bool HasSkill(string skillId)
        {
            if (!_isInitialized) Initialize();
            return !string.IsNullOrEmpty(skillId) && _skillLookup.ContainsKey(skillId);
        }

        /// <summary>
        /// Get skills by type.
        /// </summary>
        public List<SkillBase> GetSkillsByType(SkillType type)
        {
            if (!_isInitialized) Initialize();

            var result = new List<SkillBase>();
            foreach (var skill in _skills)
            {
                if (skill != null && skill.SkillType == type)
                {
                    result.Add(skill);
                }
            }
            return result;
        }

        /// <summary>
        /// Get skills by target type.
        /// </summary>
        public List<SkillBase> GetSkillsByTargetType(TargetType targetType)
        {
            if (!_isInitialized) Initialize();

            var result = new List<SkillBase>();
            foreach (var skill in _skills)
            {
                if (skill != null && skill.TargetType == targetType)
                {
                    result.Add(skill);
                }
            }
            return result;
        }

        /// <summary>
        /// Get skills available at a level.
        /// </summary>
        public List<SkillBase> GetSkillsForLevel(int level)
        {
            if (!_isInitialized) Initialize();

            var result = new List<SkillBase>();
            foreach (var skill in _skills)
            {
                if (skill != null && skill.RequiredLevel <= level)
                {
                    result.Add(skill);
                }
            }
            return result;
        }

        /// <summary>
        /// Search skills by name.
        /// </summary>
        public List<SkillBase> SearchSkills(string searchTerm)
        {
            if (!_isInitialized) Initialize();

            if (string.IsNullOrEmpty(searchTerm))
                return new List<SkillBase>(_skills);

            searchTerm = searchTerm.ToLowerInvariant();
            var result = new List<SkillBase>();

            foreach (var skill in _skills)
            {
                if (skill == null) continue;

                if (skill.DisplayName.ToLowerInvariant().Contains(searchTerm) ||
                    skill.SkillId.ToLowerInvariant().Contains(searchTerm))
                {
                    result.Add(skill);
                }
            }

            return result;
        }

        /// <summary>
        /// Create a skill instance.
        /// </summary>
        public SkillInstance CreateInstance(string skillId, int level = 1)
        {
            var skillData = GetSkill(skillId);
            if (skillData == null)
            {
                Debug.LogWarning($"[SkillDatabase] Skill not found: {skillId}");
                return null;
            }

            return new SkillInstance(skillId, level);
        }

        #region Editor Methods

#if UNITY_EDITOR
        /// <summary>
        /// Add a skill to the database (Editor only).
        /// </summary>
        public void AddSkill(SkillBase skill)
        {
            if (skill == null || _skills.Contains(skill)) return;
            _skills.Add(skill);
            _isInitialized = false;
        }

        /// <summary>
        /// Remove a skill from the database (Editor only).
        /// </summary>
        public void RemoveSkill(SkillBase skill)
        {
            _skills.Remove(skill);
            _isInitialized = false;
        }

        /// <summary>
        /// Clear all skills (Editor only).
        /// </summary>
        public void ClearAll()
        {
            _skills.Clear();
            _skillLookup?.Clear();
            _isInitialized = false;
        }

        /// <summary>
        /// Refresh database from project skills (Editor only).
        /// </summary>
        public void RefreshFromAssets()
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("t:SkillBase");
            _skills.Clear();

            foreach (var guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var skill = UnityEditor.AssetDatabase.LoadAssetAtPath<SkillBase>(path);
                if (skill != null)
                {
                    _skills.Add(skill);
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
