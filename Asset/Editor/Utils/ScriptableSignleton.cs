using System;
using System.Linq;
using UnityEngine;

namespace NBC.Asset.Editor
{
    public class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        private static T _inst;

        public static T Instance
        {
            get
            {
                if (!_inst)
                {
                    LoadOrCreate();
                }

                return _inst;
            }
        }

        public static T Get()
        {
            return Instance;
        }


        public static T LoadOrCreate()
        {
            if (_inst == null)
            {
                string filePath = GetFilePath();
                if (!string.IsNullOrEmpty(filePath))
                {
                    _inst = EditUtil.GetAssetOrCreate<T>(GetFilePath());
                }
                else
                {
                    Debug.LogError($"{nameof(ScriptableSingleton<T>)}: 请指定单例存档路径！ ");
                }
            }

            return _inst;
        }

        protected static string GetFilePath()
        {
            return typeof(T).GetCustomAttributes(inherit: true)
                .Cast<FilePathAttribute>()
                .FirstOrDefault(v => v != null)
                ?.filePath;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class FilePathAttribute : Attribute
    {
        internal readonly string filePath;

        /// <summary>
        /// 单例存放路径
        /// </summary>
        /// <param name="path">相对 Project 路径</param>
        public FilePathAttribute(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Invalid relative path (it is empty)");
            }

            if (path[0] == '/')
            {
                path = path.Substring(1);
            }

            filePath = path;
        }
    }
}