namespace CandyCore
{
    /// <summary>
    /// Interface for poolable objects. Objects implementing this interface
    /// can be managed by a pooling system, allowing them to be reused
    /// instead of instantiated and destroyed repeatedly.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Method called when the object is spawned from the pool.
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// Method called when the object is despawned and returned to the pool.
        /// </summary>
        void OnDespawn();
    }
}
