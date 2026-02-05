namespace LWT.UnityWorkbench.Core
{
    /// <summary>
    /// Interface for poolable objects.
    /// Implement this to receive pool lifecycle callbacks.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Called when object is retrieved from pool.
        /// Use for initialization/reset.
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// Called when object is returned to pool.
        /// Use for cleanup.
        /// </summary>
        void OnDespawn();
    }
}
