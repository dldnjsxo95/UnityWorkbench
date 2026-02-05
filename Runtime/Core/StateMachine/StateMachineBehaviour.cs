using UnityEngine;

namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// MonoBehaviour wrapper for StateMachine.
    /// Inherit from this for easy state machine integration.
    /// </summary>
    public abstract class StateMachineBehaviour<T> : MonoBehaviour where T : StateMachineBehaviour<T>
    {
        protected StateMachine<T> StateMachine { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;

        public string CurrentStateName => StateMachine?.CurrentStateName ?? "Not Initialized";
        public float StateTime => StateMachine?.StateTime ?? 0f;

        protected virtual void Awake()
        {
            StateMachine = new StateMachine<T>((T)this);
            SetupStates();
        }

        protected virtual void Start()
        {
            InitializeStateMachine();
        }

        protected virtual void Update()
        {
            StateMachine?.Update();
        }

        protected virtual void FixedUpdate()
        {
            StateMachine?.FixedUpdate();
        }

        /// <summary>
        /// Override to add states to the state machine.
        /// </summary>
        protected abstract void SetupStates();

        /// <summary>
        /// Override to set the initial state.
        /// </summary>
        protected abstract void InitializeStateMachine();

        /// <summary>
        /// Change to a new state.
        /// </summary>
        protected void ChangeState<TState>() where TState : StateBase<T>
        {
            StateMachine?.ChangeState<TState>();
        }

        /// <summary>
        /// Check if in a specific state.
        /// </summary>
        protected bool IsInState<TState>() where TState : StateBase<T>
        {
            return StateMachine?.IsInState<TState>() ?? false;
        }

#if UNITY_EDITOR
        protected virtual void OnGUI()
        {
            if (_showDebugInfo && StateMachine != null)
            {
                GUILayout.BeginArea(new Rect(10, 10, 300, 100));
                GUILayout.Label($"State: {CurrentStateName}");
                GUILayout.Label($"Time: {StateTime:F2}s");
                GUILayout.EndArea();
            }
        }
#endif
    }
}
