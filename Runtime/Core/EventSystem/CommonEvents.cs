using UnityEngine;
using UnityEngine.Events;

namespace LWT.UnityWorkbench.Core
{
    // ==========================================
    // Common Event Types for EventBus
    // ==========================================

    /// <summary>
    /// Empty event with no data.
    /// </summary>
    public struct EmptyEvent : IEvent { }

    /// <summary>
    /// Event with integer data.
    /// </summary>
    public struct IntEvent : IEvent
    {
        public int Value;
        public IntEvent(int value) => Value = value;
    }

    /// <summary>
    /// Event with float data.
    /// </summary>
    public struct FloatEvent : IEvent
    {
        public float Value;
        public FloatEvent(float value) => Value = value;
    }

    /// <summary>
    /// Event with string data.
    /// </summary>
    public struct StringEvent : IEvent
    {
        public string Value;
        public StringEvent(string value) => Value = value;
    }

    /// <summary>
    /// Event with bool data.
    /// </summary>
    public struct BoolEvent : IEvent
    {
        public bool Value;
        public BoolEvent(bool value) => Value = value;
    }

    /// <summary>
    /// Event with Vector3 data.
    /// </summary>
    public struct Vector3Event : IEvent
    {
        public Vector3 Value;
        public Vector3Event(Vector3 value) => Value = value;
    }

    /// <summary>
    /// Event with GameObject reference.
    /// </summary>
    public struct GameObjectEvent : IEvent
    {
        public GameObject Value;
        public GameObjectEvent(GameObject value) => Value = value;
    }

    // ==========================================
    // Common Game Events
    // ==========================================

    public struct GameStartEvent : IEvent { }
    public struct GamePauseEvent : IEvent { }
    public struct GameResumeEvent : IEvent { }
    public struct GameOverEvent : IEvent { }

    public struct ScoreChangedEvent : IEvent
    {
        public int OldScore;
        public int NewScore;
        public int Delta => NewScore - OldScore;
    }

    public struct HealthChangedEvent : IEvent
    {
        public float OldHealth;
        public float NewHealth;
        public float MaxHealth;
        public float Percent => MaxHealth > 0 ? NewHealth / MaxHealth : 0;
    }

    public struct PlayerDiedEvent : IEvent
    {
        public GameObject Player;
    }

    public struct EnemyDiedEvent : IEvent
    {
        public GameObject Enemy;
        public int ScoreValue;
    }

    public struct ItemCollectedEvent : IEvent
    {
        public string ItemId;
        public int Quantity;
    }

    public struct LevelCompletedEvent : IEvent
    {
        public int LevelIndex;
        public float CompletionTime;
    }

    // ==========================================
    // ScriptableObject Event Implementations
    // ==========================================

    [CreateAssetMenu(fileName = "NewIntEvent", menuName = "LWT/Events/Int Event")]
    public class IntGameEvent : GameEvent<int> { }

    [CreateAssetMenu(fileName = "NewFloatEvent", menuName = "LWT/Events/Float Event")]
    public class FloatGameEvent : GameEvent<float> { }

    [CreateAssetMenu(fileName = "NewStringEvent", menuName = "LWT/Events/String Event")]
    public class StringGameEvent : GameEvent<string> { }

    [CreateAssetMenu(fileName = "NewBoolEvent", menuName = "LWT/Events/Bool Event")]
    public class BoolGameEvent : GameEvent<bool> { }

    [CreateAssetMenu(fileName = "NewVector3Event", menuName = "LWT/Events/Vector3 Event")]
    public class Vector3GameEvent : GameEvent<Vector3> { }

    // ==========================================
    // Concrete Listeners
    // ==========================================

    public class IntGameEventListener : GameEventListener<int>
    {
        [SerializeField] private IntGameEvent _event;
        [SerializeField] private UnityEvent<int> _response;

        private void OnEnable() => _event?.RegisterListener(this);
        private void OnDisable() => _event?.UnregisterListener(this);
        public override void OnEventRaised(int value) => _response?.Invoke(value);
    }

    public class FloatGameEventListener : GameEventListener<float>
    {
        [SerializeField] private FloatGameEvent _event;
        [SerializeField] private UnityEvent<float> _response;

        private void OnEnable() => _event?.RegisterListener(this);
        private void OnDisable() => _event?.UnregisterListener(this);
        public override void OnEventRaised(float value) => _response?.Invoke(value);
    }

    public class StringGameEventListener : GameEventListener<string>
    {
        [SerializeField] private StringGameEvent _event;
        [SerializeField] private UnityEvent<string> _response;

        private void OnEnable() => _event?.RegisterListener(this);
        private void OnDisable() => _event?.UnregisterListener(this);
        public override void OnEventRaised(string value) => _response?.Invoke(value);
    }
}
