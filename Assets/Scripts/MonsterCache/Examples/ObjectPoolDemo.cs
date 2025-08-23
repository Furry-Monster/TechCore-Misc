using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using MonsterCache.Runtime;

namespace MonsterCache.Examples
{
    public class ObjectPoolDemo : MonoBehaviour
    {
        [Header("UI References")] public Text performanceText;
        public Button acquireButton;
        public Button releaseButton;
        public Button expandButton;
        public Button shrinkButton;
        public Button clearButton;
        public Text logText;

        [Header("Test Settings")] public int acquireCount = 10;
        public int expandCount = 20;
        public int shrinkCount = 10;

        private StringBuilder logBuilder = new StringBuilder();
        private int activeObjects = 0;

        void Start()
        {
            SetupUI();
            StartCoroutine(UpdatePerformanceDisplay());
        }

        void SetupUI()
        {
            if (acquireButton) acquireButton.onClick.AddListener(AcquireObjects);
            if (releaseButton) releaseButton.onClick.AddListener(ReleaseObjects);
            if (expandButton) expandButton.onClick.AddListener(ExpandPool);
            if (shrinkButton) shrinkButton.onClick.AddListener(ShrinkPool);
            if (clearButton) clearButton.onClick.AddListener(ClearPools);

            Log("ObjectPool Demo 已启动");
        }

        void AcquireObjects()
        {
            Log($"获取 {acquireCount} 个 TestPoolableObject...");

            var startTime = Time.realtimeSinceStartup;

            for (int i = 0; i < acquireCount; i++)
            {
                var obj = ObjectPoolMgr.Acquire<TestPoolableObject>();
                obj.Initialize($"Object_{activeObjects++}");
            }

            var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;
            Log($"完成! 用时: {elapsed:F2}ms");
        }

        void ReleaseObjects()
        {
            Log("释放所有 TestPoolableObject...");

            var startTime = Time.realtimeSinceStartup;

            // 模拟释放操作 - 在实际使用中，你需要保存对象引用来释放它们
            for (int i = 0; i < Mathf.Min(10, activeObjects); i++)
            {
                var obj = new TestPoolableObject();
                obj.Initialize($"ReleaseTest_{i}");
                ObjectPoolMgr.Release(obj);
            }

            var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;
            Log($"释放完成! 用时: {elapsed:F2}ms");
        }

        void ExpandPool()
        {
            Log($"扩展 TestPoolableObject 池 {expandCount} 个对象...");

            var startTime = Time.realtimeSinceStartup;
            ObjectPoolMgr.Expand<TestPoolableObject>(expandCount);
            var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;

            Log($"扩展完成! 用时: {elapsed:F2}ms");
        }

        void ShrinkPool()
        {
            Log($"收缩 TestPoolableObject 池 {shrinkCount} 个对象...");

            var startTime = Time.realtimeSinceStartup;
            ObjectPoolMgr.Shrink<TestPoolableObject>(shrinkCount);
            var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;

            Log($"收缩完成! 用时: {elapsed:F2}ms");
        }

        void ClearPools()
        {
            Log("清空所有对象池...");

            var startTime = Time.realtimeSinceStartup;
            ObjectPoolMgr.Clear();
            var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;

            activeObjects = 0;
            Log($"清空完成! 用时: {elapsed:F2}ms");
        }

        IEnumerator UpdatePerformanceDisplay()
        {
            while (true)
            {
                if (performanceText)
                {
                    var report = ObjectPoolMgr.GeneratePerformanceReport();
                    performanceText.text = report;
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

        void Log(string message)
        {
            logBuilder.AppendLine($"[{Time.time:F1}s] {message}");

            if (logBuilder.Length > 2000)
            {
                logBuilder.Remove(0, logBuilder.Length - 1500);
            }

            if (logText)
            {
                logText.text = logBuilder.ToString();
            }

            Debug.Log($"[ObjectPoolDemo] {message}");
        }

        void OnGUI()
        {
            if (performanceText == null)
            {
                GUILayout.BeginArea(new Rect(10, 10, 400, 600));

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

                GUILayout.EndArea();
            }
        }
    }

    public class TestPoolableObject : IPoolable
    {
        public string Name { get; private set; }
        public float CreatedTime { get; private set; }

        public void Initialize(string name)
        {
            Name = name;
            CreatedTime = Time.realtimeSinceStartup;
        }

        public void OnReturnToPool()
        {
            Name = null;
            CreatedTime = 0f;
        }
    }
}