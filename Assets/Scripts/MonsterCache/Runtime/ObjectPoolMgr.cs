using System;
using System.Collections.Generic;

namespace MonsterCache.Runtime
{
    public static class ObjectPoolMgr
    {
        private static readonly Dictionary<Type, ObjectPool> cachePoolDict = new();

        /// <summary>性能监控事件</summary>
        public static event PoolEventHandler OnPoolEvent;

        /// <summary>
        /// 当前管理的对象池数量
        /// </summary>
        public static int Count
        {
            get
            {
                lock (cachePoolDict)
                {
                    return cachePoolDict.Count;
                }
            }
        }

        /// <summary>
        /// 获取所有对象池的统计信息
        /// </summary>
        /// <returns>对象池信息数组</returns>
        public static ObjectPoolInfo[] GetAllPoolInfos()
        {
            var index = 0;
            ObjectPoolInfo[] result;

            lock (cachePoolDict)
            {
                result = new ObjectPoolInfo[cachePoolDict.Count];
                foreach (var cacheItem in cachePoolDict)
                {
                    result[index++] = new ObjectPoolInfo(cacheItem.Key, cacheItem.Value.UnusedPoolableCount,
                        cacheItem.Value.UsedPoolableCount, cacheItem.Value.AcquirePoolableCount,
                        cacheItem.Value.ReleasePoolableCount, cacheItem.Value.AddPoolableCount,
                        cacheItem.Value.RemovePoolableCount);
                }
            }

            return result;
        }

        /// <summary>
        /// 清空所有对象池
        /// </summary>
        public static void Clear()
        {
            lock (cachePoolDict)
            {
                // 逐个清空对象池
                foreach (var cachePool in cachePoolDict.Values)
                {
                    cachePool.ReleaseAll();
                }

                cachePoolDict.Clear();
            }
        }

        /// <summary>
        /// 从对象池获取一个对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <returns>对象实例</returns>
        public static T Acquire<T>() where T : class, IPoolable, new()
        {
            var cachePool = GetCache(typeof(T));
            return cachePool.Acquire<T>();
        }

        /// <summary>
        /// 从对象池获取一个对象
        /// </summary>
        /// <param name="cachedType">对象类型</param>
        /// <returns>对象实例</returns>
        public static IPoolable Acquire(Type cachedType)
        {
            var cachePool = GetCache(cachedType);
            return cachePool.Acquire();
        }

        /// <summary>
        /// 将对象归还到对象池
        /// </summary>
        /// <param name="poolable">要归还的对象实例</param>
        /// <exception cref="ArgumentNullException">对象实例为空</exception>
        public static void Release(IPoolable poolable)
        {
            if (poolable == null)
                throw new ArgumentNullException(nameof(poolable));

            var cachePool = GetCache(poolable.GetType());
            cachePool.Release(poolable);
        }

        /// <summary>
        /// 预创建指定数量的对象到对象池
        /// </summary>
        /// <param name="count">预创建数量</param>
        /// <typeparam name="T">对象类型</typeparam>
        public static void Expand<T>(int count) where T : class, IPoolable, new()
        {
            Expand(typeof(T), count);
        }

        /// <summary>
        /// 预创建指定数量的对象到对象池
        /// </summary>
        /// <param name="cachedType">对象类型</param>
        /// <param name="count">预创建数量</param>
        /// <exception cref="ArgumentOutOfRangeException">数量必须为正数</exception>
        public static void Expand(Type cachedType, int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero");

            var cachePool = GetCache(cachedType);
            cachePool.Expand(count);
        }

        /// <summary>
        /// 从对象池移除指定数量的空闲对象
        /// </summary>
        /// <param name="count">移除数量</param>
        /// <typeparam name="T">对象类型</typeparam>
        public static void Shrink<T>(int count) where T : class, IPoolable, new()
        {
            Shrink(typeof(T), count);
        }

        /// <summary>
        /// 从对象池移除指定数量的空闲对象
        /// </summary>
        /// <param name="cachedType">对象类型</param>
        /// <param name="count">移除数量</param>
        /// <exception cref="ArgumentOutOfRangeException">数量必须为正数</exception>
        public static void Shrink(Type cachedType, int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero");

            var cachePool = GetCache(cachedType);
            cachePool.Shrink(count);
        }

        /// <summary>
        /// 获取指定类型的对象池
        /// </summary>
        /// <param name="cachedType">对象类型</param>
        /// <returns>对象池实例</returns>
        /// <exception cref="ArgumentNullException">对象类型不能为空</exception>
        private static ObjectPool GetCache(Type cachedType)
        {
            if (cachedType == null)
                throw new ArgumentNullException(nameof(cachedType));

            ObjectPool objectPool;
            lock (cachePoolDict)
            {
                if (!cachePoolDict.TryGetValue(cachedType, out objectPool))
                {
                    objectPool = new ObjectPool(cachedType);
                    cachePoolDict.Add(cachedType, objectPool);
                    TriggerEvent(cachedType, "PoolCreated", null);
                }
            }

            return objectPool;
        }

        #region 性能分析和调试功能

        /// <summary>
        /// 分析指定类型对象池的性能
        /// </summary>
        /// <param name="poolType">对象池类型</param>
        /// <returns>分析报告</returns>
        public static PoolAnalysisReport AnalyzePool(Type poolType)
        {
            lock (cachePoolDict)
            {
                if (!cachePoolDict.TryGetValue(poolType, out var pool))
                    return new PoolAnalysisReport(poolType, default, default, new[]
                    {
                        new PoolIssue(PoolIssueType.LowUsage, 1, "对象池未被使用", "考虑移除或延迟初始化")
                    });

                var info = new ObjectPoolInfo(poolType, pool.UnusedPoolableCount, pool.UsedPoolableCount,
                    pool.AcquirePoolableCount, pool.ReleasePoolableCount, pool.AddPoolableCount,
                    pool.RemovePoolableCount);

                var metrics = CalculateMetrics(info);
                var issues = DetectIssues(info, metrics);

                return new PoolAnalysisReport(poolType, info, metrics, issues);
            }
        }

        /// <summary>
        /// 分析所有对象池的性能
        /// </summary>
        /// <returns>所有对象池的分析报告</returns>
        public static PoolAnalysisReport[] AnalyzeAllPools()
        {
            var reports = new List<PoolAnalysisReport>();

            lock (cachePoolDict)
            {
                foreach (var kvp in cachePoolDict)
                {
                    reports.Add(AnalyzePool(kvp.Key));
                }
            }

            return reports.ToArray();
        }

        /// <summary>
        /// 检查内存泄漏
        /// </summary>
        /// <returns>可能存在内存泄漏的对象池列表</returns>
        public static PoolAnalysisReport[] DetectMemoryLeaks()
        {
            var leaks = new List<PoolAnalysisReport>();
            var reports = AnalyzeAllPools();

            foreach (var report in reports)
            {
                if (report.Metrics.MemoryLeakRisk > 7.0f)
                {
                    leaks.Add(report);
                }
            }

            return leaks.ToArray();
        }

        /// <summary>
        /// 生成性能报告摘要
        /// </summary>
        /// <returns>格式化的性能报告字符串</returns>
        public static string GeneratePerformanceReport()
        {
            var reports = AnalyzeAllPools();
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("=== 对象池性能报告 ===");
            sb.AppendLine($"总池数量: {reports.Length}");
            sb.AppendLine();

            var totalIssues = 0;
            var highPriorityIssues = 0;

            foreach (var report in reports)
            {
                sb.AppendLine($"【{report.PoolType.Name}】");
                sb.AppendLine(
                    $"  空闲/使用/总计: {report.PoolInfo.UnusedPoolableCount}/{report.PoolInfo.UsedPoolableCount}/{report.PoolInfo.UnusedPoolableCount + report.PoolInfo.UsedPoolableCount}");
                sb.AppendLine(
                    $"  获取/归还: {report.PoolInfo.AcquirePoolableCount}/{report.PoolInfo.ReleasePoolableCount}");
                sb.AppendLine($"  效率: {report.Metrics.PoolEfficiency:P1}, 泄漏风险: {report.Metrics.MemoryLeakRisk:F1}/10");

                if (report.Issues.Length > 0)
                {
                    sb.AppendLine("  问题:");
                    foreach (var issue in report.Issues)
                    {
                        sb.AppendLine($"    [{issue.Severity}/10] {issue.Description}");
                        totalIssues++;
                        if (issue.Severity >= 7) highPriorityIssues++;
                    }
                }

                sb.AppendLine();
            }

            sb.AppendLine($"总问题数: {totalIssues} (高优先级: {highPriorityIssues})");

            return sb.ToString();
        }

        /// <summary>
        /// 自动应用优化建议
        /// </summary>
        /// <param name="applyHighPriorityOnly">是否只应用高优先级优化</param>
        /// <returns>应用的优化数量</returns>
        public static int AutoOptimize(bool applyHighPriorityOnly = true)
        {
            var reports = AnalyzeAllPools();
            var optimizationCount = 0;

            foreach (var report in reports)
            {
                foreach (var issue in report.Issues)
                {
                    if (applyHighPriorityOnly && issue.Severity < 7) continue;

                    switch (issue.IssueType)
                    {
                        case PoolIssueType.PoolTooSmall:
                            if (issue.Data.TryGetValue("RecommendedSize", out var size) && size is int recommendedSize)
                            {
                                var currentSize = report.PoolInfo.UnusedPoolableCount;
                                Expand(report.PoolType, recommendedSize - currentSize);
                                optimizationCount++;
                                TriggerEvent(report.PoolType, "AutoExpanded", new Dictionary<string, object>
                                {
                                    ["OldSize"] = currentSize,
                                    ["NewSize"] = recommendedSize
                                });
                            }

                            break;

                        case PoolIssueType.PoolTooLarge:
                            if (issue.Data.TryGetValue("ExcessSize", out var excess) && excess is int excessSize)
                            {
                                Shrink(report.PoolType, excessSize);
                                optimizationCount++;
                                TriggerEvent(report.PoolType, "AutoShrunk", new Dictionary<string, object>
                                {
                                    ["RemovedSize"] = excessSize
                                });
                            }

                            break;
                    }
                }
            }

            return optimizationCount;
        }

        /// <summary>
        /// 计算对象池性能指标
        /// </summary>
        private static PoolMetrics CalculateMetrics(ObjectPoolInfo info)
        {
            var totalOperations = info.AcquirePoolableCount + info.ReleasePoolableCount;
            if (totalOperations == 0)
            {
                return new PoolMetrics(0, 0, 0, 0, 5); // 默认推荐大小
            }

            // 内存泄漏风险 = |获取次数 - 归还次数| / 总操作次数 * 10
            var leakDiff = Math.Abs(info.AcquirePoolableCount - info.ReleasePoolableCount);
            var memoryLeakRisk = Math.Min(10f, (leakDiff / (float)totalOperations) * 20f);

            // 池效率 = 重用次数 / 总获取次数
            var reuseCount = info.AcquirePoolableCount - info.AddPoolableCount;
            var poolEfficiency = info.AcquirePoolableCount > 0 ? reuseCount / (float)info.AcquirePoolableCount : 0f;

            // 平均利用率
            var totalPoolSize = info.UnusedPoolableCount + info.UsedPoolableCount;
            var averageUtilization = totalPoolSize > 0 ? info.UsedPoolableCount / (float)totalPoolSize : 0f;

            // 创建vs复用比率
            var newVsReuseRatio = reuseCount > 0 ? info.AddPoolableCount / (float)reuseCount : float.MaxValue;

            // 建议池大小 = 当前使用量 * 1.2 + 5 (缓冲)
            var recommendedSize = Math.Max(5, (int)(info.UsedPoolableCount * 1.2f) + 5);

            return new PoolMetrics(memoryLeakRisk, poolEfficiency, averageUtilization, newVsReuseRatio,
                recommendedSize);
        }

        /// <summary>
        /// 检测对象池问题
        /// </summary>
        private static PoolIssue[] DetectIssues(ObjectPoolInfo info, PoolMetrics metrics)
        {
            var issues = new List<PoolIssue>();

            // 内存泄漏检测
            if (metrics.MemoryLeakRisk > 7.0f)
            {
                issues.Add(new PoolIssue(PoolIssueType.PotentialMemoryLeak,
                    (int)metrics.MemoryLeakRisk,
                    $"获取({info.AcquirePoolableCount})和归还({info.ReleasePoolableCount})次数相差过大",
                    "检查是否有对象未正确归还到池中"));
            }

            // 池容量检测
            if (metrics.PoolEfficiency < 0.3f && info.AddPoolableCount > 20)
            {
                issues.Add(new PoolIssue(PoolIssueType.PoolTooSmall, 8,
                    $"池效率过低({metrics.PoolEfficiency:P1})，频繁创建新对象",
                    $"建议扩容到 {metrics.RecommendedPoolSize}",
                    new Dictionary<string, object> { ["RecommendedSize"] = metrics.RecommendedPoolSize }));
            }

            if (metrics.AverageUtilization < 0.1f && info.UnusedPoolableCount > 50)
            {
                var excessSize = info.UnusedPoolableCount - metrics.RecommendedPoolSize;
                issues.Add(new PoolIssue(PoolIssueType.PoolTooLarge, 6,
                    $"池利用率过低({metrics.AverageUtilization:P1})，浪费内存",
                    $"建议缩减 {excessSize} 个对象",
                    new Dictionary<string, object> { ["ExcessSize"] = excessSize }));
            }

            // 频繁分配检测
            if (metrics.NewVsReuseRatio > 0.5f)
            {
                issues.Add(new PoolIssue(PoolIssueType.FrequentAllocation, 7,
                    "新建对象比例过高，影响性能",
                    "考虑预热对象池或增加池容量"));
            }

            // 低使用率检测
            if (info is { AcquirePoolableCount: < 10, UnusedPoolableCount: 0 })
            {
                issues.Add(new PoolIssue(PoolIssueType.LowUsage, 3,
                    "对象池使用率极低",
                    "考虑移除或延迟初始化"));
            }

            return issues.ToArray();
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        private static void TriggerEvent(Type poolType, string eventType, Dictionary<string, object> data)
        {
            OnPoolEvent?.Invoke(poolType, new PoolEventArgs(eventType, data));
        }

        #endregion
    }
}