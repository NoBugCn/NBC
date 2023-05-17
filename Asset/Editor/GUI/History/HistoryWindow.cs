using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace NBC.Asset.Editor
{
    public class HistoryWindow : EditorWindow
    {
        [MenuItem(Language.HistoryWindowNameMenuPath, false, 4)]
        public static void ShowWindow()
        {
            var s = string.Empty;
            HistoryWindow wnd = GetWindow<HistoryWindow>(Language.HistoryWindowName, true, Defs.DockedWindowTypes);
            wnd.minSize = new Vector2(Defs.DefWindowWidth, Defs.DefWindowHeight);
        }


        const int _splitterThickness = 2;

        public VersionHistoryData SelectVersion;

        /// <summary>
        /// 选中需要比较的版本
        /// </summary>
        public VersionHistoryData SelectCompareVersion;

        readonly VerticalSplitter _verticalSplitter = new VerticalSplitter(0.7f, 0.7f, 0.8f);
        readonly HorizontalSplitter _horizontalSplitter = new HorizontalSplitter(0.3f, 0.3f, 0.4f);

        private HistoryVersionTreeEditor _versionList;
        private HistoryBundleTreeEditor _bundleTreeEditor;

        private List<VersionHistoryData> _versionHistory = new List<VersionHistoryData>();

        public List<VersionHistoryData> ShowVersionHistory => _versionHistory;

        public void UpdateCompareSelectedVersion(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                foreach (var version in _versionHistory)
                {
                    if (version.ShowName != name) continue;
                    SelectCompareVersion = version;
                }
            }
            else
            {
                SelectCompareVersion = null;
            }

            _versionList?.Reload();
        }

        public void UpdateSelectedVersion(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                foreach (var version in _versionHistory)
                {
                    if (version.ShowName != name) continue;
                    SelectVersion = version;
                }
            }
            else
            {
                SelectVersion = null;
            }

            _bundleTreeEditor?.Reload();
        }

        public void ReloadVersions()
        {
            _versionHistory = HistoryUtil.GetHistoryVersions();
            if (SelectCompareVersion == null && _versionHistory.Count > 1)
            {
                SelectCompareVersion = _versionHistory[1];
            }
        }

        public void Refresh()
        {
            ReloadVersions();
            UpdateSelectedVersion(string.Empty);
        }

        protected void OnEnable()
        {
            SelectVersion = null;
            SelectCompareVersion = null;
            ReloadVersions();
            Styles.Initialize();
        }

        protected void OnGUI()
        {
            var barHeight = _splitterThickness;
            Rect contentRect = new Rect(_splitterThickness, barHeight, position.width - _splitterThickness * 4,
                position.height - _splitterThickness);

            var resizingPackage = _horizontalSplitter.OnGUI(contentRect, out var bundleRect, out var infoRect);

            DrawVersionList(bundleRect);
            bool resizingVer = _verticalSplitter.OnGUI(infoRect, out var top, out var bot);
            DrawBundleList(top);
            DrawCompareInfo(bot);
            if (resizingPackage || resizingVer)
                Repaint();
        }


        void DrawVersionList(Rect rect)
        {
            if (_versionList == null)
            {
                _versionList = new HistoryVersionTreeEditor(new TreeViewState(), this,
                    HistoryVersionTreeEditor.CreateDefaultMultiColumnHeaderState());
            }

            _versionList.OnGUI(rect);
        }

        void DrawBundleList(Rect rect)
        {
            if (_bundleTreeEditor == null)
            {
                _bundleTreeEditor = new HistoryBundleTreeEditor(new TreeViewState(), this,
                    HistoryBundleTreeEditor.CreateDefaultMultiColumnHeaderState());
            }

            _bundleTreeEditor.OnGUI(rect);
        }

        void DrawCompareInfo(Rect rect)
        {
            if (SelectVersion == null)
            {
                return;
            }

            GUILayout.BeginArea(rect);

            if (SelectCompareVersion == null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(Language.NoSelectHistoryCompareVersion);
            }
            else if (SelectCompareVersion == SelectVersion)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(Language.NoSelectHistoryCompareOneVersion);
            }
            else
            {
                DrawCompareDetails();
            }

            GUILayout.EndArea();
        }

        void DrawCompareDetails()
        {
            EditorGUILayout.Space();
            var compareInfo = HistoryUtil.CompareVersion(SelectVersion, SelectCompareVersion);
            var simpleData = compareInfo.SimpleChangeData;


            GUILayout.BeginHorizontal();
            var strAdd = GetPackageChangeInfo(simpleData.PackageAddBundle);
            EditorGUILayout.TextField(Language.HistoryAddBundleCount, strAdd);
            if (GUILayout.Button(Language.Copy, GUILayout.Width(50)))
            {
                EditUtil.CopyToClipBoard(strAdd);
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.Space();


            GUILayout.BeginHorizontal();
            var strChange = GetPackageChangeInfo(simpleData.PackageChangeBundle);
            EditorGUILayout.TextField(Language.HistoryChangeBundleCount, strChange);
            if (GUILayout.Button(Language.Copy, GUILayout.Width(50)))
            {
                EditUtil.CopyToClipBoard(strChange);
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.Space();


            GUILayout.BeginHorizontal();
            var strRemove = GetPackageChangeInfo(simpleData.PackageRemoveBundle);
            EditorGUILayout.TextField(Language.HistoryRemoveBundleCount, strRemove);
            if (GUILayout.Button(Language.Copy, GUILayout.Width(50)))
            {
                EditUtil.CopyToClipBoard(strRemove);
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            var strDownload = GetPackageDownloadInfo(simpleData.PackageDownloadSize);
            EditorGUILayout.TextField(Language.HistoryDownloadSize, strDownload);
            if (GUILayout.Button(Language.Copy, GUILayout.Width(50)))
            {
                EditUtil.CopyToClipBoard(strDownload);
            }

            GUILayout.EndHorizontal();


            EditorGUILayout.Space();
            if (GUILayout.Button(Language.HistoryCompareBuild, GUILayout.Height(40)))
            {
                var saveJson =
                    JsonConvert.SerializeObject(compareInfo,
                        Formatting.Indented); //JsonUtility.ToJson(compareInfo, true);

                var saveFolder =
                    EditorUtility.OpenFolderPanel(Language.BuildProfilerTips, Environment.CurrentDirectory, "");


                var savePath = $"{saveFolder}/history_compare_{DateTime.Now.ToString("yyyyMMddHHmmss")}.json";
                File.WriteAllText(savePath, saveJson);
                EditorUtility.DisplayDialog(Language.Tips, Language.Success + $", path={savePath}", Language.Confirm);
            }
        }

        string GetPackageChangeInfo(Dictionary<string, List<string>> dictionary)
        {
            var all = 0;
            var packageStr = "";
            var index = 0;
            foreach (var key in dictionary.Keys)
            {
                index++;
                var value = dictionary[key];
                all += value.Count;
                packageStr += $"{key}={value.Count}";
                if (index < dictionary.Count)
                {
                    packageStr += "、 ";
                }
            }

            var str = string.Format(Language.HistoryBundleChangeTitle, all, packageStr);

            return str;
        }

        string GetPackageDownloadInfo(Dictionary<string, long> dictionary)
        {
            long all = 0;
            var packageStr = "";
            var index = 0;
            foreach (var key in dictionary.Keys)
            {
                index++;
                var value = dictionary[key];
                all += value;
                packageStr += $"{key}={Util.GetFriendlySize(value)}";
                if (index < dictionary.Count)
                {
                    packageStr += "、 ";
                }
            }

            var str = string.Format(Language.HistoryBundleChangeTitle, Util.GetFriendlySize(all), packageStr);

            return str;
        }
    }
}