namespace MonsterCache.Runtime
{
    public interface IPoolable
    {
        void OnReturnToPool();
    }
}