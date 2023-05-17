using System;
using System.Collections.Generic;

namespace NBC.Asset.Editor
{
    public static class TaskId
    {
        /// <summary>
        /// 收集资源
        /// </summary>
        public const int Gather = 1;

        /// <summary>
        /// 打包bundle
        /// </summary>
        public const int BuildBundle = 2;

        /// <summary>
        /// 生成版本清单
        /// </summary>
        public const int GenPackageData = 3;

        /// <summary>
        /// 生成版本清单
        /// </summary>
        public const int GenVersionData = 4;

        /// <summary>
        /// 拷贝bundle和版本清单
        /// </summary>
        public const int CopyVersionBundle = 5;

        /// <summary>
        /// 拷贝至StreamingAssets
        /// </summary>
        public const int CopyToStreamingAssets = 6;
    }

    public class Defs
    {
        public static void AddTag(string tag)
        {
            if (!UserTags.Contains(tag))
            {
                UserTags.Add(tag);
            }
        }

        public static readonly List<string> UserTags = new List<string>() { };

        public const float DefWindowWidth = 960;
        public const float DefWindowHeight = 600;

#if UNITY_2019_4_OR_NEWER
        public static readonly Type[] DockedWindowTypes =
        {
            typeof(CollectorWindow),
            typeof(ProfilerWindow),
            typeof(BuilderWindow),
            typeof(HistoryWindow)
        };
#endif
    }
}