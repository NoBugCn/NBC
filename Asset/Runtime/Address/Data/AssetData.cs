using System;
using System.Collections.Generic;
using System.Linq;

namespace NBC.Asset
{
    [Serializable]
    public class AssetData
    {
        public string Name;
        public int Bundle;

        /// <summary>
        /// 所属目录
        /// </summary>
        public int Dir;

        // /// <summary>
        // /// 依赖的bundle
        // /// </summary>
        // public List<int> Deps = new List<int>();

        /// <summary>
        /// 资源可寻址地址
        /// </summary>
        public string Address;

        /// <summary>
        /// 资源真实地址
        /// </summary>
        public string Path { get; set; }


        /// <summary>
        /// 资源Bundle
        /// </summary>
        public string BundleName { get; set; }


        public string[] Tags { get; internal set; }

        /// <summary>
        /// 是否包含Tag
        /// </summary>
        public bool HasTag(string[] tags)
        {
            if (tags == null || tags.Length == 0)
                return false;
            if (Tags == null || Tags.Length == 0)
                return false;

            foreach (var tag in tags)
            {
                if (Tags.Contains(tag))
                    return true;
            }

            return false;
        }
    }
}