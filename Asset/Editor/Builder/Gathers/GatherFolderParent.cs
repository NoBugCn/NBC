using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NBC.Asset.Editor
{
    [Bind(BundleMode.FolderParent)]
    public class GatherFolderParent : GatherBase
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
    }
}