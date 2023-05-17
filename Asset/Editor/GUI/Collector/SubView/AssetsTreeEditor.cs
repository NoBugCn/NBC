using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace NBC.Asset.Editor
{
    public sealed class AssetTreeViewItem : TreeViewItem
    {
        private BuildAsset _asset;

        public BuildAsset Asset => _asset;

        public AssetTreeViewItem(int id, BuildAsset asset) : base(id, id, asset.Path)
        {
            _asset = asset;
            icon = AssetDatabase.GetCachedIcon(asset.Path) as Texture2D;
        }
    }

    public class AssetsTreeEditor : TreeView
    {
        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            return new MultiColumnHeaderState(GetColumns());
        }

        private static MultiColumnHeaderState.Column[] GetColumns()
        {
            List<MultiColumnHeaderState.Column> retVal = new List<MultiColumnHeaderState.Column>();
            retVal.Add(EditUtil.GetMultiColumnHeaderColumn(Language.Path,200));
            retVal.Add(EditUtil.GetMultiColumnHeaderColumn(Language.FileSize));
            retVal.Add(EditUtil.GetMultiColumnHeaderColumn(Language.AddressPath));
            return retVal.ToArray();
        }

        /// <summary>
        /// 当前选中了
        /// </summary>
        private bool mContextOnItem;

        CollectorWindow _window;
        public MultiColumnHeaderState HeaderState;
        
        public AssetsTreeEditor(TreeViewState state, CollectorWindow window, MultiColumnHeaderState header) : base(state,
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
            var cache = Caches.Get();
            if (cache.Assets.Count > 0)
            {
                if (_window.SelectPackageConfig != null && _window.SelectGroupConfig != null)
                {
                    foreach (var asset in cache.Assets)
                    {
                        if (asset.Group == null) continue;
                        if (asset.Group != _window.SelectGroupConfig)
                            continue;
                        id++;
                        var t = new AssetTreeViewItem(id, asset);
                        _root.AddChild(t);
                    }
                }
            }

            return _root;
        }

        enum MyColumns
        {
            Asset,
            Size,
            Path
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            HeaderState.AutoWidth(rect.width,2);
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
            AssetTreeViewItem assetTreeViewItem = item as AssetTreeViewItem;
            if (assetTreeViewItem == null) return;
            var assetData = assetTreeViewItem.Asset;
            Color oldColor = GUI.color;
            CenterRectUsingSingleLineHeight(ref cellRect);
            if (!File.Exists(assetData.Path))
            {
                GUI.color = Color.red;
            }

            switch ((MyColumns)column)
            {
                case MyColumns.Asset:
                {
                    var iconRect = new Rect(cellRect.x + 1, cellRect.y + 1, cellRect.height - 2, cellRect.height - 2);
                    if (item.icon != null)
                    {
                        GUI.DrawTexture(iconRect, item.icon, ScaleMode.ScaleToFit);
                    }

                    var nameRect = new Rect(cellRect.x + iconRect.xMax + 1
                        , cellRect.y, cellRect.width - iconRect.width, cellRect.height);

                    DefaultGUI.Label(nameRect,
                        item.displayName,
                        args.selected,
                        args.focused);
                }
                    break;
                case MyColumns.Size:
                    EditorGUI.LabelField(cellRect, GetSizeString(assetData), GUITools.DefLabelStyle);
                    break;
                case MyColumns.Path:
                    DefaultGUI.Label(cellRect, assetData.Address, args.selected, args.focused);
                    break;
            }

            GUI.color = oldColor;
        }


        /// <summary>
        /// 点击的时候
        /// </summary>
        protected override void ContextClicked()
        {
            if (HasSelection())
            {
                mContextOnItem = false;
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
            if (mContextOnItem)
            {
                mContextOnItem = false;
                return;
            }

            mContextOnItem = true;
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


        public string GetSizeString(BuildAsset asset)
        {
            if (asset.Size == 0)
                return "--";
            return EditorUtility.FormatBytes(asset.Size);
        }

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
    }
}