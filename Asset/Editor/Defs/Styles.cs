using System.IO;
using UnityEditor;
using UnityEngine;

namespace NBC.Asset.Editor
{
    public static class Styles
    {
        private static bool _init;
        private static GUISkin styleSheet;
        private static GUIStyle _headerBoxStyle;

        public static string EditorGUIPath;
        public static Texture2D Package;
        public static Texture2D PackageLow;
        public static Texture2D Group;
        public static Texture2D Add;

        public static GUIStyle headerBoxStyle => _headerBoxStyle != null
            ? _headerBoxStyle
            : _headerBoxStyle = styleSheet.GetStyle("HeaderBox");
        
        public static GUIStyle headerLabelStyle => _headerBoxStyle != null
            ? _headerBoxStyle
            : _headerBoxStyle = styleSheet.GetStyle("HeaderLabel");

        
        public static void Initialize()
        {
            string[] folders = { "Assets", "Packages" };
            var assets = AssetDatabase.FindAssets("t:texture icon_package", folders);
            if (assets.Length > 0)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assets[0]);
                EditorGUIPath = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            }

            
            styleSheet = LoadRes<GUISkin>("StyleSheet.guiskin");
            Package = LoadIcon("icon_package.png");
            PackageLow = LoadIcon("icon_package2.png");
            Group = LoadIcon("icon_folder.png");
            Add = LoadIcon("icon_add.png");
        }

        static Texture2D LoadIcon(string filename)
        {
            return LoadRes<Texture2D>(filename);
        }

        static T LoadRes<T>(string filename) where T : Object
        {
            return AssetDatabase.LoadMainAssetAtPath(EditorGUIPath + "/" + filename) as T;
        }
    }
}