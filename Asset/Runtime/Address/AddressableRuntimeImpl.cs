using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NBC.Asset
{
    internal class AddressableRuntimeImpl : IAddressableImpl
    {
        public static IAddressableImpl CreateInstance()
        {
            return new AddressableRuntimeImpl();
        }

        private VersionDataReader _versionDataReader;

        private readonly Dictionary<string, AssetInfo> _assetInfos = new Dictionary<string, AssetInfo>();
        private readonly Dictionary<string, BundleInfo> _bundleInfos = new Dictionary<string, BundleInfo>();

        #region Init

        public void Load()
        {
            _versionDataReader = new VersionDataReader();
        }

        #endregion

        #region manifest Data

        public VersionData GetVersionData()
        {
            return _versionDataReader.VersionData;
        }

        public PackageData GetPackageData(string packageName)
        {
            return _versionDataReader.PackageDataList.Find(p => p.Name == packageName);
        }
        
        #endregion

        #region Assets

        public AssetInfo GetAssetInfo(string path, Type type)
        {
            path = _versionDataReader.GetAssetRealPath(path);
            var guid = Util.GetAssetGUID(path, type);
            if (!_assetInfos.TryGetValue(guid, out var info))
            {
                var data = _versionDataReader.GetAsset(path);
                info = new AssetInfo(data, type);
                _assetInfos[info.GUID] = info;
            }

            return info;
        }

        #endregion

        #region Bundles

        /// <summary>
        /// 刷新bundle相关信息
        /// </summary>
        public void UpdateBundleInfo(string bundleName)
        {
            if (string.IsNullOrEmpty(bundleName))
            {
                foreach (var bundle in _bundleInfos.Values)
                {
                    bundle.LoadMode = GetBundleLoadMode(bundle.Bundle);
                }
            }
            else
            {
                if (_bundleInfos.TryGetValue(bundleName, out var bundleInfo))
                {
                    bundleInfo.LoadMode = GetBundleLoadMode(bundleInfo.Bundle);
                }
            }
        }

        public BundleInfo GetBundleInfo(AssetInfo assetInfo)
        {
            var bundleData = _versionDataReader.GetBundleByAsset(assetInfo.Path);
            return GetBundleInfo(bundleData);
        }

        public BundleInfo GetBundleInfo(BundleData bundleData)
        {
            if (bundleData == null)
                throw new Exception("BundleData NOT NULL!");
            if (!_bundleInfos.TryGetValue(bundleData.Name, out var bundleInfo))
            {
                bundleInfo = CreateBundleInfo(bundleData);
            }

            return bundleInfo;
        }

        public BundleInfo[] GetAllBundleInfo()
        {
            var bundles = _versionDataReader.GetAllBundle();
            List<BundleInfo> list = new List<BundleInfo>(bundles.Count);
            foreach (var bundle in bundles)
            {
                list.Add(GetBundleInfo(bundle));
            }

            return list.ToArray();
        }

        public BundleInfo[] GetAllDependBundleInfos(AssetInfo assetInfo)
        {
            var arr = _versionDataReader.GetAllDependBundle(assetInfo.Path);
            if (arr != null)
            {
                List<BundleInfo> list = new List<BundleInfo>();
                foreach (var bundle in arr)
                {
                    list.Add(GetBundleInfo(bundle));
                }

                return list.ToArray();
            }

            return Array.Empty<BundleInfo>();
        }


        private BundleInfo CreateBundleInfo(BundleData bundleData)
        {
            var bundleInfo = new BundleInfo(bundleData)
            {
                LoadMode = GetBundleLoadMode(bundleData)
            };
            _bundleInfos[bundleData.Name] = bundleInfo;
            return bundleInfo;
        }

        private BundleLoadMode GetBundleLoadMode(BundleData bundleData)
        {
            if (File.Exists(bundleData.CachedDataFilePath))
            {
                return BundleLoadMode.LoadFromCache;
            }

            if (File.Exists(bundleData.StreamingFilePath))
            {
                return BundleLoadMode.LoadFromStreaming;
            }

            return BundleLoadMode.LoadFromRemote;
        }

        #endregion
    }
}