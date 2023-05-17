using System.Collections.Generic;

namespace NBC.Asset
{
    /// <summary>
    /// 下载当前版本资源包任务
    /// </summary>
    public class DownloadBundlesTask : DownloadTaskBase
    {
        private readonly SequenceTaskCollection _taskList = new SequenceTaskCollection();
        private readonly List<BundleInfo> _downloadBundles;

        public DownloadBundlesTask(List<BundleInfo> downloadBundles)
        {
            _downloadBundles = downloadBundles;
        }

        protected override void OnStart()
        {
            foreach (var bundle in _downloadBundles)
            {
                if (bundle.LoadMode != BundleLoadMode.LoadFromRemote) continue;
                var bundleData = bundle.Bundle;
                _taskList.AddTask(new DownloadFileTask(bundleData.RemoteDataFilePath, bundleData.CachedDataFilePath,
                    bundleData.Hash));
            }

            _taskList.Run(TaskRunner.DownloadRunner);
        }

        protected override TaskStatus OnProcess()
        {
            return _taskList.IsDone ? TaskStatus.Success : TaskStatus.Running;
        }
    }
}