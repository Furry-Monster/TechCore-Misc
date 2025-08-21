using System;
using System.Collections.Generic;

namespace MonsterCache.Runtime
{
    public static partial class CacheMgr
    {
        private static readonly Dictionary<Type, Cache> caches = new();

        public static int Count => caches.Count;

        public static CacheInfo[] GetAllPoolInfos()
        {
            var index = 0;
            CacheInfo[] result;

            lock (caches)
            {
                result = new CacheInfo[caches.Count];
                foreach (var cacheItem in caches)
                {
                    result[index++] = new CacheInfo(cacheItem.Key, cacheItem.Value.UnusedLineCount,
                        cacheItem.Value.UsingLineCount, cacheItem.Value.AcquireLineCount,
                        cacheItem.Value.ReleaseLineCount, cacheItem.Value.AddLineCount,
                        cacheItem.Value.RemoveLineCount);
                }
            }

            return result;
        }

        public static void Clear()
        {
            lock (caches)
            {
                foreach (var cache in caches.Values)
                {
                    cache.Clear();
                }

                caches.Clear();
            }
        }

        public static T Acquire<T>() where T : class, IPoolable, new()
        {
            var cache = GetCache(typeof(T));
            return cache.Acquire<T>();
        }

        public static IPoolable Acquire(Type cachedType)
        {
            var cache = GetCache(cachedType);
            return cache.Acquire();
        }

        public static void Release(IPoolable poolable)
        {
            if (poolable == null)
                throw new ArgumentNullException(nameof(poolable));

            var cache = GetCache(poolable.GetType());
            cache.Release(poolable);
        }

        public static void Expand<T>(int count) where T : class, IPoolable, new()
        {
            Expand(typeof(T), count);
        }

        public static void Expand(Type cachedType, int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero");

            var cache = GetCache(cachedType);
            cache.Expand(count);
        }

        public static void Shrink<T>(int count) where T : class, IPoolable, new()
        {
            Shrink(typeof(T), count);
        }

        public static void Shrink(Type cachedType, int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero");

            var cache = GetCache(cachedType);
            cache.Shrink(count);
        }

        private static Cache GetCache(Type cachedType)
        {
            if (cachedType == null)
                throw new ArgumentNullException(nameof(cachedType));

            Cache cache;
            lock (caches)
            {
                if (!caches.TryGetValue(cachedType, out cache))
                {
                    cache = new Cache(cachedType);
                    caches.Add(cachedType, cache);
                }
            }

            return cache;
        }
    }
}