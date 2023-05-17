using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace NBC.Asset.Editor
{
    public class BuilderWindow : EditorWindow
    {
        [MenuItem(Language.BuilderWindowNameMenuPath, false, 2)]
        public static void ShowWindow()
        {
            BuilderWindow wnd = GetWindow<BuilderWindow>(Language.BuilderWindowName, true, Defs.DockedWindowTypes);
            wnd.minSize = new Vector2(Defs.DefWindowWidth, Defs.DefWindowHeight);
        }

        const int _splitterThickness = 2;

        public BuildBundle SelectBundleConfig;

        readonly VerticalSplitter _verticalSplitter = new VerticalSplitter(0.8f, 0.7f, 0.8f);
        readonly HorizontalSplitter _horizontalSplitter = new HorizontalSplitter(0.4f, 0.3f, 0.6f);

        private BuildBundleTreeEditor _buildBundleList;
        private BuildBundleAssetsTreeEditor _buildBundleAssetsTreeEditor;

        public void UpdateSelectedBundle(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var cache = Caches.Get();
                if (cache.Bundles == null) return;
                foreach (var bundle in cache.Bundles)
                {
                    if (bundle.Name != name) continue;
                    SelectBundleConfig = bundle;
                }
            }
            else
            {
                SelectBundleConfig = null;
            }

            _buildBundleAssetsTreeEditor?.Reload();
        }

        protected void OnEnable()
        {
            Builder.Gather();
            Styles.Initialize();
        }

        protected void OnGUI()
        {
            var barHeight = _splitterThickness;
            Rect contentRect = new Rect(_splitterThickness, barHeight, position.width - _splitterThickness * 4,
                position.height - barHeight - _splitterThickness);

            var resizingPackage = _horizontalSplitter.OnGUI(contentRect, out var bundleRect, out var infoRect);

            DrawBuildBundleList(bundleRect);
            bool resizingVer = _verticalSplitter.OnGUI(infoRect, out var top, out var bot);
            DrawAssetList(top);
            DrawBuildOperation(bot);
            if (resizingPackage || resizingVer)
                Repaint();
        }
        


        void DrawBuildBundleList(Rect rect)
        {
            if (_buildBundleList == null)
            {
                _buildBundleList = new BuildBundleTreeEditor(new TreeViewState(), this,
                    BuildBundleTreeEditor.CreateDefaultMultiColumnHeaderState());
            }

            _buildBundleList.OnGUI(rect);
        }

        void DrawAssetList(Rect rect)
        {
            if (_buildBundleAssetsTreeEditor == null)
            {
                _buildBundleAssetsTreeEditor = new BuildBundleAssetsTreeEditor(new TreeViewState(), this,
                    BuildBundleAssetsTreeEditor.CreateDefaultMultiColumnHeaderState());
            }

            _buildBundleAssetsTreeEditor.OnGUI(rect);
        }

        void DrawBuildOperation(Rect rect)
        {
            GUILayout.BeginArea(rect);
            GUILayout.Space(20);
            var height = rect.height * 0.4f;
            if (GUILayout.Button(Language.BuildStart, GUILayout.Height((int)height)))
            {
                Builder.Build();
                HistoryUtil.ShowLastBuildInfo();
            }

            GUILayout.EndArea();
        }
    }
}