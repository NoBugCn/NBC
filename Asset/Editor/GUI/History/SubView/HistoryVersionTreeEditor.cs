using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace NBC.Asset.Editor
{
    public class HistoryVersionTreeViewItem : TreeViewItem
    {
        private VersionHistoryData _version;

        public VersionHistoryData Version => _version;

        public HistoryVersionTreeViewItem(int id, VersionHistoryData version) : base(id, id, version.ShowName)
        {
            _version = version;
        }
    }

    public class HistoryVersionTreeEditor : TreeView
    {
        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            return new MultiColumnHeaderState(GetColumns());
        }

        private static MultiColumnHeaderState.Column[] GetColumns()
        {
            List<MultiColumnHeaderState.Column> retVal = new List<MultiColumnHeaderState.Column>();
            var fist = EditUtil.GetMultiColumnHeaderColumn(Language.HistoryVersionName, 200, 200, 1000);
            retVal.Add(fist);
            return retVal.ToArray();
        }

        /// <summary>
        /// 当前选中了
        /// </summary>
        private bool _contextOnItem = false;

        HistoryWindow _window;
        public MultiColumnHeaderState HeaderState;

        public HistoryVersionTreeEditor(TreeViewState state, HistoryWindow window, MultiColumnHeaderState header) :
            base(state,
                new MultiColumnHeader(header))
        {
            _window = window;
            HeaderState = header;
            showBorder = true;
            showAlternatingRowBackgrounds = true;

            Reload();
        }


        private TreeViewItem _root;

        protected override TreeViewItem BuildRoot()
        {
            _root = new TreeViewItem
            {
                id = -1,
                depth = -1,
                children = new List<TreeViewItem>()
            };
            int id = 0;
            if (_window.ShowVersionHistory != null)
            {
                foreach (var version in _window.ShowVersionHistory)
                {
                    id++;
                    var t = new HistoryVersionTreeViewItem(id, version);
                    _root.AddChild(t);
                }
            }

            return _root;
        }

        internal void Refresh()
        {
            var selection = GetSelection();

            SelectionChanged(selection);
        }

        #region 绘制

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            HeaderState.AutoWidth(rect.width);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 &&
                rect.Contains(Event.current.mousePosition))
            {
                SetSelection(Array.Empty<int>(), TreeViewSelectionOptions.FireSelectionChanged);
            }
        }

        /// <summary>
        /// 绘制行
        /// </summary>
        /// <param name="args"></param>
        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), args.item, args.GetColumn(i), ref args);
            }
        }

        private void CellGUI(Rect cellRect, TreeViewItem item, int column, ref RowGUIArgs args)
        {
            if (item is HistoryVersionTreeViewItem versionTreeViewItem)
            {
                CenterRectUsingSingleLineHeight(ref cellRect);
                if (column == 0)
                {
                    var iconRect = new Rect(cellRect.x + 4, cellRect.y + 1, cellRect.height - 2, cellRect.height - 2);
                    GUI.DrawTexture(iconRect, Styles.Package, ScaleMode.ScaleToFit);

                    var nameRect = new Rect(cellRect.x + iconRect.xMax + 1, cellRect.y, cellRect.width - iconRect.width,
                        cellRect.height);

                    if (_window.SelectCompareVersion != null &&
                        _window.SelectCompareVersion.ShowName == item.displayName)
                    {
                        DefaultGUI.Label(nameRect, "[√]" + item.displayName, args.selected, args.focused);
                    }
                    else
                    {
                        DefaultGUI.Label(nameRect, item.displayName, args.selected, args.focused);
                    }
                }
            }
        }

        #endregion

        #region 接口实现

        /// <summary>
        /// 点击的时候
        /// </summary>
        protected override void ContextClicked()
        {
            if (HasSelection())
            {
                _contextOnItem = false;
                return;
            }

            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent(Language.Refresh), false, o => { _window.Refresh(); }, null);
            menu.ShowAsContext();
        }

        /// <summary>
        /// 是否能多选
        /// </summary>
        /// <param name="item">选中的文件</param>
        /// <returns></returns>
        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            return 30;
        }

        protected override void ContextClickedItem(int id)
        {
            if (_contextOnItem)
            {
                _contextOnItem = false;
                return;
            }

            _contextOnItem = true;
            List<int> selectedNodes = new List<int>();
            foreach (var nodeID in GetSelection())
            {
                selectedNodes.Add(nodeID);
            }

            GenericMenu menu = new GenericMenu();
            if (selectedNodes.Count == 1)
            {
                var index = selectedNodes[0] - 1;
                var select = false;
                if (_window.SelectCompareVersion != null && _window.ShowVersionHistory != null)
                {
                    if (_window.ShowVersionHistory.Count > index)
                    {
                        var version = _window.ShowVersionHistory[index];
                        if (version != null)
                        {
                            if (version.ShowName == _window.SelectCompareVersion.ShowName)
                            {
                                select = true;
                            }
                        }
                    }
                }

                if (select)
                {
                    menu.AddItem(new GUIContent(Language.HistoryUnSelectCompare), false, HistoryUnSelectCompare,
                        selectedNodes);
                }
                else
                {
                    menu.AddItem(new GUIContent(Language.HistorySelectCompare), false, HistorySelectCompare,
                        selectedNodes);
                }

                menu.AddItem(new GUIContent(Language.HistoryUse), false, HistoryUse,
                    selectedNodes);
                menu.AddItem(new GUIContent(Language.HistoryCopyToFolder), false, HistoryCopyToFolder,
                    selectedNodes);
                menu.AddItem(new GUIContent(Language.HistoryDelete), false, HistoryDelete,
                    selectedNodes);
            }

            menu.ShowAsContext();
        }

        #endregion

        #region 事件回调

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            var selectedBundles = new List<string>();
            foreach (var id in selectedIds)
            {
                var item = FindItem(id, rootItem);
                if (item != null)
                    selectedBundles.Add(item.displayName);
            }

            _window.UpdateSelectedVersion(selectedBundles.Count > 0 ? selectedBundles[0] : string.Empty);
        }

        private void HistoryUse(object context)
        {
            if (context is List<int> selectedNodes && selectedNodes.Count > 0)
            {
                TreeViewItem item = FindItem(selectedNodes[0], rootItem);
                if (item is HistoryVersionTreeViewItem versionTreeViewItem)
                {
                    versionTreeViewItem.Version?.CopyToStreamingAssets();
                    EditorUtility.DisplayDialog(Language.Tips, Language.Success, Language.Confirm);
                }
            }
        }

        private void HistoryCopyToFolder(object context)
        {
            if (context is List<int> selectedNodes && selectedNodes.Count > 0)
            {
                TreeViewItem item = FindItem(selectedNodes[0], rootItem);
                if (item is HistoryVersionTreeViewItem versionTreeViewItem)
                {
                    var saveFolder =
                        EditorUtility.OpenFolderPanel(Language.BuildProfilerTips, Environment.CurrentDirectory, "");
                    versionTreeViewItem.Version?.CopyToFolder(saveFolder + "/");
                    EditorUtility.DisplayDialog(Language.Tips, Language.Success + $", path:{saveFolder}",
                        Language.Confirm);
                }
            }
        }

        private void HistoryDelete(object context)
        {
            if (context is List<int> selectedNodes && selectedNodes.Count > 0)
            {
                TreeViewItem item = FindItem(selectedNodes[0], rootItem);
                if (item is HistoryVersionTreeViewItem versionTreeViewItem)
                {
                    if (versionTreeViewItem.Version != null)
                    {
                        HistoryUtil.DeleteHistoryVersions(versionTreeViewItem.Version.FileName);
                        _window.Refresh();
                    }
                }
            }
        }

        private void HistorySelectCompare(object context)
        {
            if (context is List<int> selectedNodes && selectedNodes.Count > 0)
            {
                TreeViewItem item = FindItem(selectedNodes[0], rootItem);
                if (item is HistoryVersionTreeViewItem versionTreeViewItem)
                {
                    if (versionTreeViewItem.Version != null)
                    {
                        _window.UpdateCompareSelectedVersion(versionTreeViewItem.Version.ShowName);
                    }
                }
            }
        }

        private void HistoryUnSelectCompare(object context)
        {
            if (_window.SelectCompareVersion == null) return;
            if (context is List<int> selectedNodes && selectedNodes.Count > 0)
            {
                TreeViewItem item = FindItem(selectedNodes[0], rootItem);
                if (item is HistoryVersionTreeViewItem versionTreeViewItem)
                {
                    var v = versionTreeViewItem.Version;
                    if (v != null && _window.SelectCompareVersion.ShowName == v.ShowName)
                    {
                        _window.UpdateCompareSelectedVersion(string.Empty);
                    }
                }
            }
        }

        #endregion
    }
}