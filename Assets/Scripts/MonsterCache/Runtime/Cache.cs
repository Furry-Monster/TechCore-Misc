using System;
using System.Collections.Generic;

namespace MonsterCache.Runtime
{
    /// <summary>
    /// 单一类型的对象池实现
    /// </summary>
    public class Cache
    {
        private readonly Queue<IPoolable> poolables;
        private readonly Type poolType;
        private int usingLineCount;
        private int acquireLineCount;
        private int releaseLineCount;
        private int addLineCount;
        private int removeLineCount;

        /// <summary>
        /// 初始化指定类型的对象池
        /// </summary>
        /// <param name="poolType">池化对象类型</param>
        public Cache(Type poolType)
        {
            poolables = new Queue<IPoolable>();
            this.poolType = poolType;
            usingLineCount = 0;
            acquireLineCount = 0;
            releaseLineCount = 0;
            addLineCount = 0;
            removeLineCount = 0;
        }

        /// <summary>池化对象类型</summary>
        public Type PoolType => poolType;

        /// <summary>池中空闲对象数量</summary>
        public int UnusedLineCount => poolables.Count;

        /// <summary>当前正在使用的对象数量</summary>
        public int UsingLineCount => usingLineCount;

        /// <summary>累计获取对象次数</summary>
        public int AcquireLineCount => acquireLineCount;

        /// <summary>累计归还对象次数</summary>
        public int ReleaseLineCount => releaseLineCount;

        /// <summary>累计创建新对象次数</summary>
        public int AddLineCount => addLineCount;

        /// <summary>累计销毁对象次数</summary>
        public int RemoveLineCount => removeLineCount;

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

            usingLineCount++;
            acquireLineCount++;

            lock (poolables)
            {
                if (poolables.Count > 0)
                {
                    return (T)poolables.Dequeue();
                }
            }

            addLineCount++;
            return new T();
        }

        /// <summary>
        /// 从对象池获取一个对象
        /// </summary>
        /// <returns>对象实例（从池中获取或新创建）</returns>
        public IPoolable Acquire()
        {
            usingLineCount++;
            acquireLineCount++;
            lock (poolables)
            {
                if (poolables.Count > 0)
                {
                    return poolables.Dequeue();
                }
            }

            addLineCount++;
            return (IPoolable)Activator.CreateInstance(poolType);
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

            if (poolable.GetType() != this.poolType)
                throw new ArgumentException(
                    $"Type {poolable.GetType()} does not match the {this.poolType} type");

            poolable.OnReturnToPool();
            lock (poolables)
            {
                if (poolables.Contains(poolable))
                {
                    throw new InvalidOperationException("Cache already released");
                }

                poolables.Enqueue(poolable);
            }

            releaseLineCount++;
            usingLineCount--;
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
                for (int i = 0; i < count; i++)
                {
                    var instance = (IPoolable)Activator.CreateInstance(poolType);
                    poolables.Enqueue(instance);
                    addLineCount++;
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
                for (int i = 0; i < actualRemoveCount; i++)
                {
                    poolables.Dequeue();
                    removeLineCount++;
                }
            }
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
                removeLineCount += clearedCount;
            }
        }
    }
}