using System.Collections.Generic;

namespace NBC.Asset
{
    internal class Dependency
    {
        /// <summary>
        /// 依赖的资源包加载器列表
        /// </summary>
        internal readonly List<BundledProvider> DependBundles;

        public Dependency(List<BundledProvider> dependBundles)
        {
            DependBundles = dependBundles;
        }

        /// <summary>
        /// 是否已经完成（无论成功或失败）
        /// </summary>
        public bool IsDone
        {
            get
            {
                foreach (var bundle in DependBundles)
                {
                    if (!bundle.IsDone)
                        return false;
                }

                return true;
            }
        }

        public bool IsSucceed
        {
            get
            {
                foreach (var loader in DependBundles)
                {
                    if (loader.Status != TaskStatus.Success)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public void WaitForAsyncComplete()
        {
            foreach (var bundle in DependBundles)
            {
                if (!bundle.IsDone)
                    bundle.WaitForAsyncComplete();
            }
        }


        public void Retain()
        {
            foreach (var request in DependBundles)
            {
                // request.Retain();
            }
        }

        public void Release()
        {
            foreach (var request in DependBundles)
            {
                request.Release();
            }
        }

        public BundledProvider GetMainBundledProvider()
        {
            return DependBundles.Count > 0 ? DependBundles[0] : null;
        }


        #region Static

        internal static Dependency GetAssetDependency(AssetInfo assetInfo)
        {
            var bundleInfos = Addressable.GetAllDependBundleInfos(assetInfo);
            List<BundledProvider> list = new List<BundledProvider>();
            foreach (var info in bundleInfos)
            {
                list.Add(BundledProvider.GetBundleProvider(info));
            }

            var dep = new Dependency(list);
            return dep;
        }

        #endregion
    }
}