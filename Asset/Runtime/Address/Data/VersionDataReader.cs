using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace NBC.Asset
{
    internal class VersionDataReader
    {
        private readonly Dictionary<string, BundleData> _bundles = new Dictionary<string, BundleData>();

        private readonly Dictionary<string, AssetData> _assets = new Dictionary<string, AssetData>();
        private readonly Dictionary<string, string> _addressablePath = new Dictionary<string, string>();
        public VersionData VersionData { get; private set; }

        public List<PackageData> PackageDataList { get; private set; } = new List<PackageData>();

        public VersionDataReader()
        {
            ReadVersionData();
        }

        public string GetAssetRealPath(string path)
        {
            return _addressablePath.TryGetValue(path, out var realPath) ? realPath : path;
        }

        public AssetData GetAsset(string path)
        {
            return _assets.TryGetValue(path, out var assetRef) ? assetRef : null;
        }

        public List<BundleData> GetAllBundle()
        {
            List<BundleData> list = new List<BundleData>(_bundles.Count);
            list.AddRange(_bundles.Values);
            return list;
        }

        public BundleData GetBundle(string name)
        {
            return _bundles.TryGetValue(name, out var bundleRef) ? bundleRef : null;
        }

        public BundleData GetBundleByAsset(string assetPath)
        {
            var asset = GetAsset(assetPath);
            return asset == null ? null : GetBundle(asset.BundleName);
        }

        /// <summary>
        /// 获取资源所需的所有bundle信息（下标0为所在包）
        /// </summary>
        /// <param name="path">资源原始地址</param>
        /// <returns></returns>
        public BundleData[] GetAllDependBundle(string path)
        {
            if (_assets.TryGetValue(path, out var assetData))
            {
                if (_bundles.TryGetValue(assetData.BundleName, out var bundleData))
                {
                    var needBundle = new List<BundleData> { bundleData };
                    needBundle.AddRange(bundleData.DependBundles.Select(bundle => _bundles[bundle]));
                    return needBundle.ToArray();
                }
            }

            return Array.Empty<BundleData>();
        }

        private void ReadVersionData()
        {
            VersionData = ReadJson<VersionData>(Const.VersionFileName);
            if (VersionData != null)
            {
                var packageData =
                    ReadJson<VersionPackageData>(VersionData.NameHash);
                if (packageData != null)
                {
                    foreach (var package in packageData.Packages)
                    {
                        ReadPackage(package);
                    }
                }
            }
            else
            {
                Debug.LogError("version data is null");
            }
        }

        private void ReadPackage(PackageData packageData)
        {
            if (packageData != null)
            {
                foreach (var bundle in packageData.Bundles)
                {
                    foreach (var dep in bundle.Deps)
                    {
                        var depBundle = packageData.Bundles[dep];
                        if (depBundle != null)
                        {
                            bundle.DependBundles.Add(depBundle.Name);
                        }
                    }

                    bundle.PackageName = packageData.Name;
                    _bundles[bundle.Name] = bundle;
                }

                foreach (var asset in packageData.Assets)
                {
                    if (asset.Dir < 0 || asset.Dir >= packageData.Dirs.Count) continue;
                    if (asset.Bundle < 0 || asset.Bundle >= packageData.Bundles.Count) continue;
                    var dir = packageData.Dirs[asset.Dir];
                    var bundle = packageData.Bundles[asset.Bundle];
                    asset.Path = $"{dir}/{asset.Name}";
                    asset.BundleName = bundle.Name;
                    _assets[asset.Path] = asset;
                    var filePath = $"{dir}/{Path.GetFileNameWithoutExtension(asset.Name)}";
                    //去除后缀后，默认加入寻址
                    _addressablePath[filePath] = asset.Path;
                    if (asset.Address != asset.Path)
                    {
                        _addressablePath[asset.Address] = asset.Path;
                    }
                }

                PackageDataList.Add(packageData);
            }
        }

        private T ReadJson<T>(string fileName) where T : new()
        {
            return Util.ReadJson<T>(Const.GetCachePath(fileName));
        }
    }
}