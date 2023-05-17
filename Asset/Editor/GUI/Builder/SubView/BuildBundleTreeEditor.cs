using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace NBC.Asset.Editor
{
    public class BuildBundleTreeViewItem : TreeViewItem
    {
        private BuildBundle _bundle;

        public BuildBundle Bundle => _bundle;

        public BuildBundleTreeViewItem(int id, BuildBundle bundle) : base(id, id, bundle.Name)
        {
            _bundle = bundle;
        }
    }

    public class BuildBundleTreeEditor : TreeView
    {
        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            return new MultiColumnHeaderState(GetColumns());
        }

        private static MultiColumnHeaderState.Column[] GetColumns()
        {
            List<MultiColumnHeaderState.Column> retVal = new List<MultiColumnHeaderState.Column>();
            var fist = EditUtil.GetMultiColumnHeaderColumn(Language.BuildBundleName, 200, 200, 1000);
            retVal.Add(fist);
            return retVal.ToArray();
        }


        /// <summary>
        /// 当前选中了
        /// </summary>
        private bool mContextOnItem = false;

        BuilderWindow _window;

        public MultiColumnHeaderState HeaderState;

        public BuildBundleTreeEditor(TreeViewState state, BuilderWindow window, MultiColumnHeaderState header)
            : base(state, new MultiColumnHeader(header))
        {
            HeaderState = header;
            showBorder = true;

            showAlternatingRowBackgrounds = false;
            _window = window;
            Refresh();
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
            var caches = Caches.Get();
            foreach (var bundle in caches.Bundles)
            {
                id++;
                var t = new BuildBundleTreeViewItem(id, bundle);
                _root.AddChild(t);
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
            if (item is BuildBundleTreeViewItem bundleTreeViewItem)
            {
                CenterRectUsingSingleLineHeight(ref cellRect);
                if (column == 0)
                {
                    var iconRect = new Rect(cellRect.x + 4, cellRect.y + 1, cellRect.height - 2, cellRect.height - 2);
                    GUI.DrawTexture(iconRect, Styles.Package, ScaleMode.ScaleToFit);

                    var nameRect = new Rect(cellRect.x + iconRect.xMax + 1, cellRect.y, cellRect.width - iconRect.width,
                        cellRect.height);
                    DefaultGUI.Label(nameRect, item.displayName, args.selected, args.focused);
                }
            }
        }

        #endregion

        #region 接口实现

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

            _window.UpdateSelectedBundle(selectedBundles.Count > 0 ? selectedBundles[0] : string.Empty);
        }

        #endregion
    }
}