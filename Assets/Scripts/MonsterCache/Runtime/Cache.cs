using System;
using System.Collections.Generic;

namespace MonsterCache.Runtime
{
    public class Cache
    {
        private readonly Queue<ICachedType> cacheLines;
        private readonly Type cachedType;
        private int usingLineCount;
        private int acquireLineCount;
        private int releaseLineCount;
        private int addLineCount;
        private int removeLineCount;

        public Cache(Type cachedType)
        {
            cacheLines = new Queue<ICachedType>();
            this.cachedType = cachedType;
            usingLineCount = 0;
            acquireLineCount = 0;
            releaseLineCount = 0;
            addLineCount = 0;
            removeLineCount = 0;
        }

        public Type CachedType => cachedType;
        public int UnusedLineCount => cacheLines.Count;
        public int UsingLineCount => usingLineCount;
        public int AcquireLineCount => acquireLineCount;
        public int ReleaseLineCount => releaseLineCount;
        public int AddLineCount => addLineCount;
        public int RemoveLineCount => removeLineCount;
    }
}