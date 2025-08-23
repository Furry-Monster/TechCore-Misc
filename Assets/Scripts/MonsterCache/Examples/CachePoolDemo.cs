using System;
using MonsterCache.Runtime;

namespace MonsterCache.Examples
{
    /// <summary>
    /// 对象池系统完整演示入口
    /// </summary>
    public static class CachePoolDemo
    {
        /// <summary>
        /// 运行所有演示
        /// </summary>
        public static void RunAllDemos()
        {
            Console.WriteLine("🚀 MonsterCache 对象池系统 - 完整功能演示");
            Console.WriteLine("=".PadRight(60, '='));
            Console.WriteLine($"演示开始时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine();

            try
            {
                // 1. 基本使用演示
                Console.WriteLine("📚 第一部分: 基本使用演示");
                Console.WriteLine("-".PadRight(40, '-'));
                BasicUsageDemo.RunAllDemos();
                WaitForUserInput("\n按任意键继续到性能分析演示...");

                // 2. 性能分析演示
                Console.WriteLine("\n📊 第二部分: 性能分析演示");
                Console.WriteLine("-".PadRight(40, '-'));
                PerformanceAnalysisDemo.RunAllDemos();
                WaitForUserInput("\n按任意键继续到调试监控演示...");

                // 3. 调试监控演示
                Console.WriteLine("\n🔧 第三部分: 调试监控演示");
                Console.WriteLine("-".PadRight(40, '-'));
                DebugMonitorDemo.RunAllDemos();

                // 演示总结
                Console.WriteLine("\n" + "=".PadRight(60, '='));
                Console.WriteLine("🎉 演示完成总结");
                GenerateCompletionSummary();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 演示过程中发生异常: {ex.Message}");
                Console.WriteLine("详细信息:");
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// 运行快速演示（核心功能）
        /// </summary>
        public static void RunQuickDemo()
        {
            Console.WriteLine("⚡ MonsterCache 对象池系统 - 快速演示");
            Console.WriteLine("=".PadRight(50, '='));
            Console.WriteLine();

            try
            {
                // 基本操作演示
                Console.WriteLine("1. 基本对象池操作:");
                BasicUsageDemo.DemoBasicOperations();

                Console.WriteLine("\n2. 性能分析:");
                PerformanceAnalysisDemo.DemoBasicAnalysis();

                Console.WriteLine("\n3. 健康检查:");
                var health = PoolDebugger.PerformHealthCheck();
                Console.WriteLine($"对象池健康度: {health.OverallHealth}/100");

                if (health.Recommendations.Length > 0)
                {
                    Console.WriteLine("系统建议:");
                    foreach (var recommendation in health.Recommendations)
                    {
                        Console.WriteLine($"  • {recommendation}");
                    }
                }

                Console.WriteLine("\n✅ 快速演示完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 快速演示中发生异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 演示特定功能
        /// </summary>
        /// <param name="feature">功能名称</param>
        public static void DemoFeature(string feature)
        {
            Console.WriteLine($"🎯 演示功能: {feature}");
            Console.WriteLine("-".PadRight(30, '-'));

            try
            {
                switch (feature.ToLower())
                {
                    case "basic":
                    case "基本":
                        BasicUsageDemo.DemoBasicOperations();
                        break;

                    case "prealloc":
                    case "预分配":
                        BasicUsageDemo.DemoPreAllocation();
                        break;

                    case "analysis":
                    case "分析":
                        PerformanceAnalysisDemo.DemoBasicAnalysis();
                        break;

                    case "leak":
                    case "泄漏":
                        PerformanceAnalysisDemo.DemoMemoryLeakDetection();
                        break;

                    case "optimize":
                    case "优化":
                        PerformanceAnalysisDemo.DemoAutoOptimization();
                        break;

                    case "debug":
                    case "调试":
                        DebugMonitorDemo.DemoBasicDebugging();
                        break;

                    case "monitor":
                    case "监控":
                        DebugMonitorDemo.DemoRealTimeMonitoring();
                        break;

                    case "health":
                    case "健康检查":
                        DebugMonitorDemo.DemoHealthCheck();
                        break;

                    case "benchmark":
                    case "基准测试":
                        DebugMonitorDemo.DemoBenchmarking();
                        break;

                    default:
                        Console.WriteLine($"未知功能: {feature}");
                        Console.WriteLine("可用功能:");
                        Console.WriteLine("  basic/基本 - 基本操作演示");
                        Console.WriteLine("  prealloc/预分配 - 预分配演示");
                        Console.WriteLine("  analysis/分析 - 性能分析");
                        Console.WriteLine("  leak/泄漏 - 内存泄漏检测");
                        Console.WriteLine("  optimize/优化 - 自动优化");
                        Console.WriteLine("  debug/调试 - 调试功能");
                        Console.WriteLine("  monitor/监控 - 实时监控");
                        Console.WriteLine("  health/健康检查 - 健康检查");
                        Console.WriteLine("  benchmark/基准测试 - 性能基准测试");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 演示功能 '{feature}' 时发生异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 交互式演示菜单
        /// </summary>
        public static void InteractiveDemo()
        {
            Console.WriteLine("🎮 MonsterCache 交互式演示");
            Console.WriteLine("=".PadRight(40, '='));

            while (true)
            {
                Console.WriteLine("\n请选择要演示的功能:");
                Console.WriteLine("1. 基本使用演示");
                Console.WriteLine("2. 性能分析演示");
                Console.WriteLine("3. 调试监控演示");
                Console.WriteLine("4. 快速演示（核心功能）");
                Console.WriteLine("5. 生成当前状态报告");
                Console.WriteLine("6. 执行健康检查");
                Console.WriteLine("7. 清理所有对象池");
                Console.WriteLine("0. 退出");
                Console.Write("\n请输入选择 (0-7): ");

                var input = Console.ReadLine();
                Console.WriteLine();

                try
                {
                    switch (input)
                    {
                        case "1":
                            BasicUsageDemo.RunAllDemos();
                            break;
                        case "2":
                            PerformanceAnalysisDemo.RunAllDemos();
                            break;
                        case "3":
                            DebugMonitorDemo.RunAllDemos();
                            break;
                        case "4":
                            RunQuickDemo();
                            break;
                        case "5":
                            GenerateCurrentStatusReport();
                            break;
                        case "6":
                            PerformInteractiveHealthCheck();
                            break;
                        case "7":
                            ClearAllPools();
                            break;
                        case "0":
                            Console.WriteLine("👋 感谢使用 MonsterCache 对象池系统演示！");
                            return;
                        default:
                            Console.WriteLine("❌ 无效选择，请输入 0-7 之间的数字");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ 执行选择时发生异常: {ex.Message}");
                }

                if (input != "0")
                {
                    WaitForUserInput("\n按任意键返回主菜单...");
                }
            }
        }

        /// <summary>
        /// 生成演示完成总结
        /// </summary>
        private static void GenerateCompletionSummary()
        {
            Console.WriteLine("演示完成统计:");

            var poolInfos = ObjectPoolMgr.GetAllPoolInfos();
            Console.WriteLine($"  创建的对象池类型: {poolInfos.Length} 种");

            var totalAcquires = 0;
            var totalReleases = 0;
            var totalCreated = 0;

            foreach (var info in poolInfos)
            {
                totalAcquires += info.AcquirePoolableCount;
                totalReleases += info.ReleasePoolableCount;
                totalCreated += info.AddPoolableCount;
            }

            Console.WriteLine($"  总获取操作: {totalAcquires:N0} 次");
            Console.WriteLine($"  总归还操作: {totalReleases:N0} 次");
            Console.WriteLine($"  总对象创建: {totalCreated:N0} 个");

            if (totalAcquires > 0)
            {
                var reuseRate = (totalAcquires - totalCreated) / (float)totalAcquires;
                Console.WriteLine($"  对象重用率: {reuseRate:P1}");
            }

            // 执行最终健康检查
            var finalHealth = PoolDebugger.PerformHealthCheck();
            Console.WriteLine($"  最终系统健康度: {finalHealth.OverallHealth}/100");

            Console.WriteLine("\n🎓 学习要点:");
            Console.WriteLine("  ✅ 对象池可以显著提高性能，特别是对创建成本高的对象");
            Console.WriteLine("  ✅ 预分配可以避免运行时的对象创建延迟");
            Console.WriteLine("  ✅ 性能分析有助于识别和解决内存泄漏等问题");
            Console.WriteLine("  ✅ 自动优化可以根据使用模式调整池大小");
            Console.WriteLine("  ✅ 实时监控提供了对系统状态的深入洞察");

            Console.WriteLine($"\n演示结束时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine("感谢体验 MonsterCache 对象池系统！");
        }

        /// <summary>
        /// 生成当前状态报告
        /// </summary>
        private static void GenerateCurrentStatusReport()
        {
            Console.WriteLine("📋 当前系统状态报告");
            Console.WriteLine("-".PadRight(30, '-'));

            var report = ObjectPoolMgr.GeneratePerformanceReport();
            Console.WriteLine(report);

            var consoleReport = PoolDebugger.GenerateConsoleReport();
            Console.WriteLine("\n简化视图:");
            Console.WriteLine(consoleReport);
        }

        /// <summary>
        /// 执行交互式健康检查
        /// </summary>
        private static void PerformInteractiveHealthCheck()
        {
            Console.WriteLine("🏥 执行系统健康检查");
            Console.WriteLine("-".PadRight(25, '-'));

            var healthResult = PoolDebugger.PerformHealthCheck();

            Console.WriteLine($"健康检查结果:");
            Console.WriteLine($"  总体健康度: {healthResult.OverallHealth}/100");
            Console.WriteLine($"  对象池总数: {healthResult.TotalPools}");
            Console.WriteLine($"  健康对象池: {healthResult.HealthyPools}");
            Console.WriteLine($"  问题对象池: {healthResult.ProblematicPools}");
            Console.WriteLine($"  疑似泄漏: {healthResult.MemoryLeakSuspects}");

            if (healthResult.Recommendations.Length > 0)
            {
                Console.WriteLine("\n建议:");
                foreach (var recommendation in healthResult.Recommendations)
                {
                    Console.WriteLine($"  💡 {recommendation}");
                }

                Console.Write("\n是否执行自动优化？(y/N): ");
                var autoOptimize = Console.ReadLine()?.ToLower();
                if (autoOptimize == "y" || autoOptimize == "yes")
                {
                    var optimizedCount = ObjectPoolMgr.AutoOptimize();
                    Console.WriteLine($"✅ 应用了 {optimizedCount} 个优化");

                    var newHealth = PoolDebugger.PerformHealthCheck();
                    Console.WriteLine($"优化后健康度: {newHealth.OverallHealth}/100");
                }
            }
        }

        /// <summary>
        /// 清理所有对象池
        /// </summary>
        private static void ClearAllPools()
        {
            Console.WriteLine("🧹 清理所有对象池");
            Console.WriteLine("-".PadRight(20, '-'));

            var poolInfos = ObjectPoolMgr.GetAllPoolInfos();
            Console.WriteLine($"清理前: {poolInfos.Length} 个对象池");

            ObjectPoolMgr.Clear();

            var newPoolInfos = ObjectPoolMgr.GetAllPoolInfos();
            Console.WriteLine($"清理后: {newPoolInfos.Length} 个对象池");
            Console.WriteLine("✅ 清理完成");
        }

        /// <summary>
        /// 等待用户输入
        /// </summary>
        /// <param name="message">提示消息</param>
        private static void WaitForUserInput(string message)
        {
            Console.WriteLine(message);
            Console.ReadKey(true);
        }

        /// <summary>
        /// 显示使用帮助
        /// </summary>
        public static void ShowHelp()
        {
            Console.WriteLine("📖 MonsterCache 对象池系统演示 - 使用帮助");
            Console.WriteLine("=".PadRight(50, '='));
            Console.WriteLine();

            Console.WriteLine("🚀 快速开始:");
            Console.WriteLine("  CachePoolDemo.RunQuickDemo()    - 运行快速演示");
            Console.WriteLine("  CachePoolDemo.RunAllDemos()     - 运行完整演示");
            Console.WriteLine("  CachePoolDemo.InteractiveDemo() - 交互式演示");
            Console.WriteLine();

            Console.WriteLine("🎯 特定功能演示:");
            Console.WriteLine("  CachePoolDemo.DemoFeature(\"basic\")     - 基本操作");
            Console.WriteLine("  CachePoolDemo.DemoFeature(\"analysis\")  - 性能分析");
            Console.WriteLine("  CachePoolDemo.DemoFeature(\"debug\")     - 调试功能");
            Console.WriteLine("  CachePoolDemo.DemoFeature(\"monitor\")   - 实时监控");
            Console.WriteLine();

            Console.WriteLine("🔧 直接API使用:");
            Console.WriteLine("  // 基本使用");
            Console.WriteLine("  var obj = CachePoolMgr.Acquire<MyPoolableObject>();");
            Console.WriteLine("  CachePoolMgr.Release(obj);");
            Console.WriteLine();
            Console.WriteLine("  // 性能分析");
            Console.WriteLine("  var report = CachePoolMgr.AnalyzePool(typeof(MyObject));");
            Console.WriteLine("  var health = PoolDebugger.PerformHealthCheck();");
            Console.WriteLine();
            Console.WriteLine("  // 调试监控");
            Console.WriteLine("  PoolDebugger.EnableDebugLogging = true;");
            Console.WriteLine("  PoolDebugger.StartMonitoring();");
            Console.WriteLine();

            Console.WriteLine("📚 更多信息:");
            Console.WriteLine("  查看各个示例类的详细实现和注释");
            Console.WriteLine("  - ExampleObjects.cs  : 示例对象定义");
            Console.WriteLine("  - BasicUsageDemo.cs  : 基本使用演示");
            Console.WriteLine("  - PerformanceAnalysisDemo.cs : 性能分析演示");
            Console.WriteLine("  - DebugMonitorDemo.cs : 调试监控演示");
        }
    }
}