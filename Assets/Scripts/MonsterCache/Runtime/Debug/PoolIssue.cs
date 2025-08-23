using System.Collections.Generic;

namespace MonsterCache.Runtime.Debug
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
}