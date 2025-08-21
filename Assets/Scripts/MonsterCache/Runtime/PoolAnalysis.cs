using System;
using System.Collections.Generic;

namespace MonsterCache.Runtime
{
    /// <summary>
    /// 对象池性能问题类型
    /// </summary>
    public enum PoolIssueType
    {
        /// <summary>可能存在内存泄漏</summary>
        PotentialMemoryLeak,

        /// <summary>池容量过小，需要扩容</summary>
        PoolTooSmall,

        /// <summary>池容量过大，浪费内存</summary>
        PoolTooLarge,

        /// <summary>频繁创建对象，影响性能</summary>
        FrequentAllocation,

        /// <summary>使用率极低，可以考虑移除</summary>
        LowUsage
    }

    /// <summary>
    /// 对象池问题报告
    /// </summary>
    public readonly struct PoolIssue
    {
        /// <summary>问题类型</summary>
        public readonly PoolIssueType IssueType;

        /// <summary>问题严重程度 (1-10)</summary>
        public readonly int Severity;

        /// <summary>问题描述</summary>
        public readonly string Description;

        /// <summary>优化建议</summary>
        public readonly string Suggestion;

        /// <summary>相关数据</summary>
        public readonly Dictionary<string, object> Data;

        public PoolIssue(PoolIssueType issueType, int severity, string description, string suggestion,
            Dictionary<string, object> data = null)
        {
            IssueType = issueType;
            Severity = severity;
            Description = description;
            Suggestion = suggestion;
            Data = data ?? new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// 对象池分析报告
    /// </summary>
    public readonly struct PoolAnalysisReport
    {
        /// <summary>对象池类型</summary>
        public readonly Type PoolType;

        /// <summary>基础统计信息</summary>
        public readonly CachePoolInfo PoolInfo;

        /// <summary>计算出的性能指标</summary>
        public readonly PoolMetrics Metrics;

        /// <summary>发现的问题列表</summary>
        public readonly PoolIssue[] Issues;

        public PoolAnalysisReport(Type poolType, CachePoolInfo poolInfo, PoolMetrics metrics, PoolIssue[] issues)
        {
            PoolType = poolType;
            PoolInfo = poolInfo;
            Metrics = metrics;
            Issues = issues ?? Array.Empty<PoolIssue>();
        }
    }

    /// <summary>
    /// 对象池性能指标
    /// </summary>
    public readonly struct PoolMetrics
    {
        /// <summary>内存泄漏风险评分 (0-10)</summary>
        public readonly float MemoryLeakRisk;

        /// <summary>池效率 (0-1)</summary>
        public readonly float PoolEfficiency;

        /// <summary>平均池利用率 (0-1)</summary>
        public readonly float AverageUtilization;

        /// <summary>创建vs复用比率</summary>
        public readonly float NewVsReuseRatio;

        /// <summary>建议的池大小</summary>
        public readonly int RecommendedPoolSize;

        public PoolMetrics(float memoryLeakRisk, float poolEfficiency, float averageUtilization,
            float newVsReuseRatio, int recommendedPoolSize)
        {
            MemoryLeakRisk = memoryLeakRisk;
            PoolEfficiency = poolEfficiency;
            AverageUtilization = averageUtilization;
            NewVsReuseRatio = newVsReuseRatio;
            RecommendedPoolSize = recommendedPoolSize;
        }
    }

    /// <summary>
    /// 对象池事件监听器
    /// </summary>
    public delegate void PoolEventHandler(Type poolType, PoolEventArgs args);

    /// <summary>
    /// 对象池事件参数
    /// </summary>
    public class PoolEventArgs : EventArgs
    {
        /// <summary>事件类型</summary>
        public string EventType { get; set; }

        /// <summary>事件数据</summary>
        public Dictionary<string, object> Data { get; set; }

        public PoolEventArgs(string eventType, Dictionary<string, object> data = null)
        {
            EventType = eventType;
            Data = data ?? new Dictionary<string, object>();
        }
    }
}