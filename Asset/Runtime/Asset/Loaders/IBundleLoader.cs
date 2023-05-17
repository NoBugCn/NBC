namespace NBC.Asset
{
    internal interface IBundleLoader 
    {
        void Start(BundledProvider provider);
        void Update();
        void WaitForAsyncComplete();
        void Destroy();
    }
}