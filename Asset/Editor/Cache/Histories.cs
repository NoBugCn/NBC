using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace NBC.Asset.Editor
{
    [Serializable]
    public class HistoryData
    {
        public int Index;
        public List<BuildBundle> Bundles = new List<BuildBundle>();
    }

    [Serializable]
    public class HistoriesData
    {
        private int _historyCount = 5;
        public List<HistoryData> Histories = new List<HistoryData>();

        public HistoryData LastHistoryData => Histories.Last();

        public int LastIndex
        {
            get
            {
                var index = 0;
                if (Histories.Count > 0)
                {
                    index = Histories[^1].Index;
                }

                return index;
            }
        }

        public bool AddHistory(List<BuildBundle> bundles)
        {
            var data = new HistoryData();

            foreach (var bundle in bundles)
            {
                var b = new BuildBundle
                {
                    Name = bundle.Name,
                    Hash = bundle.Hash,
                    Size = bundle.Size
                };
                data.Bundles.Add(b);
            }

            if (CanAdd(data))
            {
                data.Index = LastIndex + 1;
                Histories.Add(data);
            }

            if (Histories.Count > _historyCount)
            {
                var count = Histories.Count - _historyCount;
                for (int i = 0; i < count; i++)
                {
                    Histories.RemoveAt(0);
                }
            }

            return true;
        }

        public bool CanAdd(HistoryData data)
        {
            var changes = new List<BuildBundle>();

            if (Histories.Count > 0 && Histories.Last() != null)
            {
                var last = Histories.Last();
                foreach (var bundle in data.Bundles)
                {
                    var old = last.Bundles.Find(t => t.Name == bundle.Name);
                    if (old != null)
                    {
                        if (old.Size != bundle.Size || old.Hash != bundle.Hash)
                        {
                            changes.Add(bundle);
                        }
                    }
                    else changes.Add(bundle);
                }
            }
            else
            {
                changes.AddRange(data.Bundles);
            }

            return changes.Count > 0;
        }
    }

    public static class Histories
    {
        private static readonly string FilePath = BuildSettings.GetCachePath("BuildHistories.json");

        private static HistoriesData _data;

        public static HistoryData LastHistoryData => _data != null ? _data.LastHistoryData : null;

        public static void AddHistory(List<BuildBundle> bundles)
        {
            if (_data == null)
            {
                Reload();
            }

            if (_data != null && _data.AddHistory(bundles))
            {
                Save();
            }
            else
            {
                Debug.LogError("添加历史记录失败！");
            }
        }

        public static void Reload()
        {
            _data = Util.ReadJson<HistoriesData>(FilePath) ?? new HistoriesData();
        }

        public static void Save()
        {
            if (_data != null)
            {
                Util.WriteJson(_data, FilePath);
            }
        }

        public static VersionData GetLastVersion()
        {
            if (!Directory.Exists(BuildSettings.PlatformPath))
            {
                return null;
            }

            var files = Directory.GetFiles(BuildSettings.PlatformPath);
            List<string> paths = new List<string>();
            foreach (var file in files)
            {
                var name = Path.GetFileName(file);
                if (name.StartsWith("version")) paths.Add(file);
            }

            var lastFilePath = string.Empty;
            if (paths.Count > 0)
            {
                long lastCreationTime = 0;
                foreach (var path in paths)
                {
                    var file = new FileInfo(path);
                    if (file.Exists)
                    {
                        var t = file.CreationTime.ToFileTime();
                        if (t > lastCreationTime)
                        {
                            lastCreationTime = t;
                            lastFilePath = path;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(lastFilePath))
            {
                return Util.ReadJson<VersionData>(lastFilePath);
            }

            return null;
        }
    }
}