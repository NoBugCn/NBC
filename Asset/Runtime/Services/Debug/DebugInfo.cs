using System;
using System.Collections.Generic;

namespace NBC.Asset
{
#if DEBUG
    [Serializable]
    public class DebugBaseInfo
    {
        public int Ref;
        public string LoadScene;
        public string LoadTime;
        public long LoadTotalTime;
        public string Status;
    }

    [Serializable]
    public class DebugAssetInfo : DebugBaseInfo
    {
        public string Path;
        public string Type;
        public bool IsAll;
        public List<string> Dependency = new List<string>();
    }

    [Serializable]
    public class DebugBundleInfo : DebugBaseInfo
    {
        public string BundleName;
    }

    [Serializable]
    public class DebugInfo
    {
        public int Frame;
        public List<DebugAssetInfo> AssetInfos = new List<DebugAssetInfo>();
        public List<DebugBundleInfo> BundleInfos = new List<DebugBundleInfo>();
    }
    
#endif
}