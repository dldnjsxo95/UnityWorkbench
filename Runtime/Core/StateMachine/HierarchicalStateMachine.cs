using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// Hierarchical Finite State Machine (HFSM).
    /// States can contain sub-states for complex behaviors.
    /// </summary>
    public class HierarchicalStateMachine<T>
    {
        private readonly T _owner;
        private readonly Dictionary<Type, HierarchicalState<T>> _states = new Dictionary<Type, HierarchicalState<T>>();

        private HierarchicalState<T> _currentState;
        private readonly Stack<HierarchicalState<T>> _stateStack = new Stack<HierarchicalState<T>>();

        public HierarchicalState<T> CurrentState => _currentState;
        public string CurrentStatePath => GetStatePath();
        public int Depth => _stateStack.Count;

        public event Action<HierarchicalState<T>, HierarchicalState<T>> OnStateChanged;

        public HierarchicalStateMachine(T owner)
        {
            _owner = owner;
        }

        public void AddState<TState>(TState state) where TState : HierarchicalState<T>
        {
            var type = typeof(TState);
            state.Initialize(_owner, this);
            _states[type] = state;
        }

        public void Start<TState>() where TState : HierarchicalState<T>
        {
            ChangeState<TState>();
        }

        public void ChangeState<TState>() where TState : HierarchicalState<T>
        {
            var type = typeof(TState);
            if (_states.TryGetValue(type, out var state))
            {
                ChangeState(state);
            }
            else
            {
                Debug.LogError($"[HFSM] State {type.Name} not found.");
            }
        }

        private void ChangeState(HierarchicalState<T> newState)
        {
            if (newState == null || _currentState == newState) return;

            var previousState = _currentState;

            // Exit current state hierarchy
            _currentState?.ExitHierarchy();
            _stateStack.Clear();

            // Enter new state
            _currentState = newState;
            _stateStack.Push(_currentState);
            _currentState.EnterHierarchy();

            OnStateChanged?.Invoke(previousState, _currentState);
        }

        /// <summary>
        /// Push a sub-state onto the stack.
        /// </summary>
        public void PushState<TState>() where TState : HierarchicalState<T>
        {
            var type = typeof(TState);
            if (_states.TryGetValue(type, out var state))
            {
                _currentState?.Pause();
                _stateStack.Push(state);
                _currentState = state;
                _currentState.EnterHierarchy();
            }
        }

        /// <summary>
        /// Pop current state and return to previous.
        /// </summary>
        public void PopState()
        {
            if (_stateStack.Count <= 1) return;

            _currentState?.ExitHierarchy();
            _stateStack.Pop();

            _currentState = _stateStack.Peek();
            _currentState?.Resume();
        }

        public void Update()
        {
            _currentState?.UpdateHierarchy();
        }

        public void FixedUpdate()
        {
            _currentState?.FixedUpdateHierarchy();
        }

        private string GetStatePath()
        {
            if (_stateStack.Count == 0) return "None";

            var path = new System.Text.StringBuilder();
            var states = _stateStack.ToArray();
            Array.Reverse(states);

            for (int i = 0; i < states.Length; i++)
            {
                if (i > 0) path.Append(" > ");
                path.Append(states[i].GetType().Name);
            }

            return path.ToString();
        }

        public bool IsInState<TState>() where TState : HierarchicalState<T>
        {
            var type = typeof(TState);
            foreach (var state in _stateStack)
            {
                if (state.GetType() == type) return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Base class for hierarchical states.
    /// </summary>
    public abstract class HierarchicalState<T>
    {
        protected T Owner { get; private set; }
        protected HierarchicalStateMachine<T> StateMachine { get; private set; }
        protected HierarchicalState<T> SubState { get; private set; }

        private readonly Dictionary<Type, HierarchicalState<T>> _subStates = new Dictionary<Type, HierarchicalState<T>>();

        public void Initialize(T owner, HierarchicalStateMachine<T> stateMachine)
        {
            Owner = owner;
            StateMachine = stateMachine;
        }

        /// <summary>
        /// Add a sub-state to this state.
        /// </summary>
        protected void AddSubState<TState>(TState state) where TState : HierarchicalState<T>
        {
            var type = typeof(TState);
            state.Initialize(Owner, StateMachine);
            _subStates[type] = state;
        }

        /// <summary>
        /// Change to a sub-state.
        /// </summary>
        protected void ChangeSubState<TState>() where TState : HierarchicalState<T>
        {
            var type = typeof(TState);
            if (_subStates.TryGetValue(type, out var state))
            {
                SubState?.ExitHierarchy();
                SubState = state;
                SubState.EnterHierarchy();
            }
        }

        internal void EnterHierarchy()
        {
            Enter();
        }

        internal void ExitHierarchy()
        {
            SubState?.ExitHierarchy();
            SubState = null;
            Exit();
        }

        internal void UpdateHierarchy()
        {
            Update();
            SubState?.UpdateHierarchy();
        }

        internal void FixedUpdateHierarchy()
        {
            FixedUpdate();
            SubState?.FixedUpdateHierarchy();
        }

        internal void Pause()
        {
            OnPause();
        }

        internal void Resume()
        {
            OnResume();
        }

        protected virtual void Enter() { }
        protected virtual void Update() { }
        protected virtual void FixedUpdate() { }
        protected virtual void Exit() { }
        protected virtual void OnPause() { }
        protected virtual void OnResume() { }

        protected void TransitionTo<TState>() where TState : HierarchicalState<T>
        {
            StateMachine?.ChangeState<TState>();
        }
    }
}
