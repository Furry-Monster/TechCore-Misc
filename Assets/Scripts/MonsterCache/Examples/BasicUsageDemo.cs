using System;
using System.Collections.Generic;
using MonsterCache.Runtime;

namespace MonsterCache.Examples
{
    /// <summary>
    /// 基本使用演示
    /// </summary>
    public static class BasicUsageDemo
    {
        /// <summary>
        /// 演示基本的对象池操作
        /// </summary>
        public static void DemoBasicOperations()
        {
            Console.WriteLine("=== 基本对象池操作演示 ===\n");

            // 1. 基本获取和归还
            Console.WriteLine("1. 基本获取和归还操作:");

            // 获取子弹对象
            var bullet1 = CachePoolMgr.Acquire<Bullet>();
            bullet1.Initialize(100, 200, 10, 5, 25.0f);
            Console.WriteLine($"获取子弹: 位置({bullet1.X}, {bullet1.Y}), 伤害:{bullet1.Damage}");

            // 归还子弹
            CachePoolMgr.Release(bullet1);
            Console.WriteLine("子弹已归还到池中");

            // 再次获取，应该是同一个对象（已被重置）
            var bullet2 = CachePoolMgr.Acquire<Bullet>();
            Console.WriteLine($"再次获取子弹: 位置({bullet2.X}, {bullet2.Y}) - 应该是重置状态");
            CachePoolMgr.Release(bullet2);

            Console.WriteLine();

            // 2. 批量操作演示
            Console.WriteLine("2. 批量对象操作:");

            var bullets = new List<Bullet>();

            // 创建10个子弹
            for (int i = 0; i < 10; i++)
            {
                var bullet = CachePoolMgr.Acquire<Bullet>();
                bullet.Initialize(i * 10, i * 20, 5, 2, 10.0f);
                bullets.Add(bullet);
            }

            Console.WriteLine($"创建了 {bullets.Count} 个子弹");

            // 模拟游戏循环
            for (int frame = 0; frame < 5; frame++)
            {
                Console.WriteLine($"第 {frame + 1} 帧:");
                for (int i = bullets.Count - 1; i >= 0; i--)
                {
                    bullets[i].Update(0.016f); // 60FPS

                    if (!bullets[i].IsActive)
                    {
                        Console.WriteLine($"  子弹 {i} 超出边界，归还到池中");
                        CachePoolMgr.Release(bullets[i]);
                        bullets.RemoveAt(i);
                    }
                }
            }

            // 清理剩余子弹
            foreach (var bullet in bullets)
            {
                CachePoolMgr.Release(bullet);
            }

            Console.WriteLine("剩余子弹全部归还");

            Console.WriteLine();

            // 3. 不同类型对象演示
            Console.WriteLine("3. 多种对象类型演示:");

            var effect = CachePoolMgr.Acquire<ParticleEffect>();
            effect.Play("Explosion", 2.0f);
            Console.WriteLine($"播放特效: {effect.EffectName}, 持续时间: {effect.Duration}秒");

            var message = CachePoolMgr.Acquire<GameMessage>();
            message.SetMessage("玩家获得金币!", 1);
            Console.WriteLine($"创建消息: {message.Content}, 优先级: {message.Priority}");

            // 归还对象
            CachePoolMgr.Release(effect);
            CachePoolMgr.Release(message);
            Console.WriteLine("特效和消息已归还");

            Console.WriteLine();
        }

        /// <summary>
        /// 演示预分配和优化
        /// </summary>
        public static void DemoPreAllocation()
        {
            Console.WriteLine("=== 预分配和优化演示 ===\n");

            // 1. 预分配对象
            Console.WriteLine("1. 预分配演示:");
            Console.WriteLine("预分配20个Bullet对象...");
            CachePoolMgr.Expand<Bullet>(20);

            Console.WriteLine("预分配10个HeavyObject对象...");
            CachePoolMgr.Expand<HeavyObject>(10);

            var poolInfos = CachePoolMgr.GetAllPoolInfos();
            foreach (var info in poolInfos)
            {
                Console.WriteLine($"{info.PoolType.Name}: 空闲={info.UnusedPoolableCount}, 使用={info.UsedPoolableCount}");
            }

            Console.WriteLine();

            // 2. 性能对比演示
            Console.WriteLine("2. 性能对比演示:");

            // 不使用对象池的情况（仅作演示）
            var start = DateTime.Now;
            for (int i = 0; i < 1000; i++)
            {
                var obj = new HeavyObject(); // 直接创建
                obj.ProcessData($"Data {i}");
                // 直接丢弃，让GC处理
            }

            var timeWithoutPool = DateTime.Now - start;
            Console.WriteLine($"不使用对象池创建1000个HeavyObject: {timeWithoutPool.TotalMilliseconds:F2}ms");

            // 使用对象池
            start = DateTime.Now;
            var heavyObjects = new List<HeavyObject>();
            for (int i = 0; i < 1000; i++)
            {
                var obj = CachePoolMgr.Acquire<HeavyObject>();
                obj.ProcessData($"Data {i}");
                heavyObjects.Add(obj);
            }

            // 归还对象
            foreach (var obj in heavyObjects)
            {
                CachePoolMgr.Release(obj);
            }

            var timeWithPool = DateTime.Now - start;
            Console.WriteLine($"使用对象池处理1000个HeavyObject: {timeWithPool.TotalMilliseconds:F2}ms");
            Console.WriteLine(
                $"性能提升: {((timeWithoutPool.TotalMilliseconds - timeWithPool.TotalMilliseconds) / timeWithoutPool.TotalMilliseconds * 100):F1}%");

            Console.WriteLine();
        }

        /// <summary>
        /// 演示错误处理
        /// </summary>
        public static void DemoErrorHandling()
        {
            Console.WriteLine("=== 错误处理演示 ===\n");

            try
            {
                // 1. 重复归还同一对象
                Console.WriteLine("1. 测试重复归还对象:");
                var bullet = CachePoolMgr.Acquire<Bullet>();
                CachePoolMgr.Release(bullet);
                Console.WriteLine("第一次归还成功");

                try
                {
                    CachePoolMgr.Release(bullet); // 这应该抛出异常
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"捕获到预期异常: {ex.Message}");
                }

                // 2. 归还null对象
                Console.WriteLine("\n2. 测试归还null对象:");
                try
                {
                    CachePoolMgr.Release(null);
                }
                catch (ArgumentNullException ex)
                {
                    Console.WriteLine($"捕获到预期异常: {ex.GetType().Name}");
                }

                // 3. 无效的扩容参数
                Console.WriteLine("\n3. 测试无效扩容参数:");
                try
                {
                    CachePoolMgr.Expand<Bullet>(-5);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Console.WriteLine($"捕获到预期异常: {ex.GetType().Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"未预期的异常: {ex}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// 演示对象生命周期管理
        /// </summary>
        public static void DemoObjectLifecycle()
        {
            Console.WriteLine("=== 对象生命周期管理演示 ===\n");

            // 1. 消息对象的生命周期管理
            Console.WriteLine("1. 消息系统演示:");
            var activeMessages = new List<GameMessage>();

            // 创建一些消息
            for (int i = 0; i < 5; i++)
            {
                var message = CachePoolMgr.Acquire<GameMessage>();
                message.SetMessage($"消息 {i + 1}", i);
                activeMessages.Add(message);
                Console.WriteLine($"创建消息: {message.Content}");

                // 模拟一些消息创建时间的差异
                if (i < 2)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }

            Console.WriteLine("\n等待消息过期...");
            System.Threading.Thread.Sleep(200);

            // 检查并清理过期消息
            var maxAge = TimeSpan.FromMilliseconds(150);
            for (int i = activeMessages.Count - 1; i >= 0; i--)
            {
                if (activeMessages[i].IsExpired(maxAge))
                {
                    Console.WriteLine($"消息过期，归还: {activeMessages[i].Content}");
                    CachePoolMgr.Release(activeMessages[i]);
                    activeMessages.RemoveAt(i);
                }
            }

            // 清理剩余消息
            foreach (var message in activeMessages)
            {
                Console.WriteLine($"清理剩余消息: {message.Content}");
                CachePoolMgr.Release(message);
            }

            Console.WriteLine();

            // 2. 特效系统演示
            Console.WriteLine("2. 特效系统演示:");
            var activeEffects = new List<ParticleEffect>();

            // 创建特效
            string[] effectNames = { "火花", "爆炸", "治愈光环", "冰冻效果" };
            for (int i = 0; i < effectNames.Length; i++)
            {
                var effect = CachePoolMgr.Acquire<ParticleEffect>();
                effect.Play(effectNames[i], 0.5f + i * 0.2f);
                activeEffects.Add(effect);
                Console.WriteLine($"播放特效: {effect.EffectName}, 持续: {effect.Duration:F1}秒");
            }

            // 模拟游戏循环，更新特效
            float totalTime = 0f;
            while (activeEffects.Count > 0)
            {
                System.Threading.Thread.Sleep(100);
                totalTime += 0.1f;
                Console.WriteLine($"游戏时间: {totalTime:F1}秒");

                for (int i = activeEffects.Count - 1; i >= 0; i--)
                {
                    activeEffects[i].Update(0.1f);

                    if (!activeEffects[i].IsPlaying)
                    {
                        Console.WriteLine($"  特效结束: {activeEffects[i].EffectName}");
                        CachePoolMgr.Release(activeEffects[i]);
                        activeEffects.RemoveAt(i);
                    }
                }
            }

            Console.WriteLine("所有特效播放完毕");
            Console.WriteLine();
        }

        /// <summary>
        /// 运行所有基本演示
        /// </summary>
        public static void RunAllDemos()
        {
            Console.WriteLine("开始对象池基本使用演示...\n");

            DemoBasicOperations();
            DemoPreAllocation();
            DemoErrorHandling();
            DemoObjectLifecycle();

            Console.WriteLine("=== 基本演示完成 ===");
        }
    }
}