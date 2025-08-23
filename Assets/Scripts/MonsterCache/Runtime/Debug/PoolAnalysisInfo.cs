using System;

namespace MonsterCache.Runtime.Debug
{
    /// <summary>
    /// 对象池分析报告
    /// </summary>
    public readonly struct PoolAnalysisReport
    {
        /// <summary>对象池类型</summary>
        public readonly Type PoolType;

        /// <summary>基础统计信息</summary>
        public readonly ObjectPoolInfo PoolInfo;

        /// <summary>计算出的性能指标</summary>
        public readonly PoolMetrics Metrics;

        /// <summary>发现的问题列表</summary>
        public readonly PoolIssue[] Issues;

        public PoolAnalysisReport(Type poolType, ObjectPoolInfo poolInfo, PoolMetrics metrics, PoolIssue[] issues)
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
        public readonly float NewVersusReuseRatio;

        /// <summary>建议的池大小</summary>
        public readonly int RecommendedPoolSize;

        public PoolMetrics(float memoryLeakRisk, float poolEfficiency, float averageUtilization,
            float newVersusReuseRatio, int recommendedPoolSize)
        {
            MemoryLeakRisk = memoryLeakRisk;
            PoolEfficiency = poolEfficiency;
            AverageUtilization = averageUtilization;
            NewVersusReuseRatio = newVersusReuseRatio;
            RecommendedPoolSize = recommendedPoolSize;
        }
    }
}