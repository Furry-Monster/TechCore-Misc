using System;
using System.Collections.Generic;

namespace MonsterCache.Runtime
{
    /// <summary>
    /// 单一类型的对象池实现
    /// </summary>
    public class ObjectPool
    {
        private readonly Queue<IPoolable> poolables;
        private readonly Type poolType;
        private int usedPoolableCount;
        private int acquirePoolableCount;
        private int releasePoolableCount;
        private int addPoolableCount;
        private int removePoolableCount;

        /// <summary>
        /// 初始化指定类型的对象池
        /// </summary>
        /// <param name="poolType">池化对象类型</param>
        public ObjectPool(Type poolType)
        {
            if (!typeof(IPoolable).IsAssignableFrom(poolType))
                throw new ArgumentException($"{poolType} is not an IPoolable");

            poolables = new Queue<IPoolable>();
            this.poolType = poolType;
            usedPoolableCount = 0;
            acquirePoolableCount = 0;
            releasePoolableCount = 0;
            addPoolableCount = 0;
            removePoolableCount = 0;
        }

        /// <summary>池化对象类型</summary>
        public Type PoolType => poolType;

        /// <summary>
        /// 当前池中空闲对象数量
        /// </summary>
        public int UnusedPoolableCount => poolables.Count;

        /// <summary>
        /// 当前正在使用的对象数量
        /// </summary>
        public int UsedPoolableCount => usedPoolableCount;

        /// <summary>
        /// 累计获取对象次数
        /// </summary>
        public int AcquirePoolableCount => acquirePoolableCount;

        /// <summary>
        /// 累计归还对象次数
        /// </summary>
        public int ReleasePoolableCount => releasePoolableCount;

        /// <summary>
        /// 累计创建新对象次数
        /// </summary>
        public int AddPoolableCount => addPoolableCount;

        /// <summary>
        /// 累计销毁对象次数
        /// </summary>
        public int RemovePoolableCount => removePoolableCount;

        /// <summary>
        /// 从对象池获取一个指定类型的对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <returns>对象实例（从池中获取或新创建）</returns>
        /// <exception cref="ArgumentException">类型不匹配</exception>
        public T Acquire<T>() where T : class, IPoolable, new()
        {
            if (typeof(T) != poolType)
                throw new ArgumentException($"Type {typeof(T)} does not match the {poolType} type");

            usedPoolableCount++;
            acquirePoolableCount++;

            lock (poolables)
            {
                // NOTE: 每次Acquire/Release都要加锁，高并发下可能存在性能问题
                // 但是暂时不进行优化，因为对于这个简单项目，我认为是必要的，后面的Release方法类同此处
                if (poolables.Count > 0)
                {
                    return (T)poolables.Dequeue();
                }
            }

            addPoolableCount++;
            return new T();
        }

        /// <summary>
        /// 从对象池获取一个对象
        /// </summary>
        /// <returns>对象实例（从池中获取或新创建）</returns>
        public IPoolable Acquire()
        {
            usedPoolableCount++;
            acquirePoolableCount++;

            lock (poolables)
            {
                if (poolables.Count > 0)
                {
                    return poolables.Dequeue();
                }
            }

            addPoolableCount++;
            return (IPoolable)Activator.CreateInstance(poolType);
            // NOTE: 这里直接使用反射创建较慢，或许可以用Factory来优化？
        }

        public void Release<T>(T poolable) where T : class, IPoolable
        {
            if (poolable == null)
                throw new ArgumentNullException(nameof(poolable));

            if (typeof(T) != poolType)
                throw new ArgumentException($"Type {typeof(T)} does not match the {poolType} type");

            poolable.OnReturnToPool();

            lock (poolables)
            {
                // NOTE: _.Contains此处是O(n) 复杂度，或许可以优化，应该是个大工程？
                if (poolables.Contains(poolable))
                {
                    throw new InvalidOperationException("Cache already released");
                }

                poolables.Enqueue(poolable);
            }

            releasePoolableCount++;
            usedPoolableCount--;
        }

        /// <summary>
        /// 将对象归还到对象池
        /// </summary>
        /// <param name="poolable">要归还的对象</param>
        /// <exception cref="ArgumentNullException">对象为空</exception>
        /// <exception cref="ArgumentException">对象类型不匹配</exception>
        /// <exception cref="InvalidOperationException">对象已经在池中</exception>
        public void Release(IPoolable poolable)
        {
            if (poolable == null)
                throw new ArgumentNullException(nameof(poolable));

            if (poolable.GetType() != poolType)
                throw new ArgumentException($"Type {poolable.GetType()} does not match the {poolType} type");

            poolable.OnReturnToPool();

            lock (poolables)
            {
                if (poolables.Contains(poolable))
                {
                    throw new InvalidOperationException("Cache already released");
                }

                poolables.Enqueue(poolable);
            }

            releasePoolableCount++;
            usedPoolableCount--;
        }


        /// <summary>
        /// 清空对象池中的所有空闲对象
        /// </summary>
        public void ReleaseAll()
        {
            lock (poolables)
            {
                var clearedCount = poolables.Count;
                poolables.Clear();
                removePoolableCount += clearedCount;
            }
        }

        /// <summary>
        /// 预创建指定数量的对象到池中
        /// </summary>
        /// <param name="count">预创建数量</param>
        /// <exception cref="ArgumentOutOfRangeException">数量必须为正数</exception>
        public void Expand(int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero");

            lock (poolables)
            {
                for (var i = 0; i < count; i++)
                {
                    var instance = (IPoolable)Activator.CreateInstance(poolType);
                    poolables.Enqueue(instance);
                    addPoolableCount++;
                }
            }
        }

        /// <summary>
        /// 从池中移除指定数量的空闲对象
        /// </summary>
        /// <param name="count">移除数量</param>
        /// <exception cref="ArgumentOutOfRangeException">数量必须为正数</exception>
        public void Shrink(int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero");

            lock (poolables)
            {
                var actualRemoveCount = Math.Min(count, poolables.Count);
                for (var i = 0; i < actualRemoveCount; i++)
                {
                    poolables.Dequeue();
                    removePoolableCount++;
                }
            }
        }
    }
}