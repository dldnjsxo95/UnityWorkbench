namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// Base class for states with owner reference.
    /// Inherit from this for convenient state implementation.
    /// </summary>
    public abstract class StateBase<T> : IState<T>
    {
        protected T Owner { get; private set; }
        protected StateMachine<T> StateMachine { get; private set; }

        public void Initialize(T owner)
        {
            Owner = owner;
        }

        internal void SetStateMachine(StateMachine<T> stateMachine)
        {
            StateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void Exit() { }

        /// <summary>
        /// Request transition to another state.
        /// </summary>
        protected void TransitionTo<TState>() where TState : StateBase<T>
        {
            StateMachine?.ChangeState<TState>();
        }
    }
}
