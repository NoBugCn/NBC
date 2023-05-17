using System;

namespace NBC.Asset
{
    public class DownloadTaskBase : NTask
    {
        public virtual void Run()
        {
            Run(TaskRunner.DownloadRunner);
        }
    }
}