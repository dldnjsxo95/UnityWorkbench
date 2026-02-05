using UnityEngine;

namespace LWT.UnityWorkbench.Data
{
    /// <summary>
    /// Base class for ScriptableObject data containers.
    /// Use for items, characters, abilities, etc.
    /// </summary>
    public abstract class DataContainer : ScriptableObject
    {
        [Header("Base Info")]
        [SerializeField] protected string _id;
        [SerializeField] protected string _displayName;
        [TextArea(2, 4)]
        [SerializeField] protected string _description;
        [SerializeField] protected Sprite _icon;

        public string Id => string.IsNullOrEmpty(_id) ? name : _id;
        public string DisplayName => string.IsNullOrEmpty(_displayName) ? name : _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;

        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(_id))
            {
                _id = name;
            }
        }
    }

    /// <summary>
    /// Generic data container with typed data.
    /// </summary>
    public abstract class DataContainer<T> : DataContainer where T : new()
    {
        [Header("Data")]
        [SerializeField] protected T _data = new T();

        public T Data => _data;
    }
}
