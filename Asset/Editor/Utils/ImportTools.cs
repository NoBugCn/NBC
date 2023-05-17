using UnityEditor;

namespace NBC.Asset.Editor
{
    public class ImportAsset : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            Builder.Gather();
        }
    }
}