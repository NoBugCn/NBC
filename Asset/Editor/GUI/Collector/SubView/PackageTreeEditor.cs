using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace NBC.Asset.Editor
{
    public class PackageTreeViewItem : TreeViewItem
    {
        private PackageConfig _package;

        public PackageConfig Package => _package;

        public PackageTreeViewItem(int id, PackageConfig package) : base(id, id, package.Name)
        {
            _package = package;
        }
    }

    public class PackageTreeEditor : TreeView
    {
        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            return new MultiColumnHeaderState(GetColumns());
        }

        private static MultiColumnHeaderState.Column[] GetColumns()
        {
            List<MultiColumnHeaderState.Column> retVal = new List<MultiColumnHeaderState.Column>();
            var fist = EditUtil.GetMultiColumnHeaderColumn(Language.AssetPackageName);
            retVal.Add(fist);
            return retVal.ToArray();
        }


        /// <summary>
        /// 当前选中了
        /// </summary>
        private bool mContextOnItem = false;

        CollectorWindow m_Window;
        public MultiColumnHeaderState HeaderState;
        public PackageTreeEditor(TreeViewState state, CollectorWindow window, MultiColumnHeaderState header)
            : base(state, new MultiColumnHeader(header))
        {
            showBorder = true;
            HeaderState = header;
            showAlternatingRowBackgrounds = false;
            m_Window = window;
            Refresh();
            Reload();
        }


        private TreeViewItem mRoot = null;

        protected override TreeViewItem BuildRoot()
        {
            mRoot = new TreeViewItem
            {
                id = -1,
                depth = -1,
                children = new List<TreeViewItem>()
            };
            int id = 0;
            var packages = CollectorSetting.Instance.Packages;
            foreach (var package in packages)
            {
                id++;
                var t = new PackageTreeViewItem(id, package);
                mRoot.AddChild(t);
            }

            return mRoot;
        }

        internal void Refresh()
        {
            var selection = GetSelection();

            SelectionChanged(selection);
        }

        private void ReloadAndSelect(IList<int> hashCodes)
        {
            Reload();
            SetSelection(hashCodes, TreeViewSelectionOptions.RevealAndFrame);
            SelectionChanged(hashCodes);
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
            if (item is PackageTreeViewItem packageTreeViewItem)
            {
                Color oldColor = GUI.color;
                if (!packageTreeViewItem.Package.Enable)
                {
                    GUI.color = Color.gray;
                }

                CenterRectUsingSingleLineHeight(ref cellRect);
                if (column == 0)
                {
                    var iconRect = new Rect(cellRect.x + 4, cellRect.y + 1, cellRect.height - 2, cellRect.height - 2);
                    var icon = packageTreeViewItem.Package.Default ? Styles.Package : Styles.PackageLow;
                    GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                    
                    var nameRect = new Rect(cellRect.x + iconRect.xMax + 1, cellRect.y, cellRect.width - iconRect.width,
                        cellRect.height);
                    if (packageTreeViewItem.Package.Default)
                    {
                        DefaultGUI.BoldLabel(nameRect, item.displayName, args.selected, args.focused);
                    }
                    else
                    {
                        DefaultGUI.Label(nameRect, item.displayName, args.selected, args.focused);
                    }
                }

                GUI.color = oldColor;
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

        /// <summary>
        /// 能否重新命名
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override bool CanRename(TreeViewItem item)
        {
            return item.displayName.Length > 0;
        }

        protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item)
        {
            return rowRect;
        }

        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            return 30;
        }

        #endregion

        #region 事件回调

        /// <summary>
        /// 更改名称完成
        /// </summary>
        /// <param name="args"></param>
        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (!string.IsNullOrEmpty(args.newName) && args.newName != args.originalName)
            {
                if (NameIsOnly(args.newName))
                {
                    var packages = CollectorSetting.Instance.Packages;
                    var package = packages[args.itemID - 1];
                    if (package.Name == args.originalName)
                    {
                        package.Name = args.newName;
                    }

                    Reload();
                }
                else
                {
                    Debug.LogError(Language.RepetitiveName);
                }
            }
            else
            {
                args.acceptedRename = false;
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            var selectedBundles = new List<string>();
            foreach (var id in selectedIds)
            {
                var item = FindItem(id, rootItem);
                if (item != null)
                    selectedBundles.Add(item.displayName);
            }

            m_Window.UpdateSelectedPackage(selectedBundles.Count > 0 ? selectedBundles[0] : string.Empty);
        }

        #endregion

        #region 菜单事件

        /// <summary>
        /// 点击的时候
        /// </summary>
        protected override void ContextClicked()
        {
            if (HasSelection())
            {
                mContextOnItem = false;
                return;
            }

            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent(Language.AddPackage), false, CreateResGroup, null);
            menu.ShowAsContext();
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
                menu.AddItem(new GUIContent(Language.AddGroup), false, CreateResGroup, null);
                try
                {
                    var index = selectedNodes[0] - 1;
                    var package = CollectorSetting.Instance.Packages[index];
                    menu.AddItem(new GUIContent(package.Enable ? Language.Disable : Language.Enable), false,
                        ChangeEnable,
                        selectedNodes);

                    var str = package.Default ? Language.DisableDefPackage : Language.EnableDefPackage;
                    menu.AddItem(new GUIContent(str), false, ChangeDefPackage, selectedNodes);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                menu.AddItem(new GUIContent(Language.ReName), false, RenameGroupName, selectedNodes);
                menu.AddItem(new GUIContent(Language.Del), false, DeleteGroups, selectedNodes);
            }

            menu.ShowAsContext();
        }

        protected override void KeyEvent()
        {
            if (Event.current.keyCode == KeyCode.Delete && GetSelection().Count > 0)
            {
                var list = GetSelection();
                DeleteGroups(list.ToList());
            }
        }

        void ChangeDefPackage(object b)
        {
            if (b is List<int> selectedNodes)
            {
                var packages = CollectorSetting.Instance.Packages;
                foreach (var id in selectedNodes)
                {
                    var pack = packages[id - 1];
                    pack.Default = !pack.Default;
                }
            }
        }

        void ChangeEnable(object b)
        {
            if (b is List<int> selectedNodes)
            {
                var packages = CollectorSetting.Instance.Packages;
                foreach (var id in selectedNodes)
                {
                    var pack = packages[id - 1];
                    pack.Enable = !pack.Enable;
                }
            }
        }

        void CreateResGroup(object context)
        {
            var package = new PackageConfig()
            {
                Name = GetOnlyName()
            };
            CollectorSetting.Instance.Packages.Add(package);
            var id = CollectorSetting.Instance.Packages.Count;
            ReloadAndSelect(new List<int>() { id });
        }

        void DeleteGroups(object b)
        {
            if (b is List<int> selectedNodes)
            {
                foreach (var i in selectedNodes)
                {
                    var index = i - 1;
                    CollectorSetting.Instance.Packages.RemoveAt(index);
                }

                ReloadAndSelect(new List<int>());
                m_Window.UpdateSelectedPackage(string.Empty);
            }
        }

        void RenameGroupName(object context)
        {
            if (context is List<int> selectedNodes && selectedNodes.Count > 0)
            {
                TreeViewItem item = FindItem(selectedNodes[0], rootItem);
                BeginRename(item, 0.25f);
            }
        }

        #endregion

        #region 工具方法

        bool NameIsOnly(string name)
        {
            var packages = CollectorSetting.Instance.Packages;
            foreach (var package in packages)
            {
                if (package.Name == name)
                {
                    return false;
                }
            }

            return true;
        }

        string GetOnlyName()
        {
            var index = CollectorSetting.Instance.Packages.Count;
            for (int i = 0; i < 10; i++)
            {
                var name = $"package{index + i}";
                if (NameIsOnly(name))
                {
                    return name;
                }
            }

            return $"package{DateTimeOffset.Now.ToUnixTimeSeconds()}";
        }

        #endregion
    }
}