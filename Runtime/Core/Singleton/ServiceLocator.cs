using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// Service Locator pattern for dependency management.
    /// Allows registering and retrieving services by interface type.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private static readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();

        /// <summary>
        /// Register a service instance.
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Service {type.Name} is already registered. Overwriting.");
            }
            _services[type] = service;
        }

        /// <summary>
        /// Register a factory function for lazy instantiation.
        /// </summary>
        public static void RegisterFactory<T>(Func<T> factory) where T : class
        {
            var type = typeof(T);
            _factories[type] = () => factory();
        }

        /// <summary>
        /// Get a registered service. Throws if not found.
        /// </summary>
        public static T Get<T>() where T : class
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var service))
            {
                return (T)service;
            }

            if (_factories.TryGetValue(type, out var factory))
            {
                var instance = (T)factory();
                _services[type] = instance;
                _factories.Remove(type);
                return instance;
            }

            throw new InvalidOperationException($"[ServiceLocator] Service {type.Name} is not registered.");
        }

        /// <summary>
        /// Try to get a service. Returns null if not found.
        /// </summary>
        public static T GetOrNull<T>() where T : class
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var service))
            {
                return (T)service;
            }

            if (_factories.TryGetValue(type, out var factory))
            {
                var instance = (T)factory();
                _services[type] = instance;
                _factories.Remove(type);
                return instance;
            }

            return null;
        }

        /// <summary>
        /// Check if a service is registered.
        /// </summary>
        public static bool IsRegistered<T>() where T : class
        {
            var type = typeof(T);
            return _services.ContainsKey(type) || _factories.ContainsKey(type);
        }

        /// <summary>
        /// Unregister a service.
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            var type = typeof(T);
            _services.Remove(type);
            _factories.Remove(type);
        }

        /// <summary>
        /// Clear all registered services.
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
            _factories.Clear();
        }

        /// <summary>
        /// Get count of registered services.
        /// </summary>
        public static int ServiceCount => _services.Count + _factories.Count;
    }
}
