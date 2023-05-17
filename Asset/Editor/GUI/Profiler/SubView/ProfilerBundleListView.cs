using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace NBC.Asset.Editor
{
    public class ProfilerBundleListView : TreeView
    {
        public sealed class BundleViewItem : TreeViewItem
        {
            private DebugBundleInfo _bundle;

            public DebugBundleInfo Bundle => _bundle;

            public BundleViewItem(int id, DebugBundleInfo bundle) : base(id, id, bundle.BundleName)
            {
                _bundle = bundle;
            }
        }

        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            return new MultiColumnHeaderState(GetColumns());
        }

        private static MultiColumnHeaderState.Column[] GetColumns()
        {
            List<MultiColumnHeaderState.Column> retVal = new List<MultiColumnHeaderState.Column>();

            var f1 = EditUtil.GetMultiColumnHeaderColumn(Language.ProfilerBundleName, 410, 100, 10000);
            f1.headerTextAlignment = TextAlignment.Left;
            var f3 = EditUtil.GetMultiColumnHeaderColumn(Language.ProfilerLoadTime);
            var f4 = EditUtil.GetMultiColumnHeaderColumn(Language.ProfilerLoadTotalTime);
            var f5 = EditUtil.GetMultiColumnHeaderColumn(Language.ProfilerLoadScene);
            var f6 = EditUtil.GetMultiColumnHeaderColumn(Language.ProfilerRefCount);
            var f7 = EditUtil.GetMultiColumnHeaderColumn(Language.ProfilerStatus);
            retVal.Add(f1);
            retVal.Add(f3);
            retVal.Add(f4);
            retVal.Add(f5);
            retVal.Add(f6);
            retVal.Add(f7);
            return retVal.ToArray();
        }

        enum MyColumns
        {
            BundleName,
            LoadTime,
            LoadTotalTime,
            LoadScene,
            RefCount,
            Status
        }

        ProfilerWindow _window;
        private List<DebugBundleInfo> _bundleInfos;
        private readonly GUIStyle _defLabelStyle;
        public string SelectName;

        public ProfilerBundleListView(TreeViewState state, ProfilerWindow window, MultiColumnHeaderState mchs) : base(
            state,
            new MultiColumnHeader(mchs))
        {
            _window = window;
            showBorder = true;
            showAlternatingRowBackgrounds = true;

            _defLabelStyle = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                alignment = TextAnchor.MiddleCenter
            };

            Reload();
        }

        public void SetData(List<DebugBundleInfo> list)
        {
            _bundleInfos = list;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem(-1, -1);
            root.children = new List<TreeViewItem>();
            if (_bundleInfos != null)
            {
                for (var i = 0; i < _bundleInfos.Count; i++)
                {
                    root.AddChild(new BundleViewItem(i, _bundleInfos[i]));
                }
            }

            return root;
        }

        protected override void SingleClickedItem(int id)
        {
            if (FindItem(id, rootItem) is BundleViewItem item)
            {
                SelectName = item.Bundle.BundleName;
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
            BundleViewItem treeViewItem = item as BundleViewItem;
            if (treeViewItem == null) return;
            var bundleData = treeViewItem.Bundle;
            switch ((MyColumns)column)
            {
                case MyColumns.BundleName:
                    EditorGUI.LabelField(cellRect, bundleData.BundleName);
                    break;
                case MyColumns.LoadTime:
                    EditorGUI.LabelField(cellRect, bundleData.LoadTime, _defLabelStyle);
                    break;
                case MyColumns.LoadTotalTime:
                    EditorGUI.LabelField(cellRect, bundleData.LoadTotalTime + "ms", _defLabelStyle);
                    break;
                case MyColumns.LoadScene:
                    EditorGUI.LabelField(cellRect, bundleData.LoadScene, _defLabelStyle);
                    break;
                case MyColumns.RefCount:
                    EditorGUI.LabelField(cellRect, bundleData.Ref.ToString(), _defLabelStyle);
                    break;
                case MyColumns.Status:
                    EditorGUI.LabelField(cellRect, bundleData.Status, _defLabelStyle);
                    break;
            }
        }
    }
}