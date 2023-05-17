using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NBC.Asset.Editor
{
    [FilePath("Assets/CollectorSetting.asset")]
    public class CollectorSetting : ScriptableSingleton<CollectorSetting>
    {
        /// <summary>
        /// 资源包
        /// </summary>
        public List<PackageConfig> Packages = new List<PackageConfig>();

        public void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(CollectorSetting.Instance);
            AssetDatabase.SaveAssets();
#endif
        }
    }
}