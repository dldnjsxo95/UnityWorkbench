using System;

namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// Generic Singleton for plain C# classes (non-MonoBehaviour).
    /// Thread-safe lazy initialization.
    /// </summary>
    /// <typeparam name="T">Type of the singleton class</typeparam>
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        private static readonly Lazy<T> _instance = new Lazy<T>(() => new T());

        public static T Instance => _instance.Value;

        public static bool HasInstance => _instance.IsValueCreated;

        protected Singleton() { }
    }
}
