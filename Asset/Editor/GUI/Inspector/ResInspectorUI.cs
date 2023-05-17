using System.IO;
using UnityEditor;
using UnityEngine;

namespace NBC.Asset.Editor
{
    [InitializeOnLoad]
    public class ResInspectorUI
    {
        static ResInspectorUI()
        {
            UnityEditor.Editor.finishedDefaultHeaderGUI -= OnPostHeaderGUI;
            UnityEditor.Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
        }

        #region style

        private static GUIStyle _style_bg;

        private static GUIStyle style_bg
        {
            get
            {
                if (_style_bg == null)
                {
                    _style_bg = new GUIStyle(EditorStyles.helpBox);
                }

                return _style_bg;
            }
        }

        #endregion

        private static void OnPostHeaderGUI(UnityEditor.Editor editor)
        {
            if (editor.targets.Length == 1)
            {
                var t = editor.target;
                try
                {
                    ShowRulePath(FindAsset(t));
                }
                catch
                {
                    // ignored
                    // XLog.LogError(e);
                }
            }
        }

        private static BuildAsset FindAsset(Object t)
        {
            var path = AssetDatabase.GetAssetPath(t);
            var cache = Caches.Get();
            var buildAsset = cache.Assets.Find(p => p.Path == path);
            return buildAsset;
        }

        private static void ShowRulePath(BuildAsset ruleAsset)
        {
            if (ruleAsset != null)
            {
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical(style_bg);


                if (ruleAsset.Address != ruleAsset.Path)
                {
                    EditorGUILayout.LabelField(Language.InspectorUITitle, EditorStyles.centeredGreyMiniLabel);
                    DrawAddressPath(ruleAsset);
                    DrawPath(ruleAsset);
                }
                else
                {
                    EditorGUILayout.LabelField(Language.InspectorUITitleNotOpen, EditorStyles.centeredGreyMiniLabel);
                    DrawPath(ruleAsset);
                }


                EditorGUILayout.Separator();
                EditorGUILayout.EndVertical();
            }
        }

        private static void DrawAddressPath(BuildAsset ruleAsset)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(Language.InspectorUIAddressPath, GUILayout.MaxWidth(65));
            EditorGUILayout.TextField(ruleAsset.Address);
            if (GUILayout.Button(Language.Copy, GUILayout.Width(40)))
            {
                EditUtil.CopyToClipBoard(ruleAsset.Address);
            }

            GUILayout.EndHorizontal();
        }

        private static void DrawPath(BuildAsset ruleAsset)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(Language.InspectorUIPath, GUILayout.MaxWidth(65));
            EditorGUILayout.TextField(ruleAsset.Path);
            if (GUILayout.Button(Language.Copy, GUILayout.Width(40)))
            {
                EditUtil.CopyToClipBoard(ruleAsset.Path);
            }

            GUILayout.EndHorizontal();
        }
    }
}