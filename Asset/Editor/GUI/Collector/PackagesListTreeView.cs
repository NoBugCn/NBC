// using System.Collections.Generic;
// using UnityEditor.IMGUI.Controls;
//
// namespace NBC.Asset.Editor
// {
//     public class PackagesListTreeView : TreeView
//     {
//         private TreeViewItem mRoot = null;
//         
//         public PackagesListTreeView(TreeViewState state, AssetGroupMgr ctrl, MultiColumnHeaderState mchs) : base(state,
//             new MultiColumnHeader(mchs))
//         {
//             showBorder = true;
//
//             showAlternatingRowBackgrounds = false;
//             mController = ctrl;
//             Refresh();
//             Reload();
//         }
//         
//         protected override TreeViewItem BuildRoot()
//         {
//             mRoot = new TreeViewItem
//             {
//                 id = -1,
//                 depth = -1,
//                 children = new List<TreeViewItem>()
//             };
//             int id = 0;
//             var packages = CollectorSetting.Instance.Packages;
//             foreach (var package in packages)
//             {
//                 id++;
//                 var t = new PackageTreeViewItem(id, package);
//                 mRoot.AddChild(t);
//             }
//
//             return mRoot;
//         }
//
//         public PackagesListTreeView(TreeViewState state) : base(state)
//         {
//         }
//
//         public PackagesListTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
//         {
//         }
//     }
// }