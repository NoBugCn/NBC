using System.Collections.Generic;
using System.Linq;

namespace NBC.Asset
{
    public class UpdateContext
    {
        /// <summary>
        /// 需要更新的文件总大小
        /// </summary>
        public long DownloadTotalSize => NeedUpdateBundleList.Sum(data => data.Size);

        /// <summary>
        /// 需要更新的bundles
        /// </summary>
        public readonly HashSet<BundleData> NeedUpdateBundleList = new HashSet<BundleData>();

        // /// <summary>
        // /// 需要更新的package
        // /// </summary>
        // public readonly HashSet<string> NeedVersionPackages = new HashSet<string>();

        public VersionData NewVersionData;

    }
}