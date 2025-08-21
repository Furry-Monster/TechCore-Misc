using System;
using MonsterCache.Runtime;

namespace MonsterCache.Examples
{
    /// <summary>
    /// 示例游戏对象 - 子弹
    /// </summary>
    public class Bullet : IPoolable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public float Damage { get; set; }
        public bool IsActive { get; private set; } = true;

        public void Initialize(float x, float y, float vx, float vy, float damage)
        {
            X = x;
            Y = y;
            VelocityX = vx;
            VelocityY = vy;
            Damage = damage;
            IsActive = true;
        }

        public void Update(float deltaTime)
        {
            if (!IsActive) return;

            X += VelocityX * deltaTime;
            Y += VelocityY * deltaTime;

            // 模拟边界检查，超出范围就设为非活跃
            if (X < -100 || X > 1000 || Y < -100 || Y > 1000)
            {
                IsActive = false;
            }
        }

        public void OnReturnToPool()
        {
            // 重置状态
            X = 0;
            Y = 0;
            VelocityX = 0;
            VelocityY = 0;
            Damage = 0;
            IsActive = false;
            Console.WriteLine($"Bullet returned to pool");
        }
    }

    /// <summary>
    /// 示例游戏对象 - 粒子效果
    /// </summary>
    public class ParticleEffect : IPoolable
    {
        public string EffectName { get; set; }
        public float Duration { get; set; }
        public float RemainingTime { get; set; }
        public bool IsPlaying { get; private set; }

        public void Play(string effectName, float duration)
        {
            EffectName = effectName;
            Duration = duration;
            RemainingTime = duration;
            IsPlaying = true;
        }

        public void Update(float deltaTime)
        {
            if (!IsPlaying) return;

            RemainingTime -= deltaTime;
            if (RemainingTime <= 0)
            {
                IsPlaying = false;
            }
        }

        public void OnReturnToPool()
        {
            EffectName = null;
            Duration = 0;
            RemainingTime = 0;
            IsPlaying = false;
            Console.WriteLine($"ParticleEffect returned to pool");
        }
    }

    /// <summary>
    /// 示例游戏对象 - 临时消息
    /// </summary>
    public class GameMessage : IPoolable
    {
        public string Content { get; set; }
        public DateTime CreatedTime { get; set; }
        public int Priority { get; set; }

        public void SetMessage(string content, int priority = 0)
        {
            Content = content;
            Priority = priority;
            CreatedTime = DateTime.Now;
        }

        public bool IsExpired(TimeSpan maxAge)
        {
            return DateTime.Now - CreatedTime > maxAge;
        }

        public void OnReturnToPool()
        {
            Content = null;
            Priority = 0;
            CreatedTime = default;
            Console.WriteLine($"GameMessage returned to pool");
        }
    }

    /// <summary>
    /// 示例重对象 - 模拟创建成本高的对象
    /// </summary>
    public class HeavyObject : IPoolable
    {
        // ReSharper disable once CollectionNeverQueried.Global
        public byte[] LargeData { get; private set; }
        public string ProcessedResult { get; set; }

        public HeavyObject()
        {
            // 模拟创建成本高的操作
            LargeData = new byte[1024 * 10]; // 10KB数据
            Console.WriteLine("HeavyObject created (expensive operation!)");
        }

        public void ProcessData(string input)
        {
            // 模拟复杂处理
            ProcessedResult = $"Processed: {input.ToUpper()} at {DateTime.Now:HH:mm:ss}";

            // 填充一些数据
            for (int i = 0; i < LargeData.Length; i++)
            {
                LargeData[i] = (byte)(i % 256);
            }
        }

        public void OnReturnToPool()
        {
            ProcessedResult = null;
            // 注意：我们不清空 LargeData，因为它可以重用
            Console.WriteLine("HeavyObject returned to pool (keeping allocated memory)");
        }
    }
}