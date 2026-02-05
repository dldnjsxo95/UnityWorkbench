using System.Threading.Tasks;

namespace LWT.UnityWorkbench.Data
{
    /// <summary>
    /// Interface for save system implementations.
    /// Allows swapping between local, cloud, or custom storage.
    /// </summary>
    public interface ISaveSystem
    {
        /// <summary>
        /// Save data to storage.
        /// </summary>
        void Save<T>(string key, T data) where T : class;

        /// <summary>
        /// Load data from storage.
        /// </summary>
        T Load<T>(string key) where T : class;

        /// <summary>
        /// Check if data exists.
        /// </summary>
        bool Exists(string key);

        /// <summary>
        /// Delete saved data.
        /// </summary>
        void Delete(string key);

        /// <summary>
        /// Delete all saved data.
        /// </summary>
        void DeleteAll();

        /// <summary>
        /// Async save for large data.
        /// </summary>
        Task SaveAsync<T>(string key, T data) where T : class;

        /// <summary>
        /// Async load for large data.
        /// </summary>
        Task<T> LoadAsync<T>(string key) where T : class;
    }
}
