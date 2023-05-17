using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NBC.Asset.Editor
{
    [Id(TaskId.GenVersionData)]
    public class GenVersionDataTask : BuildTask
    {
        private BuildContext _context;

        public override void Run(BuildContext context)
        {
            _context = context;
            BuildVersionData();
        }


        private void BuildVersionData()
        {
            VersionData versionData = new VersionData();
            var versionPackageData = new VersionPackageData();
            var packages = CollectorSetting.Instance.Packages;
            // Dictionary<string, PackageTempData> dictionary = new Dictionary<string, PackageTempData>();
            foreach (var package in packages)
            {
                var packPath = BuildSettings.GetCachePath($"{package.Name}.json");
                var buildPackage = Util.ReadJson<BuildPackage>(packPath);
                if (buildPackage != null)
                {
                    var packageData = buildPackage.ToPackagesData();
                    versionPackageData.Packages.Add(packageData);
                }
            }

            var versionPackagesPath = BuildSettings.GetCachePath("packages.json");
            Util.WriteJson(versionPackageData, versionPackagesPath);

            versionData.Hash = Util.ComputeHash(versionPackagesPath);
            versionData.Size = Util.GetFileSize(versionPackagesPath);
            var versionSavePath = BuildSettings.GetCachePath(Const.VersionFileName);
            versionData.AppVer = UnityEditor.PlayerSettings.bundleVersion;
            versionData.BuildTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            Util.WriteJson(versionData, versionSavePath);
        }
    }
}