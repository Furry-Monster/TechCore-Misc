using System;
using System.Collections.Generic;

namespace MonsterCache.Runtime
{
    public static partial class CacheMgr
    {
        public class Cache
        {
            private readonly Queue<IPoolable> poolables;
            private readonly Type poolType;
            private int usingLineCount;
            private int acquireLineCount;
            private int releaseLineCount;
            private int addLineCount;
            private int removeLineCount;

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

            public Type PoolType => poolType;
            public int UnusedLineCount => poolables.Count;
            public int UsingLineCount => usingLineCount;
            public int AcquireLineCount => acquireLineCount;
            public int ReleaseLineCount => releaseLineCount;
            public int AddLineCount => addLineCount;
            public int RemoveLineCount => removeLineCount;

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

            public void Release(IPoolable poolable)
            {
                if (poolable == null)
                    throw new ArgumentNullException(nameof(poolable));

                if (poolable.GetType() != this.poolType)
                    throw new ArgumentException(
                        $"Type {poolable.GetType()} does not match the {this.poolType} type");

                poolable.Release();
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

            public void Clear()
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
}