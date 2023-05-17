using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace NBC.Asset
{
    /// <summary>
    /// 下载文件
    /// </summary>
    public class DownloadFileTask : DownloadTaskBase
    {
        private const string DownloadHeaderKey = "Content-Length";
        private const int RetryDownloadCount = 3;

        public readonly string DownloadPath;

        public readonly string SavePath;
        public readonly string FileHash;

        /// <summary>
        /// 开启断点续传
        /// </summary>
        public bool ReDownload = true;

        public DownloadFileTask(string path, string savePath, string hash = "")
        {
            DownloadPath = path;
            SavePath = savePath;
            FileHash = hash;
        }

        public enum DownLoadStatus
        {
            None,
            GetHeader,
            PrepareDownload,
            Download,
            VerifyingFile,
            Success,
            Failed,
        }

        private ulong _downloadTotalSize = 1;
        private UnityWebRequest _content;
        private UnityWebRequest _header;
        private bool _isAbort;
        private ulong _latestDownloadBytes;
        private float _latestDownloadRealtime;
        private ulong _fileOriginLength;
        private int RetryCount;

        private long ResponseCode = 0;

        public DownLoadStatus DownloadStatus { get; protected internal set; } = DownLoadStatus.None;
        public override float Progress => DownloadedBytes * 1f / DownloadTotalSize;

        public ulong DownloadedBytes { get; protected set; }

        public ulong DownloadTotalSize
        {
            get => _downloadTotalSize;
            set
            {
                _downloadTotalSize = value;
                if (_downloadTotalSize < 1) _downloadTotalSize = 1;
            }
        }


        public void Abort()
        {
            Fail("abort");
            Dispose();
        }

        protected override void OnStart()
        {
            DownloadStatus = DownLoadStatus.GetHeader;
        }

        protected override TaskStatus OnProcess()
        {
            if (DownloadStatus == DownLoadStatus.GetHeader)
            {
                _header = UnityWebRequest.Head(DownloadPath);
                _header.SendWebRequest();
                DownloadStatus = DownLoadStatus.PrepareDownload;
            }

            if (DownloadStatus == DownLoadStatus.PrepareDownload)
            {
                if (_header == null)
                {
                    Fail($"not file");
                    return TaskStatus.Fail;
                }

                if (!_header.isDone) return TaskStatus.Running;

                Reset();
                //远程文件信息
                var value = _header.GetResponseHeader(DownloadHeaderKey);
                if (ulong.TryParse(value, out var totalSize))
                {
                    DownloadTotalSize = totalSize;
                }

                if (ReDownload)
                {
                    //读取未下载完成的文件信息
                    var tempInfo = new FileInfo(SavePath);
                    if (tempInfo.Exists)
                    {
                        _fileOriginLength = (ulong)tempInfo.Length;
                        if (_fileOriginLength == DownloadTotalSize)
                        {
                            DownloadedBytes = _fileOriginLength;
                            DownloadStatus = DownLoadStatus.VerifyingFile;
                            return TaskStatus.Running;
                        }
                    }
                }
                else
                {
                    _fileOriginLength = 0;
                }

                _content = UnityWebRequest.Get(DownloadPath);
                if (_fileOriginLength > 0)
                {
                    Debug.Log($"断点续传===={_fileOriginLength}");
#if UNITY_2019_1_OR_NEWER
                    _content.SetRequestHeader("Range", $"bytes={_fileOriginLength}-");
                    _content.downloadHandler = new DownloadHandlerFile(SavePath, true);
#else
                _request.DownloadedBytes = 0;
                _content.downloadHandler = new DownloadHandlerFile(TempPath);
#endif
                }
                else
                {
                    _content.downloadHandler = new DownloadHandlerFile(SavePath);
                }

                _content.certificateHandler = new DownloadCertificateHandler();
                _content.disposeDownloadHandlerOnDispose = true;
                _content.disposeCertificateHandlerOnDispose = true;
                _content.disposeUploadHandlerOnDispose = true;
                _content.SendWebRequest();
                DownloadStatus = DownLoadStatus.Download;
            }

            if (DownloadStatus == DownLoadStatus.Download)
            {
                DownloadedBytes = _fileOriginLength + _content.downloadedBytes;
                if (!_content.isDone)
                {
                    CheckTimeout();
                    return TaskStatus.Running;
                }

                bool hasError = false;
                // 检查网络错误
#if UNITY_2020_3_OR_NEWER
                if (_content.result != UnityWebRequest.Result.Success)
                {
                    hasError = true;
                    _errorMsg = _content.error;
                    ResponseCode = _content.responseCode;
                }
#else
				if (_content.isNetworkError || _content.isHttpError)
				{
					hasError = true;
					_errorMsg = _content.error;
					ResponseCode = _content.responseCode;
				}
#endif
                // 如果网络异常
                if (hasError)
                {
                    RetryCount++;
                    if (RetryCount <= RetryDownloadCount)
                    {
                        Debug.Log($"重新开始下载={DownloadPath}");
                        //重新开始下载
                        DownloadStatus = DownLoadStatus.PrepareDownload;
                    }
                    else
                    {
                        //重试后还是网络错误，直接失败
                        Debug.Log("重试后还是网络错误，直接失败");
                        DownloadStatus = DownLoadStatus.Failed;
                    }
                }
                else
                {
                    DownloadStatus = DownLoadStatus.VerifyingFile;
                }

                Dispose();
            }

            if (DownloadStatus == DownLoadStatus.VerifyingFile)
            {
                var tryPass = false;
                var tempInfo = new FileInfo(SavePath);

                if (tempInfo.Exists)
                {
                    if (tempInfo.Length == (long)DownloadTotalSize)
                    {
                        tryPass = true;
                    }
                    else
                    {
                        _errorMsg = "file size error";
                    }

                    if (!string.IsNullOrEmpty(FileHash))
                    {
                        var hash = Util.ComputeHash(SavePath);
                        if (FileHash.Equals(hash))
                        {
                            tryPass = true;
                        }
                        else
                        {
                            _errorMsg = "file hash error";
                        }
                    }
                }
                else
                {
                    _errorMsg = "file not exists";
                }

                if (!tryPass)
                {
                    // 验证失败后删除文件
                    if (File.Exists(SavePath))
                        File.Delete(SavePath);
                    Debug.Log("验证失败后删除文件，尝试重新下载");
                    //重新下载
                    DownloadStatus = DownLoadStatus.PrepareDownload;
                }
                else
                {
                    DownloadStatus = DownLoadStatus.Success;
                }
            }

            if (DownloadStatus == DownLoadStatus.Success)
            {
                return TaskStatus.Success;
            }

            if (DownloadStatus == DownLoadStatus.Failed)
            {
                return TaskStatus.Fail;
            }

            return TaskStatus.Running;
        }


        private void CheckTimeout()
        {
            if (_isAbort) return;

            if (_latestDownloadBytes != DownloadedBytes)
            {
                _latestDownloadBytes = DownloadedBytes;
                _latestDownloadRealtime = Time.realtimeSinceStartup;
            }

            float offset = Time.realtimeSinceStartup - _latestDownloadRealtime;
            if (offset > Const.DownloadTimeOut)
            {
                _content.Abort();
                _isAbort = true;
            }
        }

        private void Dispose()
        {
            if (_header != null)
            {
                _header.Dispose();
                _header = null;
            }

            if (_content != null)
            {
                _content.Dispose();
                _content = null;
            }
        }
    }
}