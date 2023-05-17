using UnityEngine;

namespace NBC.Asset
{
    internal class BundleLoaderFromDownload : IBundleLoader
    {
        private enum Steps
        {
            Download,
            Load,
            Check,
            Done,
        }

        private BundledProvider _provider;
        private Steps _steps = Steps.Download;
        private AssetBundleCreateRequest _createRequest;
        private string _downloadPath;
        private string _loadPath;
        private bool _isWaitForAsyncComplete;
        private DownloadFileTask _downloadFileTask;

        public AssetBundle Bundle { set; get; }

        public void Start(BundledProvider provider)
        {
            _provider = provider;
            var bundleData = provider.BundleInfo.Bundle;
            _downloadPath = bundleData.RemoteDataFilePath;
            _loadPath = bundleData.CachedDataFilePath;
            _downloadFileTask = new DownloadFileTask(_downloadPath, bundleData.CachedDataFilePath);
            _downloadFileTask.Run();
        }

        public void Update()
        {
            if (_steps == Steps.Download)
            {
                if (!_downloadFileTask.IsDone) return;
                Addressable.UpdateBundleInfo(_provider.BundleInfo.Bundle.Name);
                _steps = Steps.Load;
            }

            if (_steps == Steps.Load)
            {
                if (_isWaitForAsyncComplete)
                    Bundle = AssetBundle.LoadFromFile(_loadPath);
                else
                    _createRequest = AssetBundle.LoadFromFileAsync(_loadPath);
                _steps = Steps.Check;
            }

            if (_steps == Steps.Check)
            {
                if (_createRequest != null)
                {
                    if (_isWaitForAsyncComplete)
                    {
                        Bundle = _createRequest.assetBundle;
                    }
                    else
                    {
                        if (!_createRequest.isDone)
                            return;
                        Bundle = _createRequest.assetBundle;
                    }
                }

                if (Bundle == null)
                {
                    _steps = Steps.Done;
                    _provider.SetStatus(TaskStatus.Fail,
                        $"failed load assetBundle : {_provider.BundleInfo.Bundle.Name}");
                    Debug.LogError(_provider.ErrorMsg);
                }
                else
                {
                    _provider.SetStatus(TaskStatus.Success);
                    _provider.AssetBundle = Bundle;
                    _steps = Steps.Done;
                }
            }
        }

        public void WaitForAsyncComplete()
        {
            _isWaitForAsyncComplete = true;

            int frame = 1000;
            while (true)
            {
                // 保险机制
                frame--;
                if (frame == 0)
                {
                    break;
                }

                Update();
                _downloadFileTask.Process();

                if (_provider.IsDone)
                    break;
            }
        }

        public void Destroy()
        {
            _createRequest = null;
            _downloadFileTask = null;
        }
    }
}