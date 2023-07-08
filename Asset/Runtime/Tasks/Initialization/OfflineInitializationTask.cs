namespace NBC.Asset
{
    /// <summary>
    /// 离线模式初始化任务
    /// </summary>
    internal sealed class OfflineInitializationTask : InitializationTask
    {
        public override float Progress => _taskList.Progress;
        private readonly SequenceTaskCollection _taskList = new SequenceTaskCollection();

        protected override void OnStart()
        {
            _taskList.AddTask(new CheckCoverInstallTask());
            _taskList.AddTask(new UnpackVersionTask());
            _taskList.AddTask(new CheckUnpackPackageTask());
            _taskList.AddTask(new RunFunctionTask(Addressable.Load));
            _taskList.Run(TaskRunner.Def);
        }

        protected override TaskStatus OnProcess()
        {
            return _taskList.IsDone ? _taskList.Status : TaskStatus.Running;
        }
    }
}