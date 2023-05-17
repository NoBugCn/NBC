using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace NBC.Asset.Editor
{
    [Bind(BundleMode.Folder)]
    public class GatherFolder : GatherBase
    {
        protected override BuildAsset[] Execute()
        {
            List<BuildAsset> assets = GetAssets();
            var singleAssetPaths = GetSingleAssetPaths();
            Dictionary<string, List<BuildAsset>> dictionary = new Dictionary<string, List<BuildAsset>>();
            foreach (var asset in assets)
            {
                var dirPath = Path.GetDirectoryName(asset.Path);
                if (singleAssetPaths.Contains(asset.Path)) dirPath = asset.Path;

                if (string.IsNullOrEmpty(dirPath)) continue;
                if (!dictionary.TryGetValue(dirPath, out var list))
                {
                    list = new List<BuildAsset>();
                    dictionary[dirPath] = list;
                }

                if (!list.Contains(asset)) list.Add(asset);
            }

            List<BuildAsset> ret = new List<BuildAsset>();

            foreach (var key in dictionary.Keys)
            {
                var value = dictionary[key];
                foreach (var asset in value)
                {
                    asset.Bundle = GetBundleName(asset, key);
                    ret.Add(asset);
                }
            }

            return ret.ToArray();
        }
        
        private List<string> GetSingleAssetPaths()
        {
            List<string> list = new List<string>();
            foreach (var obj in GroupConfig.Collectors)
            {
                var path = AssetDatabase.GetAssetPath(obj);
                if (!Directory.Exists(path))
                {
                    list.Add(path);
                }
            }

            return list;
        }
    }
}