using System.Collections.Generic;

namespace NBC.Asset.Editor
{
    [Bind(BundleMode.File)]
    public class GatherFile : GatherBase
    {
        protected override BuildAsset[] Execute()
        {
            List<BuildAsset> assets = GetAssets();
            foreach (var asset in assets)
            {
                asset.Bundle = GetBundleName(asset);
            }

            return assets.ToArray();
        }
    }
}