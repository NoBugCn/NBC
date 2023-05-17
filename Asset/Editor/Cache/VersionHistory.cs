using System;
using System.Collections.Generic;
using System.Linq;

namespace NBC.Asset.Editor
{
    [Serializable]
    public class VersionHistoryData
    {
        public string ShowName; // => $"version_{Index}_{VersionData?.BuildTime}";
        public string FileName;
        public VersionData VersionData;

        /// <summary>
        /// 版本包
        /// </summary>
        public readonly List<PackageData> Packages = new List<PackageData>();
    }

    [Serializable]
    public class PackageChangeData
    {
        public string PackageName;

        /// <summary>
        /// 变化的bundle
        /// </summary>
        public List<BundleData> ChangeBundles = new List<BundleData>();

        /// <summary>
        /// 新增的bundle
        /// </summary>
        public List<BundleData> AddBundles = new List<BundleData>();

        /// <summary>
        /// 减少的bundle
        /// </summary>
        public List<BundleData> RemoveBundles = new List<BundleData>();
    }

    /// <summary>
    /// 版本变化简略信息
    /// </summary>
    [Serializable]
    public class VersionSimpleChangeData
    {
        /// <summary>
        /// 需要下载总数
        /// </summary>
        public long DownloadSize;

        /// <summary>
        /// 每个资源包需要下载总数
        /// </summary>
        public Dictionary<string, long> PackageDownloadSize = new Dictionary<string, long>();

        /// <summary>
        /// 每个资源包新增bundle
        /// </summary>
        public Dictionary<string, List<string>> PackageAddBundle = new Dictionary<string, List<string>>();

        /// <summary>
        /// 每个资源包移除bundle
        /// </summary>
        public Dictionary<string, List<string>> PackageRemoveBundle = new Dictionary<string, List<string>>();

        /// <summary>
        /// 每个资源包变化的bundle
        /// </summary>
        public Dictionary<string, List<string>> PackageChangeBundle = new Dictionary<string, List<string>>();
    }

    [Serializable]
    public class VersionChangeData
    {
        public string NewVersionName;
        public string OldVersionName;
        public VersionSimpleChangeData SimpleChangeData = new VersionSimpleChangeData();

        /// <summary>
        /// 变化的package
        /// </summary>
        public List<PackageChangeData> ChangePackage = new List<PackageChangeData>();

        public enum TypeEnum
        {
            Add,
            Change,
            Remove
        }

        public void Change(BundleData bundleData, TypeEnum typeEnum)
        {
            var packageChangeData = ChangePackage.Find(p => p.PackageName == bundleData.PackageName);
            if (packageChangeData == null)
            {
                packageChangeData = new PackageChangeData();
                packageChangeData.PackageName = bundleData.PackageName;
                ChangePackage.Add(packageChangeData);
            }

            switch (typeEnum)
            {
                case TypeEnum.Add:
                    packageChangeData.AddBundles.Add(bundleData);
                    break;
                case TypeEnum.Remove:
                    packageChangeData.RemoveBundles.Add(bundleData);
                    break;
                case TypeEnum.Change:
                    packageChangeData.ChangeBundles.Add(bundleData);
                    break;
            }
        }

        public void Processing()
        {
            SimpleChangeData = new VersionSimpleChangeData();
            long allSize = 0;
            foreach (var package in ChangePackage)
            {
                var name = package.PackageName;
                var size = package.AddBundles.Sum(b => b.Size) + package.ChangeBundles.Sum(b => b.Size);
                allSize += size;
                SimpleChangeData.PackageDownloadSize[name] = size;

                SimpleChangeData.PackageAddBundle[name] = package.AddBundles.Select(bundle => bundle.Name).ToList();
                SimpleChangeData.PackageRemoveBundle[name] =
                    package.RemoveBundles.Select(bundle => bundle.Name).ToList();
                SimpleChangeData.PackageChangeBundle[name] =
                    package.ChangeBundles.Select(bundle => bundle.Name).ToList();
            }

            SimpleChangeData.DownloadSize = allSize;
        }
    }
}