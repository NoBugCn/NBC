using System.Collections.Generic;

namespace NBC.Asset.Editor
{
    [Bind(BundleMode.Single)]
    public class GatherSingle : GatherBase
    {
        protected override BuildAsset[] Execute()
        {
            List<BuildAsset> assets = GetAssets();
            foreach (var asset in assets)
            {
                asset.Bundle = GetBundleName(asset, GroupConfig.Name);
            }

            return assets.ToArray();
        }
    }
}