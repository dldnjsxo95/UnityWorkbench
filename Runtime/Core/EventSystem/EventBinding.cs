using System;
using UnityEngine;

namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// Helper class for automatic event subscription/unsubscription.
    /// Attach to MonoBehaviour for lifecycle management.
    /// </summary>
    public class EventBinding<T> : IDisposable where T : struct, IEvent
    {
        private readonly Action<T> _handler;
        private bool _isSubscribed;
        private bool _isDisposed;

        public EventBinding(Action<T> handler, bool autoSubscribe = true)
        {
            _handler = handler;
            if (autoSubscribe)
            {
                Subscribe();
            }
        }

        public void Subscribe()
        {
            if (_isSubscribed || _isDisposed) return;
            EventBus<T>.Subscribe(_handler);
            _isSubscribed = true;
        }

        public void Unsubscribe()
        {
            if (!_isSubscribed || _isDisposed) return;
            EventBus<T>.Unsubscribe(_handler);
            _isSubscribed = false;
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            Unsubscribe();
            _isDisposed = true;
        }
    }

    /// <summary>
    /// MonoBehaviour extension for easy event binding with automatic cleanup.
    /// </summary>
    public static class EventBindingExtensions
    {
        /// <summary>
        /// Subscribe to an event and automatically unsubscribe when GameObject is destroyed.
        /// </summary>
        public static EventBinding<T> BindEvent<T>(this MonoBehaviour mono, Action<T> handler)
            where T : struct, IEvent
        {
            var binding = new EventBinding<T>(handler);

            // Auto cleanup on destroy
            var cleanup = mono.gameObject.AddComponent<EventBindingCleanup>();
            cleanup.AddBinding(binding);

            return binding;
        }
    }

    /// <summary>
    /// Internal component for automatic cleanup.
    /// </summary>
    [AddComponentMenu("")] // Hide from menu
    internal class EventBindingCleanup : MonoBehaviour
    {
        private readonly System.Collections.Generic.List<IDisposable> _bindings =
            new System.Collections.Generic.List<IDisposable>();

        public void AddBinding(IDisposable binding)
        {
            _bindings.Add(binding);
        }

        private void OnDestroy()
        {
            foreach (var binding in _bindings)
            {
                binding?.Dispose();
            }
            _bindings.Clear();
        }
    }
}
