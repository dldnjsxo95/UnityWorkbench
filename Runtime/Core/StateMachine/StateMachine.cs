using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// Generic Finite State Machine.
    /// Manages state transitions and lifecycle.
    /// </summary>
    public class StateMachine<T>
    {
        private readonly T _owner;
        private readonly Dictionary<Type, StateBase<T>> _states = new Dictionary<Type, StateBase<T>>();

        private StateBase<T> _currentState;
        private StateBase<T> _previousState;
        private float _stateTime;

        public StateBase<T> CurrentState => _currentState;
        public StateBase<T> PreviousState => _previousState;
        public float StateTime => _stateTime;
        public string CurrentStateName => _currentState?.GetType().Name ?? "None";

        public event Action<StateBase<T>, StateBase<T>> OnStateChanged;

        public StateMachine(T owner)
        {
            _owner = owner;
        }

        /// <summary>
        /// Add a state to the state machine.
        /// </summary>
        public void AddState<TState>(TState state) where TState : StateBase<T>
        {
            var type = typeof(TState);
            if (_states.ContainsKey(type))
            {
                Debug.LogWarning($"[StateMachine] State {type.Name} already exists.");
                return;
            }

            state.Initialize(_owner);
            state.SetStateMachine(this);
            _states[type] = state;
        }

        /// <summary>
        /// Add multiple states at once.
        /// </summary>
        public void AddStates(params StateBase<T>[] states)
        {
            foreach (var state in states)
            {
                var type = state.GetType();
                if (!_states.ContainsKey(type))
                {
                    state.Initialize(_owner);
                    state.SetStateMachine(this);
                    _states[type] = state;
                }
            }
        }

        /// <summary>
        /// Set initial state without calling Enter.
        /// </summary>
        public void SetInitialState<TState>() where TState : StateBase<T>
        {
            var type = typeof(TState);
            if (_states.TryGetValue(type, out var state))
            {
                _currentState = state;
                _stateTime = 0f;
            }
            else
            {
                Debug.LogError($"[StateMachine] State {type.Name} not found.");
            }
        }

        /// <summary>
        /// Start the state machine with initial state.
        /// </summary>
        public void Start<TState>() where TState : StateBase<T>
        {
            ChangeState<TState>();
        }

        /// <summary>
        /// Change to a new state.
        /// </summary>
        public void ChangeState<TState>() where TState : StateBase<T>
        {
            var type = typeof(TState);
            if (!_states.TryGetValue(type, out var newState))
            {
                Debug.LogError($"[StateMachine] State {type.Name} not found.");
                return;
            }

            ChangeState(newState);
        }

        /// <summary>
        /// Change to a new state by instance.
        /// </summary>
        public void ChangeState(StateBase<T> newState)
        {
            if (newState == null) return;
            if (_currentState == newState) return;

            _previousState = _currentState;
            _currentState?.Exit();

            _currentState = newState;
            _stateTime = 0f;
            _currentState.Enter();

            OnStateChanged?.Invoke(_previousState, _currentState);
        }

        /// <summary>
        /// Return to previous state.
        /// </summary>
        public void RevertToPreviousState()
        {
            if (_previousState != null)
            {
                ChangeState(_previousState);
            }
        }

        /// <summary>
        /// Update current state. Call from MonoBehaviour.Update().
        /// </summary>
        public void Update()
        {
            _stateTime += Time.deltaTime;
            _currentState?.Update();
        }

        /// <summary>
        /// Fixed update current state. Call from MonoBehaviour.FixedUpdate().
        /// </summary>
        public void FixedUpdate()
        {
            _currentState?.FixedUpdate();
        }

        /// <summary>
        /// Check if currently in a specific state.
        /// </summary>
        public bool IsInState<TState>() where TState : StateBase<T>
        {
            return _currentState?.GetType() == typeof(TState);
        }

        /// <summary>
        /// Get a registered state.
        /// </summary>
        public TState GetState<TState>() where TState : StateBase<T>
        {
            if (_states.TryGetValue(typeof(TState), out var state))
            {
                return (TState)state;
            }
            return null;
        }
    }
}
