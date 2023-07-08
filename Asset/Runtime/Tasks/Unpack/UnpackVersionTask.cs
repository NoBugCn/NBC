using System.IO;

namespace NBC.Asset
{
    public class UnpackVersionTask : NTask
    {
        private readonly bool _download;
        private readonly string _savePath;
        private UnpackFileTask _unpackFileTask;

        public UnpackVersionTask(bool download = false)
        {
            _savePath = Const.GetCachePath(Const.VersionFileName);
            _download = download;
        }

        protected override void OnStart()
        {
            if (File.Exists(_savePath))
            {
                Finish();
            }
            else
            {
                _unpackFileTask = new UnpackFileTask(Const.VersionFileName, _download);
                _unpackFileTask.Run(TaskRunner.Def);
            }
        }

        protected override TaskStatus OnProcess()
        {
            if (_unpackFileTask != null)
            {
                return _unpackFileTask.Status;
            }

            return TaskStatus.Success;
        }
    }
}