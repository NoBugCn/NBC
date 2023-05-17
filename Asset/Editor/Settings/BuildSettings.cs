using System;
using System.Collections.Generic;
using UnityEngine;

namespace NBC.Asset.Editor
{
    [FilePath("Assets/BuildSettings.asset")]
    public class BuildSettings : ScriptableSingleton<BuildSettings>
    {
        [Header("模拟运行模式")] public bool Simulate = true;
        
        [Header("Bundle文件后缀")] public string BundlesExtension = ".bundle";

        [Header("Shader打包到一起")] public bool ShaderBuildTogether = true;

        [Header("Shader文件后缀")]
        public List<string> ShaderExtensions = new List<string> { ".shader", ".shadervariants", ".compute" };


        #region 静态成员

        public static string BuildTargetName => EditUtil.GetActiveBuildTargetName();

        public static string PlatformCachePath =>
            $"{Environment.CurrentDirectory}/Bundles/Cache/{BuildTargetName}"
                .Replace('\\', '/');

        public static string PlatformPath =>
            $"{Environment.CurrentDirectory}/Bundles/{BuildTargetName}"
                .Replace('\\', '/');

        public static string GetCachePath(string file)
        {
            return $"{PlatformCachePath}/{file}";
        }

        public static string GetBuildPath(string file)
        {
            return $"{PlatformPath}/{file}";
        }

        #endregion
    }
}