using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonsterCache.Runtime
{
    /// <summary>
    /// 对象池调试器，提供友好的调试和监控功能
    /// </summary>
    public static class PoolDebugger
    {
        private static readonly List<string> _debugLog = new();
        private static bool _enableDebugLogging;

        /// <summary>
        /// 是否启用调试日志记录
        /// </summary>
        public static bool EnableDebugLogging
        {
            get => _enableDebugLogging;
            set => _enableDebugLogging = value;
        }

        /// <summary>
        /// 获取调试日志
        /// </summary>
        /// <param name="maxLines">最大行数，-1表示获取所有</param>
        /// <returns>调试日志数组</returns>
        public static string[] GetDebugLog(int maxLines = 100)
        {
            lock (_debugLog)
            {
                if (maxLines == -1 || maxLines >= _debugLog.Count)
                    return _debugLog.ToArray();

                return _debugLog.Skip(_debugLog.Count - maxLines).ToArray();
            }
        }

        /// <summary>
        /// 清空调试日志
        /// </summary>
        public static void ClearDebugLog()
        {
            lock (_debugLog)
            {
                _debugLog.Clear();
            }
        }

        /// <summary>
        /// 记录调试信息
        /// </summary>
        /// <param name="message">调试消息</param>
        public static void Log(string message)
        {
            if (!_enableDebugLogging) return;

            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] {message}";

            lock (_debugLog)
            {
                _debugLog.Add(logEntry);
                // 限制日志数量，避免内存泄漏
                if (_debugLog.Count > 1000)
                {
                    _debugLog.RemoveRange(0, 200); // 删除前200条记录
                }
            }
        }

        /// <summary>
        /// 生成详细的对象池状态报告
        /// </summary>
        /// <param name="includeEmptyPools">是否包含空的对象池</param>
        /// <returns>详细状态报告</returns>
        public static string GenerateDetailedReport(bool includeEmptyPools = false)
        {
            var reports = CachePoolMgr.AnalyzeAllPools();
            var sb = new StringBuilder();

            sb.AppendLine("═══════════════════════════════════════════════");
            sb.AppendLine("              对象池详细状态报告");
            sb.AppendLine("═══════════════════════════════════════════════");
            sb.AppendLine($"报告生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"对象池总数: {reports.Length}");
            sb.AppendLine();

            // 统计概览
            var totalUsed = reports.Sum(r => r.PoolInfo.UsedPoolableCount);
            var totalUnused = reports.Sum(r => r.PoolInfo.UnusedPoolableCount);
            var totalAcquires = reports.Sum(r => r.PoolInfo.AcquirePoolableCount);
            var totalReleases = reports.Sum(r => r.PoolInfo.ReleasePoolableCount);
            var avgEfficiency = reports.Where(r => r.PoolInfo.AcquirePoolableCount > 0)
                .Average(r => r.Metrics.PoolEfficiency);

            sb.AppendLine("【系统概览】");
            sb.AppendLine($"  总对象数: {totalUsed + totalUnused} (使用中: {totalUsed}, 空闲: {totalUnused})");
            sb.AppendLine($"  总操作数: {totalAcquires + totalReleases} (获取: {totalAcquires}, 归还: {totalReleases})");
            sb.AppendLine($"  平均效率: {avgEfficiency:P2}");

            // 问题统计
            var allIssues = reports.SelectMany(r => r.Issues).ToArray();
            var highPriorityIssues = allIssues.Where(i => i.Severity >= 7).ToArray();
            sb.AppendLine($"  发现问题: {allIssues.Length} 个 (高优先级: {highPriorityIssues.Length} 个)");
            sb.AppendLine();

            // 按效率排序显示各个池
            var sortedReports = reports.OrderByDescending(r => r.Metrics.PoolEfficiency).ToArray();

            foreach (var report in sortedReports)
            {
                // 跳过空池（如果设置了不包含）
                if (!includeEmptyPools && report.PoolInfo.AcquirePoolableCount == 0 &&
                    report.PoolInfo.UnusedPoolableCount == 0 && report.PoolInfo.UsedPoolableCount == 0)
                    continue;

                sb.AppendLine($"┌─ {report.PoolType.Name} ─────────────────────");
                sb.AppendLine(
                    $"│ 状态: 空闲:{report.PoolInfo.UnusedPoolableCount} | 使用:{report.PoolInfo.UsedPoolableCount} | 总计:{report.PoolInfo.UnusedPoolableCount + report.PoolInfo.UsedPoolableCount}");
                sb.AppendLine(
                    $"│ 统计: 获取:{report.PoolInfo.AcquirePoolableCount} | 归还:{report.PoolInfo.ReleasePoolableCount} | 创建:{report.PoolInfo.AddPoolableCount} | 销毁:{report.PoolInfo.RemovePoolableCount}");

                // 性能指标
                var metrics = report.Metrics;
                sb.AppendLine(
                    $"│ 性能: 效率:{metrics.PoolEfficiency:P1} | 利用率:{metrics.AverageUtilization:P1} | 泄漏风险:{metrics.MemoryLeakRisk:F1}/10");
                sb.AppendLine($"│ 建议: 推荐池大小 {metrics.RecommendedPoolSize}");

                // 问题列表
                if (report.Issues.Length > 0)
                {
                    sb.AppendLine("│ 问题:");
                    foreach (var issue in report.Issues.OrderByDescending(i => i.Severity))
                    {
                        var severityIcon = GetSeverityIcon(issue.Severity);
                        sb.AppendLine($"│   {severityIcon} [{issue.Severity}/10] {issue.Description}");
                        sb.AppendLine($"│      建议: {issue.Suggestion}");
                    }
                }

                sb.AppendLine("└─────────────────────────────────────────────");
                sb.AppendLine();
            }

            // 推荐操作
            if (highPriorityIssues.Length > 0)
            {
                sb.AppendLine("【紧急建议】");
                foreach (var issue in highPriorityIssues.Take(5))
                {
                    sb.AppendLine($"  🚨 {issue.Description} - {issue.Suggestion}");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// 生成简化的控制台报告
        /// </summary>
        /// <returns>简化报告</returns>
        public static string GenerateConsoleReport()
        {
            var reports = CachePoolMgr.AnalyzeAllPools()
                .Where(r => r.PoolInfo.AcquirePoolableCount > 0 || r.PoolInfo.UsedPoolableCount > 0)
                .OrderByDescending(r => r.PoolInfo.AcquirePoolableCount)
                .ToArray();

            if (reports.Length == 0)
                return "暂无活跃的对象池";

            var sb = new StringBuilder();
            sb.AppendLine("对象池状态:");
            sb.AppendLine("类型名称".PadRight(20) + "空闲".PadLeft(6) + "使用".PadLeft(6) + "效率".PadLeft(8) + "问题".PadLeft(6));
            sb.AppendLine(new string('-', 46));

            foreach (var report in reports)
            {
                var typeName = report.PoolType.Name;
                if (typeName.Length > 19) typeName = typeName.Substring(0, 16) + "...";

                var problemCount = report.Issues.Where(i => i.Severity >= 7).Count();
                var problemIcon = problemCount > 0 ? $"{problemCount}⚠" : "✓";

                sb.AppendLine($"{typeName.PadRight(20)}" +
                              $"{report.PoolInfo.UnusedPoolableCount.ToString().PadLeft(6)}" +
                              $"{report.PoolInfo.UsedPoolableCount.ToString().PadLeft(6)}" +
                              $"{report.Metrics.PoolEfficiency.ToString("P0").PadLeft(8)}" +
                              $"{problemIcon.PadLeft(6)}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 监控对象池并在出现问题时记录
        /// </summary>
        public static void StartMonitoring()
        {
            if (_enableDebugLogging)
            {
                // 订阅池事件
                CachePoolMgr.OnPoolEvent += OnPoolEvent;
                Log("对象池监控已启动");
            }
        }

        /// <summary>
        /// 停止监控
        /// </summary>
        public static void StopMonitoring()
        {
            CachePoolMgr.OnPoolEvent -= OnPoolEvent;
            Log("对象池监控已停止");
        }

        /// <summary>
        /// 执行健康检查并返回结果
        /// </summary>
        /// <returns>健康检查结果</returns>
        public static PoolHealthCheckResult PerformHealthCheck()
        {
            var reports = CachePoolMgr.AnalyzeAllPools();
            var memoryLeaks = CachePoolMgr.DetectMemoryLeaks();

            var result = new PoolHealthCheckResult
            {
                TotalPools = reports.Length,
                HealthyPools = reports.Count(r => r.Issues.Length == 0),
                ProblematicPools = reports.Count(r => r.Issues.Any(i => i.Severity >= 7)),
                MemoryLeakSuspects = memoryLeaks.Length,
                OverallHealth = CalculateOverallHealth(reports),
                Recommendations = GenerateRecommendations(reports)
            };

            Log($"健康检查完成 - 总体健康度: {result.OverallHealth}/100");
            return result;
        }

        private static void OnPoolEvent(Type poolType, PoolEventArgs args)
        {
            Log(
                $"[{poolType.Name}] {args.EventType}: {string.Join(", ", args.Data.Select(kv => $"{kv.Key}={kv.Value}"))}");
        }

        private static string GetSeverityIcon(int severity)
        {
            return severity switch
            {
                >= 9 => "🔴",
                >= 7 => "🟡",
                >= 5 => "🟠",
                _ => "🔵"
            };
        }

        private static int CalculateOverallHealth(PoolAnalysisReport[] reports)
        {
            if (reports.Length == 0) return 100;

            var totalScore = 0;
            foreach (var report in reports)
            {
                var poolScore = 100;
                foreach (var issue in report.Issues)
                {
                    poolScore -= issue.Severity;
                }

                totalScore += Math.Max(0, poolScore);
            }

            return totalScore / reports.Length;
        }

        private static string[] GenerateRecommendations(PoolAnalysisReport[] reports)
        {
            var recommendations = new List<string>();
            var highPriorityIssues = reports.SelectMany(r => r.Issues)
                .Where(i => i.Severity >= 7)
                .GroupBy(i => i.IssueType)
                .ToArray();

            foreach (var group in highPriorityIssues)
            {
                switch (group.Key)
                {
                    case PoolIssueType.PotentialMemoryLeak:
                        recommendations.Add($"发现 {group.Count()} 个可能的内存泄漏，请检查对象归还逻辑");
                        break;
                    case PoolIssueType.PoolTooSmall:
                        recommendations.Add($"{group.Count()} 个对象池过小，建议扩容以提高性能");
                        break;
                    case PoolIssueType.FrequentAllocation:
                        recommendations.Add($"{group.Count()} 个对象池频繁创建对象，考虑预热或扩容");
                        break;
                }
            }

            if (recommendations.Count == 0)
                recommendations.Add("所有对象池运行良好！");

            return recommendations.ToArray();
        }
    }

    /// <summary>
    /// 对象池健康检查结果
    /// </summary>
    public struct PoolHealthCheckResult
    {
        /// <summary>总对象池数量</summary>
        public int TotalPools { get; set; }

        /// <summary>健康的对象池数量</summary>
        public int HealthyPools { get; set; }

        /// <summary>有问题的对象池数量</summary>
        public int ProblematicPools { get; set; }

        /// <summary>可能内存泄漏的对象池数量</summary>
        public int MemoryLeakSuspects { get; set; }

        /// <summary>总体健康度 (0-100)</summary>
        public int OverallHealth { get; set; }

        /// <summary>建议列表</summary>
        public string[] Recommendations { get; set; }
    }
}