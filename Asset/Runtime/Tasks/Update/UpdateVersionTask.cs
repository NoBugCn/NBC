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

        public UpdateVersionTask(UpdateContext context)
        {
            _context = context;
        }

        protected override void OnStart()
        {
            ParallelTaskCollection downloadParallel = new ParallelTaskCollection();
            var bundles = _context.NeedUpdateBundleList;
            if (bundles != null && bundles.Count > 0)
            {
                foreach (var bundle in bundles)
                {
                    downloadParallel.AddTask(new DownloadFileTask(bundle.RemoteDataFilePath,
                        bundle.CachedDataFilePath));
                }
                
                _sequence.AddTask(downloadParallel);
            }
            
            _sequence.AddTask(new RunFunctionTask(TryCoverNewVersionData));
            _sequence.AddTask(new RunFunctionTask(Addressable.Load));

            _sequence.Run(TaskRunner.DownloadRunner);
        }

        protected override TaskStatus OnProcess()
        {
            return _sequence.IsDone ? TaskStatus.Success : TaskStatus.Running;
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