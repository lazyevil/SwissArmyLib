﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Archon.SwissArmyLib.Pooling
{
    /// <summary>
    /// Simple static helper class for pooling Unity prefab instances.
    /// 
    /// If the pooled objects implement <see cref="IPoolable"/> they will be notified when they're spawned and despawned.
    /// 
    /// For non-Unity objects see <see cref="PoolHelper{T}"/>.
    /// </summary>
    public static class PoolHelper
    {
        private static readonly Dictionary<Object, GameObjectPool<Object>> Pools = new Dictionary<Object, GameObjectPool<Object>>();
        private static readonly Dictionary<Object, Object> Prefabs = new Dictionary<Object, Object>();

        /// <summary>
        /// Spawns a recycled object if there's one available, otherwise creates a new instance.
        /// </summary>
        /// <returns>The spawned object.</returns>
        public static T Spawn<T>(T prefab)
            where T : Object
        {
            var pool = GetPool(prefab);
            var obj = pool.Spawn();
            Prefabs[obj] = prefab;

            return obj as T;
        }

        /// <summary>
        /// Spawns a recycled object if there's one available, otherwise creates a new instance.
        /// </summary>
        /// <returns>The spawned object.</returns>
        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent)
            where T : Object
        {
            var pool = GetPool(prefab);
            var obj = pool.Spawn(position, rotation, parent);
            Prefabs[obj] = prefab;

            return obj as T;
        }

        /// <summary>
        /// Despawns an instance and marks it for reuse.
        /// </summary>
        /// <param name="target">The instance to despawn.</param>
        public static void Despawn(IPoolable target)
        {
            var unityObject = target as Object;

            if (unityObject == null)
                throw new InvalidOperationException("Cannot despawn target because it is not a UnityEngine.Object!");

            var prefab = GetPrefab(unityObject);
            var pool = GetPool(prefab);
            pool.Despawn(unityObject);
        }

        /// <summary>
        /// Gets the prefab that was used to spawn <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">The instance to get the prefab for.</param>
        /// <returns>The prefab for the instance, or null if not found.</returns>
        public static Object GetPrefab(Object instance)
        {
            Object prefab;
            Prefabs.TryGetValue(instance, out prefab);
            return prefab;
        }

        /// <summary>
        /// Gets the prefab that was used to spawn <paramref name="instance"/>.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="instance">The instance to get the prefab for.</param>
        /// <returns>The prefab for the instance, or null if not found.</returns>
        public static T GetPrefab<T>(T instance)
            where T : Object
        {
            Object prefab;
            Prefabs.TryGetValue(instance, out prefab);
            return prefab as T;
        }

        /// <summary>
        /// Gets or creates the pool for the given prefab.
        /// </summary>
        /// <param name="prefab">The prefab to get a pool for.</param>
        /// <returns>The pool for the prefab.</returns>
        public static GameObjectPool<Object> GetPool(Object prefab)
        {
            GameObjectPool<Object> pool;
            Pools.TryGetValue(prefab, out pool);

            if (pool == null)
            {
                pool = new GameObjectPool<Object>(prefab);
                Pools[prefab] = pool;
            }

            return pool;
        }
    }

    /// <summary>
    /// Simple static helper class for pooling non-Unity objects.
    /// 
    /// If the pooled objects implement <see cref="IPoolable"/> they will be notified when they're spawned and despawned.
    /// 
    /// For Unity GameObjects see <see cref="PoolHelper"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to pool.</typeparam>
    public static class PoolHelper<T> where T : class, new()
    {
        private static readonly Pool<T> Pool = new Pool<T>(() => new T());

        /// <summary>
        /// Spawns a recycled or new instance of the type <typeparamref name="T"/>.
        /// </summary>
        /// <returns>The spawned instance.</returns>
        public static T Spawn()
        {
            return Pool.Spawn();
        }

        /// <summary>
        /// Despawns an instance of the type <typeparamref name="T"/> and marks it for reuse.
        /// </summary>
        /// <param name="target">The instance to despawn.</param>
        public static void Despawn(T target)
        {
            Pool.Despawn(target);
        }
    }
}