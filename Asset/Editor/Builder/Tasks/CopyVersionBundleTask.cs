using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NBC.Asset.Editor
{
    /// <summary>
    /// 将生成的版本保存为可用的历史记录
    /// </summary>
    [Id(TaskId.CopyVersionBundle)]
    public class CopyVersionBundleTask : BuildTask
    {
        private Dictionary<string, string> _copyPaths = new Dictionary<string, string>();

        public override void Run(BuildContext context)
        {
            TryCopy();
        }

        private void TryCopy()
        {
            var versionPath = BuildSettings.GetCachePath(Const.VersionFileName);
            var versionData = Util.ReadJson<VersionData>(versionPath);

            if (IsChange(versionData))
            {
                var versionPackagePath = BuildSettings.GetCachePath("packages.json");
                var lastHistoryData = Histories.LastHistoryData;
                _copyPaths.Add(versionPath, BuildSettings.GetBuildPath($"version_{lastHistoryData.Index}.json"));
                _copyPaths.Add(versionPackagePath, BuildSettings.GetBuildPath(versionData.NameHash));
                var versionPackageData = Util.ReadJson<VersionPackageData>(versionPackagePath);
                foreach (var package in versionPackageData.Packages)
                {
                    foreach (var bundle in package.Bundles)
                    {
                        var bundleNameAddHash = Util.NameAddHash(bundle.Name, bundle.Hash);
                        _copyPaths.Add(BuildSettings.GetCachePath(bundle.Name),
                            BuildSettings.GetBuildPath(bundleNameAddHash));
                    }
                }

                CopyAll();
            }
        }

        private void CopyAll()
        {
            foreach (var path in _copyPaths.Keys)
            {
                var toPath = _copyPaths[path];
                if (File.Exists(path))
                {
                    EditUtil.CreateDirectory(Path.GetDirectoryName(toPath));
                    if (File.Exists(toPath)) File.Delete(toPath);
                    File.Copy(path, toPath);
                }
            }
        }

        private bool IsChange(VersionData versionData)
        {
            var lastVersionData = Histories.GetLastVersion();
            if (lastVersionData != null)
            {
                if (versionData.Size != lastVersionData.Size || versionData.Hash != lastVersionData.Hash)
                {
                    return true;
                }

                return false;
            }

            return true;
        }
    }
}