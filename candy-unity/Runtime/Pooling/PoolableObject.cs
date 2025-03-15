using UnityEngine;

namespace CandyCore
{
    /// <summary>
    /// Base class for objects that can be managed by an ObjectPool.
    /// </summary>
    /// <remarks>
    /// This class implements the IPoolable interface and inherits from MonoBehaviour.
    /// </remarks>
    public class PoolableObject : MonoBehaviour, IPoolable
    {
        /// <summary>
        /// Called when the object is spawned from the pool.
        /// </summary>
        /// <remarks>
        /// Override this method in derived classes to initialize the object
        /// or reset its state when it's retrieved from the pool.
        /// </remarks>
        public virtual void OnSpawn()
        {
            // Default implementation is empty.
            // Derived classes should override this method to provide specific initialization logic.
        }

        /// <summary>
        /// Called when the object is despawned and returned to the pool.
        /// </summary>
        /// <remarks>
        /// Override this method in derived classes to reset the object's state
        /// or perform any necessary cleanup before it's returned to the pool.
        /// </remarks>
        public virtual void OnDespawn()
        {
            // Default implementation is empty.
            // Derived classes should override this method to provide specific cleanup logic.
        }
    }
}
