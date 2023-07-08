using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NBC.Asset.Editor
{
    public static class HistoryUtil
    {
        public static void CopyToStreamingAssets(this VersionHistoryData versionHistory)
        {
            if (versionHistory == null) return;
            try
            {
                var streamingAssetsPath = $"{Application.streamingAssetsPath}/";
                if (Directory.Exists(streamingAssetsPath))
                    Directory.Delete(streamingAssetsPath, true);
                Directory.CreateDirectory(streamingAssetsPath);
                versionHistory.CopyToFolder(streamingAssetsPath);
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError("copy version is null");
            }
        }

        public static void CopyToFolder(this VersionHistoryData versionHistory, string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Debug.LogError("copy error，Folder not exists");
                return;
            }

            var versionData = versionHistory.VersionData;
            var versionPackageName = versionData.NameHash;
            var filePath = BuildSettings.GetBuildPath(versionPackageName);
            EditUtil.CopyToFolder(filePath, folderPath + versionPackageName);
            // Const.GetStreamingPath(versionPackageName));
            foreach (var package in versionHistory.Packages)
            {
                foreach (var bundle in package.Bundles)
                {
                    //Const.GetStreamingPath(bundleNameAddHash)
                    var bundleNameAddHash = Util.NameAddHash(bundle.Name, bundle.Hash);
                    EditUtil.CopyToFolder(BuildSettings.GetBuildPath(bundleNameAddHash),
                        folderPath + bundleNameAddHash);
                }
            }

            Util.WriteJson(versionData, folderPath + "version.json"); //Const.GetStreamingPath("version.json"));
        }

        public static void ShowLastBuildInfo()
        {
            var last = GetLastBuildCompareVersion();
            var simpleData = last.SimpleChangeData;
            var addCount = simpleData.PackageAddBundle.Values.Sum(v => v.Count);
            var changeCount = simpleData.PackageChangeBundle.Values.Sum(v => v.Count);
            var removeCount = simpleData.PackageRemoveBundle.Values.Sum(v => v.Count);
            var tips = string.Format(Language.BuildSuccessTips, Util.GetFriendlySize(simpleData.DownloadSize), addCount,
                changeCount,
                removeCount);
            Debug.Log(tips);
            EditorUtility.DisplayDialog(Language.Tips, tips, Language.Confirm);
        }

        /// <summary>
        /// 获取最后一次构建和上次构建变化内容
        /// </summary>
        /// <returns></returns>
        public static VersionChangeData GetLastBuildCompareVersion()
        {
            var versions = GetHistoryVersions();
            VersionHistoryData newHistoryData = null;
            VersionHistoryData oldHistoryData = null;
            if (versions != null)
            {
                if (versions.Count > 0)
                {
                    newHistoryData = versions[0];
                }

                if (versions.Count > 1)
                {
                    oldHistoryData = versions[1];
                }
            }

            return CompareVersion(newHistoryData, oldHistoryData);
        }

        /// <summary>
        /// 比较版本相差内容
        /// </summary>
        /// <param name="newHistoryData">更新的版本</param>
        /// <param name="oldHistoryData">更老的版本</param>
        public static VersionChangeData CompareVersion(VersionHistoryData newHistoryData,
            VersionHistoryData oldHistoryData)
        {
            List<BundleData> oldBundles = new List<BundleData>();
            if (oldHistoryData != null)
            {
                foreach (var package in oldHistoryData.Packages)
                {
                    foreach (var bundle in package.Bundles)
                    {
                        oldBundles.Add(bundle);
                    }
                }
            }

            List<BundleData> newBundles = new List<BundleData>();
            if (newHistoryData != null)
            {
                foreach (var package in newHistoryData.Packages)
                {
                    foreach (var bundle in package.Bundles)
                    {
                        newBundles.Add(bundle);
                    }
                }
            }

            VersionChangeData ret = new VersionChangeData();

            //查找新增和修改的bundle
            foreach (var bundle in newBundles)
            {
                var old = oldBundles.Find(b => b.Name == bundle.Name);
                if (old != null)
                {
                    if (old.Hash != bundle.Hash || old.Size != bundle.Size)
                    {
                        ret.Change(bundle, VersionChangeData.TypeEnum.Change);
                    }
                }
                else
                {
                    ret.Change(bundle, VersionChangeData.TypeEnum.Add);
                }
            }

            //查找删除的bundle
            foreach (var bundle in oldBundles)
            {
                var old = newBundles.Find(b => b.Name == bundle.Name);
                if (old == null)
                {
                    ret.Change(bundle, VersionChangeData.TypeEnum.Remove);
                }
            }

            ret.Processing();
            ret.NewVersionName = newHistoryData?.ShowName;
            ret.OldVersionName = oldHistoryData?.ShowName;

            return ret;
        }

        /// <summary>
        /// 获取最后一个历史记录
        /// </summary>
        /// <returns></returns>
        public static VersionHistoryData GetLastVersionHistory()
        {
            var history = GetHistoryVersions();
            if (history != null && history.Count > 0)
            {
                return history[0];
            }

            return null;
        }

        /// <summary>
        /// 获取所有历史记录
        /// </summary>
        public static List<VersionHistoryData> GetHistoryVersions()
        {
            var platformPath = BuildSettings.PlatformPath;
            List<VersionHistoryData> ret = new List<VersionHistoryData>();
            DirectoryInfo root = new DirectoryInfo(platformPath);
            if (root.Exists)
            {
                FileInfo[] files = root.GetFiles();
                List<string> filePaths = new List<string>();
                foreach (var file in files)
                {
                    if (file.Exists)
                    {
                        var ext = Path.GetExtension(file.FullName);
                        var fileName = Path.GetFileName(file.FullName);
                        if (ext.ToLower() == ".json" && fileName.StartsWith("version"))
                        {
                            filePaths.Add(file.FullName);
                        }
                    }
                }

                filePaths.Sort((a, b) =>
                {
                    var fName1 = Path.GetFileNameWithoutExtension(a);
                    var fName2 = Path.GetFileNameWithoutExtension(b);
                    var index1 = fName1.Replace("version_", "");
                    var index2 = fName2.Replace("version_", "");
                    int.TryParse(index1, out var i1);
                    int.TryParse(index2, out var i2);
                    return i1 - i2;
                });
                filePaths.Reverse();
                foreach (var file in filePaths)
                {
                    var json = File.ReadAllText(file);
                    var versionName = Path.GetFileName(file);
                    versionName = versionName.Replace(Path.GetExtension(file), "");
                    var version = JsonUtility.FromJson<VersionData>(json);
                    if (version == null) continue;
                    var historyData = new VersionHistoryData();
                    historyData.VersionData = version;
                    var showTime = Util.TimestampToTime(version.BuildTime);
                    historyData.FileName = Path.GetFileName(file);
                    historyData.ShowName = versionName + "_" + showTime.ToString("yyyy-MM-dd HH:mm:ss");

                    var fileName = version.NameHash;
                    var packages = Util.ReadJson<VersionPackageData>(BuildSettings.GetBuildPath(fileName));
                    if (packages != null)
                    {
                        foreach (var packageData in packages.Packages)
                        {
                            foreach (var bundle in packageData.Bundles)
                            {
                                bundle.PackageName = packageData.Name;
                            }

                            historyData.Packages.Add(packageData);
                        }
                    }

                    ret.Add(historyData);
                }
            }

            return ret;
        }

        /// <summary>
        /// 删除某个历史版本记录
        /// </summary>
        public static void DeleteHistoryVersions(string versionFileName)
        {
            VersionHistoryData needDeleteVersion = null;
            //所有bundle使用次数
            Dictionary<string, int> useDic = new Dictionary<string, int>();
            var versions = GetHistoryVersions();
            foreach (var version in versions)
            {
                if (version.FileName == versionFileName)
                {
                    needDeleteVersion = version;
                }

                var fileName = version.VersionData.NameHash;

                UseDic(useDic, fileName);

                foreach (var p in version.Packages)
                {
                    foreach (var bundle in p.Bundles)
                    {
                        var name = bundle.NameHash;
                        if (useDic.ContainsKey(name))
                        {
                            useDic[name]++;
                        }
                        else
                        {
                            useDic[name] = 1;
                        }
                    }
                }
            }

            if (needDeleteVersion != null)
            {
                List<string> canDeleteBundle = new List<string>();
                foreach (var p in needDeleteVersion.Packages)
                {
                    foreach (var bundle in p.Bundles)
                    {
                        var name = bundle.NameHash;
                        if (useDic.TryGetValue(name, out var count))
                        {
                            if (count <= 1) canDeleteBundle.Add(name);
                        }
                        else
                        {
                            canDeleteBundle.Add(name);
                        }
                    }
                }

                var platformPath = BuildSettings.PlatformPath;
                DeleteFile($"{platformPath}/{versionFileName}");
                var fileName = needDeleteVersion.VersionData.NameHash;
                if (useDic.TryGetValue(fileName, out var c))
                {
                    if (c <= 1)
                    {
                        DeleteFile($"{platformPath}/{fileName}");
                    }
                }

                foreach (var bundleName in canDeleteBundle)
                {
                    DeleteFile($"{platformPath}/{bundleName}");
                }
            }
        }

        private static void UseDic(Dictionary<string, int> dic, string name)
        {
            if (dic.ContainsKey(name))
            {
                dic[name]++;
            }
            else
            {
                dic[name] = 1;
            }
        }

        private static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"Delete File Path:{filePath}");
            }
        }
    }
}