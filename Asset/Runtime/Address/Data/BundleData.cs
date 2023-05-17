using System;
using System.Collections.Generic;

namespace NBC.Asset
{
    [Serializable]
    public class BundleData
    {
        public string Name;
        public string Hash;
        public int Size;

        // /// <summary>
        // /// 加载方法
        // /// </summary>
        // public byte LoadMethod;

        /// <summary>
        /// 资源包的分类标签
        /// </summary>
        public string[] Tags;

        /// <summary>
        /// 依赖的bundleId
        /// </summary>
        public List<int> Deps = new List<int>();

        /// <summary>
        /// 所属的包裹名称
        /// </summary>
        public string PackageName { set; get; }

        public List<string> DependBundles { get; private set; } = new List<string>();

        private string _nameHash = string.Empty;

        public string NameHash
        {
            get
            {
                if (!string.IsNullOrEmpty(_nameHash)) return _nameHash;
                _nameHash = Util.NameAddHash(Name, Hash);
                return _nameHash;
            }
        }


        /// <summary>
        /// 内置文件路径
        /// </summary>
        private string _streamingFilePath;

        public string StreamingFilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_streamingFilePath) == false)
                    return _streamingFilePath;

                _streamingFilePath = Const.GetStreamingPath(NameHash);
                return _streamingFilePath;
            }
        }

        /// <summary>
        /// 缓存的数据文件路径
        /// </summary>
        private string _cachedDataFilePath;

        public string CachedDataFilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_cachedDataFilePath) == false)
                    return _cachedDataFilePath;
                _cachedDataFilePath = Const.GetCachePath(NameHash);
                return _cachedDataFilePath;
            }
        }

        /// <summary>
        /// 远程的数据文件路径
        /// </summary>
        private string _remoteDataFilePath;

        public string RemoteDataFilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_remoteDataFilePath) == false)
                    return _remoteDataFilePath;
                _remoteDataFilePath = Const.GetRemotePath(NameHash);
                return _remoteDataFilePath;
            }
        }

        /// <summary>
        /// 临时的数据文件路径
        /// </summary>
        private string _tempDataFilePath;

        public string TempDataFilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_tempDataFilePath) == false)
                    return _tempDataFilePath;

                _tempDataFilePath = $"{CachedDataFilePath}.temp";
                return _tempDataFilePath;
            }
        }
    }
}