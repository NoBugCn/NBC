using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Networking;

namespace NBC.Asset.Editor
{
    [Serializable]
    public class ProfilerInfo
    {
        public int CurrentIndex;
        public List<DebugInfo> DebugInfos = new List<DebugInfo>();

        public DebugInfo Current
        {
            get
            {
                if (CurrentIndex < DebugInfos.Count && DebugInfos.Count > 0)
                {
                    return DebugInfos[CurrentIndex];
                }

                return null;
            }
        }

        public void Add(DebugInfo debugInfo)
        {
            DebugInfos.Add(debugInfo);
            CurrentIndex = DebugInfos.Count - 1;
        }

        public void Clear()
        {
            CurrentIndex = 0;
            DebugInfos.Clear();
        }
    }

    public class ProfilerWindow : EditorWindow
    {
        [MenuItem(Language.ProfilerWindowNameMenuPath, false, 3)]
        public static void ShowWindow()
        {
            ProfilerWindow wnd =
                GetWindow<ProfilerWindow>(Language.ProfilerWindowName, true, Defs.DockedWindowTypes);
            wnd.minSize = new Vector2(Defs.DefWindowWidth, Defs.DefWindowHeight);
        }

        enum ShowModel
        {
            Asset,
            Bundle
        }

        enum DataModel
        {
            Local,
            Remote
        }

        GUIContent _prevFrameIcon;
        GUIContent _nextFrameIcon;

        private List<GUIStyle> _searchStyles;
        private SearchField _searchField;
        private string _searchValue;
        private ShowModel _showModel = ShowModel.Asset;
        private DataModel _dataModel = DataModel.Local;
        private string _remoteUrl;

        readonly VerticalSplitter _verticalSplitter = new VerticalSplitter();

        private string _selectName;

        private ProfilerInfo _profilerInfo = new ProfilerInfo();
        // private int _currentIndex;
        // private readonly List<DebugInfo> _debugInfos = new List<DebugInfo>();

        ProfilerAssetListView _assetListView;
        ProfilerBundleListView _bundleListView;

        private bool _refreshListView;

        protected void OnEnable()
        {
            _remoteUrl = EditorPrefs.GetString("ProfilerRemoteUrl", "http://127.0.0.1:8080");
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
            _prevFrameIcon = EditorGUIUtility.IconContent("Profiler.PrevFrame", Language.PrevFrame);
            _nextFrameIcon = EditorGUIUtility.IconContent("Profiler.NextFrame", Language.NextFrame);
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
        }

        private void PlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.EnteredPlayMode)
            {
                _profilerInfo?.Clear();
            }
        }

        protected void OnGUI()
        {
            _refreshListView = false;
            if (_searchField == null) _searchField = new SearchField();
            if (_profilerInfo == null) _profilerInfo = new ProfilerInfo();
            DrawToolBar();

            var r = EditorGUILayout.GetControlRect();
            Rect contentRect = new Rect(r.x, r.y, r.width, position.height - r.y);

            bool resizingVer = _verticalSplitter.OnGUI(contentRect, out var top, out var bot);

            var selectName = _selectName;

            if (_showModel == ShowModel.Asset)
            {
                DrawAssetView(top);
                DrawBundleView(bot);
                if (_assetListView != null)
                {
                    selectName = _assetListView.SelectName;
                }
            }
            else
            {
                DrawBundleView(top);
                DrawAssetView(bot);
                if (_assetListView != null)
                {
                    selectName = _bundleListView.SelectName;
                }
            }

            if (selectName != _selectName)
            {
                _selectName = selectName;
                _refreshListView = true;
            }

            if (_refreshListView)
            {
                _refreshListView = false;
                SetShowInfo();
            }

            if (resizingVer)
                Repaint();
        }

        void DrawToolBar()
        {
            if (_searchStyles == null)
            {
                _searchStyles = new List<GUIStyle>
                {
                    GUITools.GetStyle("ToolbarSeachTextField"),
                    GUITools.GetStyle("ToolbarSeachCancelButton"),
                    GUITools.GetStyle("ToolbarSeachCancelButtonEmpty")
                };
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            var showModeName = _showModel == ShowModel.Asset
                ? Language.ProfilerAssetMode
                : Language.ProfilerBundleMode;
            var guiMode = new GUIContent(Language.ProfilerShowMode + showModeName);
            Rect rMode = GUILayoutUtility.GetRect(guiMode, EditorStyles.toolbarDropDown);
            if (EditorGUI.DropdownButton(rMode, guiMode, FocusType.Passive, EditorStyles.toolbarDropDown))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent(Language.ProfilerAssetMode), false,
                    () =>
                    {
                        _showModel = ShowModel.Asset;
                        _refreshListView = true;
                    });
                menu.AddItem(new GUIContent(Language.ProfilerBundleMode), false,
                    () =>
                    {
                        _showModel = ShowModel.Bundle;
                        _refreshListView = true;
                    });
                menu.DropDown(rMode);
            }

            var dataModeName = _dataModel == DataModel.Local
                ? Language.ProfilerLocalData
                : Language.ProfilerRemoteData;
            var dataMode = new GUIContent(Language.ProfilerDataMode + dataModeName);
            Rect rdataMode = GUILayoutUtility.GetRect(dataMode, EditorStyles.toolbarDropDown);
            if (EditorGUI.DropdownButton(rdataMode, dataMode, FocusType.Passive, EditorStyles.toolbarDropDown))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent(Language.ProfilerLocalData), false,
                    () =>
                    {
                        _dataModel = DataModel.Local;
                        _refreshListView = true;
                    });
                menu.AddItem(new GUIContent(Language.ProfilerRemoteData), false,
                    () =>
                    {
                        _dataModel = DataModel.Remote;
                        _refreshListView = true;
                    });
                menu.DropDown(rdataMode);
            }


            if (GUILayout.Button(Language.ImportProfiler, EditorStyles.toolbarButton))
            {
                DebugInfoImport();
            }

            if (GUILayout.Button(Language.BuildProfiler, EditorStyles.toolbarButton))
            {
                DebugInfoBuild();
            }

            if (GUILayout.Button(Language.Clear, EditorStyles.toolbarButton))
            {
                Clear();
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label(_profilerInfo.CurrentIndex >= _profilerInfo.DebugInfos.Count
                    ? Language.CurrentFrame
                    : Language.CurrentFrame + $"{_profilerInfo.Current.Frame} (" + (_profilerInfo.CurrentIndex + 1) +
                      "/" +
                      (_profilerInfo.DebugInfos.Count) + ")",
                EditorStyles.miniLabel);

            using (new EditorGUI.DisabledScope(_profilerInfo.CurrentIndex <= 0))
            {
                if (GUILayout.Button(_prevFrameIcon, EditorStyles.toolbarButton))
                {
                    _profilerInfo.CurrentIndex--;
                    _refreshListView = true;
                }
            }

            using (new EditorGUI.DisabledScope(_profilerInfo.CurrentIndex >= (_profilerInfo.DebugInfos.Count - 1)))
            {
                if (GUILayout.Button(_nextFrameIcon, EditorStyles.toolbarButton))
                {
                    _profilerInfo.CurrentIndex++;
                    _refreshListView = true;
                }
            }

            if (GUILayout.Button(Language.Current, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
                GetDebugInfo();
            }


            Rect searchRect = EditorGUILayout.GetControlRect();
            var newSearchValue = _searchField.OnGUI(searchRect, _searchValue, _searchStyles[0], _searchStyles[1],
                _searchStyles[2]);
            if (newSearchValue != _searchValue)
            {
                _searchValue = newSearchValue;
                _refreshListView = true;
            }

            GUILayout.EndHorizontal();

            if (_dataModel == DataModel.Remote)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(Language.ProfilerRemoteUrl, GUILayout.Width(80));
                var newUrl = EditorGUILayout.TextField(string.Empty, _remoteUrl);
                if (_remoteUrl != null && newUrl != _remoteUrl)
                {
                    _remoteUrl = newUrl;
                    EditorPrefs.SetString("ProfilerRemoteUrl", _remoteUrl);
                }

                GUILayout.EndHorizontal();
            }
        }

        void DrawAssetView(Rect rect)
        {
            if (_assetListView == null)
            {
                _assetListView = new ProfilerAssetListView(new TreeViewState(), this,
                    ProfilerAssetListView.CreateDefaultMultiColumnHeaderState());
            }

            _assetListView.OnGUI(rect);
        }

        void DrawBundleView(Rect rect)
        {
            if (_bundleListView == null)
            {
                _bundleListView = new ProfilerBundleListView(new TreeViewState(), this,
                    ProfilerBundleListView.CreateDefaultMultiColumnHeaderState());
            }

            _bundleListView.OnGUI(rect);
        }


        private void DebugInfoBuild()
        {
            var saveFolder =
                EditorUtility.OpenFolderPanel(Language.BuildProfilerTips, Environment.CurrentDirectory, "");
            var savePath = $"{saveFolder}/debug_info_{DateTime.Now.ToString("yyyyMMddHHmmss")}.json";
            var json = JsonUtility.ToJson(_profilerInfo, true);
            File.WriteAllText(savePath, json);
            EditorUtility.DisplayDialog(Language.Tips, Language.Success + $", path:{savePath}", Language.Confirm);
        }

        private void DebugInfoImport()
        {
            var path = EditorUtility.OpenFilePanel(Language.ImportProfilerTips, Environment.CurrentDirectory, "json");
            if (File.Exists(path))
            {
                var data = Util.ReadJson<ProfilerInfo>(path);
                if (data != null)
                {
                    _profilerInfo = data;
                    _refreshListView = true;
                }
            }
        }

        private void Clear()
        {
            _profilerInfo?.Clear();
            _assetListView?.SetData(null);
            _bundleListView?.SetData(null);
        }

        private void GetDebugInfo()
        {
            if (_dataModel == DataModel.Local)
            {
                var debugInfo = Assets.GetDebugInfos();
                _profilerInfo.Add(debugInfo);
                _refreshListView = true;
            }
            else
            {
                GetRemoteDebugInfo();
            }
        }

        private async void GetRemoteDebugInfo()
        {
            if (string.IsNullOrEmpty(_remoteUrl))
            {
                EditorUtility.DisplayDialog(Language.Error, Language.ProfilerRemoteUrlIsNull, Language.Confirm);
                return;
            }

            var result = await EditUtil.GetHttpRequest(_remoteUrl);
            if (!string.IsNullOrEmpty(result))
            {
                Debug.Log("请求远程数据====" + result);
                try
                {
                    var data = JsonUtility.FromJson<DebugInfo>(result);
                    if (data == null) return;
                    _profilerInfo.Add(data);
                    _refreshListView = true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"解析远程数据失败，e={e}");
                }
            }
        }

        private void SetShowInfo()
        {
            if (_profilerInfo.CurrentIndex >= _profilerInfo.DebugInfos.Count) return;
            SetAssetShowInfo();
            SetBundleShowInfo();
        }

        private void SetAssetShowInfo()
        {
            List<DebugAssetInfo> list = new List<DebugAssetInfo>();
            var currentDebugInfo = _profilerInfo.Current;
            if (_showModel == ShowModel.Asset)
            {
                list.AddRange(currentDebugInfo.AssetInfos);
            }
            else
            {
                //bundle模式，根据选中bundle筛选asset
                var bundle = currentDebugInfo.BundleInfos.Find(a => a.BundleName == _selectName);
                if (bundle != null)
                {
                    foreach (var asset in currentDebugInfo.AssetInfos)
                    {
                        if (asset.Dependency.Count > 0)
                        {
                            if (asset.Dependency[0] == bundle.BundleName) list.Add(asset);
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(_searchValue))
            {
                for (var i = 0; i < list.Count; i++)
                {
                    var l = list[i];
                    if (!l.Path.Contains(_searchValue))
                    {
                        list.RemoveAt(i);
                        i--;
                    }
                }
            }

            _assetListView?.SetData(list);
        }

        private void SetBundleShowInfo()
        {
            var currentDebugInfo = _profilerInfo.Current;
            List<DebugBundleInfo> list = new List<DebugBundleInfo>();
            if (_showModel == ShowModel.Asset)
            {
                //asset模式，根据选中asset筛选bundle
                var asset = currentDebugInfo.AssetInfos.Find(a => a.Path == _selectName);
                if (asset != null)
                {
                    foreach (var bundle in currentDebugInfo.BundleInfos)
                    {
                        if (asset.Dependency.Contains(bundle.BundleName)) list.Add(bundle);
                    }
                }

                _bundleListView?.SetData(list);
            }
            else
            {
                list.AddRange(currentDebugInfo.BundleInfos);
            }

            if (!string.IsNullOrEmpty(_searchValue))
            {
                for (var i = 0; i < list.Count; i++)
                {
                    var l = list[i];
                    if (!l.BundleName.Contains(_searchValue))
                    {
                        list.RemoveAt(i);
                        i--;
                    }
                }
            }

            _bundleListView?.SetData(list);
        }
    }
}