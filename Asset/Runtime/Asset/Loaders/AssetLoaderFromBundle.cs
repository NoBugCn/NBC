using UnityEngine;

namespace NBC.Asset
{
    internal class AssetLoaderFromBundle : IAssetLoader
    {
        public static IAssetLoader CreateInstance()
        {
            return new AssetLoaderFromBundle();
        }

        private enum Steps
        {
            LoadDependency,
            LoadAsset,
        }

        private AssetProvider _provider;
        private AssetInfo _assetInfo;

        private AssetBundleRequest _cacheRequest;
        private Dependency _dependency;
        private Steps _steps = Steps.LoadDependency;
        private bool _isWaitForAsyncComplete;

        private AssetBundleRequest _assetBundleRequest;
        public AssetBundle _assetBundle;

        public void Start(AssetProvider provider)
        {
            _provider = provider;
            _assetInfo = provider.AssetInfo;

            _dependency = Dependency.GetAssetDependency(_assetInfo);
            _dependency.Retain();
        }

        public void Update()
        {
            if (_steps == Steps.LoadDependency)
            {
                if (_isWaitForAsyncComplete)
                {
                    _dependency.WaitForAsyncComplete();
                }

                if (!_dependency.IsDone) return;

                if (!_dependency.IsSucceed)
                {
                    Debug.LogError("error");
                    SetStatus("dependency fail");
                }

                var provider = _dependency.GetMainBundledProvider();
                if (provider != null && provider.AssetBundle != null)
                {
                    _assetBundle = provider.AssetBundle;

                    if (_isWaitForAsyncComplete)
                    {
                        if (_provider.IsAll)
                        {
                            SetStatus(_assetBundle.LoadAssetWithSubAssets(_assetInfo.Path, _assetInfo.AssetType));
                        }
                        else
                        {
                            SetStatus(_assetBundle.LoadAsset(_assetInfo.Path, _assetInfo.AssetType));
                        }
                    }
                    else
                    {
                        _assetBundleRequest = _provider.IsAll
                            ? _assetBundle.LoadAssetWithSubAssetsAsync(_assetInfo.Path, _assetInfo.AssetType)
                            : _assetBundle.LoadAssetAsync(_assetInfo.Path, _assetInfo.AssetType);
                        _steps = Steps.LoadAsset;
                    }
                }
                else
                {
                    //失败，后续补全失败逻辑
                    Debug.LogError("error1");
                    SetStatus("error");
                }
            }
            else if (_steps == Steps.LoadAsset)
            {
                if (!_assetBundleRequest.isDone) return;
                if (_provider.IsAll)
                {
                    SetStatus(_assetBundleRequest.allAssets);
                }
                else
                {
                    SetStatus(_assetBundleRequest.asset);
                }
            }
        }

        public void WaitForAsyncComplete()
        {
            _isWaitForAsyncComplete = true;

            int frame = 1000;
            while (true)
            {
                frame--;
                if (frame == 0)
                {
                    break;
                }

                Update();

                if (_provider.IsDone)
                    break;
            }
        }

        public void Destroy()
        {
            _dependency.Release();
            _assetBundleRequest = null;
        }


        private void SetStatus(Object asset)
        {
            _provider.Asset = asset;
            _provider.SetStatus(TaskStatus.Success);
        }

        private void SetStatus(Object[] allAsset)
        {
            _provider.AllAsset = allAsset;
            _provider.SetStatus(TaskStatus.Success);
        }

        private void SetStatus(string error)
        {
            _provider.SetStatus(TaskStatus.Fail, error);
        }
    }
}