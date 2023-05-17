using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NBC.Asset.Editor
{
    [Id(TaskId.BuildBundle)]
    public class BuildBundleTask : BuildTask
    {
        private BuildContext _context;

        public override void Run(BuildContext context)
        {
            _context = context;
            var bundleDataList = _context.GenBundles();
            List<AssetBundleBuild> builds = _context.GetAssetBundleBuilds();
            EditUtil.CreateDirectory(BuildSettings.PlatformCachePath);
            var manifest = BuildAssetBundles(BuildSettings.PlatformCachePath, builds.ToArray(),
                BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
            if (manifest != null)
            {
                var bundles = manifest.GetAllAssetBundles();
                foreach (var bundleName in bundles)
                {
                    var bundle = _context.GetBundle(bundleName);
                    if (bundle != null)
                    {
                        var path = BuildSettings.GetCachePath(bundleName);
                        var hash = Util.ComputeHash(path);
                        bundle.Dependencies = manifest.GetAllDependencies(bundleName);
                        bundle.Hash = hash;
                        bundle.Size = Util.GetFileSize(path);
                    }
                }
            }

            Histories.AddHistory(bundleDataList);
            _context.SaveBundles();
        }

        private static AssetBundleManifest BuildAssetBundles(string outputPath, AssetBundleBuild[] builds,
            BuildAssetBundleOptions options, BuildTarget target)
        {
            var manifest = BuildPipeline.BuildAssetBundles(outputPath, builds, options, target);

            return manifest;
        }
    }
}