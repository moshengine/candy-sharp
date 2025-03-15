using UnityEngine;
using Zenject;

namespace CandyCore
{
    /// <summary>
    /// Factory class for creating ObjectPool instances using Zenject dependency injection framework.
    /// </summary>
    /// <remarks>
    /// This factory extends PlaceholderFactory to create ObjectPool instances for PoolableObjects.
    /// It's designed to work with the Zenject framework for dependency injection in Unity.
    /// </remarks>
    public class ObjectPoolFactory : PlaceholderFactory<GameObject, Transform, int, ObjectPool<PoolableObject>>
    {
        /// <summary>
        /// Creates a new ObjectPool instance for PoolableObjects.
        /// </summary>
        /// <param name="prefab">The GameObject prefab containing a PoolableObject component.</param>
        /// <param name="parent">The Transform that will be the parent of instantiated objects.</param>
        /// <param name="initialSize">The initial size of the object pool.</param>
        /// <returns>A new instance of ObjectPool<PoolableObject>.</returns>
        public override ObjectPool<PoolableObject> Create(GameObject prefab, Transform parent, int initialSize)
        {
            // Get the PoolableObject component from the prefab and use it to create a new ObjectPool
            return new ObjectPool<PoolableObject>(prefab.GetComponent<PoolableObject>(), parent, initialSize);
        }
    }
}
