using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NBC.Asset.Editor
{
    /// <summary>
    /// 生成打包的清单文件
    /// </summary>
    [Id(TaskId.GenPackageData)]
    public class GenPackageDataTask : BuildTask
    {
        private BuildContext _context;

        public override void Run(BuildContext context)
        {
            _context = context;
            BuildPackagesData();
        }

        private void BuildPackagesData()
        {
            Dictionary<string, BuildPackage> dictionary = new Dictionary<string, BuildPackage>();
            foreach (var bundle in _context.Bundles)
            {
                var package = Path.GetDirectoryName(bundle.Name);
                if (string.IsNullOrEmpty(package)) continue;
                if (!dictionary.TryGetValue(package, out var buildPackage))
                {
                    buildPackage = new BuildPackage
                    {
                        Name = package,
                    };
                    dictionary[package] = buildPackage;
                }

                buildPackage.Bundles.Add(bundle);
            }

            var packages = CollectorSetting.Instance.Packages;
            foreach (var key in dictionary.Keys)
            {
                var p = packages.Find(p => p.Name == key);
                var package = dictionary[key];
                package.Size = package.Bundles.Sum(b => b.Size);
                package.Def = p.Default ? 1 : 0;
                var savePath = BuildSettings.GetCachePath($"{key}.json");
                Util.WriteJson(package, savePath);
            }

            // var packages = CollectorSetting.Instance.Packages;
            // foreach (var key in dictionary.Keys)
            // {
            //     var value = dictionary[key];
            //     var packageData = value.ToPackagesData();
            //     var savePath = BuildSettings.GetCachePath($"{key}.json");
            //     Util.WriteJson(packageData, savePath);
            // }
            //
            // foreach (var key in dictionary.Keys)
            // {
            //     var p = packages.Find(p => p.Name == key);
            //     var packPath = BuildSettings.GetCachePath($"{key}.json");
            //     VersionPackagesData versionPackagesData = new VersionPackagesData
            //     {
            //         Ver = DateTimeOffset.Now.ToUnixTimeSeconds().ToString(),
            //         Def = p.Default ? 1 : 0,
            //         Name = key,
            //         Hash = Util.ComputeHash(packPath),
            //         Size = Util.GetFileSize(packPath)
            //     };
            //     versionData.Packages.Add(versionPackagesData);
            // }
            //
            // var versionSavePath = BuildSettings.GetCachePath("version.json");
            // versionData.AppVer = UnityEditor.PlayerSettings.bundleVersion;
            // versionData.BuildTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            // Util.WriteJson(versionData, versionSavePath);
        }
    }
}