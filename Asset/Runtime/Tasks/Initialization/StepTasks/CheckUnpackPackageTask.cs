using System.IO;

namespace NBC.Asset
{
    /// <summary>
    /// 检查是否需要解压package清单文件
    /// </summary>
    public class CheckUnpackPackageTask : NTask
    {
        private readonly ParallelTaskCollection _taskList = new ParallelTaskCollection();
        private bool _download;

        public CheckUnpackPackageTask(bool download = false)
        {
            _download = download;
        }

        protected override void OnStart()
        {
            var p = Const.GetCachePath(Const.VersionFileName);
            var versionData = Util.ReadJson<VersionData>(Const.GetCachePath(Const.VersionFileName));
            if (versionData != null)
            {
                var cachePath = Const.GetCachePath(versionData.NameHash);
                if (!File.Exists(cachePath))
                {
                    _taskList.AddTask(new UnpackFileTask(versionData.NameHash, _download));
                }
            }

            _taskList.Run(TaskRunner.Def);
        }

        protected override TaskStatus OnProcess()
        {
            return _taskList.IsDone ? _taskList.Status : TaskStatus.Running;
        }
    }
}