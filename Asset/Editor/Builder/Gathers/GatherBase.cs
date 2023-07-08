using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NBC.Asset.Editor
{
    public abstract class GatherBase
    {
        protected PackageConfig PackageConfig;
        protected GroupConfig GroupConfig;

        public BuildAsset[] Run(PackageConfig packageConfig, GroupConfig groupConfig)
        {
            PackageConfig = packageConfig;
            GroupConfig = groupConfig;
            return Execute();
        }

        protected abstract BuildAsset[] Execute();

        protected string GetBundleName(BuildAsset asset, string bundleName = "")
        {
            if (string.IsNullOrEmpty(bundleName))
            {
                bundleName = asset.Path;
            }

            var settings = BuildSettings.Instance;
            bundleName = bundleName.Replace("\\", "/").Replace("/", "_").Replace(".", "_").ToLower();
            if (settings.ShaderBuildTogether && settings.ShaderExtensions.Exists(asset.Path.EndsWith))
            {
                bundleName = "shaders";
            }

            return $"{PackageConfig.Name.ToLower()}/{bundleName}{BuildSettings.Instance.BundlesExtension}";
        }

        #region GetAssets

        protected List<BuildAsset> GetAssets()
        {
            List<BuildAsset> assets = new List<BuildAsset>();

            var list = GroupConfig.Collectors;
            foreach (var obj in list)
            {
                var arr = GetAssets(obj, GroupConfig.Filter);
                if (arr != null && arr.Count > 0)
                {
                    assets.AddRange(arr);
                }
            }
            
            return assets;
        }

        protected List<BuildAsset> GetAssets(Object obj, string filter = "")
        {
            List<BuildAsset> retList = new List<BuildAsset>();
            var path = AssetDatabase.GetAssetPath(obj);
            if (Directory.Exists(path))
            {
                var assetGuids = AssetDatabase.FindAssets(filter, new[]
                {
                    path
                });
                foreach (var guid in assetGuids)
                {
                    var childPath = AssetDatabase.GUIDToAssetPath(guid);

                    if (string.IsNullOrEmpty(childPath) || Directory.Exists(childPath)) continue;
                    retList.Add(PathToBuildAsset(childPath, obj));
                }
            }
            else
            {
                //如果是对象则不做过滤。直接加入列表
                retList.Add(ObjectToBuildAsset(obj));
            }

            return retList;
        }


        protected BuildAsset ObjectToBuildAsset(Object collector)
        {
            var path = AssetDatabase.GetAssetPath(collector);
            return PathToBuildAsset(path, collector);
        }

        protected BuildAsset PathToBuildAsset(string path, Object collector)
        {
            var type = AssetDatabase.GetMainAssetTypeAtPath(path);
            var ret = new BuildAsset
            {
                Path = path,
                Address = ToAddressPath(path, collector),
                Type = type == null ? "Missing" : type.Name,
                Tags = GroupConfig.Tags,
                Group = GroupConfig,
                // Dependencies = GetDependencies(path)
            };
            return ret;
        }

        protected string[] GetDependencies(string path)
        {
            var ret = new HashSet<string>();
            ret.UnionWith(AssetDatabase.GetDependencies(path));
            ret.RemoveWhere(s => s.EndsWith(".unity"));
            ret.Remove(path);
            return ret.ToArray();
        }

        /// <summary>
        /// 转换文件地址为可寻址地址
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="collector"></param>
        /// <returns></returns>
        protected string ToAddressPath(string filePath, Object collector)
        {
            var mode = GroupConfig.AddressMode;
            var collectorName = collector != null ? collector.name : string.Empty;
            var path = AssetDatabase.GetAssetPath(collector);
            if (path == filePath)
            {
                collectorName = string.Empty;
            }

            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var groupName = GroupConfig.Name;
            var packageName = PackageConfig.Name;
            switch (mode)
            {
                case AddressMode.None:
                    return filePath;
                case AddressMode.FileName:
                    return fileName;
                case AddressMode.GroupAndFileName:
                    return $"{groupName}/{fileName}";
                case AddressMode.PackAndGroupAndFileName:
                    return $"{packageName}/{groupName}/{fileName}";
                case AddressMode.PackAndFileName:
                    return $"{packageName}/{fileName}";
                case AddressMode.CollectorAndFileName:
                    return string.IsNullOrEmpty(collectorName) ? fileName : $"{collectorName}/{fileName}";
                case AddressMode.PackAndCollectorAndFileName:
                    return string.IsNullOrEmpty(collectorName)
                        ? $"{packageName}/{fileName}"
                        : $"{packageName}/{collectorName}/{fileName}";
            }

            return filePath;
        }

        #endregion
    }
}