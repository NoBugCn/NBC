using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace NBC.Asset.Editor
{
    public class CollectorWindow : EditorWindow
    {
        [MenuItem(Language.CollectorWindowMenuPath, false, 1)]
        public static void ShowWindow()
        {
            CollectorWindow wnd =
                GetWindow<CollectorWindow>(Language.CollectorWindowName, true, Defs.DockedWindowTypes);
            wnd.minSize = new Vector2(Defs.DefWindowWidth, Defs.DefWindowHeight);
        }

        public PackageConfig SelectPackageConfig;
        public GroupConfig SelectGroupConfig;

        readonly VerticalSplitter _verticalSplitter = new VerticalSplitter();
        readonly HorizontalSplitter _horizontalSplitter1 = new HorizontalSplitter(0.2f, 0.1f, 0.3f);
        readonly HorizontalSplitter _horizontalSplitter2 = new HorizontalSplitter(0.2f, 0.1f, 0.3f);

        const int _splitterThickness = 2;

        PackageTreeEditor _packagesList;
        GroupTreeEditor _groupsList;
        private AssetsTreeEditor _assetsList;
        private GroupInfoGUI _groupInfoGUI;

        public void UpdateSelectedPackage(string packageName)
        {
            if (!string.IsNullOrEmpty(packageName))
            {
                var packages = CollectorSetting.Instance.Packages;
                if (packages == null) return;
                foreach (var package in packages)
                {
                    if (package.Name != packageName) continue;
                    SelectPackageConfig = package;
                    SelectGroupConfig = null;
                }
            }
            else
            {
                SelectPackageConfig = null;
                SelectGroupConfig = null;
            }

            _groupsList?.Reload();
            _groupsList?.SetSelection(new List<int>());
            _assetsList?.Reload();
        }

        public void UpdateSelectedGroup(string groupName)
        {
            if (!string.IsNullOrEmpty(groupName))
            {
                if (SelectPackageConfig == null) return;
                foreach (var group in SelectPackageConfig.Groups)
                {
                    if (group.Name != groupName) continue;
                    SelectGroupConfig = group;
                }
            }
            else
            {
                SelectGroupConfig = null;
            }

            _assetsList?.Reload();
        }

        public void UpdateAssets()
        {
            Builder.Gather();
            _assetsList?.Reload();
        }

        protected void OnEnable()
        {
            Builder.Gather();
            Styles.Initialize();
        }

        protected void OnGUI()
        {
            GUI.skin.label.richText = true;
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            EditorStyles.label.richText = true;
            EditorStyles.textField.wordWrap = true;
            EditorStyles.foldout.richText = true;

            DrawToolBar();
            var barHeight = EditorStyles.toolbar.fixedHeight + _splitterThickness;
            Rect contentRect = new Rect(_splitterThickness, barHeight, position.width - _splitterThickness * 4,
                position.height - barHeight - _splitterThickness);

            var resizingPackage = _horizontalSplitter1.OnGUI(contentRect, out var packageRect, out var groupRect);

            DrawPackagesList(packageRect);

            var resizingGroup = _horizontalSplitter2.OnGUI(groupRect, out var groupListRect, out var groupInfoRect);

            DrawGroupList(groupListRect);

            bool resizingVer = _verticalSplitter.OnGUI(groupInfoRect, out var groupBaseInfo, out var groupAssetList);

            DrawGroupInfo(groupBaseInfo);
            DrawAssetList(groupAssetList);

            if (resizingPackage || resizingGroup || resizingVer)
                Repaint();
        }

        void DrawToolBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            var guiMode = new GUIContent(Language.Tools);
            Rect rMode = GUILayoutUtility.GetRect(guiMode, EditorStyles.toolbarDropDown);
            if (EditorGUI.DropdownButton(rMode, guiMode, FocusType.Passive, EditorStyles.toolbarDropDown))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent(Language.Profiler), false, () => { });
                menu.AddItem(new GUIContent(Language.Analyse), false, () => { });
                menu.DropDown(rMode);
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(Language.Build, EditorStyles.toolbarButton))
            {
                Builder.Build();
            }

            if (GUILayout.Button(Language.Save, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
                CollectorSetting.Instance.Save();
            }

            GUILayout.EndHorizontal();
        }

        void DrawPackagesList(Rect rect)
        {
            if (_packagesList == null)
            {
                _packagesList = new PackageTreeEditor(new TreeViewState(), this,
                    PackageTreeEditor.CreateDefaultMultiColumnHeaderState());
            }

            _packagesList.OnGUI(rect);
        }

        void DrawGroupList(Rect rect)
        {
            if (_groupsList == null)
            {
                _groupsList = new GroupTreeEditor(new TreeViewState(), this,
                    GroupTreeEditor.CreateDefaultMultiColumnHeaderState());
            }

            _groupsList.OnGUI(rect);
        }


        void DrawGroupInfo(Rect rect)
        {
            if (_groupInfoGUI == null)
            {
                _groupInfoGUI = new GroupInfoGUI(this);
            }

            _groupInfoGUI.OnGUI(rect);
        }

        void DrawAssetList(Rect rect)
        {
            if (_assetsList == null)
            {
                _assetsList = new AssetsTreeEditor(new TreeViewState(), this,
                    AssetsTreeEditor.CreateDefaultMultiColumnHeaderState());
            }

            _assetsList.OnGUI(rect);
        }
    }
}