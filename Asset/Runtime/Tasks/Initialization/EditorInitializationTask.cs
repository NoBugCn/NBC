namespace NBC.Asset
{
    /// <summary>
    /// 编辑器初始化任务
    /// </summary>
    internal sealed class EditorInitializationTask : InitializationTask
    {
        public override float Progress => _taskList.Progress;
        private readonly SequenceTaskCollection _taskList = new SequenceTaskCollection();

        protected override void OnStart()
        {
            _taskList.AddTask(new RunFunctionTask(Addressable.Load));
            _taskList.Run(TaskRunner.Def);
        }

        protected override TaskStatus OnProcess()
        {
            return _taskList.IsDone ? TaskStatus.Success : TaskStatus.Running;
        }
    }
}