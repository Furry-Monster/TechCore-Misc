namespace MonsterCache.Runtime
{
    /// <summary>
    /// 可池化类型接口
    /// <remarks>内置的对象池将代替Mono内置的GC/IDisposable托管内存</remarks>
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// 对象释放回调
        /// </summary>
        void OnReturnToPool();
    }
}