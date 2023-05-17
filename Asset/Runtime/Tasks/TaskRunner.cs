namespace NBC.Asset
{
    public static class TaskRunner
    {
        #region Static

        public static readonly DownloadRunner DownloadRunner = new DownloadRunner();
        public static readonly ProviderRunner ProviderRunner = new ProviderRunner();
        public static readonly Runner Def = new Runner();

        
        public static void Update()
        {
            DownloadRunner.Process();
            ProviderRunner.Process();
            Def.Process();
        }

        #endregion
    }
}