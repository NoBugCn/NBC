using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NBC.Asset
{
    internal class SceneLoaderFromBundle : ISceneLoader
    {
        public static ISceneLoader CreateInstance()
        {
            return new SceneLoaderFromBundle();
        }

        private enum Steps
        {
            LoadDependency,
            LoadScene,
        }

        private Dependency _dependency;
        private AssetInfo _assetInfo;
        private Steps _steps = Steps.LoadDependency;
        private SceneProvider _provider;

        private bool _isWaitForAsyncComplete;
        private AsyncOperation _asyncOperation;

        public void Start(SceneProvider provider)
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
                else if (!_dependency.IsDone) return;

                if (!_dependency.IsSucceed)
                {
                    Debug.LogError("error");
                    SetStatus("dependency fail");
                }

                var provider = _dependency.GetMainBundledProvider();
                if (provider != null && provider.AssetBundle != null)
                {
                    var scenePath = Path.GetFileNameWithoutExtension(_assetInfo.Path);
                    if (_isWaitForAsyncComplete)
                    {
                        SceneManager.LoadScene(scenePath, _provider.SceneMode);
                        SetStatus();
                    }
                    else
                    {
                        _asyncOperation = SceneManager.LoadSceneAsync(scenePath, _provider.SceneMode);
                        _asyncOperation.allowSceneActivation = true;
                        _steps = Steps.LoadScene;
                    }
                }
                else
                {
                    //失败，后续补全失败逻辑
                    Debug.LogError("error1");
                    SetStatus("error");
                }
            }
            else if (_steps == Steps.LoadScene)
            {
                if (!_asyncOperation.isDone) return;
                SetStatus();
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
            _asyncOperation = null;
        }


        private void SetStatus()
        {
            Debug.Log("场景加载完成=====");
            var sceneObj = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            _provider.SceneObject = sceneObj;
            _provider.SetStatus(TaskStatus.Success);
        }


        private void SetStatus(string error)
        {
            _provider.SetStatus(TaskStatus.Fail, error);
        }
    }
}