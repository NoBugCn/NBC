using System.IO;
using UnityEngine.Networking;

namespace NBC.Asset
{
    /// <summary>
    /// 解压文件到指定目录
    /// </summary>
    public class UnpackFileTask : NTask
    {
        private enum Steps
        {
            LoadStreaming,
            Download,
            Done,
        }

        private readonly string _fileName;
        private readonly string _savePath;
        private Steps _steps;
        private TaskStatus _taskStatus = TaskStatus.Success;
        private UnityWebRequest _request;
        private DownloadFileTask _downloadFileTask;
        private bool _download;

        public UnpackFileTask(string fileName, bool download = false)
        {
            _fileName = fileName;
            _savePath = Const.GetCachePath(fileName);
            _download = download;
        }


        protected override void OnStart()
        {
            if (File.Exists(_savePath))
            {
                File.Delete(_savePath);
            }

            _steps = Steps.LoadStreaming;
        }


        protected override TaskStatus OnProcess()
        {
            if (_steps == Steps.LoadStreaming)
            {
                _progress = 0;
                if (_request == null)
                {
                    var filePath = Const.GetStreamingPath(_fileName);
                    _request = UnityWebRequest.Get(filePath);
                    _request.downloadHandler = new DownloadHandlerFile(_savePath);
                    _request.SendWebRequest();
                }

                _progress = _request.downloadProgress * 0.5f;

                if (!_request.isDone) return TaskStatus.Running;
                if (_request.result == UnityWebRequest.Result.Success)
                {
                    //结束，判断是否成功
                    if (File.Exists(_savePath))
                    {
                        _progress = 1;
                        _steps = Steps.Done;
                    }
                    else
                    {
                        if (_download)
                        {
                            _taskStatus = TaskStatus.Fail;
                            _steps = Steps.Download;
                        }
                    }
                }
                else
                {
                    if (_download)
                    {
                        _taskStatus = TaskStatus.Fail;
                        _steps = Steps.Download;
                    }
                }
            }
            else if (_steps == Steps.Download)
            {
                if (_downloadFileTask == null)
                {
                    _downloadFileTask = new DownloadFileTask(Const.GetRemotePath(_fileName), _savePath);
                    _downloadFileTask.Run();
                }

                _progress = 0.5f + _downloadFileTask.Progress * 0.5f;
                if (!_downloadFileTask.IsDone) return TaskStatus.Running;
                _taskStatus = _downloadFileTask.Status;
                _steps = Steps.Done;
                _progress = 1;
            }


            return _steps == Steps.Done ? _taskStatus : TaskStatus.Running;
        }
    }
}