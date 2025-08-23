using System;
using System.Runtime.InteropServices;

// ReSharper disable ConvertToAutoProperty

namespace MonsterCache.Runtime
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ObjectPoolInfo
    {
        private readonly Type poolType;
        private readonly int unusedPoolableCount;
        private readonly int usedPoolableCount;
        private readonly int acquirePoolableCount;
        private readonly int releasePoolableCount;
        private readonly int addPoolableCount;
        private readonly int removePoolableCount;

        /// <summary>
        /// 初始化对象池信息。
        /// </summary>
        /// <param name="poolType">池化对象类型。</param>
        /// <param name="unusedPoolableCount">池中空闲对象数量。</param>
        /// <param name="usedPoolableCount">当前正在使用的对象数量。</param>
        /// <param name="acquirePoolableCount">累计获取对象次数。</param>
        /// <param name="releasePoolableCount">累计归还对象次数。</param>
        /// <param name="addPoolableCount">累计创建新对象次数。</param>
        /// <param name="removePoolableCount">累计销毁对象次数。</param>
        public ObjectPoolInfo(Type poolType, int unusedPoolableCount, int usedPoolableCount, int acquirePoolableCount,
            int releasePoolableCount, int addPoolableCount, int removePoolableCount)
        {
            this.poolType = poolType;
            this.unusedPoolableCount = unusedPoolableCount;
            this.usedPoolableCount = usedPoolableCount;
            this.acquirePoolableCount = acquirePoolableCount;
            this.releasePoolableCount = releasePoolableCount;
            this.addPoolableCount = addPoolableCount;
            this.removePoolableCount = removePoolableCount;
        }

        public Type PoolType => poolType;
        public int UnusedPoolableCount => unusedPoolableCount;
        public int UsedPoolableCount => usedPoolableCount;
        public int AcquirePoolableCount => acquirePoolableCount;
        public int ReleasePoolableCount => releasePoolableCount;
        public int AddPoolableCount => addPoolableCount;
        public int RemovePoolableCount => removePoolableCount;
    }
}