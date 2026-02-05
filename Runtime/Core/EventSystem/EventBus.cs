using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// Generic Event Bus for type-safe publish/subscribe pattern.
    /// Decouples event publishers from subscribers.
    /// </summary>
    public static class EventBus<T> where T : struct, IEvent
    {
        private static readonly HashSet<Action<T>> _subscribers = new HashSet<Action<T>>();

        /// <summary>
        /// Subscribe to this event type.
        /// </summary>
        public static void Subscribe(Action<T> handler)
        {
            if (handler == null) return;
            _subscribers.Add(handler);
        }

        /// <summary>
        /// Unsubscribe from this event type.
        /// </summary>
        public static void Unsubscribe(Action<T> handler)
        {
            if (handler == null) return;
            _subscribers.Remove(handler);
        }

        /// <summary>
        /// Publish an event to all subscribers.
        /// </summary>
        public static void Publish(T eventData)
        {
            foreach (var subscriber in _subscribers)
            {
                try
                {
                    subscriber?.Invoke(eventData);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        /// <summary>
        /// Clear all subscribers. Use with caution.
        /// </summary>
        public static void Clear()
        {
            _subscribers.Clear();
        }

        /// <summary>
        /// Get current subscriber count.
        /// </summary>
        public static int SubscriberCount => _subscribers.Count;
    }

    /// <summary>
    /// Marker interface for events.
    /// All event structs should implement this.
    /// </summary>
    public interface IEvent { }
}
