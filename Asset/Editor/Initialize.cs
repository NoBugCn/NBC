using UnityEditor;
using UnityEngine;

namespace NBC.Asset.Editor
{
    public static class Initialize
    {
        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitializeOnLoad()
        {
            //调用一次设置和收集，初始化
            var collectorSetting = CollectorSetting.Instance;
            var buildSettings = BuildSettings.Instance;

            Const.Simulate = buildSettings.Simulate;
        }

        [InitializeOnLoadMethod]
        private static void InitializeOnLoadMethod()
        {
            
        }
    }
}