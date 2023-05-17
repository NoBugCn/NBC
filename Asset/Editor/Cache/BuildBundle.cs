using System;
using System.Collections.Generic;
using System.Linq;

namespace NBC.Asset.Editor
{
    [Serializable]
    public class BuildBundle
    {
        public string Name;
        public string Hash;
        public int Size;
        public List<BuildAsset> Assets = new List<BuildAsset>();
        public string[] Dependencies;

        /// <summary>
        /// 资源包标签
        /// </summary>
        public string Tags;

        public void AddAsset(ICollection<BuildAsset> assets)
        {
            foreach (var asset in assets)
            {
                AddAsset(asset);
            }
        }

        public void AddAsset(BuildAsset asset)
        {
            asset.Bundle = Name;
            Assets.Add(asset);
        }

        public string[] GetAssetNames()
        {
            HashSet<string> assetNames = new HashSet<string>();
            foreach (var asset in Assets)
            {
                assetNames.Add(asset.Path);
            }

            return assetNames.ToArray();
        }
    }
}