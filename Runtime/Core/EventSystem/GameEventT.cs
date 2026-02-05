using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// Generic ScriptableObject-based event channel with data payload.
    /// </summary>
    /// <typeparam name="T">Type of data to pass with event</typeparam>
    public abstract class GameEvent<T> : ScriptableObject
    {
        private readonly List<GameEventListener<T>> _listeners = new List<GameEventListener<T>>();

#if UNITY_EDITOR
        [TextArea(3, 5)]
        [SerializeField] private string _description;

        [SerializeField] private T _debugValue;
#endif

        public void Raise(T value)
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i]?.OnEventRaised(value);
            }
        }

        public void RegisterListener(GameEventListener<T> listener)
        {
            if (!_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        public void UnregisterListener(GameEventListener<T> listener)
        {
            _listeners.Remove(listener);
        }

#if UNITY_EDITOR
        [ContextMenu("Raise (Debug)")]
        private void RaiseDebug()
        {
            Raise(_debugValue);
        }
#endif
    }

    /// <summary>
    /// Generic listener for GameEvent with data.
    /// </summary>
    public abstract class GameEventListener<T> : MonoBehaviour
    {
        public abstract void OnEventRaised(T value);
    }
}
