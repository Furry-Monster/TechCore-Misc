using System;
using System.Collections.Generic;

namespace MonsterCache.Runtime
{
    public static class CachePoolMgr
    {
        private static readonly Dictionary<Type, CachePool> cachePoolDict = new();

        /// <summary>
        /// 当前管理的对象池数量
        /// </summary>
        public static int Count
        {
            get
            {
                lock (cachePoolDict)
                {
                    return cachePoolDict.Count;
                }
            }
        }

        /// <summary>
        /// 获取所有对象池的统计信息
        /// </summary>
        /// <returns>对象池信息数组</returns>
        public static CachePoolInfo[] GetAllPoolInfos()
        {
            var index = 0;
            CachePoolInfo[] result;

            lock (cachePoolDict)
            {
                result = new CachePoolInfo[cachePoolDict.Count];
                foreach (var cacheItem in cachePoolDict)
                {
                    result[index++] = new CachePoolInfo(cacheItem.Key, cacheItem.Value.UnusedPoolableCount,
                        cacheItem.Value.UsedPoolableCount, cacheItem.Value.AcquirePoolableCount,
                        cacheItem.Value.ReleasePoolableCount, cacheItem.Value.AddPoolableCount,
                        cacheItem.Value.RemovePoolableCount);
                }
            }

            return result;
        }

        /// <summary>
        /// 清空所有对象池
        /// </summary>
        public static void Clear()
        {
            lock (cachePoolDict)
            {
                // 逐个清空对象池
                foreach (var cachePool in cachePoolDict.Values)
                {
                    cachePool.ReleaseAll();
                }

                cachePoolDict.Clear();
            }
        }

        /// <summary>
        /// 从对象池获取一个对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <returns>对象实例</returns>
        public static T Acquire<T>() where T : class, IPoolable, new()
        {
            var cachePool = GetCache(typeof(T));
            return cachePool.Acquire<T>();
        }

        /// <summary>
        /// 从对象池获取一个对象
        /// </summary>
        /// <param name="cachedType">对象类型</param>
        /// <returns>对象实例</returns>
        public static IPoolable Acquire(Type cachedType)
        {
            var cachePool = GetCache(cachedType);
            return cachePool.Acquire();
        }

        /// <summary>
        /// 将对象归还到对象池
        /// </summary>
        /// <param name="poolable">要归还的对象实例</param>
        /// <exception cref="ArgumentNullException">对象实例为空</exception>
        public static void Release(IPoolable poolable)
        {
            if (poolable == null)
                throw new ArgumentNullException(nameof(poolable));

            var cachePool = GetCache(poolable.GetType());
            cachePool.Release(poolable);
        }

        /// <summary>
        /// 预创建指定数量的对象到对象池
        /// </summary>
        /// <param name="count">预创建数量</param>
        /// <typeparam name="T">对象类型</typeparam>
        public static void Expand<T>(int count) where T : class, IPoolable, new()
        {
            Expand(typeof(T), count);
        }

        /// <summary>
        /// 预创建指定数量的对象到对象池
        /// </summary>
        /// <param name="cachedType">对象类型</param>
        /// <param name="count">预创建数量</param>
        /// <exception cref="ArgumentOutOfRangeException">数量必须为正数</exception>
        public static void Expand(Type cachedType, int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero");

            var cachePool = GetCache(cachedType);
            cachePool.Expand(count);
        }

        /// <summary>
        /// 从对象池移除指定数量的空闲对象
        /// </summary>
        /// <param name="count">移除数量</param>
        /// <typeparam name="T">对象类型</typeparam>
        public static void Shrink<T>(int count) where T : class, IPoolable, new()
        {
            Shrink(typeof(T), count);
        }

        /// <summary>
        /// 从对象池移除指定数量的空闲对象
        /// </summary>
        /// <param name="cachedType">对象类型</param>
        /// <param name="count">移除数量</param>
        /// <exception cref="ArgumentOutOfRangeException">数量必须为正数</exception>
        public static void Shrink(Type cachedType, int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero");

            var cachePool = GetCache(cachedType);
            cachePool.Shrink(count);
        }

        /// <summary>
        /// 获取指定类型的对象池
        /// </summary>
        /// <param name="cachedType">对象类型</param>
        /// <returns>对象池实例</returns>
        /// <exception cref="ArgumentNullException">对象类型不能为空</exception>
        private static CachePool GetCache(Type cachedType)
        {
            if (cachedType == null)
                throw new ArgumentNullException(nameof(cachedType));

            CachePool cachePool;
            lock (cachePoolDict)
            {
                if (!cachePoolDict.TryGetValue(cachedType, out cachePool))
                {
                    cachePool = new CachePool(cachedType);
                    cachePoolDict.Add(cachedType, cachePool);
                }
            }

            return cachePool;
        }
    }
}