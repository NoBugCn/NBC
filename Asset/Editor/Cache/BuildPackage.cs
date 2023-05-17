using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NBC.Asset.Editor
{
    [Serializable]
    public class BuildPackage
    {
        public string Name;
        public int Size;
        public int Def;
        public List<BuildBundle> Bundles = new List<BuildBundle>();

        [NonSerialized] public List<BuildAsset> Assets = new List<BuildAsset>();

        public int GetBundleIndex(string name)
        {
            return Bundles.FindIndex(p => p.Name == name);
        }

        public int GetAssetIndex(string asset)
        {
            return Assets.FindIndex(p => p.Path == asset);
        }

        public PackageData ToPackagesData()
        {
            PackageData packageData = new PackageData
            {
                Name = Name,
                Def = Def,
                Bundles = new List<BundleData>(),
                Assets = new List<AssetData>()
            };

            foreach (var bundle in Bundles)
            {
                var b = new BundleData
                {
                    Name = bundle.Name,
                    Hash = bundle.Hash,
                    Size = bundle.Size,
                    Tags = EditUtil.GetTagsArr(bundle.Tags),
                };
                if (bundle.Dependencies != null && bundle.Dependencies.Length > 0)
                {
                    foreach (var dependency in bundle.Dependencies)
                    {
                        b.Deps.Add(GetBundleIndex(dependency));
                    }
                }

                Assets.AddRange(bundle.Assets);
                packageData.Bundles.Add(b);
            }

            HashSet<string> dirs = new HashSet<string>();
            foreach (var asset in Assets)
            {
                var dir = Path.GetDirectoryName(asset.Path)?.Replace("\\", "/");
                if (!string.IsNullOrEmpty(dir))
                {
                    dirs.Add(dir);
                }
            }

            packageData.Dirs = dirs.ToList();

            foreach (var asset in Assets)
            {
                var dir = Path.GetDirectoryName(asset.Path)?.Replace("\\", "/");
                var a = new AssetData
                {
                    Name = Path.GetFileName(asset.Path),
                    Dir = packageData.Dirs.FindIndex(b => b == dir),
                    Address = asset.Address,
                    // Tags = EditUtil.GetTagsArr(asset.Tags),
                    Bundle = GetBundleIndex(asset.Bundle)
                };
                if (asset.Dependencies != null && asset.Dependencies.Length > 0)
                {
                    foreach (var dependency in asset.Dependencies)
                    {
                        var index = GetAssetIndex(dependency);
                        if (index >= 0)
                            a.Deps.Add(index);
                    }
                }

                packageData.Assets.Add(a);
            }


            return packageData;
        }
    }
}