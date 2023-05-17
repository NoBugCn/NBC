using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NBC.Asset
{
    public class AssetProvider : ProviderBase
    {
        public static Func<IAssetLoader> CreateLoader { get; set; } = AssetLoaderFromBundle.CreateInstance;

        private IAssetLoader _loader;

        public AssetInfo AssetInfo { get; internal set; }

        public bool IsAll { get; set; }

        public UnityEngine.Object Asset { get; set; }

        public UnityEngine.Object[] AllAsset { get; set; }


        protected override void OnStart()
        {
            if (_loader == null) _loader = CreateLoader();
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
            Debug.Log($"卸载资源==={AssetInfo.Path}");
            base.Destroy();
            _assets.Remove(this);
            _loader.Destroy();
            _loader = null;
        }

        #region Static

        private static readonly List<AssetProvider> _assets = new List<AssetProvider>();

        internal static List<AssetProvider> GetAssetProviders()
        {
            return _assets;
        }

        internal static void ReleaseAllAssets(bool force = true)
        {
            foreach (var asset in _assets)
            {
                asset.Release(force);
            }
        }

        internal static void ReleaseAllAssetsByTag(string[] tags, bool force = true)
        {
            foreach (var asset in _assets)
            {
                if (asset.AssetInfo.HasTag(tags))
                {
                    asset.Release(force);
                }
            }
        }

        internal static AssetProvider GetAssetProvider(AssetInfo assetInfo, bool isAll = false)
        {
            AssetProvider provider = null;
            foreach (var asset in _assets)
            {
                if (asset.AssetInfo == assetInfo)
                {
                    provider = asset;
                    break;
                }
            }

            if (provider == null)
            {
                provider = new AssetProvider
                {
                    AssetInfo = assetInfo,
                    IsAll = isAll
                };
#if DEBUG
                provider.InitDebugInfo();
#endif
                _assets.Add(provider);
            }

            provider.Run();

            return provider;
        }

        #endregion
    }
}