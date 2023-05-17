namespace NBC.Asset
{
    public interface IAssetLoader
    {
        void Start(AssetProvider provider);
        void Update();
        void WaitForAsyncComplete();
        void Destroy();
    }
}