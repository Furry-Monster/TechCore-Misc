using MonsterCache.Runtime;
using UnityEngine;

namespace MonsterCache.Examples
{
    public class TestPoolableObject : IPoolable
    {
        public string Name { get; private set; }
        public float CreatedTime { get; private set; }

        public void Initialize(string name)
        {
            Name = name;
            CreatedTime = Time.realtimeSinceStartup;
        }

        public void OnReturnToPool()
        {
            Name = null;
            CreatedTime = 0f;
        }
    }
}