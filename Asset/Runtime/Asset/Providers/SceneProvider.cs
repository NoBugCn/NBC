using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NBC.Asset
{
    public class SceneProvider : ProviderBase
    {
        public static Func<ISceneLoader> CreateLoader { get; set; } = SceneLoaderFromBundle.CreateInstance;
        private ISceneLoader _loader;

        public AssetInfo AssetInfo { get; internal set; }

        public Scene SceneObject { get; set; }

        public LoadSceneMode SceneMode;
        private AsyncOperation _asyncOp;
        private int _priority;

        protected override void OnStart()
        {
            _loader.Start(this);
        }

        protected override TaskStatus OnProcess()
        {
#if DEBUG
            DebugRecord();
#endif
            if (IsDone) return TaskStatus.Success;
            if (IsWaitForAsyncComplete)
            {
                _loader.WaitForAsyncComplete();
            }
            else
            {
                _loader.Update();
            }

            return base.OnProcess();
        }

        public override void Destroy()
        {
            base.Destroy();
            _scenes.Remove(this);
            _loader.Destroy();
            _loader = null;
        }

        #region Static

        private static readonly List<SceneProvider> _scenes = new List<SceneProvider>();

        internal static List<SceneProvider> GetSceneProviders()
        {
            return _scenes;
        }

        internal static void ReleaseAllAssets(bool force = true)
        {
            foreach (var asset in _scenes)
            {
                asset.Release(force);
            }
        }

        internal static void ReleaseAllAssetsByTag(string[] tags, bool force = true)
        {
            foreach (var asset in _scenes)
            {
                if (asset.AssetInfo.HasTag(tags))
                {
                    asset.Release(force);
                }
            }
        }

        internal static SceneProvider GetSceneProvider(AssetInfo assetInfo, bool additive = false)
        {
            SceneProvider provider = null;
            foreach (var scene in _scenes)
            {
                if (scene.AssetInfo == assetInfo)
                {
                    provider = scene;
                    break;
                }
            }

            if (provider == null)
            {
                provider = new SceneProvider
                {
                    AssetInfo = assetInfo,
                    _loader = CreateLoader(),
                    SceneMode = additive ? LoadSceneMode.Additive : LoadSceneMode.Single
                };
#if DEBUG
                provider.InitDebugInfo();
#endif
                _scenes.Add(provider);
            }

            provider.Run();

            return provider;
        }

        #endregion
    }
}