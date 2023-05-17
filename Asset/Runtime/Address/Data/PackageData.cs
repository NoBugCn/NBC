using System;
using System.Collections.Generic;

namespace NBC.Asset
{
    [Serializable]
    public class PackageData
    {
        public string Name;
        public int Def;
        public List<string> Dirs;
        public List<AssetData> Assets;
        public List<BundleData> Bundles;
    }
}