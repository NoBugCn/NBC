namespace NBC.Asset
{
    internal class AssetLoadFromDatabase : IAssetLoader
    {
        public static IAssetLoader CreateInstance()
        {
            return new AssetLoadFromDatabase();
        }

        private AssetProvider _provider;

        public void Start(AssetProvider provider)
        {
            _provider = provider;
        }

        public void Update()
        {
#if UNITY_EDITOR
            var assetInfo = _provider.AssetInfo;
            var path = assetInfo.Path;
            string guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid))
            {
                _provider.SetStatus(TaskStatus.Success, $"Not found asset : {path}");
                return;
            }

            UnityEngine.Object obj;
            if (assetInfo.AssetType == null)
                obj = UnityEditor.AssetDatabase.LoadMainAssetAtPath(assetInfo.Path);
            else
                obj = UnityEditor.AssetDatabase.LoadAssetAtPath(assetInfo.Path, assetInfo.AssetType);

            if (obj == null)
            {
                _provider.SetStatus(TaskStatus.Fail);
            }
            else
            {
                _provider.Asset = obj;
                _provider.SetStatus(TaskStatus.Success);
            }
#endif
        }

        public void WaitForAsyncComplete()
        {
            Update();
        }

        public void Destroy()
        {
        }
    }
}