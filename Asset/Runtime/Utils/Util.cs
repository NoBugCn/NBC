using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace NBC.Asset
{
    public static class Util
    {
        public static DateTime TimestampToTime(long timestamp)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return startTime.AddSeconds(timestamp);
        }

        public static string GetFriendlySize(long byteSize)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            long prevOrderRemainder = 0;
            while (byteSize >= 1024 && order < sizes.Length - 1)
            {
                order++;
                prevOrderRemainder = byteSize % 1024;
                byteSize /= 1024;
            }

            double byteSizeFloat = byteSize + (double)prevOrderRemainder / 1024;

            string result = $"{byteSizeFloat:0.##}{sizes[order]}";
            return result;
        }

        public static string GetAssetGUID(string path, Type type)
        {
            return type == null ? $"{path}[null]" : $"{path}[{type.Name}]";
        }

        public static string NameAddHash(string name, string hash)
        {
            var ext = Path.GetExtension(name);
            var nameWithExt = name.Replace(ext, string.Empty);
            return $"{nameWithExt}_{hash}{ext}";
        }

        public static int GetFileSize(string path)
        {
            if (!File.Exists(path)) return 0;
            var bytes = File.ReadAllBytes(path);
            return bytes.Length;
        }

        public static string ComputeHash(byte[] bytes)
        {
            var data = MD5.Create().ComputeHash(bytes);
            return GetHash(data);
        }

        public static string ComputeHash(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return string.Empty;
            }

            using (var stream = File.OpenRead(filePath))
            {
                return GetHash(MD5.Create().ComputeHash(stream));
            }
        }

        public static string GetHash(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }


        public static void WriteJson(object so, string filePath)
        {
            var json = JsonUtility.ToJson(so);

            File.WriteAllText(filePath, json);
        }

        public static T ReadJson<T>(string filePath)
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                return JsonUtility.FromJson<T>(json);
            }

            return default;
        }
    }
}