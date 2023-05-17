using System.IO;
using UnityEngine;

namespace NBC.Asset
{
    public static class Const
    {
        public const string BundleDirName = "Bundles";
        public const string VersionFileName = "version.json";

        public static readonly string SavePath =
            $"{Application.persistentDataPath}{Path.DirectorySeparatorChar}{BundleDirName}{Path.DirectorySeparatorChar}";

        public static readonly string StreamingAssetsPath =
            $"{Application.streamingAssetsPath}{Path.DirectorySeparatorChar}";

        public static string RemoteUrl = "http://127.0.0.1:8181/";

        public static bool Offline;
        public static bool Simulate;

        public static int DownloadTimeOut = 10;
        

        public static string GetStreamingPath(string file)
        {
            return $"{StreamingAssetsPath}{file}";
        }

        public static string GetCachePath(string file)
        {
            return $"{SavePath}{file}";
        }

        public static string GetCacheTempPath(string file)
        {
            return $"{SavePath}{file}.temp";
        }

        public static string GetRemotePath(string file)
        {
            return $"{RemoteUrl}{BundleDirName}/{file}";
        }
    }
}