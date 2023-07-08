using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace NBC.Asset
{
    public static class Assets
    {
        private static GameObject _monoGameObject;
        private static bool _isInitialize;
        private static TaskStatus _initializeTaskStatus = TaskStatus.None;
        private static string _initializeError = string.Empty;


        public static InitializationTask Initialize()
        {
            if (_isInitialize)
                throw new Exception($"Repeated initialization!");

            return InitConfirm();
        }

        #region 资源加载

        public static AssetProvider LoadAsset<T>(string path)
        {
            return LoadAsset(path, typeof(T));
        }

        public static AssetProvider LoadAsset(string path, Type type)
        {
            var req = LoadAssetAsync(path, type);
            req?.WaitForAsyncComplete();
            return req;
        }

        public static AssetProvider LoadAssetAsync<T>(string path)
        {
            return LoadAssetAsync(path, typeof(T));
        }

        public static AssetProvider LoadAssetAsync(string path, Type type)
        {
            var assetInfo = Addressable.GetAssetInfo(path, type);
            return AssetProvider.GetAssetProvider(assetInfo);
        }


        public static AssetProvider LoadAssetAll<T>(string path)
        {
            return LoadAssetAll(path, typeof(T));
        }

        public static AssetProvider LoadAssetAll(string path, Type type)
        {
            var req = LoadAssetAllAsync(path, type);
            req?.WaitForAsyncComplete();
            return req;
        }

        public static AssetProvider LoadAssetAllAsync<T>(string path)
        {
            return LoadAssetAllAsync(path, typeof(T));
        }

        public static AssetProvider LoadAssetAllAsync(string path, Type type)
        {
            var assetInfo = Addressable.GetAssetInfo(path, type);
            return AssetProvider.GetAssetProvider(assetInfo, true);
        }

        #endregion

        #region 场景加载

        public static SceneProvider LoadScene(string path, bool additive = false)
        {
            var assetInfo = Addressable.GetAssetInfo(path, typeof(Scene));
            return SceneProvider.GetSceneProvider(assetInfo, additive);
        }

        #endregion

        #region 资源卸载

        /// <summary>
        /// 释放所有资源
        /// </summary>
        /// <param name="force">强制释放</param>
        public static void ReleaseAllAssets(bool force = true)
        {
            AssetProvider.ReleaseAllAssets(force);
            SceneProvider.ReleaseAllAssets(force);
        }

        /// <summary>
        /// 根据标签，释放所有资源
        /// </summary>
        /// <param name="tags">标签</param>
        /// <param name="force">强制释放</param>
        public static void ReleaseAllAssetsByTag(string[] tags, bool force = true)
        {
            AssetProvider.ReleaseAllAssetsByTag(tags, force);
            SceneProvider.ReleaseAllAssetsByTag(tags, force);
        }

        #endregion

        #region 检查解压

        /// <summary>
        /// 可以解压的bundle包数量
        /// </summary>
        /// <returns></returns>
        public static int CanUnpackBundleCount()
        {
            var arr = Addressable.GetCanUnpackBundles();
            return arr != null ? arr.Length : 0;
        }

        /// <summary>
        /// 解压资源包任务（解压本地存在的所有资源）
        /// </summary>
        /// <param name="run">自动运行</param>
        /// <returns></returns>
        public static UnpackPackagesTask CreateUnpackPackagesTask(bool run = true)
        {
            var task = new UnpackPackagesTask();
            if (run) task.Run(TaskRunner.Def);
            return task;
        }

        #endregion

        #region 检查更新

        /// <summary>
        /// 获取当前版本可以下载到本地的bundle包
        /// </summary>
        /// <returns></returns>
        public static List<BundleInfo> GetCanDownloadBundles()
        {
            List<BundleInfo> ret = new List<BundleInfo>();
            var bundles = Addressable.GetCanDownloadBundles();
            foreach (var bundleInfo in bundles)
            {
                var bundleData = bundleInfo.Bundle;
                //可优化。缓存所有packageName，但该逻辑理论全局只会调用一次，是否需要缓存值得考虑
                if (bundleData != null && IsNeedfulPackage(bundleData.PackageName))
                {
                    ret.Add(bundleInfo);
                }
            }

            return ret;
        }

        /// <summary>
        /// 主动下载需要的bundles包任务
        /// </summary>
        /// <param name="downloadBundles">需要下载的bundle</param>
        /// <param name="run">自动运行</param>
        /// <returns></returns>
        public static DownloadBundlesTask CreateDownloadBundlesTask(List<BundleInfo> downloadBundles, bool run = true)
        {
            var task = new DownloadBundlesTask(downloadBundles);
            if (run) task.Run();
            return task;
        }

        /// <summary>
        /// 创建检查更新任务
        /// </summary>
        /// <returns></returns>
        public static CheckUpdateTask CreateCheckUpdateTask(bool run = true)
        {
            var task = new CheckUpdateTask();
            if (run) task.Run(TaskRunner.Def);
            return task;
        }

        /// <summary>
        /// 创建版本更新任务
        /// </summary>
        public static UpdateVersionTask CreateUpdateVersionTask(UpdateContext context, bool run = true)
        {
            var task = new UpdateVersionTask(context);
            if (run) task.Run();
            return task;
        }

        #endregion

        #region 默认资源包

        private static readonly HashSet<string> _defaultPackage = new HashSet<string>();

        /// <summary>
        /// 添加一个包进入需要列表
        /// </summary>
        /// <param name="packageName"></param>
        public static void AddNeedfulPackage(string packageName)
        {
            _defaultPackage.Add(packageName);
        }

        /// <summary>
        /// 移除一个需要的资源包
        /// </summary>
        /// <param name="packageName"></param>
        public static void RemoveNeedfulPackage(string packageName)
        {
            if (_defaultPackage.Contains(packageName)) _defaultPackage.Remove(packageName);
        }

        /// <summary>
        /// 移除全部额外需要包
        /// </summary>
        public static void RemoveAllNeedful()
        {
            _defaultPackage.Clear();
        }

        /// <summary>
        /// 是否需要这个资源包
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static bool IsNeedfulPackage(string packageName)
        {
            var ret = _defaultPackage.Contains(packageName);
            if (!ret)
            {
                var package = Addressable.GetPackageData(packageName);
                if (package != null)
                {
                    return package.Def == 1;
                }
            }

            return ret;
        }

        #endregion

        #region 调试

#if DEBUG
        private static DebugRemoteServer _debugRemoteServer;


        public static void StartDebugRemoteServer()
        {
            _debugRemoteServer = _monoGameObject.AddComponent<DebugRemoteServer>();
        }

        public static void StopDebugRemoteServer()
        {
            if (_debugRemoteServer != null)
            {
                Object.DestroyImmediate(_debugRemoteServer);
            }
        }

        public static DebugInfo GetDebugInfos()
        {
            DebugInfo info = new DebugInfo();
            info.Frame = Time.frameCount;
            var assetProviders = AssetProvider.GetAssetProviders();
            foreach (var asset in assetProviders)
            {
                var i = CreateDebugAssetInfo(asset.AssetInfo, asset.IsAll);
                SetDebugBaseInfo(asset, i);
                info.AssetInfos.Add(i);
            }

            var sceneProviders = SceneProvider.GetSceneProviders();
            foreach (var scene in sceneProviders)
            {
                var i = CreateDebugAssetInfo(scene.AssetInfo, false);
                SetDebugBaseInfo(scene, i);
                info.AssetInfos.Add(i);
            }

            var bundleProviders = BundledProvider.GetBundleProviders();
            foreach (var bundle in bundleProviders)
            {
                var bundleInfo = new DebugBundleInfo
                {
                    BundleName = bundle.BundleInfo.Bundle.Name
                };
                SetDebugBaseInfo(bundle, bundleInfo);
                info.BundleInfos.Add(bundleInfo);
            }

            return info;
        }

        private static DebugAssetInfo CreateDebugAssetInfo(AssetInfo asset, bool isAll)
        {
            var assetInfo = new DebugAssetInfo
            {
                Path = asset.Path,
                Type = asset.AssetType.Name,
                IsAll = isAll,
            };
            var bundleInfos = Addressable.GetAllDependBundleInfos(asset);
            foreach (var bundle in bundleInfos)
            {
                assetInfo.Dependency.Add(bundle.Bundle.Name);
            }

            return assetInfo;
        }

        private static void SetDebugBaseInfo(ProviderBase provider, DebugBaseInfo info)
        {
            info.LoadScene = provider.LoadScene;
            info.Ref = provider.RefCount;
            info.Status = provider.Status.ToString();
            info.LoadTime = provider.LoadTime;
            info.LoadTotalTime = provider.LoadTotalTime;
        }
#endif

        #endregion

        #region Private

        private static void Update()
        {
            TaskRunner.Update();
            Recycler.Update();
        }

        private static InitializationTask InitConfirm()
        {
            _monoGameObject = new GameObject("Assets", typeof(Mono));
            Mono.AddUpdate(Update);
            InitializationTask task;
            if (Const.Simulate)
            {
                task = new EditorInitializationTask();
                AssetProvider.CreateLoader = AssetLoadFromDatabase.CreateInstance;
                SceneProvider.CreateLoader = SceneLoadFromDatabase.CreateInstance;
            }
            else if (Const.Offline)
            {
                task = new OfflineInitializationTask();
            }
            else
            {
                task = new OnlineInitializationTask();
            }

            if (Const.IsWebGLPlatform)
            {
                Const.RemoteUrl = $"{Application.streamingAssetsPath}/";
            }
            
            task.OnCompleted(InitDone);
            task.Run(TaskRunner.Def);
            _isInitialize = true;
            return task;
        }

        /// <summary>
        /// 初始化完成回调
        /// </summary>
        /// <param name="taskBase"></param>
        private static void InitDone(ITask taskBase)
        {
            Debug.Log("初始化完成===");
            _initializeTaskStatus = taskBase.Status;
            _initializeError = taskBase.ErrorMsg;
        }

        #endregion
    }
}