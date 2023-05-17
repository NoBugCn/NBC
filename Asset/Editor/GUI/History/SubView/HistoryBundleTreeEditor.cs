using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace NBC.Asset.Editor
{
    public sealed class HistoryBundleViewItem : TreeViewItem
    {
        private BundleData _bundle;

        public BundleData Bundle => _bundle;

        public HistoryBundleViewItem(int id, BundleData bundle) : base(id, id, bundle.Name)
        {
            _bundle = bundle;
        }
    }

    public class HistoryBundleTreeEditor : TreeView
    {
        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            return new MultiColumnHeaderState(GetColumns());
        }

        private static MultiColumnHeaderState.Column[] GetColumns()
        {
            List<MultiColumnHeaderState.Column> retVal = new List<MultiColumnHeaderState.Column>();
            retVal.Add(EditUtil.GetMultiColumnHeaderColumn(Language.HistoryBundleName, 260));
            retVal.Add(EditUtil.GetMultiColumnHeaderColumn(Language.HistorySize, 70, 50, 100));
            retVal.Add(EditUtil.GetMultiColumnHeaderColumn(Language.HistoryHash, 240,200,300));

            return retVal.ToArray();
        }
        enum MyColumns
        {
            BundleName,
            Size,
            Hash,
        }
        /// <summary>
        /// 当前选中了
        /// </summary>
        private bool _contextOnItem = false;

        HistoryWindow _window;
        public MultiColumnHeaderState HeaderState;

        public HistoryBundleTreeEditor(TreeViewState state, HistoryWindow window, MultiColumnHeaderState header) :
            base(state,
                new MultiColumnHeader(header))
        {
            _window = window;
            HeaderState = header;
            showBorder = true;
            showAlternatingRowBackgrounds = true;

            Reload();
        }

        private TreeViewItem _root = null;

        protected override TreeViewItem BuildRoot()
        {
            _root = new TreeViewItem
            {
                id = -1,
                depth = -1,
                children = new List<TreeViewItem>()
            };
            int id = 0;

            if (_window.SelectVersion != null)
            {
                List<BundleData> list = new List<BundleData>();
                foreach (var package in _window.SelectVersion.Packages)
                {
                    list.AddRange(package.Bundles);
                }


                foreach (var bundle in list)
                {
                    id++;
                    var t = new HistoryBundleViewItem(id, bundle);
                    _root.AddChild(t);
                }
            }

            return _root;
        }


        #region 绘制
        

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            HeaderState.AutoWidth(rect.width, 0);
            if (UnityEngine.Event.current.type == EventType.MouseDown && UnityEngine.Event.current.button == 0 &&
                rect.Contains(UnityEngine.Event.current.mousePosition))
            {
                SetSelection(Array.Empty<int>(), TreeViewSelectionOptions.FireSelectionChanged);
            }
        }


        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), args.item, args.GetColumn(i), ref args);
            }
        }

        private void CellGUI(Rect cellRect, TreeViewItem item, int column, ref RowGUIArgs args)
        {
            HistoryBundleViewItem bundleTreeViewItem = item as HistoryBundleViewItem;
            if (bundleTreeViewItem == null) return;
            var bundleData = bundleTreeViewItem.Bundle;
            Color oldColor = GUI.color;
            CenterRectUsingSingleLineHeight(ref cellRect);
            // if (!File.Exists(assetData.Path))
            // {
            //     GUI.color = Color.red;
            // }

            switch ((MyColumns)column)
            {
                case MyColumns.BundleName:
                    EditorGUI.LabelField(cellRect, bundleData.Name);
                    break;
                case MyColumns.Hash:
                    EditorGUI.LabelField(cellRect, bundleData.Hash);
                    break;
                case MyColumns.Size:
                    EditorGUI.LabelField(cellRect, Util.GetFriendlySize(bundleData.Size), GUITools.DefLabelStyle);
                    break;
            }

            GUI.color = oldColor;
        }

        #endregion

        #region 事件

        /// <summary>
        /// 点击的时候
        /// </summary>
        protected override void ContextClicked()
        {
            if (HasSelection())
            {
                _contextOnItem = false;
            }
        }

        protected override void DoubleClickedItem(int id)
        {
            if (FindItem(id, rootItem) is AssetTreeViewItem assetItem)
            {
                UnityEngine.Object o = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetItem.Asset.Path);
                EditorGUIUtility.PingObject(o);
                Selection.activeObject = o;
            }
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
                menu.AddItem(new GUIContent(Language.CopyPath), false, CopyPath, selectedNodes);
                menu.AddItem(new GUIContent(Language.CopyAddressPath), false, CopyAddressPath, selectedNodes);
            }

            menu.ShowAsContext();
        }

        #endregion

        #region 私有方法

        private void CopyAddressPath(object context)
        {
            if (context is List<int> selectedNodes && selectedNodes.Count > 0)
            {
                TreeViewItem item = FindItem(selectedNodes[0], rootItem);
                if (item is AssetTreeViewItem assetTreeViewItem)
                {
                    EditUtil.CopyToClipBoard(assetTreeViewItem.Asset.Address);
                    Debug.Log($"copy success：{assetTreeViewItem.Asset.Address}");
                }
            }
        }

        private void CopyPath(object context)
        {
            if (context is List<int> selectedNodes && selectedNodes.Count > 0)
            {
                TreeViewItem item = FindItem(selectedNodes[0], rootItem);
                if (item is AssetTreeViewItem assetTreeViewItem)
                {
                    EditUtil.CopyToClipBoard(assetTreeViewItem.Asset.Path);
                    Debug.Log($"copy success：{assetTreeViewItem.Asset.Path}");
                }
            }
        }

        #endregion
    }
}