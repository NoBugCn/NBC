using System.Collections.Generic;
using System.IO;
using UnityEditor;
using Object = UnityEngine.Object;

namespace NBC.Asset.Editor
{
    [Bind(BundleMode.WithoutSub)]
    public class GatherWithoutSub : GatherBase
    {
        protected override BuildAsset[] Execute()
        {
            List<BuildAsset> ret = new List<BuildAsset>();
            var list = GroupConfig.Collectors;
            foreach (var obj in list)
            {
                var assets = GetAssets(obj, GroupConfig.Filter);
                if (assets != null && assets.Count > 0)
                {
                    assets = WithoutSub(obj, assets);
                    var bundleName = AssetDatabase.GetAssetPath(obj);
                    foreach (var asset in assets)
                    {
                        asset.Bundle = GetBundleName(asset, bundleName);
                        ret.Add(asset);
                    }
                }
            }

            return ret.ToArray();
        }

        private List<BuildAsset> WithoutSub(Object obj, List<BuildAsset> assets)
        {
            List<BuildAsset> ret = new List<BuildAsset>();
            var path = AssetDatabase.GetAssetPath(obj);
            if (Directory.Exists(path))
            {
                foreach (var asset in assets)
                {
                    var childPath = asset.Path;
                    var dirName = Path.GetDirectoryName(childPath)?.Replace("\\", "/");
                    if (dirName == path)
                    {
                        ret.Add(asset);
                    }
                }
            }
            else
            {
                ret.AddRange(assets);
            }

            return ret;
        }
    }
}