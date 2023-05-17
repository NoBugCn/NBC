using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace NBC.Asset.Editor
{
    [FilePath("Assets/AssetCaches.asset")]
    public class Caches : ScriptableSingleton<Caches>
    {
        public List<BuildAsset> Assets = new List<BuildAsset>();
        public List<BuildBundle> Bundles = new List<BuildBundle>();

        public void ClearBundles()
        {
            Bundles.Clear();
            Save();
        }
        public void ClearAssets()
        {
            Assets.Clear();
            Save();
        }

        public void AddOrUpdate(ICollection<BuildAsset> assets)
        {
            foreach (var asset in assets)
            {
                AddOrUpdate(asset);
            }
        }

        public void AddOrUpdate(BuildAsset asset)
        {
            Assets.Add(asset);
        }

        public void AddOrUpdate(ICollection<BuildBundle> bundles)
        {
            foreach (var bundle in bundles)
            {
                AddBundle(bundle);
            }
        }

        public void AddBundle(BuildBundle bundle)
        {
            Bundles.Add(bundle);
        }

        public BuildBundle GetBundle(string name)
        {
            return Bundles.FirstOrDefault(bundle => bundle.Name == name);
        }


        public void Save()
        {
            EditorUtility.SetDirty(this);
        }
    }
}