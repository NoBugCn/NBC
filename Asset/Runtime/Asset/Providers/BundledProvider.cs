using System;
using System.Collections.Generic;
using UnityEngine;

namespace NBC.Asset
{
    internal class BundledProvider : ProviderBase
    {
        public static Func<IBundleLoader> CreateLoader { get; set; } = null;

        private IBundleLoader _loader;
        public BundleInfo BundleInfo;
        public AssetBundle AssetBundle { get; set; }

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
            Debug.Log($"卸载Bundle==={BundleInfo.Bundle.Name}");
            base.Destroy();
            _bundled.Remove(this);
            _loader.Destroy();
            if (AssetBundle != null)
            {
                AssetBundle.Unload(true);
                AssetBundle = null;
            }

            _loader = null;
        }

        #region Static

        private static readonly List<BundledProvider> _bundled = new List<BundledProvider>();

        internal static List<BundledProvider> GetBundleProviders()
        {
            return _bundled;
        }

        internal static BundledProvider GetBundleProvider(BundleInfo bundleInfo)
        {
            BundledProvider provider = null;
            foreach (var bundled in _bundled)
            {
                if (bundled.BundleInfo == bundleInfo)
                {
                    provider = bundled;
                    break;
                }
            }

            if (provider == null)
            {
                provider = new BundledProvider
                {
                    BundleInfo = bundleInfo
                };
                provider._loader = GetBundleLoader(provider);
#if DEBUG
                provider.InitDebugInfo();
#endif
                _bundled.Add(provider);
            }

            provider.Run();

            return provider;
        }

        internal static IBundleLoader GetBundleLoader(BundledProvider provider)
        {
            var loader = CreateLoader?.Invoke();
            if (loader != null) return loader;
            var bundleInfo = provider.BundleInfo;
            if (bundleInfo.LoadMode == BundleLoadMode.LoadFromRemote)
            {
                return new BundleLoaderFromDownload();
            }

            return new BundleLoaderFromLocal();
        }

        #endregion
    }
}