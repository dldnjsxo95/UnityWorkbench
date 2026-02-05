using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// State Machine with automatic condition-based transitions.
    /// </summary>
    public class StateMachineWithTransitions<T>
    {
        private readonly T _owner;
        private readonly Dictionary<Type, StateBase<T>> _states = new Dictionary<Type, StateBase<T>>();
        private readonly Dictionary<Type, List<Transition>> _transitions = new Dictionary<Type, List<Transition>>();
        private readonly List<Transition> _anyTransitions = new List<Transition>();

        private StateBase<T> _currentState;
        private StateBase<T> _previousState;
        private float _stateTime;

        public StateBase<T> CurrentState => _currentState;
        public string CurrentStateName => _currentState?.GetType().Name ?? "None";
        public float StateTime => _stateTime;

        public event Action<StateBase<T>, StateBase<T>> OnStateChanged;

        public StateMachineWithTransitions(T owner)
        {
            _owner = owner;
        }

        private class Transition
        {
            public Type ToState;
            public Func<bool> Condition;
            public int Priority;
        }

        public void AddState<TState>(TState state) where TState : StateBase<T>
        {
            var type = typeof(TState);
            state.Initialize(_owner);
            state.SetStateMachine(null); // Uses this class instead
            _states[type] = state;
            _transitions[type] = new List<Transition>();
        }

        /// <summary>
        /// Add transition from one state to another with condition.
        /// </summary>
        public void AddTransition<TFrom, TTo>(Func<bool> condition, int priority = 0)
            where TFrom : StateBase<T>
            where TTo : StateBase<T>
        {
            var fromType = typeof(TFrom);
            if (!_transitions.ContainsKey(fromType))
            {
                _transitions[fromType] = new List<Transition>();
            }

            _transitions[fromType].Add(new Transition
            {
                ToState = typeof(TTo),
                Condition = condition,
                Priority = priority
            });

            // Sort by priority (higher first)
            _transitions[fromType].Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        /// <summary>
        /// Add transition from any state with condition.
        /// </summary>
        public void AddAnyTransition<TTo>(Func<bool> condition, int priority = 0)
            where TTo : StateBase<T>
        {
            _anyTransitions.Add(new Transition
            {
                ToState = typeof(TTo),
                Condition = condition,
                Priority = priority
            });

            _anyTransitions.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        public void Start<TState>() where TState : StateBase<T>
        {
            ForceState<TState>();
        }

        public void ForceState<TState>() where TState : StateBase<T>
        {
            var type = typeof(TState);
            if (_states.TryGetValue(type, out var state))
            {
                ChangeState(state);
            }
        }

        private void ChangeState(StateBase<T> newState)
        {
            if (newState == null || _currentState == newState) return;

            _previousState = _currentState;
            _currentState?.Exit();

            _currentState = newState;
            _stateTime = 0f;
            _currentState.Enter();

            OnStateChanged?.Invoke(_previousState, _currentState);
        }

        public void Update()
        {
            // Check any transitions first
            foreach (var transition in _anyTransitions)
            {
                if (_currentState?.GetType() != transition.ToState && transition.Condition())
                {
                    if (_states.TryGetValue(transition.ToState, out var state))
                    {
                        ChangeState(state);
                        break;
                    }
                }
            }

            // Check state-specific transitions
            if (_currentState != null)
            {
                var currentType = _currentState.GetType();
                if (_transitions.TryGetValue(currentType, out var transitions))
                {
                    foreach (var transition in transitions)
                    {
                        if (transition.Condition())
                        {
                            if (_states.TryGetValue(transition.ToState, out var state))
                            {
                                ChangeState(state);
                                break;
                            }
                        }
                    }
                }
            }

            _stateTime += Time.deltaTime;
            _currentState?.Update();
        }

        public void FixedUpdate()
        {
            _currentState?.FixedUpdate();
        }

        public bool IsInState<TState>() where TState : StateBase<T>
        {
            return _currentState?.GetType() == typeof(TState);
        }
    }
}
