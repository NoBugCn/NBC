using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace NBC.Asset.Editor
{
    public class BuildContext
    {
        public List<BuildAsset> Assets = new List<BuildAsset>();
        public List<BuildBundle> Bundles = new List<BuildBundle>();
        public List<string> Packages = new List<string>();

        public void Add(GroupConfig groupConfig, List<BuildAsset> assets)
        {
            foreach (var asset in assets)
            {
                asset.Group = groupConfig;
                AddOrUpdate(asset);
            }
        }

        public void AddOrUpdate(BuildAsset asset)
        {
            Assets.Add(asset);
        }

        public void AddBundle(BuildBundle bundle)
        {
            Bundles.Add(bundle);
        }

        public BuildBundle GetBundle(string name)
        {
            return Bundles.FirstOrDefault(bundle => bundle.Name == name);
        }

        public List<BuildBundle> GenBundles()
        {
            Bundles.Clear();

            Dictionary<string, BuildBundle> dictionary = new Dictionary<string, BuildBundle>();
            foreach (var asset in Assets)
            {
                if (!dictionary.TryGetValue(asset.Bundle, out var bundle))
                {
                    bundle = new BuildBundle
                    {
                        Name = asset.Bundle,
                        Tags = asset.Tags
                    };
                    dictionary[bundle.Name] = bundle;
                }

                bundle.AddAsset(asset);
            }

            foreach (var bundle in dictionary.Values)
            {
                AddBundle(bundle);
            }

            return Bundles;
        }

        public void SaveAssets()
        {
            var cache = Caches.Get();
            cache.ClearAssets();
            cache.AddOrUpdate(Assets);
            cache.Save();
        }

        public void SaveBundles()
        {
            var cache = Caches.Get();
            cache.ClearBundles();
            cache.AddOrUpdate(Bundles);
            cache.Save();
        }

        public List<AssetBundleBuild> GetAssetBundleBuilds()
        {
            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

            for (int i = 0; i < Bundles.Count; i++)
            {
                var bundle = Bundles[i];
                var build = new AssetBundleBuild
                {
                    assetNames = bundle.GetAssetNames(),
                    assetBundleName = bundle.Name
                };
                builds.Add(build);
            }

            return builds;
        }
    }
}