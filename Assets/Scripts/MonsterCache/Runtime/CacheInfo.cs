using System;
using System.Runtime.InteropServices;

// ReSharper disable ConvertToAutoProperty

namespace MonsterCache.Runtime
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct CacheInfo
    {
        private readonly Type poolType;
        private readonly int unusedLineCount;
        private readonly int usingLineCount;
        private readonly int acquireLineCount;
        private readonly int releaseLineCount;
        private readonly int addLineCount;
        private readonly int removeLineCount;

        /// <summary>
        /// 初始化对象池信息。
        /// </summary>
        /// <param name="poolType">池化对象类型。</param>
        /// <param name="unusedLineCount">池中空闲对象数量。</param>
        /// <param name="usingLineCount">当前正在使用的对象数量。</param>
        /// <param name="acquireLineCount">累计获取对象次数。</param>
        /// <param name="releaseLineCount">累计归还对象次数。</param>
        /// <param name="addLineCount">累计创建新对象次数。</param>
        /// <param name="removeLineCount">累计销毁对象次数。</param>
        public CacheInfo(Type poolType, int unusedLineCount, int usingLineCount, int acquireLineCount,
            int releaseLineCount, int addLineCount, int removeLineCount)
        {
            this.poolType = poolType;
            this.unusedLineCount = unusedLineCount;
            this.usingLineCount = usingLineCount;
            this.acquireLineCount = acquireLineCount;
            this.releaseLineCount = releaseLineCount;
            this.addLineCount = addLineCount;
            this.removeLineCount = removeLineCount;
        }

        public Type PoolType => poolType;
        public int UnusedLineCount => unusedLineCount;
        public int UsingLineCount => usingLineCount;
        public int AcquireLineCount => acquireLineCount;
        public int ReleaseLineCount => releaseLineCount;
        public int AddLineCount => addLineCount;
        public int RemoveLineCount => removeLineCount;
    }
}