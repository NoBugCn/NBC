namespace NBC.Asset
{
    /// <summary>
    /// 解压资源包任务
    /// </summary>
    public class UnpackPackagesTask : NTask
    {
        private readonly ParallelTaskCollection _taskList = new ParallelTaskCollection();

        public override float Progress => _taskList.Progress;

        protected override void OnStart()
        {
            _taskList.ParallelNum = 5;
            var bundles = Addressable.GetCanUnpackBundles();
            foreach (var bundle in bundles)
            {
                _taskList.AddTask(new UnpackFileTask(bundle.Bundle.NameHash));
            }

            _taskList.Run(TaskRunner.Def);
        }

        protected override TaskStatus OnProcess()
        {
            return _taskList.IsDone ? TaskStatus.Success : TaskStatus.Running;
        }
    }
}