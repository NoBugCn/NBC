using System;
using System.Collections.Generic;

namespace NBC.Asset
{
    // [Serializable]
    // public class VersionPackagesData
    // {
    //     public string Ver;
    //     public string Name;
    //     public string Hash;
    //     public int Size;
    //     public int Def;
    //     public string NameHash => $"{Name}_{Hash}.json";
    // }
    //
    // [Serializable]
    // public class VersionData
    // {
    //     /// <summary>
    //     /// app版本号
    //     /// </summary>
    //     public string AppVer;
    //
    //     /// <summary>
    //     /// 版本包
    //     /// </summary>
    //     public List<VersionPackagesData> Packages = new List<VersionPackagesData>();
    //
    //     /// <summary>
    //     /// 导出时间
    //     /// </summary>
    //     public long BuildTime;
    // }

    [Serializable]
    public class VersionPackageData
    {
        public List<PackageData> Packages = new List<PackageData>();
    }

    [Serializable]
    public class VersionData
    {
        /// <summary>
        /// app版本号
        /// </summary>
        public string AppVer;

        /// <summary>
        /// 版本包hash
        /// </summary>
        public string Hash;

        public long Size;
        /// <summary>
        /// 导出时间
        /// </summary>
        public long BuildTime;

        public string NameHash => $"packages_{Hash}.json";
    }
}