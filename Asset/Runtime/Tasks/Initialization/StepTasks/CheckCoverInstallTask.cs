namespace NBC.Asset
{
    /// <summary>
    /// 检查是否覆盖安装相关操作
    /// </summary>
    public class CheckCoverInstallTask : NTask
    {
        protected override TaskStatus OnProcess()
        {
            return TaskStatus.Success;
        }
    }
}