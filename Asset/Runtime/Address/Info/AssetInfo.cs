using System;

namespace NBC.Asset
{
    public class AssetInfo
    {
        private readonly AssetData _assetData;

        // public AssetData Data => _assetData;

        /// <summary>
        /// 资源路径
        /// </summary>
        public string Path { private set; get; }

        /// <summary>
        /// 资源类型
        /// </summary>
        public Type AssetType { private set; get; }


        private string _providerGUID;

        /// <summary>
        /// 唯一标识符
        /// </summary>
        public string GUID
        {
            get
            {
                if (!string.IsNullOrEmpty(_providerGUID))
                    return _providerGUID;

                _providerGUID = Util.GetAssetGUID(Path, AssetType);
                return _providerGUID;
            }
        }

        public AssetInfo(AssetData assetData, System.Type assetType)
        {
            if (assetData == null)
                throw new Exception("assetData is null!");

            _assetData = assetData;
            AssetType = assetType;
            Path = assetData.Path;
        }

        public AssetInfo(AssetData assetData)
        {
            if (assetData == null)
                throw new System.Exception("assetData is null!");

            _assetData = assetData;
            AssetType = null;
            Path = assetData.Path;
        }

        public bool HasTag(string[] tags)
        {
            return _assetData.HasTag(tags);
        }
    }
}