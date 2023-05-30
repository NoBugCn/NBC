using System;
using System.Collections.Generic;
using System.Linq;
using FairyGUI;
using UnityEngine;
using NBC.Asset;

namespace NBC
{
    public class UIManager : EventDispatcher
    {
        /// <summary>
        /// 所有UI
        /// </summary>
        private readonly Dictionary<string, IUIPanel> _uiArray = new Dictionary<string, IUIPanel>();
        
        private GGraph _modalLayer = null;
        private GRoot _uiRoot;

        public void Start()
        {
            Log.I("UI 模块初始化");
            _uiRoot = FairyGUI.GRoot.inst;
            MonoManager.Inst.OnUpdate += Update;
            UICommand.Init(this);
        }

        public void Quit()
        {
            MonoManager.Inst.OnUpdate -= Update;
        }

        void Update()
        {
            UIRunner.Update();
        }
        
        #region 内部方法

        internal void ShowUI(string uiName, object param = null)
        {
            IUIPanel panel = GetUI(uiName);
            panel.SetData(param);
            if (panel.IsShowing)
            {
                panel.Refresh();
            }
            else
            {
                panel.Show();
            }
        }

        internal void RemoveUI(string uiName)
        {
            IUIPanel wind = GetUI(uiName);
            if (wind == null)
            {
                Log.W($"要删除的界面不存在:{uiName}");
                return;
            }

            if (!wind.IsDotDel)
            {
                wind.Dispose();
                _uiArray.Remove(uiName);
            }
        }

        private void CreateModalLayer()
        {
            var modalLayerColor = FairyGUI.UIConfig.modalLayerColor;

            if (this._modalLayer != null)
            {
                this._modalLayer.onClick.Clear();
            }

            this._modalLayer = new FairyGUI.GGraph();
            this._modalLayer.DrawRect(this._uiRoot.width, this._uiRoot.height, 0, UnityEngine.Color.white,
                modalLayerColor);
            this._modalLayer.AddRelation(this._uiRoot, FairyGUI.RelationType.Size);
            this._modalLayer.name = this._modalLayer.gameObjectName = "ModalLayer";
            this._modalLayer.SetHome(this._uiRoot);
        }

        #endregion

        public void OpenUI<T>(object param = null)
        {
            Type type = typeof(T);

            OpenUI(type.Name, type, param);
        }

        public void OpenUI(Type type, object param = null)
        {
            OpenUI(type.Name, type, param);
        }

        public void OpenUI<T>(string uiName, object param = null,
            Action<IUIPanel> callback = null)
        {
            Type type = typeof(T);
            OpenUI(uiName, type, param, callback);
        }

        public void OpenUI(string uiName, Type type, object param = null,
            Action<IUIPanel> callback = null)
        {
            UICommand.GetCmd(UICmdType.Show).SetUIData(type, uiName, param).SetCallback(callback).Run();
        }

        internal void AddUI(string uiName, IUIPanel panel)
        {
            if (_uiArray.ContainsKey(uiName))
            {
                Log.E("AddUI重复添加");
            }

            _uiArray.Add(uiName, panel);
        }

        public IUIPanel GetUI<T>()
        {
            IUIPanel wind = null;
            Type type = typeof(T);
            var uiName = type.Name;
            foreach (var name in _uiArray.Keys)
            {
                if (name != uiName) continue;
                wind = _uiArray[name];
                break;
            }

            return wind;
        }

        public IUIPanel GetUI(Type type)
        {
            return GetUI(type.Name);
        }

        public IUIPanel GetUI(string uiName)
        {
            IUIPanel wind = null;
            foreach (var name in _uiArray.Keys)
            {
                if (name != uiName) continue;
                wind = _uiArray[name];
                break;
            }

            return wind;
        }

        public IUIPanel[] GetAllUI()
        {
            return _uiArray.Values.ToArray();
        }

        public List<string> GetAllUIName()
        {
            return this._uiArray.Keys.ToList();
        }

        public void HideUI(Type type)
        {
            HideUI(type.Name);
        }

        public void HideUI<T>()
        {
            Type type = typeof(T);
            HideUI(type.Name);
        }

        public void HideUI(string uiName)
        {
            UICommand.GetCmd(UICmdType.Hide).SetUIData(null, uiName, null).Run();
        }


        /// <summary>
        /// 隐藏所有窗口
        /// </summary>
        public void HideAllUI(bool isDotDel = false)
        {
            var names = GetAllUIName();
            foreach (var uiName in names)
            {
                IUIPanel panel = GetUI(uiName);
                if (panel.IsShowing)
                {
                    if (!panel.IsDotDel || isDotDel)
                    {
                        HideUI(uiName);
                    }
                }
            }
        }

        /// <summary>
        /// 删除所有打开的窗口
        /// </summary>
        public void DeleteAllUI()
        {
            var names = GetAllUIName();
            foreach (var uiName in names)
            {
                DestroyUI(uiName);
            }
        }

        public void DestroyUI<T>()
        {
            Type type = typeof(T);
            DestroyUI(type.Name);
        }

        public void DestroyUI(Type type)
        {
            DestroyUI(type.Name);
        }

        public void DestroyUI(string uiName)
        {
            UICommand.GetCmd(UICmdType.Del).SetUIData(null, uiName, null).Run();
        }

        public void BringToFront(IUIPanel uiPanel)
        {
            var uiRoot = FairyGUI.GRoot.inst;
            var contentPane = uiPanel.ContentPane;
            if (contentPane.parent != uiRoot)
            {
                Log.E("不在root内，无法置顶==");
                return;
            }

            var cnt = uiRoot.numChildren;
            var i = 0;
            if (this._modalLayer != null && this._modalLayer.parent != null && !uiPanel.IsModal)
            {
                i = uiRoot.GetChildIndex(this._modalLayer);
            }
            else
            {
                i = cnt - 1;
            }

            if (i >= 0)
            {
                uiRoot.SetChildIndex(contentPane, i);
            }

            // //缓存最顶层UI信息
            // var sortingOrder = uiPanel.ContentPane.sortingOrder;
            // _uiTopInfo[sortingOrder] = uiPanel;
        }

        public bool IsTop(IUIPanel uiPanel)
        {
            // if (uiPanel == null || uiPanel.ContentPane == null || uiPanel.ContentPane.parent == null)
            // {
            //     return false;
            // }
            //
            // var sortingOrder = uiPanel.ContentPane.sortingOrder;
            // if (_uiTopInfo.TryGetValue(sortingOrder, out var ui))
            // {
            //     if (ui == uiPanel && uiPanel.IsShowing) return true;
            // }
            //
            // return false;

            var parent = uiPanel.ContentPane.parent;
            if (parent == null) return false;
            var sortingOrder = uiPanel.ContentPane.sortingOrder;
            var maxIndex = -1;
            var panels = this._uiArray.Values;
            foreach (var panel in panels)
            {
                if (panel.IsShowing && panel.ContentPane.sortingOrder == sortingOrder)
                {
                    //只判断同层级的
                    var index = parent.GetChildIndex(panel.ContentPane);
                    if (index > maxIndex)
                    {
                        maxIndex = index;
                    }
                }
            }

            var uiIndex = parent.GetChildIndex(uiPanel.ContentPane);
            return uiIndex >= maxIndex;
        }

        public void AdjustModalLayer()
        {
            if (this._modalLayer == null || this._modalLayer.isDisposed)
            {
                this.CreateModalLayer();
            }

            var showDic = new Dictionary<FairyGUI.GObject, IUIPanel>();
            var panels = _uiArray.Values;
            foreach (var panel in panels)
            {
                if (panel.IsShowing)
                {
                    showDic[panel.ContentPane] = panel;
                }
            }

            var cnt = this._uiRoot.numChildren;
            for (var i = cnt - 1; i >= 0; i--)
            {
                var g = this._uiRoot.GetChildAt(i);
                if (showDic.TryGetValue(g, out var panel))
                {
                    if (panel.IsModal)
                    {
                        if (this._modalLayer.parent == null)
                            this._uiRoot.AddChildAt(this._modalLayer, i);
                        else
                        {
                            this._uiRoot.SetChildIndexBefore(this._modalLayer, i);
                        }

                        return;
                    }
                }
            }

            if (this._modalLayer.parent != null)
            {
                this._uiRoot.RemoveChild(this._modalLayer);
            }
        }

        public bool IsOpen(string uiName)
        {
            IUIPanel wind = GetUI(uiName);
            return wind != null && wind.IsShowing;
        }

        public void AddPackage(string assetPath)
        {
            AddPackage(UIConst.UIPackRootUrl, assetPath);
        }

        public void AddPackage(string root, string assetPath)
        {
            var path = root + assetPath;
            if (path.StartsWith("Assets/"))
            {
                FairyGUI.UIPackage.AddPackage(path, (string name, string extension, Type type,
                    out DestroyMethod method) =>
                {
                    method = DestroyMethod.None;
                    var pro = Assets.LoadAsset(name.Replace("\\", "/") + extension, type);
                    return pro?.Asset;
                });
            }
            else
            {
                FairyGUI.UIPackage.AddPackage(path);
            }
        }

     
        public void SetFrozenAll(bool isFrozen, List<Type> unexpectName = null)
        {
            if (isFrozen)
            {
                foreach (var kv in _uiArray)
                {
                    var value = unexpectName?.Find(val => kv.Value.GetType() == val);
                    if (value != null)
                    {
                        continue;
                    }

                    if (kv.Value.ContentPane.visible)
                    {
                        kv.Value.IsFrozen = true;
                        kv.Value.ContentPane.visible = false;
                    }
                }
            }
            else
            {
                foreach (var kv in _uiArray)
                {
                    var value = unexpectName?.Find(val => kv.Value.GetType() == val);
                    if (value != null)
                    {
                        continue;
                    }

                    if (kv.Value.IsFrozen)
                    {
                        kv.Value.IsFrozen = false;
                        kv.Value.ContentPane.visible = true;
                    }
                }
            }
        }

        public void OpenSafeArea()
        {
            UIConst.OpenSafeArea = true;
        }

        public void SetSafeArea(Rect rect)
        {
            UIConst.SafeArea = rect;
        }

        public Rect GetSafeArea()
        {
            return UIConst.SafeArea;
        }
    }
}