using System.IO;
using UnityEngine.Networking;

namespace NBC.Asset
{
    /// <summary>
    /// 解压文件到指定目录
    /// </summary>
    public class UnpackFileTask : NTask
    {
        private readonly string _fileName;
        private readonly string _savePath;
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
            if (!File.Exists(_savePath))
            {
                if (File.Exists(Const.GetStreamingPath(_fileName)))
                {
                    _request = UnityWebRequest.Get(Const.GetStreamingPath(_fileName));
                    _request.downloadHandler = new DownloadHandlerFile(_savePath);
                    _request.SendWebRequest();
                }
                else if (_download)
                {
                    _downloadFileTask = new DownloadFileTask(Const.GetRemotePath(_fileName), _savePath);
                    _downloadFileTask.Run();
                }
            }
            else
            {
                Finish();
            }
        }

        protected override TaskStatus OnProcess()
        {
            if (_request != null)
            {
                return _request.isDone ? TaskStatus.Success : TaskStatus.Running;
            }

            if (_downloadFileTask != null)
            {
                if (_downloadFileTask.IsDone)
                {
                    
                }
                return _downloadFileTask.IsDone ? _downloadFileTask.Status : TaskStatus.Running;
            }

            return TaskStatus.Success;
        }
    }
}