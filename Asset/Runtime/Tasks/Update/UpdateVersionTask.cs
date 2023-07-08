using System.Collections.Generic;
using System.IO;

namespace NBC.Asset
{
    /// <summary>
    /// 更新版本内容任务
    /// </summary>
    public class UpdateVersionTask : DownloadTaskBase
    {
        private UpdateContext _context;
        private SequenceTaskCollection _sequence = new SequenceTaskCollection();
        private ParallelTaskCollection _downloadParallel = new ParallelTaskCollection();
        public override float Progress => _downloadParallel.Progress;
        
        public UpdateVersionTask(UpdateContext context)
        {
            _context = context;
        }

        protected override void OnStart()
        {
            _sequence.FailBreak = true;
            _downloadParallel.ParallelNum = 5;
            _downloadParallel.FailBreak = true;
            var bundles = _context.NeedUpdateBundleList;
            if (bundles != null && bundles.Count > 0)
            {
                foreach (var bundle in bundles)
                {
                    _downloadParallel.AddTask(new DownloadFileTask(bundle.RemoteDataFilePath,
                        bundle.CachedDataFilePath));
                }

                _sequence.AddTask(_downloadParallel);
            }

            _sequence.AddTask(new RunFunctionTask(TryCoverNewVersionData));
            _sequence.AddTask(new RunFunctionTask(Addressable.Load));

            _sequence.Run(TaskRunner.DownloadRunner);
        }

        protected override TaskStatus OnProcess()
        {
            return _sequence.IsDone ? _sequence.Status : TaskStatus.Running;
        }

        /// <summary>
        /// 尝试覆盖旧的版本清单文件
        /// </summary>
        private void TryCoverNewVersionData()
        {
            if (_context.NewVersionData != null)
            {
                var nameHash = _context.NewVersionData.NameHash;
                //覆盖旧的清单文件
                File.Copy(Const.GetCacheTempPath(nameHash), Const.GetCachePath(nameHash), true);
            }

            //覆盖旧的version.json
            File.Copy(Const.GetCacheTempPath(Const.VersionFileName), Const.GetCachePath(Const.VersionFileName), true);
        }
    }
}