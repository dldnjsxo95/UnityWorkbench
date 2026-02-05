using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// Debug utility for tracking event subscriptions and publications.
    /// Enable in development builds for debugging event flow.
    /// </summary>
    public static class EventDebugger
    {
        private static bool _isEnabled;
        private static readonly List<EventLogEntry> _eventLog = new List<EventLogEntry>();
        private static int _maxLogEntries = 100;

        public static bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public static int MaxLogEntries
        {
            get => _maxLogEntries;
            set => _maxLogEntries = Mathf.Max(10, value);
        }

        public static IReadOnlyList<EventLogEntry> EventLog => _eventLog;

        public static void LogPublish<T>(T eventData) where T : struct, IEvent
        {
            if (!_isEnabled) return;

            var entry = new EventLogEntry
            {
                Timestamp = Time.time,
                EventType = typeof(T).Name,
                Action = "Publish",
                Data = eventData.ToString()
            };

            AddLogEntry(entry);
            Debug.Log($"[EventDebugger] PUBLISH: {entry.EventType} - {entry.Data}");
        }

        public static void LogSubscribe<T>(Action<T> handler) where T : struct, IEvent
        {
            if (!_isEnabled) return;

            var entry = new EventLogEntry
            {
                Timestamp = Time.time,
                EventType = typeof(T).Name,
                Action = "Subscribe",
                Data = handler.Method.DeclaringType?.Name + "." + handler.Method.Name
            };

            AddLogEntry(entry);
            Debug.Log($"[EventDebugger] SUBSCRIBE: {entry.EventType} <- {entry.Data}");
        }

        public static void LogUnsubscribe<T>(Action<T> handler) where T : struct, IEvent
        {
            if (!_isEnabled) return;

            var entry = new EventLogEntry
            {
                Timestamp = Time.time,
                EventType = typeof(T).Name,
                Action = "Unsubscribe",
                Data = handler.Method.DeclaringType?.Name + "." + handler.Method.Name
            };

            AddLogEntry(entry);
            Debug.Log($"[EventDebugger] UNSUBSCRIBE: {entry.EventType} <- {entry.Data}");
        }

        private static void AddLogEntry(EventLogEntry entry)
        {
            _eventLog.Add(entry);
            while (_eventLog.Count > _maxLogEntries)
            {
                _eventLog.RemoveAt(0);
            }
        }

        public static void Clear()
        {
            _eventLog.Clear();
        }

        public struct EventLogEntry
        {
            public float Timestamp;
            public string EventType;
            public string Action;
            public string Data;

            public override string ToString()
            {
                return $"[{Timestamp:F2}] {Action}: {EventType} - {Data}";
            }
        }
    }

    /// <summary>
    /// MonoBehaviour component for controlling EventDebugger.
    /// </summary>
    public class EventDebuggerController : MonoBehaviour
    {
        [SerializeField] private bool _enableOnStart = true;
        [SerializeField] private int _maxLogEntries = 100;
        [SerializeField] private KeyCode _toggleKey = KeyCode.F12;

        private void Start()
        {
            EventDebugger.IsEnabled = _enableOnStart;
            EventDebugger.MaxLogEntries = _maxLogEntries;
        }

        private void Update()
        {
            if (Input.GetKeyDown(_toggleKey))
            {
                EventDebugger.IsEnabled = !EventDebugger.IsEnabled;
                Debug.Log($"[EventDebugger] {(EventDebugger.IsEnabled ? "ENABLED" : "DISABLED")}");
            }
        }
    }
}
