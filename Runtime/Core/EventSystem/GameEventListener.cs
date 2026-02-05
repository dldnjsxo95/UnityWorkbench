using UnityEngine;
using UnityEngine.Events;

namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// Listens to a GameEvent and invokes UnityEvent response.
    /// Attach to any GameObject to respond to ScriptableObject events.
    /// </summary>
    public class GameEventListener : MonoBehaviour
    {
        [Tooltip("Event to listen for")]
        [SerializeField] private GameEvent _event;

        [Tooltip("Response to invoke when event is raised")]
        [SerializeField] private UnityEvent _response;

        private void OnEnable()
        {
            _event?.RegisterListener(this);
        }

        private void OnDisable()
        {
            _event?.UnregisterListener(this);
        }

        public void OnEventRaised()
        {
            _response?.Invoke();
        }
    }
}
