using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MonsterCache.Runtime;

namespace MonsterCache.Examples
{
    /// <summary>
    /// 调试和监控演示
    /// </summary>
    public static class DebugMonitorDemo
    {
        /// <summary>
        /// 演示基本调试功能
        /// </summary>
        public static void DemoBasicDebugging()
        {
            Console.WriteLine("=== 基本调试功能演示 ===\n");

            // 启用调试日志
            Console.WriteLine("1. 启用调试日志记录:");
            PoolDebugger.EnableDebugLogging = true;
            PoolDebugger.StartMonitoring();
            Console.WriteLine("调试监控已启动\n");

            // 执行一些对象池操作
            Console.WriteLine("2. 执行对象池操作...");
            var bullets = new List<Bullet>();
            for (var i = 0; i < 5; i++)
            {
                var bullet = ObjectPoolMgr.Acquire<Bullet>();
                bullet.Initialize(i * 10, i * 20, 5, 3, 10.0f);
                bullets.Add(bullet);

                Thread.Sleep(50); // 让日志时间戳更明显
            }

            foreach (var bullet in bullets)
            {
                ObjectPoolMgr.Release(bullet);
                Thread.Sleep(30);
            }

            // 查看调试日志
            Console.WriteLine("\n3. 查看调试日志:");
            var debugLogs = PoolDebugger.GetDebugLog(10);
            foreach (var log in debugLogs)
            {
                Console.WriteLine($"  {log}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// 演示详细状态报告
        /// </summary>
        public static void DemoDetailedReporting()
        {
            Console.WriteLine("=== 详细状态报告演示 ===\n");

            // 先创建一些多样化的使用模式
            Console.WriteLine("1. 创建测试数据...");
            CreateTestData();

            Console.WriteLine("\n2. 生成详细报告:");
            var detailedReport = PoolDebugger.GenerateDetailedReport(includeEmptyPools: false);
            Console.WriteLine(detailedReport);

            Console.WriteLine("\n3. 生成控制台简化报告:");
            var consoleReport = PoolDebugger.GenerateConsoleReport();
            Console.WriteLine(consoleReport);
        }

        /// <summary>
        /// 演示健康检查功能
        /// </summary>
        public static void DemoHealthCheck()
        {
            Console.WriteLine("=== 健康检查演示 ===\n");

            Console.WriteLine("1. 创建不同健康状态的对象池...");

            // 健康的对象池使用
            ObjectPoolMgr.Expand<Bullet>(30);
            var healthyBullets = new List<Bullet>();
            for (int i = 0; i < 50; i++)
            {
                var bullet = ObjectPoolMgr.Acquire<Bullet>();
                healthyBullets.Add(bullet);

                if (i % 5 == 0 && healthyBullets.Count > 10) // 定期归还一些
                {
                    var toReturn = healthyBullets.Take(3).ToArray();
                    foreach (var b in toReturn)
                    {
                        ObjectPoolMgr.Release(b);
                        healthyBullets.Remove(b);
                    }
                }
            }

            // 问题对象池 - 内存泄漏模拟
            for (var i = 0; i < 30; i++)
            {
                // ReSharper disable once UnusedVariable
                var effect = ObjectPoolMgr.Acquire<ParticleEffect>(); // 获取但不归还
                // 故意不归还，模拟内存泄漏
            }

            // 问题对象池 - 过度分配
            ObjectPoolMgr.Expand<GameMessage>(200); // 预分配很多
            for (int i = 0; i < 3; i++) // 但只用很少
            {
                var msg = ObjectPoolMgr.Acquire<GameMessage>();
                ObjectPoolMgr.Release(msg);
            }

            Console.WriteLine("\n2. 执行健康检查:");
            var healthResult = PoolDebugger.PerformHealthCheck();

            Console.WriteLine($"健康检查结果:");
            Console.WriteLine($"  总对象池数量: {healthResult.TotalPools}");
            Console.WriteLine($"  健康对象池: {healthResult.HealthyPools}");
            Console.WriteLine($"  有问题对象池: {healthResult.ProblematicPools}");
            Console.WriteLine($"  疑似内存泄漏: {healthResult.MemoryLeakSuspects}");
            Console.WriteLine($"  总体健康度: {healthResult.OverallHealth}/100");

            Console.WriteLine("\n建议:");
            foreach (var recommendation in healthResult.Recommendations)
            {
                Console.WriteLine($"  • {recommendation}");
            }

            // 清理一些对象以改善健康状况
            Console.WriteLine("\n3. 应用优化建议...");
            var optimizedCount = ObjectPoolMgr.AutoOptimize(applyHighPriorityOnly: true);
            Console.WriteLine($"应用了 {optimizedCount} 个优化");

            // 再次检查
            var healthResult2 = PoolDebugger.PerformHealthCheck();
            Console.WriteLine(
                $"\n优化后健康度: {healthResult2.OverallHealth}/100 (改善: +{healthResult2.OverallHealth - healthResult.OverallHealth})");
        }

        /// <summary>
        /// 演示实时监控和警报
        /// </summary>
        public static void DemoRealTimeMonitoring()
        {
            Console.WriteLine("=== 实时监控和警报演示 ===\n");

            // 设置监控和警报
            var eventCount = 0;
            var warningEvents = new List<string>();

            Console.WriteLine("1. 设置实时监控...");
            ObjectPoolMgr.OnPoolEvent += (poolType, args) =>
            {
                eventCount++;
                var eventMsg = $"{poolType.Name}: {args.EventType}";

                // 检查是否是警告事件
                if (args.EventType.Contains("Auto") || args.EventType.Contains("Warning"))
                {
                    warningEvents.Add(eventMsg);
                    Console.WriteLine($"[警告] {eventMsg}");
                }
                else
                {
                    Console.WriteLine($"[信息] {eventMsg}");
                }
            };

            Console.WriteLine("\n2. 模拟各种对象池活动...");

            // 模拟正常活动
            Console.WriteLine("  执行正常活动...");
            for (int i = 0; i < 10; i++)
            {
                var bullet = ObjectPoolMgr.Acquire<Bullet>();
                ObjectPoolMgr.Release(bullet);
            }

            // 模拟会触发警告的活动
            Console.WriteLine("  创建会触发优化的使用模式...");

            // 创建低效率使用模式
            for (int i = 0; i < 50; i++)
            {
                var heavy = ObjectPoolMgr.Acquire<HeavyObject>();
                ObjectPoolMgr.Release(heavy); // 立即归还，导致频繁创建
            }

            Thread.Sleep(100);

            // 触发自动优化
            Console.WriteLine("  触发自动优化...");
            ObjectPoolMgr.AutoOptimize();

            Console.WriteLine($"\n3. 监控总结:");
            Console.WriteLine($"  总事件数: {eventCount}");
            Console.WriteLine($"  警告事件数: {warningEvents.Count}");

            if (warningEvents.Count > 0)
            {
                Console.WriteLine("  警告事件详情:");
                foreach (var warning in warningEvents)
                {
                    Console.WriteLine($"    - {warning}");
                }
            }
        }

        /// <summary>
        /// 演示性能基准测试
        /// </summary>
        public static void DemoBenchmarking()
        {
            Console.WriteLine("=== 性能基准测试演示 ===\n");

            const int testIterations = 10000;

            Console.WriteLine($"1. 基准测试参数:");
            Console.WriteLine($"   测试迭代次数: {testIterations:N0}");
            Console.WriteLine($"   测试对象: HeavyObject (模拟高创建成本)");

            // 预热对象池
            Console.WriteLine("\n2. 预热对象池...");
            ObjectPoolMgr.Expand<HeavyObject>(100);

            var preWarmObjects = new List<HeavyObject>();
            for (int i = 0; i < 50; i++)
            {
                preWarmObjects.Add(ObjectPoolMgr.Acquire<HeavyObject>());
            }

            foreach (var obj in preWarmObjects)
            {
                ObjectPoolMgr.Release(obj);
            }

            Console.WriteLine("\n3. 执行基准测试...");

            // 测试1: 对象池性能
            var poolObjects = new List<HeavyObject>();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < testIterations; i++)
            {
                var obj = ObjectPoolMgr.Acquire<HeavyObject>();
                obj.ProcessData($"Test {i}");
                poolObjects.Add(obj);

                // 定期归还一些对象来模拟真实使用
                if (i % 100 == 0 && poolObjects.Count > 20)
                {
                    var toReturn = poolObjects.Take(10).ToArray();
                    foreach (var o in toReturn)
                    {
                        ObjectPoolMgr.Release(o);
                        poolObjects.Remove(o);
                    }
                }
            }

            // 归还剩余对象
            foreach (var obj in poolObjects)
            {
                ObjectPoolMgr.Release(obj);
            }

            stopwatch.Stop();
            var poolTime = stopwatch.ElapsedMilliseconds;

            // 测试2: 直接创建性能（对比）
            stopwatch.Restart();
            var directObjects = new List<HeavyObject>();

            for (int i = 0; i < testIterations; i++)
            {
                var obj = new HeavyObject();
                obj.ProcessData($"Test {i}");
                directObjects.Add(obj);

                // 模拟清理（在实际情况下这些对象会被GC回收）
                if (i % 100 == 0 && directObjects.Count > 20)
                {
                    directObjects.RemoveRange(0, 10);
                }
            }

            stopwatch.Stop();
            var directTime = stopwatch.ElapsedMilliseconds;

            // 结果分析
            Console.WriteLine($"\n4. 基准测试结果:");
            Console.WriteLine($"   使用对象池: {poolTime:N0} ms");
            Console.WriteLine($"   直接创建: {directTime:N0} ms");
            Console.WriteLine($"   性能提升: {((directTime - poolTime) / (float)directTime * 100):F1}%");
            Console.WriteLine($"   每次操作节省: {((directTime - poolTime) / (float)testIterations):F3} ms");

            // 获取详细的池统计
            Console.WriteLine($"\n5. 对象池统计:");
            var analysis = ObjectPoolMgr.AnalyzePool(typeof(HeavyObject));
            var info = analysis.PoolInfo;
            var metrics = analysis.Metrics;

            Console.WriteLine($"   总获取次数: {info.AcquirePoolableCount:N0}");
            Console.WriteLine($"   对象创建次数: {info.AddPoolableCount:N0}");
            Console.WriteLine($"   复用次数: {info.AcquirePoolableCount - info.AddPoolableCount:N0}");
            Console.WriteLine($"   池效率: {metrics.PoolEfficiency:P2}");
            Console.WriteLine(
                $"   缓存命中率: {((info.AcquirePoolableCount - info.AddPoolableCount) / (float)info.AcquirePoolableCount):P2}");
        }

        /// <summary>
        /// 演示调试工具集成
        /// </summary>
        public static void DemoDebugIntegration()
        {
            Console.WriteLine("=== 调试工具集成演示 ===\n");

            Console.WriteLine("1. 模拟开发环境中的调试场景...");

            // 清空之前的日志
            PoolDebugger.ClearDebugLog();
            PoolDebugger.EnableDebugLogging = true;

            // 模拟开发中的典型操作序列
            SimulateGameplaySession();

            Console.WriteLine("\n2. 调试信息汇总:");

            // 显示最近的调试日志
            Console.WriteLine("\n最近的操作日志:");
            var recentLogs = PoolDebugger.GetDebugLog(15);
            foreach (var log in recentLogs.TakeLast(10))
            {
                Console.WriteLine($"  {log}");
            }

            // 生成当前状态快照
            Console.WriteLine("\n当前对象池状态:");
            var snapshot = PoolDebugger.GenerateConsoleReport();
            Console.WriteLine(snapshot);

            // 检查是否有问题需要关注
            Console.WriteLine("\n问题检测:");
            var healthCheck = PoolDebugger.PerformHealthCheck();
            if (healthCheck.ProblematicPools > 0)
            {
                Console.WriteLine($"发现 {healthCheck.ProblematicPools} 个有问题的对象池");
                Console.WriteLine("建议立即处理:");
                foreach (var rec in healthCheck.Recommendations.Take(3))
                {
                    Console.WriteLine($"  • {rec}");
                }
            }
            else
            {
                Console.WriteLine("所有对象池状态良好 ✓");
            }

            Console.WriteLine("\n3. 调试助手功能演示:");

            // 模拟在调试器中检查特定对象池
            Console.WriteLine("\n检查 Bullet 对象池详情:");
            var bulletReport = ObjectPoolMgr.AnalyzePool(typeof(Bullet));
            Console.WriteLine($"  效率: {bulletReport.Metrics.PoolEfficiency:P}");
            Console.WriteLine($"  利用率: {bulletReport.Metrics.AverageUtilization:P}");
            Console.WriteLine($"  建议大小: {bulletReport.Metrics.RecommendedPoolSize}");

            if (bulletReport.Issues.Length > 0)
            {
                Console.WriteLine("  发现问题:");
                foreach (var issue in bulletReport.Issues)
                {
                    Console.WriteLine($"    {issue.Description}");
                }
            }
        }

        /// <summary>
        /// 模拟游戏会话中的对象池使用
        /// </summary>
        private static void SimulateGameplaySession()
        {
            Console.WriteLine("  模拟游戏开始，预分配资源...");
            ObjectPoolMgr.Expand<Bullet>(50);
            ObjectPoolMgr.Expand<ParticleEffect>(20);
            ObjectPoolMgr.Expand<GameMessage>(10);

            // 模拟游戏循环
            var activeBullets = new List<Bullet>();
            var activeEffects = new List<ParticleEffect>();
            var activeMessages = new List<GameMessage>();

            Console.WriteLine("  模拟战斗场景...");
            for (int wave = 0; wave < 3; wave++)
            {
                // 创建子弹
                for (int i = 0; i < 15; i++)
                {
                    var bullet = ObjectPoolMgr.Acquire<Bullet>();
                    bullet.Initialize(i * 10, 100, 10, 5, 25);
                    activeBullets.Add(bullet);
                }

                // 创建特效
                for (int i = 0; i < 3; i++)
                {
                    var effect = ObjectPoolMgr.Acquire<ParticleEffect>();
                    effect.Play($"Explosion{i}", 2.0f);
                    activeEffects.Add(effect);
                }

                // 创建消息
                var message = ObjectPoolMgr.Acquire<GameMessage>();
                message.SetMessage($"第 {wave + 1} 波敌人", 1);
                activeMessages.Add(message);

                Thread.Sleep(100); // 模拟帧间隔

                // 清理一些对象（模拟对象生命周期结束）
                var bulletsToRemove = activeBullets.Take(10).ToArray();
                foreach (var bullet in bulletsToRemove)
                {
                    ObjectPoolMgr.Release(bullet);
                    activeBullets.Remove(bullet);
                }
            }

            Console.WriteLine("  清理场景资源...");
            // 清理所有剩余对象
            foreach (var bullet in activeBullets)
                ObjectPoolMgr.Release(bullet);
            foreach (var effect in activeEffects)
                ObjectPoolMgr.Release(effect);
            foreach (var message in activeMessages)
                ObjectPoolMgr.Release(message);
        }

        /// <summary>
        /// 创建测试数据以演示报告功能
        /// </summary>
        private static void CreateTestData()
        {
            // 创建不同使用模式的数据

            // 高效使用的对象池
            ObjectPoolMgr.Expand<Bullet>(30);
            var bullets = new List<Bullet>();
            for (int i = 0; i < 100; i++)
            {
                bullets.Add(ObjectPoolMgr.Acquire<Bullet>());
                if (i % 10 == 0 && bullets.Count > 5)
                {
                    var toReturn = bullets.Take(3).ToArray();
                    foreach (var b in toReturn)
                    {
                        ObjectPoolMgr.Release(b);
                        bullets.Remove(b);
                    }
                }
            }

            // 归还大部分子弹
            foreach (var bullet in bullets.Take(bullets.Count * 3 / 4))
            {
                ObjectPoolMgr.Release(bullet);
            }

            // 低效使用的对象池（频繁创建）
            for (int i = 0; i < 30; i++)
            {
                var heavy = ObjectPoolMgr.Acquire<HeavyObject>();
                ObjectPoolMgr.Release(heavy);
            }

            // 过度分配的对象池
            ObjectPoolMgr.Expand<GameMessage>(100);
            for (int i = 0; i < 5; i++)
            {
                var msg = ObjectPoolMgr.Acquire<GameMessage>();
                ObjectPoolMgr.Release(msg);
            }
        }

        /// <summary>
        /// 运行所有调试监控演示
        /// </summary>
        public static void RunAllDemos()
        {
            Console.WriteLine("开始调试监控演示...\n");

            DemoBasicDebugging();
            Console.WriteLine("\n" + new string('=', 50) + "\n");

            DemoDetailedReporting();
            Console.WriteLine("\n" + new string('=', 50) + "\n");

            DemoHealthCheck();
            Console.WriteLine("\n" + new string('=', 50) + "\n");

            DemoRealTimeMonitoring();
            Console.WriteLine("\n" + new string('=', 50) + "\n");

            DemoBenchmarking();
            Console.WriteLine("\n" + new string('=', 50) + "\n");

            DemoDebugIntegration();

            // 停止监控
            PoolDebugger.StopMonitoring();
            PoolDebugger.EnableDebugLogging = false;

            Console.WriteLine("\n=== 调试监控演示完成 ===");
        }
    }
}