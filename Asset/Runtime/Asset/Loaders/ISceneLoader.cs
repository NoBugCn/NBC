namespace NBC.Asset
{
    public interface ISceneLoader
    {
        void Start(SceneProvider provider);
        void Update();
        void WaitForAsyncComplete();
        void Destroy();
    }
}