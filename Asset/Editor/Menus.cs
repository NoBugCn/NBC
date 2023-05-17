using UnityEditor;
using UnityEngine;

namespace NBC.Asset.Editor
{
    public static class Menus
    {
        [MenuItem(Language.MenuDownloadPath)]
        public static void ViewDownload()
        {
            EditorUtility.OpenWithDefaultApp(Const.SavePath);
        }
        
        [MenuItem(Language.MenuBuildPath)]
        public static void ViewBuild()
        {
            EditorUtility.OpenWithDefaultApp(BuildSettings.PlatformPath);
        }
    }
}