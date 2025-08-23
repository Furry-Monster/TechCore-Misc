using System;
using System.Collections.Generic;

namespace MonsterCache.Runtime.Debug
{
    /// <summary>
    /// 对象池事件监听器
    /// </summary>
    public delegate void PoolEventHandler(Type poolType, PoolEventArgs args);

    /// <summary>
    /// 对象池事件参数
    /// </summary>
    public class PoolEventArgs : EventArgs
    {
        /// <summary>
        /// 事件类型
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// 事件数据
        /// </summary>
        public Dictionary<string, object> Data { get; set; }

        public PoolEventArgs(string eventType, Dictionary<string, object> data = null)
        {
            EventType = eventType;
            Data = data ?? new Dictionary<string, object>();
        }
    }
}