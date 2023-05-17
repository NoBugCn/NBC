namespace NBC.Asset
{
    public class BundleInfo
    {
        public readonly BundleData Bundle;
        public BundleLoadMode LoadMode;


        public BundleInfo(BundleData bundleData)
        {
            Bundle = bundleData;
        }

        /// <summary>
        /// 资源地址
        /// </summary>
        public string BundlePath
        {
            get
            {
                if (LoadMode == BundleLoadMode.LoadFromStreaming)
                {
                    return Bundle.StreamingFilePath;
                }

                if (LoadMode == BundleLoadMode.LoadFromCache)
                {
                    return Bundle.CachedDataFilePath;
                }

                if (LoadMode == BundleLoadMode.LoadFromRemote)
                {
                    return Bundle.RemoteDataFilePath;
                }

                return string.Empty;
            }
        }
    }
}