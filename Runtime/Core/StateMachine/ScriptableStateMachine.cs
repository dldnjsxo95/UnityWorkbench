using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// Data-driven state machine using ScriptableObject states.
    /// Configure states in Inspector.
    /// </summary>
    public class ScriptableStateMachine : MonoBehaviour
    {
        [Header("States")]
        [SerializeField] private ScriptableState _initialState;
        [SerializeField] private List<ScriptableState> _states = new List<ScriptableState>();

        [Header("Debug")]
        [SerializeField] private bool _showDebug;

        private ScriptableState _currentState;
        private ScriptableState _previousState;
        private float _stateTime;

        private readonly Dictionary<ScriptableState, ScriptableState> _stateInstances =
            new Dictionary<ScriptableState, ScriptableState>();

        public ScriptableState CurrentState => _currentState;
        public ScriptableState PreviousState => _previousState;
        public string CurrentStateName => _currentState != null ? _currentState.name : "None";
        public float StateTime => _stateTime;

        private void Awake()
        {
            // Create runtime instances of states
            foreach (var state in _states)
            {
                if (state != null)
                {
                    var instance = Instantiate(state);
                    instance.Initialize(gameObject, this);
                    _stateInstances[state] = instance;
                }
            }

            if (_initialState != null && !_stateInstances.ContainsKey(_initialState))
            {
                var instance = Instantiate(_initialState);
                instance.Initialize(gameObject, this);
                _stateInstances[_initialState] = instance;
            }
        }

        private void Start()
        {
            if (_initialState != null)
            {
                ChangeState(_initialState);
            }
        }

        private void Update()
        {
            _stateTime += Time.deltaTime;
            _currentState?.Update();
        }

        private void FixedUpdate()
        {
            _currentState?.FixedUpdate();
        }

        public void ChangeState(ScriptableState state)
        {
            if (state == null) return;

            // Get runtime instance
            if (!_stateInstances.TryGetValue(state, out var instance))
            {
                Debug.LogWarning($"[ScriptableStateMachine] State {state.name} not registered.");
                return;
            }

            if (_currentState == instance) return;

            _previousState = _currentState;
            _currentState?.Exit();

            _currentState = instance;
            _stateTime = 0f;
            _currentState.Enter();
        }

        public void RevertToPreviousState()
        {
            if (_previousState != null)
            {
                // Find original asset for previous state
                foreach (var kvp in _stateInstances)
                {
                    if (kvp.Value == _previousState)
                    {
                        ChangeState(kvp.Key);
                        return;
                    }
                }
            }
        }

        public bool IsInState(ScriptableState state)
        {
            if (state == null) return false;
            return _stateInstances.TryGetValue(state, out var instance) && _currentState == instance;
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (_showDebug)
            {
                GUILayout.BeginArea(new Rect(10, 10, 300, 100));
                GUILayout.Label($"State: {CurrentStateName}");
                GUILayout.Label($"Time: {_stateTime:F2}s");
                GUILayout.EndArea();
            }
        }
#endif

        private void OnDestroy()
        {
            // Clean up instances
            foreach (var instance in _stateInstances.Values)
            {
                if (instance != null)
                {
                    Destroy(instance);
                }
            }
            _stateInstances.Clear();
        }
    }
}
