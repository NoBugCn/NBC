using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace NBC.Asset.Editor
{
    /// <summary>
    /// 开发期寻址管理
    /// </summary>
    public class AddressableEditImpl : IAddressableImpl
    {
        public static IAddressableImpl CreateInstance()
        {
            return new AddressableEditImpl();
        }

        private readonly Dictionary<string, BundleData> _bundles = new Dictionary<string, BundleData>();
        private readonly Dictionary<string, AssetData> _assets = new Dictionary<string, AssetData>();
        private readonly Dictionary<string, string> _addressablePath = new Dictionary<string, string>();

        private readonly Dictionary<string, AssetInfo> _assetInfos = new Dictionary<string, AssetInfo>();
        private readonly Dictionary<string, BundleInfo> _bundleInfos = new Dictionary<string, BundleInfo>();


        private bool _isConfig;

        public void Load()
        {
            var collectorSetting = CollectorSetting.Instance;
            var caches = Caches.Get();
            if (collectorSetting.Packages.Count > 0)
            {
                _isConfig = true;
                LoadCaches();
            }


            if (!_isConfig)
            {
                Debug.LogWarning("没有配置收集规则，无法使用寻址模式，且只能用于编辑器开发测试。若要打包发布请先配置资源收集规则！");
            }
            else
            {
                if (caches.Bundles.Count < 1)
                {
                    _isConfig = false;
                    Debug.LogWarning("没有被收集的资源，请检查收集规则是否有误！将启用开发模式");
                }
            }
        }

        public VersionData GetVersionData()
        {
            return null;
        }

        public PackageData GetPackageData(string packageName)
        {
            return null;
        }

        public void UpdateBundleInfo(string bundleName)
        {
        }
        
        public AssetInfo GetAssetInfo(string path, Type type)
        {
            // path = _versionDataReader.GetAssetRealPath(path);
            var guid = Util.GetAssetGUID(path, type);
            if (!_assetInfos.TryGetValue(guid, out var info))
            {
                var data = GetAsset(path);
                info = new AssetInfo(data, type);
                _assetInfos[info.GUID] = info;
            }

            return info;
        }

        public BundleInfo GetBundleInfo(AssetInfo assetInfo)
        {
            var bundleData = GetBundleByAsset(assetInfo.Path);
            return GetBundleInfo(bundleData);
        }

        public BundleInfo GetBundleInfo(BundleData bundleData)
        {
            if (!_bundleInfos.TryGetValue(bundleData.Name, out var bundleInfo))
            {
                bundleInfo = new BundleInfo(bundleData);
                bundleInfo.LoadMode = BundleLoadMode.LoadFromStreaming;
                _bundleInfos[bundleData.Name] = bundleInfo;
            }

            return bundleInfo;
        }

        public BundleInfo[] GetAllBundleInfo()
        {
            List<BundleInfo> list = new List<BundleInfo>();
            list.AddRange(_bundleInfos.Values);
            return list.ToArray();
        }

        public BundleInfo[] GetAllDependBundleInfos(AssetInfo assetInfo)
        {
            if (_assets.TryGetValue(assetInfo.Path, out var assetData))
            {
                if (_bundles.TryGetValue(assetData.BundleName, out var bundleData))
                {
                    List<BundleData> needBundle = new List<BundleData>();
                    needBundle.Add(bundleData);
                    needBundle.AddRange(bundleData.DependBundles.Select(bundle => _bundles[bundle]));

                    List<BundleInfo> list = new List<BundleInfo>();
                    foreach (var bundle in needBundle)
                    {
                        list.Add(GetBundleInfo(bundle));
                    }

                    return list.ToArray();
                }
            }

            return Array.Empty<BundleInfo>();
        }

        #region Privite

        private string GetAssetRealPath(string path)
        {
            return _addressablePath.TryGetValue(path, out var p) ? p : path;
        }

        private void LoadCaches()
        {
            var caches = Caches.Get();
            foreach (var bundle in caches.Bundles)
            {
                BundleData bundleData = new BundleData();
                bundleData.Name = bundle.Name;
                bundleData.Hash = bundle.Hash;
                bundleData.Size = bundle.Size;
                _bundles[bundle.Name] = bundleData;

                foreach (var asset in bundle.Assets)
                {
                    var path = asset.Path;
                    if (string.IsNullOrEmpty(path)) continue;
                    AssetData assetData = new AssetData();
                    assetData.Name = Path.GetFileName(path);
                    assetData.Path = path;
                    assetData.BundleName = asset.Bundle;
                    _assets[path] = assetData;
                    _addressablePath[assetData.Address] = assetData.Path;
                }
            }
        }

        private BundleData GetBundle(string name)
        {
            return _bundles.TryGetValue(name, out var bundleRef) ? bundleRef : null;
        }

        private BundleData GetBundleByAsset(string assetPath)
        {
            var asset = GetAsset(assetPath);
            return asset == null ? null : GetBundle(asset.BundleName);
        }

        private AssetData GetAsset(string path)
        {
            if (!_assets.TryGetValue(path, out var assetRef))
            {
                if (_isConfig)
                {
                    return null;
                }

                //未配置的测试模式
                assetRef = new AssetData();
                assetRef.Path = path;
            }

            return assetRef;
        }

        #endregion
    }
}