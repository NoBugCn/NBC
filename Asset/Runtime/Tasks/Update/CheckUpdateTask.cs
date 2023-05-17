using System;
using System.Collections.Generic;

namespace NBC.Asset
{
    /// <summary>
    /// 检查更新内容任务
    /// </summary>
    public class CheckUpdateTask : NTask
    {
        public readonly UpdateContext Context = new UpdateContext();

        enum Step
        {
            /// <summary>
            /// 获取远程版本清单
            /// </summary>
            GetVersionData,

            /// <summary>
            /// 检测版本清单
            /// </summary>
            CheckVersionData,

            /// <summary>
            /// 获取包清单
            /// </summary>
            GetPackageData,

            /// <summary>
            /// 检查包清单
            /// </summary>
            CheckPackageData,
            Success,
        }

        private Step _step = Step.GetVersionData;

        private DownloadFileTask _getVersionFileTask;

        private DownloadFileTask _getPackageTask;

        protected override TaskStatus OnProcess()
        {
            if (Const.Simulate) return TaskStatus.Success;
            if (_step == Step.GetVersionData)
            {
                if (_getVersionFileTask == null)
                {
                    _getVersionFileTask = new DownloadFileTask(Const.GetRemotePath(Const.VersionFileName),
                        Const.GetCacheTempPath(Const.VersionFileName));
                    _getVersionFileTask.ReDownload = false;
                    _getVersionFileTask.Run();
                }

                if (!_getVersionFileTask.IsDone) return TaskStatus.Running;
                _step = Step.CheckVersionData;
            }

            if (_step == Step.CheckVersionData)
            {
                CheckVersionData();
            }

            if (_step == Step.GetPackageData)
            {
                if (Context.NewVersionData == null)
                {
                    _step = Step.Success;
                    return TaskStatus.Running;
                }

                if (_getPackageTask == null)
                {
                    var newVersionData = Context.NewVersionData;
                    var fileName = newVersionData.NameHash;
                    _getPackageTask = new DownloadFileTask(Const.GetRemotePath(fileName),
                        Const.GetCacheTempPath(fileName), newVersionData.Hash);
                    _getPackageTask.Run(TaskRunner.Def);
                }

                if (!_getPackageTask.IsDone) return TaskStatus.Running;
                _step = Step.CheckPackageData;
            }

            if (_step == Step.CheckPackageData)
            {
                //检查需要下载的bundle信息
                CheckPackageData();
            }


            return TaskStatus.Success;
        }

        private void CheckVersionData()
        {
            var newVersionData = Util.ReadJson<VersionData>(Const.GetCacheTempPath(Const.VersionFileName));
            var nowVersionData = Addressable.GetVersionData();
            if (newVersionData != null)
            {
                if (newVersionData.Hash == nowVersionData.Hash && newVersionData.Size == nowVersionData.Size)
                {
                    //没有变化，不需要检查
                    _step = Step.Success;
                    return;
                }

                Context.NewVersionData = newVersionData;
            }

            _step = Step.GetPackageData;
        }


        /// <summary>
        /// 检查资源包需要更新的bundle
        /// </summary>
        private void CheckPackageData()
        {
            var fileName = Context.NewVersionData.NameHash;
            var versionPackageData = Util.ReadJson<VersionPackageData>(Const.GetCacheTempPath(fileName));
            if (versionPackageData != null)
            {
                foreach (var package in versionPackageData.Packages)
                {
                    var can = package.Def == 1 || Assets.IsNeedfulPackage(package.Name);
                    //不需要检测的包直接跳过
                    if (!can) return;
                    var oldPackageData = Addressable.GetPackageData(package.Name);
                    var different = CompareBundles(package.Bundles, oldPackageData.Bundles);
                    if (different.Count > 0)
                    {
                        foreach (var data in different)
                        {
                            Context.NeedUpdateBundleList.Add(data);
                        }
                    }
                }
            }
        }

        private List<BundleData> CompareBundles(List<BundleData> newBundles, List<BundleData> oldBundles)
        {
            List<BundleData> list = new List<BundleData>();
            foreach (var bundle in newBundles)
            {
                var o = oldBundles.Find(b => b.Name == bundle.Name);
                if (o == null || o.Hash != bundle.Hash || o.Size != bundle.Size)
                {
                    list.Add(bundle);
                }
            }

            return list;
        }
    }
}