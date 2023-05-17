using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NBC.Asset.Editor
{
    // [CreateAssetMenu(menuName = "NB/Res/" + nameof(PackageConfig), fileName = nameof(PackageConfig))]
    [Serializable]
    public class PackageConfig
    {
        public string Name;
        [Header("是否启用")] public bool Enable = true;
        [Header("是否默认包")] public bool Default = true;
        [Header("打包选项")] public BuildAssetBundleOptions BundleOptions = BuildAssetBundleOptions.ChunkBasedCompression;
        [Header("资源组")] public List<GroupConfig> Groups = new List<GroupConfig>();
    }
}