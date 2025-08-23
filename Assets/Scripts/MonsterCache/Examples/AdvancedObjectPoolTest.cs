using System;
using System.Collections.Generic;
using MonsterCache.Runtime;
using MonsterCache.Runtime.Debug;
using UnityEngine;

namespace MonsterCache.Examples
{
    public class AdvancedObjectPoolTest : MonoBehaviour
    {
        [Header("Test Settings")] [SerializeField]
        public int acquireCount = 10;

        [SerializeField] public int expandCount = 20;
        [SerializeField] public int shrinkCount = 10;
        [SerializeField] public int logCount = 200;

        private readonly List<TestPoolableObject> objects = new();
        private int activeObjects;
        private int LogCnt;

        private void Start()
        {
            PoolDebugger.EnableDebugLogging = true;
        }

        private void AcquireObjects()
        {
            Log($"获取 {acquireCount} 个 TestPoolableObject...");

            var startTime = Time.realtimeSinceStartup;

            for (var i = 0; i < acquireCount; i++)
            {
                var obj = ObjectPoolMgr.Acquire<TestPoolableObject>();
                obj.Initialize($"Object_{activeObjects++}");
                objects.Add(obj);
            }

            var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;
            Log($"完成! 用时: {elapsed:F2}ms");
        }

        private void ReleaseObjects()
        {
            Log("释放所有 TestPoolableObject...");

            var startTime = Time.realtimeSinceStartup;

            for (var i = 0; i < Mathf.Min(10, activeObjects); i++)
            {
                if (objects.Count == 0)
                {
                    Log($"释放失败！还没有引用任何池中对象！");
                }

                var obj = objects[0];
                objects.RemoveAt(0);
                ObjectPoolMgr.Release(obj);
            }

            var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;
            Log($"释放完成! 用时: {elapsed:F2}ms");
        }

        private void ExpandPool()
        {
            Log($"扩展 TestPoolableObject 池 {expandCount} 个对象...");

            var startTime = Time.realtimeSinceStartup;
            ObjectPoolMgr.Expand<TestPoolableObject>(expandCount);
            var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;

            Log($"扩展完成! 用时: {elapsed:F2}ms");
        }

        private void ShrinkPool()
        {
            Log($"收缩 TestPoolableObject 池 {shrinkCount} 个对象...");

            var startTime = Time.realtimeSinceStartup;
            ObjectPoolMgr.Shrink<TestPoolableObject>(shrinkCount);
            var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;

            Log($"收缩完成! 用时: {elapsed:F2}ms");
        }

        private void ClearPools()
        {
            Log("清空所有对象池...");

            var startTime = Time.realtimeSinceStartup;
            ObjectPoolMgr.Clear();
            var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;

            activeObjects = 0;
            Log($"清空完成! 用时: {elapsed:F2}ms");
        }

        private static void Log(string message)
        {
            PoolDebugger.Log(message);
        }

        private void LogToUnity()
        {
            var logs = PoolDebugger.GetDebugLog(logCount);
            Debug.Log(logs);
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(610, 10, 400, 600));

            GUILayout.Label("ObjectPool 性能监控", GUI.skin.label);
            GUILayout.Space(10);

            var report = ObjectPoolMgr.GeneratePerformanceReport();
            GUILayout.Label(report, GUI.skin.textArea);

            GUILayout.Space(10);

            if (GUILayout.Button($"获取 {acquireCount} 个对象")) AcquireObjects();
            if (GUILayout.Button("释放对象")) ReleaseObjects();
            if (GUILayout.Button($"扩展池 {expandCount}")) ExpandPool();
            if (GUILayout.Button($"收缩池 {shrinkCount}")) ShrinkPool();
            if (GUILayout.Button("清空所有池")) ClearPools();
            if (GUILayout.Button($"查看{logCount}条日志")) LogToUnity();

            GUILayout.EndArea();
        }
    }
}