using System;
using System.Collections.Generic;
using System.Linq;
using MonsterCache.Runtime;
using MonsterCache.Runtime.Debug;
using UnityEngine;

namespace MonsterCache.Examples
{
    /// <summary>
    /// 性能分析演示
    /// </summary>
    public static class PerformanceAnalysisDemo
    {
        /// <summary>
        /// 演示基本性能分析
        /// </summary>
        public static void DemoBasicAnalysis()
        {
            Console.WriteLine("=== 基本性能分析演示 ===\n");

            // 先创建一些使用数据
            Console.WriteLine("1. 模拟对象池使用...");
            SimulatePoolUsage();

            Console.WriteLine("\n2. 分析单个对象池:");
            var bulletReport = ObjectPoolMgr.AnalyzePool(typeof(Bullet));
            PrintPoolReport(bulletReport);

            Console.WriteLine("\n3. 生成性能报告摘要:");
            var report = ObjectPoolMgr.GeneratePerformanceReport();
            Console.WriteLine(report);
        }

        /// <summary>
        /// 演示内存泄漏检测
        /// </summary>
        public static void DemoMemoryLeakDetection()
        {
            Console.WriteLine("=== 内存泄漏检测演示 ===\n");

            Console.WriteLine("1. 模拟正常使用（无泄漏）:");
            var bullets = new List<Bullet>();
            for (int i = 0; i < 50; i++)
            {
                var bullet = ObjectPoolMgr.Acquire<Bullet>();
                bullets.Add(bullet);
            }

            // 正常归还所有对象
            foreach (var bullet in bullets)
            {
                ObjectPoolMgr.Release(bullet);
            }

            bullets.Clear();

            var leaks1 = ObjectPoolMgr.DetectMemoryLeaks();
            Console.WriteLine($"检测到内存泄漏: {leaks1.Length} 个");

            Console.WriteLine("\n2. 模拟内存泄漏（忘记归还对象）:");
            for (int i = 0; i < 30; i++)
            {
                // ReSharper disable once UnusedVariable
                var bullet = ObjectPoolMgr.Acquire<Bullet>(); // 获取但不归还
                // 故意不归还，模拟泄漏
            }

            var leaks2 = ObjectPoolMgr.DetectMemoryLeaks();
            Console.WriteLine($"检测到内存泄漏: {leaks2.Length} 个");

            if (leaks2.Length > 0)
            {
                Console.WriteLine("泄漏详情:");
                foreach (var leak in leaks2)
                {
                    Console.WriteLine($"  {leak.PoolType.Name}: 泄漏风险 {leak.Metrics.MemoryLeakRisk:F1}/10");
                    Console.WriteLine(
                        $"    获取: {leak.PoolInfo.AcquirePoolableCount}, 归还: {leak.PoolInfo.ReleasePoolableCount}");
                }
            }
        }

        /// <summary>
        /// 演示性能问题检测和建议
        /// </summary>
        public static void DemoPerformanceIssueDetection()
        {
            Console.WriteLine("=== 性能问题检测演示 ===\n");

            Console.WriteLine("1. 创建低效率的对象池使用模式...");

            // 模拟池过小的情况 - 频繁创建新对象
            for (int i = 0; i < 100; i++)
            {
                var heavy = ObjectPoolMgr.Acquire<HeavyObject>();
                heavy.ProcessData($"Data {i}");
                // 立即归还，但因为池小，下次又要创建新的
                ObjectPoolMgr.Release(heavy);
            }

            // 模拟池过大的情况 - 预分配过多但很少使用
            ObjectPoolMgr.Expand<ParticleEffect>(100);
            for (int i = 0; i < 5; i++) // 只使用很少的几个
            {
                var effect = ObjectPoolMgr.Acquire<ParticleEffect>();
                effect.Play($"Effect {i}", 1.0f);
                ObjectPoolMgr.Release(effect);
            }

            Console.WriteLine("\n2. 分析所有对象池的问题:");
            var reports = ObjectPoolMgr.AnalyzeAllPools();

            foreach (var report in reports)
            {
                if (report.Issues.Length > 0)
                {
                    Console.WriteLine($"\n{report.PoolType.Name} 发现问题:");
                    foreach (var issue in report.Issues.OrderByDescending(i => i.Severity))
                    {
                        var severityLevel = issue.Severity >= 8 ? "严重" :
                            issue.Severity >= 6 ? "警告" : "提示";
                        Console.WriteLine($"  [{severityLevel}] {issue.Description}");
                        Console.WriteLine($"    建议: {issue.Suggestion}");
                    }
                }
            }
        }

        /// <summary>
        /// 演示自动优化
        /// </summary>
        public static void DemoAutoOptimization()
        {
            Console.WriteLine("=== 自动优化演示 ===\n");

            Console.WriteLine("1. 优化前的状态:");
            PrintPoolSummary();

            Console.WriteLine("\n2. 执行自动优化...");
            int optimizationCount = ObjectPoolMgr.AutoOptimize(applyHighPriorityOnly: true);
            Console.WriteLine($"应用了 {optimizationCount} 个优化");

            Console.WriteLine("\n3. 优化后的状态:");
            PrintPoolSummary();

            if (optimizationCount > 0)
            {
                Console.WriteLine("\n4. 验证优化效果:");
                var reports = ObjectPoolMgr.AnalyzeAllPools();
                var remainingHighPriorityIssues = reports
                    .SelectMany(r => r.Issues)
                    .Where(i => i.Severity >= 7)
                    .ToArray();

                Console.WriteLine($"剩余高优先级问题: {remainingHighPriorityIssues.Length} 个");
            }
        }

        /// <summary>
        /// 演示详细性能指标
        /// </summary>
        public static void DemoDetailedMetrics()
        {
            Console.WriteLine("=== 详细性能指标演示 ===\n");

            // 创建不同使用模式的数据
            Console.WriteLine("1. 创建多样化的使用模式...");

            // 高效使用模式
            ObjectPoolMgr.Expand<Bullet>(50); // 预分配
            var bullets = new List<Bullet>();
            for (int i = 0; i < 200; i++)
            {
                var bullet = ObjectPoolMgr.Acquire<Bullet>();
                bullets.Add(bullet);
                if (i % 10 == 0) // 定期归还一些
                {
                    var toReturn = bullets.Take(5).ToArray();
                    foreach (var b in toReturn)
                    {
                        ObjectPoolMgr.Release(b);
                        bullets.Remove(b);
                    }
                }
            }

            // 归还剩余的
            foreach (var bullet in bullets)
            {
                ObjectPoolMgr.Release(bullet);
            }

            // 低效使用模式
            for (int i = 0; i < 50; i++)
            {
                var message = ObjectPoolMgr.Acquire<GameMessage>(); // 小池，频繁分配
                ObjectPoolMgr.Release(message);
            }

            Console.WriteLine("\n2. 详细性能指标:");
            var allReports = ObjectPoolMgr.AnalyzeAllPools();

            foreach (var report in allReports.Where(r => r.PoolInfo.AcquirePoolableCount > 0))
            {
                Console.WriteLine($"\n=== {report.PoolType.Name} ===");
                var info = report.PoolInfo;
                var metrics = report.Metrics;

                Console.WriteLine($"基础统计:");
                Console.WriteLine($"  当前状态: 空闲={info.UnusedPoolableCount}, 使用={info.UsedPoolableCount}");
                Console.WriteLine($"  操作统计: 获取={info.AcquirePoolableCount}, 归还={info.ReleasePoolableCount}");
                Console.WriteLine($"  对象管理: 创建={info.AddPoolableCount}, 销毁={info.RemovePoolableCount}");

                Console.WriteLine($"性能指标:");
                Console.WriteLine($"  池效率: {metrics.PoolEfficiency:P2} (重用率)");
                Console.WriteLine($"  利用率: {metrics.AverageUtilization:P2} (当前使用/总容量)");
                Console.WriteLine($"  内存泄漏风险: {metrics.MemoryLeakRisk:F2}/10");
                Console.WriteLine(
                    $"  创建vs复用比率: {(Mathf.Approximately(metrics.NewVsReuseRatio, float.MaxValue) ? "∞" : metrics.NewVsReuseRatio.ToString("F2"))}");
                Console.WriteLine($"  建议池大小: {metrics.RecommendedPoolSize}");

                // 计算一些额外的有用指标
                var totalObjects = info.UnusedPoolableCount + info.UsedPoolableCount;
                var hitRate = info.AcquirePoolableCount > 0
                    ? (info.AcquirePoolableCount - info.AddPoolableCount) / (float)info.AcquirePoolableCount
                    : 0f;
                var memoryEfficiency = info.AddPoolableCount > 0
                    ? info.AcquirePoolableCount / (float)info.AddPoolableCount
                    : 0f;

                Console.WriteLine($"额外指标:");
                Console.WriteLine($"  缓存命中率: {hitRate:P2}");
                Console.WriteLine($"  内存效率: {memoryEfficiency:F2}x (总获取/创建对象)");
                Console.WriteLine($"  池饱和度: {(info.UsedPoolableCount / (float)Math.Max(1, totalObjects)):P2}");
            }
        }

        /// <summary>
        /// 演示实时监控
        /// </summary>
        public static void DemoRealTimeMonitoring()
        {
            Console.WriteLine("=== 实时监控演示 ===\n");

            // 启用事件监控
            Console.WriteLine("1. 启用性能监控...");
            var eventLog = new List<string>();
            ObjectPoolMgr.OnPoolEvent += (poolType, args) =>
            {
                var eventMsg = $"{DateTime.Now:HH:mm:ss.fff} - {poolType.Name}: {args.EventType}";
                if (args.Data.Count > 0)
                {
                    eventMsg += $" ({string.Join(", ", args.Data.Select(kv => $"{kv.Key}={kv.Value}"))})";
                }

                eventLog.Add(eventMsg);
                Console.WriteLine($"[事件] {eventMsg}");
            };

            Console.WriteLine("\n2. 模拟活动并监控事件...");

            // 模拟一系列活动
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine($"\n第 {i + 1} 轮活动:");

                // 获取对象
                var bullets = new List<Bullet>();
                for (int j = 0; j < 10; j++)
                {
                    bullets.Add(ObjectPoolMgr.Acquire<Bullet>());
                }

                // 归还对象
                foreach (var bullet in bullets)
                {
                    ObjectPoolMgr.Release(bullet);
                }

                System.Threading.Thread.Sleep(100);
            }

            // 执行自动优化来触发更多事件
            Console.WriteLine("\n3. 执行自动优化...");
            ObjectPoolMgr.AutoOptimize();

            Console.WriteLine($"\n4. 监控总结:");
            Console.WriteLine($"总共捕获了 {eventLog.Count} 个事件");

            var eventTypes = eventLog
                .Select(log => log.Split(':')[1].Split(' ')[1])
                .GroupBy(type => type)
                .ToArray();

            Console.WriteLine("事件类型统计:");
            foreach (var group in eventTypes)
            {
                Console.WriteLine($"  {group.Key}: {group.Count()} 次");
            }
        }

        /// <summary>
        /// 模拟对象池使用以生成分析数据
        /// </summary>
        private static void SimulatePoolUsage()
        {
            // 预分配一些对象
            ObjectPoolMgr.Expand<Bullet>(20);
            ObjectPoolMgr.Expand<ParticleEffect>(10);

            // 使用 Bullet 对象池
            var bullets = new List<Bullet>();
            for (int i = 0; i < 100; i++)
            {
                var bullet = ObjectPoolMgr.Acquire<Bullet>();
                bullets.Add(bullet);

                // 随机归还一些对象
                if (i > 20 && UnityEngine.Random.Range(0f, 1f) < 0.3f)
                {
                    var toReturn = bullets[0];
                    bullets.RemoveAt(0);
                    ObjectPoolMgr.Release(toReturn);
                }
            }

            // 归还大部分对象
            var bulletsToReturn = bullets.Take(bullets.Count * 2 / 3).ToArray();
            foreach (var bullet in bulletsToReturn)
            {
                ObjectPoolMgr.Release(bullet);
                bullets.Remove(bullet);
            }

            // 使用 ParticleEffect 对象池
            for (int i = 0; i < 30; i++)
            {
                var effect = ObjectPoolMgr.Acquire<ParticleEffect>();
                ObjectPoolMgr.Release(effect);
            }

            // 使用 HeavyObject（创建成本高）
            for (int i = 0; i < 5; i++)
            {
                var heavy = ObjectPoolMgr.Acquire<HeavyObject>();
                ObjectPoolMgr.Release(heavy);
            }
        }

        /// <summary>
        /// 打印对象池报告
        /// </summary>
        private static void PrintPoolReport(PoolAnalysisReport report)
        {
            Console.WriteLine($"对象池类型: {report.PoolType.Name}");

            var info = report.PoolInfo;
            Console.WriteLine($"  状态: 空闲={info.UnusedPoolableCount}, 使用={info.UsedPoolableCount}");
            Console.WriteLine(
                $"  统计: 获取={info.AcquirePoolableCount}, 归还={info.ReleasePoolableCount}, 创建={info.AddPoolableCount}");

            var metrics = report.Metrics;
            Console.WriteLine(
                $"  性能: 效率={metrics.PoolEfficiency:P}, 利用率={metrics.AverageUtilization:P}, 泄漏风险={metrics.MemoryLeakRisk:F1}");
            Console.WriteLine($"  建议池大小: {metrics.RecommendedPoolSize}");

            if (report.Issues.Length > 0)
            {
                Console.WriteLine("  问题:");
                foreach (var issue in report.Issues)
                {
                    Console.WriteLine($"    [{issue.Severity}/10] {issue.Description}");
                }
            }
        }

        /// <summary>
        /// 打印对象池摘要
        /// </summary>
        private static void PrintPoolSummary()
        {
            var poolInfos = ObjectPoolMgr.GetAllPoolInfos();
            Console.WriteLine("对象池状态摘要:");
            Console.WriteLine("类型".PadRight(15) + "空闲".PadLeft(8) + "使用".PadLeft(8) + "获取".PadLeft(8) +
                              "创建".PadLeft(8));
            Console.WriteLine(new string('-', 47));

            foreach (var info in poolInfos)
            {
                Console.WriteLine($"{info.PoolType.Name,-15}" +
                                  $"{info.UnusedPoolableCount,8}" +
                                  $"{info.UsedPoolableCount,8}" +
                                  $"{info.AcquirePoolableCount,8}" +
                                  $"{info.AddPoolableCount,8}");
            }
        }

        /// <summary>
        /// 运行所有性能分析演示
        /// </summary>
        public static void RunAllDemos()
        {
            Console.WriteLine("开始性能分析演示...\n");

            DemoBasicAnalysis();
            Console.WriteLine("\n" + new string('=', 50) + "\n");

            DemoMemoryLeakDetection();
            Console.WriteLine("\n" + new string('=', 50) + "\n");

            DemoPerformanceIssueDetection();
            Console.WriteLine("\n" + new string('=', 50) + "\n");

            DemoAutoOptimization();
            Console.WriteLine("\n" + new string('=', 50) + "\n");

            DemoDetailedMetrics();
            Console.WriteLine("\n" + new string('=', 50) + "\n");

            DemoRealTimeMonitoring();

            Console.WriteLine("\n=== 性能分析演示完成 ===");
        }
    }
}