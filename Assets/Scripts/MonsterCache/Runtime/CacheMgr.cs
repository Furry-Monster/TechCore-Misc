using System;
using System.Collections.Generic;

namespace MonsterCache.Runtime
{
    public static class CacheMgr
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
                    // 清除每个缓存
                }

                caches.Clear();
            }
        }

        public static T Acquire<T>() where T : class, ICachedType, new()
        {
            throw new NotImplementedException();
        }

        public static ICachedType Acquire(Type cachedType)
        {
            throw new NotImplementedException();
        }

        public static void Release(ICachedType cachedType)
        {
            throw new NotImplementedException();
        }

        public static void Expand<T>(int count) where T : class, ICachedType, new()
        {
            throw new NotImplementedException();
        }

        public static void Expand(Type cachedType, int count)
        {
            throw new NotImplementedException();
        }

        public static void Shrink<T>(int count) where T : class, ICachedType, new()
        {
            throw new NotImplementedException();
        }

        public static void Shrink(Type cachedType, int count)
        {
            throw new NotImplementedException();
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