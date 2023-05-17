namespace NBC.Asset
{
    /// <summary>
    /// 线上模式初始化任务
    /// 优先从persistentData读取，如果没有，则从StreamingAssets拷贝清单文件到解压目录
    /// </summary>
    internal sealed class OnlineInitializationTask : InitializationTask
    {
        public override float Progress => _taskList.Progress;
        private readonly SequenceTaskCollection _taskList = new SequenceTaskCollection();

        protected override void OnStart()
        {
            _taskList.AddTask(new CheckCoverInstallTask());
            _taskList.AddTask(new UnpackFileTask(Const.VersionFileName, true));
            _taskList.AddTask(new CheckUnpackPackageTask(true));
            _taskList.AddTask(new RunFunctionTask(Addressable.Load));
            _taskList.Run(TaskRunner.Def);
        }

        protected override TaskStatus OnProcess()
        {
            return _taskList.IsDone ? _taskList.Status : TaskStatus.Running;
        }
    }
}