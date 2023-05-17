using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace NBC.Asset.Editor
{
    public class ProfilerAssetListView : TreeView
    {
        public sealed class AssetViewItem : TreeViewItem
        {
            private DebugAssetInfo _asset;

            public DebugAssetInfo Asset => _asset;

            public AssetViewItem(int id, DebugAssetInfo asset) : base(id, id, asset.Path)
            {
                _asset = asset;
                icon = AssetDatabase.GetCachedIcon(asset.Path) as Texture2D;
            }
        }

        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            return new MultiColumnHeaderState(GetColumns());
        }

        private static MultiColumnHeaderState.Column[] GetColumns()
        {
            List<MultiColumnHeaderState.Column> retVal = new List<MultiColumnHeaderState.Column>();

            var f1 = EditUtil.GetMultiColumnHeaderColumn(Language.ProfilerAssetPath, 350, 100, 10000);
            f1.headerTextAlignment = TextAlignment.Left;
            var f2 = EditUtil.GetMultiColumnHeaderColumn(Language.ProfilerAssetType);
            var f3 = EditUtil.GetMultiColumnHeaderColumn(Language.ProfilerLoadTime);
            var f4 = EditUtil.GetMultiColumnHeaderColumn(Language.ProfilerLoadTotalTime, 80, 80);
            var f5 = EditUtil.GetMultiColumnHeaderColumn(Language.ProfilerLoadScene);
            var f6 = EditUtil.GetMultiColumnHeaderColumn(Language.ProfilerRefCount, 80, 60);
            var f7 = EditUtil.GetMultiColumnHeaderColumn(Language.ProfilerStatus);
            retVal.Add(f1);
            retVal.Add(f2);
            retVal.Add(f3);
            retVal.Add(f4);
            retVal.Add(f5);
            retVal.Add(f6);
            retVal.Add(f7);
            return retVal.ToArray();
        }

        enum MyColumns
        {
            AssetPath,
            AssetType,
            LoadTime,
            LoadTotalTime,
            LoadScene,
            RefCount,
            Status
        }

        ProfilerWindow _window;
        private List<DebugAssetInfo> _assetInfos;

        public string SelectName;

        public ProfilerAssetListView(TreeViewState state, ProfilerWindow window, MultiColumnHeaderState mchs) : base(
            state,
            new MultiColumnHeader(mchs))
        {
            _window = window;
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            Reload();
        }

        public void SetData(List<DebugAssetInfo> list)
        {
            _assetInfos = list;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem(-1, -1);
            root.children = new List<TreeViewItem>();
            if (_assetInfos != null)
            {
                for (var i = 0; i < _assetInfos.Count; i++)
                {
                    root.AddChild(new AssetViewItem(i, _assetInfos[i]));
                }
            }

            return root;
        }

        protected override void SingleClickedItem(int id)
        {
            if (FindItem(id, rootItem) is AssetViewItem assetItem)
            {
                SelectName = assetItem.Asset.Path;
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
            AssetViewItem assetTreeViewItem = item as AssetViewItem;
            if (assetTreeViewItem == null) return;
            var assetData = assetTreeViewItem.Asset;
            switch ((MyColumns)column)
            {
                case MyColumns.AssetPath:
                {
                    var iconRect = new Rect(cellRect.x + 1, cellRect.y + 1, cellRect.height - 2, cellRect.height - 2);
                    if (item.icon != null)
                    {
                        GUI.DrawTexture(iconRect, item.icon, ScaleMode.ScaleToFit);
                    }

                    var nameRect = new Rect(cellRect.x + iconRect.xMax + 1, cellRect.y, cellRect.width - iconRect.width,
                        cellRect.height);

                    DefaultGUI.Label(nameRect, item.displayName, args.selected, args.focused);
                }
                    break;
                case MyColumns.AssetType:
                    EditorGUI.LabelField(cellRect, assetData.Type, GUITools.DefLabelStyle);
                    break;
                case MyColumns.LoadTime:
                    EditorGUI.LabelField(cellRect, assetData.LoadTime, GUITools.DefLabelStyle);
                    break;
                case MyColumns.LoadTotalTime:
                    EditorGUI.LabelField(cellRect, assetData.LoadTotalTime + "ms", GUITools.DefLabelStyle);
                    break;
                case MyColumns.LoadScene:
                    EditorGUI.LabelField(cellRect, assetData.LoadScene, GUITools.DefLabelStyle);
                    break;
                case MyColumns.RefCount:
                    EditorGUI.LabelField(cellRect, assetData.Ref.ToString(), GUITools.DefLabelStyle);
                    break;
                case MyColumns.Status:
                    EditorGUI.LabelField(cellRect, assetData.Status, GUITools.DefLabelStyle);
                    break;
            }
        }
    }
}