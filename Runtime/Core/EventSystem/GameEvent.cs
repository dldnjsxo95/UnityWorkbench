using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// ScriptableObject-based event channel.
    /// Create instances via Create menu: LWT/Events/Game Event
    /// </summary>
    [CreateAssetMenu(fileName = "NewGameEvent", menuName = "LWT/Events/Game Event")]
    public class GameEvent : ScriptableObject
    {
        private readonly List<GameEventListener> _listeners = new List<GameEventListener>();

#if UNITY_EDITOR
        [TextArea(3, 5)]
        [SerializeField] private string _description;
#endif

        public void Raise()
        {
            // Iterate backwards to safely handle unsubscription during iteration
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i]?.OnEventRaised();
            }
        }

        public void RegisterListener(GameEventListener listener)
        {
            if (!_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        public void UnregisterListener(GameEventListener listener)
        {
            _listeners.Remove(listener);
        }

        public int ListenerCount => _listeners.Count;
    }
}
