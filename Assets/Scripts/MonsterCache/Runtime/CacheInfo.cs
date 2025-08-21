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
        /// 初始化缓存信息的新实例。
        /// </summary>
        /// <param name="poolType">缓存类型。</param>
        /// <param name="unusedLineCount">未使用缓存数量。</param>
        /// <param name="usingLineCount">正在使用缓存数量。</param>
        /// <param name="acquireLineCount">获取缓存数量。</param>
        /// <param name="releaseLineCount">归还缓存数量。</param>
        /// <param name="addLineCount">增加缓存数量。</param>
        /// <param name="removeLineCount">移除缓存数量。</param>
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