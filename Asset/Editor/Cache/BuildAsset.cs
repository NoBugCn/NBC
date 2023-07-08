using System;
using System.IO;
using UnityEngine;

namespace NBC.Asset.Editor
{
    [Serializable]
    public class BuildAsset
    {
        /// <summary>
        /// 资源路径
        /// </summary>
        public string Path;

        /// <summary>
        /// 可寻址地址
        /// </summary>
        public string Address;

        /// <summary>
        /// 资源类型
        /// </summary>
        public string Type;

        /// <summary>
        /// 资源所属的bundle包
        /// </summary>
        public string Bundle;

        /// <summary>
        /// 资源标签
        /// </summary>
        public string Tags;

        // /// <summary>
        // /// 资源依赖
        // /// </summary>
        // public string[] Dependencies = Array.Empty<string>();

        /// <summary>
        /// 资源所属组
        /// </summary>
        [HideInInspector] public GroupConfig Group;

        private long _size;

        public long Size
        {
            get
            {
                if (_size == 0)
                {
                    if (File.Exists(Path))
                    {
                        FileInfo info = new FileInfo(Path);
                        _size = info.Length;
                    }
                }

                return _size;
            }
        }
    }
}