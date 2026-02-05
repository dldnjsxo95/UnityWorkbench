namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// Base interface for all states.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Called when entering this state.
        /// </summary>
        void Enter();

        /// <summary>
        /// Called every frame while in this state.
        /// </summary>
        void Update();

        /// <summary>
        /// Called every fixed frame while in this state.
        /// </summary>
        void FixedUpdate();

        /// <summary>
        /// Called when exiting this state.
        /// </summary>
        void Exit();
    }

    /// <summary>
    /// Generic state interface with owner reference.
    /// </summary>
    public interface IState<T> : IState
    {
        /// <summary>
        /// Initialize state with owner reference.
        /// </summary>
        void Initialize(T owner);
    }
}
