using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NBC.Asset
{
    internal class SceneLoadFromDatabase : ISceneLoader
    {
        public static ISceneLoader CreateInstance()
        {
            return new SceneLoadFromDatabase();
        }

        private enum Steps
        {
            LoadDependency,
            LoadScene,
        }

        private bool _isWaitForAsyncComplete;
        private SceneProvider _provider;
        private AssetInfo _assetInfo;
        private Steps _steps = Steps.LoadDependency;
        private AsyncOperation _asyncOperation;

        public void Start(SceneProvider provider)
        {
            _provider = provider;
            _assetInfo = provider.AssetInfo;
        }

        public void Update()
        {
#if UNITY_EDITOR
            if (_steps == Steps.LoadDependency)
            {
                var scenePath = _assetInfo.Path; //Path.GetFileNameWithoutExtension(_assetInfo.Path);
                LoadSceneParameters loadSceneParameters = new LoadSceneParameters
                {
                    loadSceneMode = _provider.SceneMode
                };
                if (_isWaitForAsyncComplete)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(scenePath, loadSceneParameters);
                    SetStatus();
                }
                else
                {
                    _asyncOperation =
                        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(scenePath,
                            loadSceneParameters);
                    _steps = Steps.LoadScene;
                }
            }
            else if (_steps == Steps.LoadScene)
            {
                if (!_asyncOperation.isDone) return;
                SetStatus();
            }
#endif
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
            
        }

        private void SetStatus()
        {
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