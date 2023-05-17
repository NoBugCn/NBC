using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Networking;

namespace NBC.Asset.Editor
{
    public static class EditUtil
    {
        public static async Task<string> GetHttpRequest(string url)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                webRequest.SendWebRequest();
                while (!webRequest.isDone)
                {
                    await Task.Delay(100);
                }

                if (!string.IsNullOrEmpty(webRequest.error))
                {
                    Debug.LogError(webRequest.error);
                    return string.Empty;
                }

                Debug.Log(webRequest.downloadHandler.text);
                return webRequest.downloadHandler.text;
            }
        }

        public static void CopyToClipBoard(string str)
        {
            TextEditor text2Editor = new TextEditor
            {
                text = str
            };
            text2Editor.OnFocus();
            text2Editor.Copy();
        }

        public static string[] GetTagsArr(string tags)
        {
            if (!string.IsNullOrEmpty(tags))
            {
                return tags.Split(',');
            }

            return Array.Empty<string>();
        }

        public static void CopyToFolder(string filePath, string toPath)
        {
            if (!File.Exists(filePath)) return;
            CreateDirectory(Path.GetDirectoryName(toPath));
            if (File.Exists(toPath)) File.Delete(toPath);
            File.Copy(filePath, toPath);
        }

        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static T GetAssetOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            var cfgPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(cfgPath))
            {
                Directory.CreateDirectory(cfgPath ?? string.Empty);
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }

        public static string GetActiveBuildTargetName()
        {
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.StandaloneOSX:
                    return "OSX";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.WebGL:
                    return "WebGL";
                case BuildTarget.StandaloneLinux64:
                    return "Linux";
                default:
                    return "Default";
            }
        }


        #region 窗口相关

        public static MultiColumnHeaderState.Column GetMultiColumnHeaderColumn(string name, int width = 100,
            int min = 80, int max = 200)
        {
            var val = new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent(name),
                minWidth = min,
                maxWidth = max,
                width = width,
                sortedAscending = true,
                headerTextAlignment = TextAlignment.Center,
                canSort = false,
                autoResize = true,
                allowToggleVisibility = false
            };

            return val;
        }

        #endregion

        #region 反射

        public static List<Type> FindAllSubclass<T>()
        {
            var listOfBs = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                from assemblyType in domainAssembly.GetTypes()
                where typeof(T).IsAssignableFrom(assemblyType)
                select assemblyType).ToArray();

            List<Type> list = new List<Type>();
            foreach (var type in listOfBs)
            {
                if (type.IsSubclassOf(typeof(T)))
                {
                    list.Add(type);
                }
            }

            return list;
        }

        #endregion
    }
}