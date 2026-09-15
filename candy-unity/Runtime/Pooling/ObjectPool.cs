using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoshEngine.Candy.Unity
{
    /// <summary>
    /// Generic object pool for Unity components that implement IPoolable interface.
    /// </summary>
    /// <typeparam name="T">The type of component to pool, must implement IPoolable.</typeparam>
    public class ObjectPool<T>
        where T : Component, IPoolable
    {
        private readonly Queue<T> _pool = new(); // Queue to store inactive objects
        private readonly T _prefab; // Prefab to instantiate new objects
        private readonly Transform _parent; // Parent transform for instantiated objects

        /// <summary>
        /// Constructor for ObjectPool.
        /// </summary>
        /// <param name="prefab">Prefab to instantiate new objects.</param>
        /// <param name="parent">Parent transform for instantiated objects.</param>
        /// <param name="initialSize">Initial number of objects to create.</param>
        public ObjectPool(T prefab, Transform parent, int initialSize)
        {
            this._prefab = prefab ? prefab : throw new ArgumentNullException(nameof(prefab));
            this._parent = parent ? parent : throw new ArgumentNullException(nameof(parent));

            // Create initial objects
            for (var i = 0; i < initialSize; i++)
            {
                CreateNewObject(true);
            }
        }

        /// <summary>
        /// Creates a new object instance.
        /// </summary>
        /// <param name="queueIntoPool">If true, adds the new object to the pool queue.</param>
        /// <returns>The newly created object.</returns>
        private T CreateNewObject(bool queueIntoPool)
        {
            var newObj = UnityEngine.Object.Instantiate(_prefab, _parent);

            if (queueIntoPool)
            {
                newObj.gameObject.SetActive(false);
                _pool.Enqueue(newObj);
            }

            return newObj;
        }

        /// <summary>
        /// Spawns an object from the pool or creates a new one if the pool is empty.
        /// </summary>
        /// <returns>The spawned object.</returns>
        public T Spawn()
        {
            T obj = _pool.Count > 0 ? _pool.Dequeue() : CreateNewObject(false);
            obj.gameObject.SetActive(true);
            obj.OnSpawn(); // Call IPoolable.OnSpawn()
            return obj;
        }

        /// <summary>
        /// Despawns an object, returning it to the pool.
        /// </summary>
        /// <param name="obj">The object to despawn.</param>
        public void Despawn(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            obj.OnDespawn(); // Call IPoolable.OnDespawn()
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }

        /// <summary>
        /// Gets the number of objects currently available in the pool.
        /// </summary>
        public int AvailableCount => _pool.Count;
    }
}
