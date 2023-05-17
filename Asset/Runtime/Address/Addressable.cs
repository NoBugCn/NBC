using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using System.Reflection;
#endif

namespace NBC.Asset
{
    public interface IAddressableImpl
    {
        public void Load();
        void UpdateBundleInfo(string bundleName);
        VersionData GetVersionData();
        PackageData GetPackageData(string packageName);
        AssetInfo GetAssetInfo(string path, Type type);
        BundleInfo GetBundleInfo(AssetInfo assetInfo);
        BundleInfo GetBundleInfo(BundleData bundleData);
        BundleInfo[] GetAllBundleInfo();
        BundleInfo[] GetAllDependBundleInfos(AssetInfo assetInfo);
    }

    internal static class Addressable
    {
        public static Func<IAddressableImpl> CreateHandler { get; set; } = AddressableRuntimeImpl.CreateInstance;
        private static IAddressableImpl _impl;

        public static void Load()
        {
            CreateHandlerImpl();
            _impl.Load();
        }

        public static void UpdateBundleInfo(string bundleName)
        {
            _impl.UpdateBundleInfo(bundleName);
        }

        #region Version

        public static VersionData GetVersionData()
        {
            return _impl.GetVersionData();
        }

        public static PackageData GetPackageData(string packageName)
        {
            return _impl.GetPackageData(packageName);
        }

        #endregion
        
        #region Asset

        public static AssetInfo GetAssetInfo(string path, Type type)
        {
            return _impl.GetAssetInfo(path, type);
        }

        #endregion

        #region Bundle

        public static BundleInfo GetBundleInfo(AssetInfo assetInfo)
        {
            return _impl.GetBundleInfo(assetInfo);
        }

        public static BundleInfo GetBundleInfo(BundleData bundleData)
        {
            return _impl.GetBundleInfo(bundleData);
        }

        public static BundleInfo[] GetAllBundleInfo()
        {
            return _impl.GetAllBundleInfo();
        }

        public static BundleInfo[] GetAllDependBundleInfos(AssetInfo assetInfo)
        {
            return _impl.GetAllDependBundleInfos(assetInfo);
        }

        /// <summary>
        /// 获取当前版本可以解压到本地缓存的bundle包
        /// </summary>
        /// <returns></returns>
        public static BundleInfo[] GetCanUnpackBundles()
        {
            var bundles = GetAllBundleInfo();
            return bundles.Where(bundle => bundle.LoadMode == BundleLoadMode.LoadFromStreaming).ToArray();
        }

        /// <summary>
        /// 获取当前版本可以下载到本地的bundle包
        /// </summary>
        /// <returns></returns>
        public static BundleInfo[] GetCanDownloadBundles()
        {
            var bundles = GetAllBundleInfo();
            return bundles.Where(bundle => bundle.LoadMode == BundleLoadMode.LoadFromRemote).ToArray();
        }

        #endregion

        #region 内部方法

        private static void CreateHandlerImpl()
        {
#if UNITY_EDITOR
            if (Const.Simulate)
            {
                _impl = GetAddressableEditImpl();
                return;
            }
#endif
            _impl = CreateHandler();
        }

#if UNITY_EDITOR
        /// <summary>
        /// 通过反射，实例化editor下的接口实现
        /// </summary>
        /// <returns></returns>
        private static IAddressableImpl GetAddressableEditImpl()
        {
            var ass = AppDomain.CurrentDomain.GetAssemblies()
                .First(assembly => assembly.GetName().Name == "NBC.Asset.Editor");
            var type = ass.GetType("NBC.Asset.Editor.AddressableEditImpl");
            var manifestFilePath = InvokePublicStaticMethod(type, "CreateInstance") as IAddressableImpl;
            return manifestFilePath;
        }

        private static object InvokePublicStaticMethod(System.Type type, string method, params object[] parameters)
        {
            var methodInfo = type.GetMethod(method, BindingFlags.Public | BindingFlags.Static);
            if (methodInfo == null)
            {
                UnityEngine.Debug.LogError($"{type.FullName} not found method : {method}");
                return null;
            }

            return methodInfo.Invoke(null, parameters);
        }
#endif

        #endregion
    }
}