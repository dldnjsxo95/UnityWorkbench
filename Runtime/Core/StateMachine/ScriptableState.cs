using UnityEngine;

namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// ScriptableObject-based state for data-driven state machines.
    /// Create state assets via Create menu.
    /// </summary>
    public abstract class ScriptableState : ScriptableObject
    {
        [TextArea(2, 4)]
        [SerializeField] private string _description;

        protected GameObject Owner { get; private set; }
        protected ScriptableStateMachine StateMachine { get; private set; }

        public void Initialize(GameObject owner, ScriptableStateMachine stateMachine)
        {
            Owner = owner;
            StateMachine = stateMachine;
            OnInitialize();
        }

        protected virtual void OnInitialize() { }
        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void Exit() { }

        protected T GetComponent<T>() where T : Component
        {
            return Owner != null ? Owner.GetComponent<T>() : null;
        }

        protected void TransitionTo(ScriptableState state)
        {
            StateMachine?.ChangeState(state);
        }
    }

    /// <summary>
    /// Generic ScriptableObject state with typed owner.
    /// </summary>
    public abstract class ScriptableState<T> : ScriptableState where T : Component
    {
        protected new T Owner { get; private set; }

        protected override void OnInitialize()
        {
            Owner = base.Owner?.GetComponent<T>();
        }
    }
}
