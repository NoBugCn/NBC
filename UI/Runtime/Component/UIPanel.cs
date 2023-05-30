using System;
using System.Collections.Generic;
using FairyGUI;
using UnityEngine;

namespace NBC
{
    public abstract class UIPanel : IUIPanel
    {
        public static Func<string, string> GetUIPackNameFunc = s => $"{s}/{s}";

        /// <summary>
        /// 是否显示在最顶层
        /// </summary>
        public bool IsTop => _ui.IsTop(this);

        /// <summary>
        /// 是否显示黑色背景蒙版
        /// </summary>
        public bool IsModal { get; protected set; }

        public bool IsFrozen { get; set; }

        public virtual bool IsShowing => ContentPane != null && ContentPane.parent != null;

        public virtual bool IsCanVisible => ContentPane != null && ContentPane.parent != null && ContentPane.visible;

        public bool IsDotDel { get; protected set; }

        public virtual string UIPackRootUrl => string.Empty;

        public virtual string UIPackName { get; set; }
        public virtual string UIResName { get; set; }

        /// <summary>
        /// 模块id，(可用于对模块编号实现一些特定功能，如新手引导)
        /// </summary>
        public virtual int Id { get; protected set; }


        /// <summary>
        /// 面板打开动画
        /// </summary>
        public NTask ShowAnim = null;

        /// <summary>
        /// 面板关闭动画
        /// </summary>
        public NTask HideAnim = null;

        private object _paramData;
        private bool _isInited;

        public GComponent ContentPane { get; protected set; }
        protected UIManager _ui;

        public void SetUIManager(UIManager manager)
        {
            _ui = manager;
        }

        public void SetData(object args)
        {
            _paramData = args;
        }

        public object GetData()
        {
            return _paramData;
        }

        public virtual string[] GetDependPackages()
        {
            return new string[] { };
        }

        public virtual Dictionary<string, string> GetLanguageConfig()
        {
            return new Dictionary<string, string>();
        }

        public void Init()
        {
            try
            {
                var uiPackRootUrl = string.IsNullOrEmpty(UIPackRootUrl) ? UIConst.UIPackRootUrl : UIPackRootUrl;
                //实例化预设
                Log.I("实例化资源===");
                if (!_isInited)
                {
                    var dependPackages = GetDependPackages();
                    if (dependPackages != null && dependPackages.Length > 0)
                    {
                        foreach (var package in dependPackages)
                        {
                            if (package != UIPackName)
                            {
                                _ui.AddPackage(uiPackRootUrl, GetUIPackNameFunc(package));
                            }
                        }
                    }

                    _ui.AddPackage(uiPackRootUrl, GetUIPackNameFunc(UIPackName));

                    GObject panelObj = UIPackage.CreateObject(UIPackName, UIResName);
                    if (panelObj == null)
                    {
                        throw new Exception("不存在包名：" + UIPackName + "/ResName=" + UIResName);
                    }

                    // panelObj.name = UIResName;
                    panelObj.SetSize(GRoot.inst.width, GRoot.inst.height);
                    panelObj.position = Vector3.zero;
                    panelObj.ToSafeArea();
                    panelObj.scale = Vector2.one;
                    panelObj.pivotX = 0.5f;
                    panelObj.pivotY = 0.5f;
                    ContentPane = panelObj.asCom;
                    ContentPane.name = UIResName;
                    this.AutoFindAllField();
                    OnInit();

                    // FairyBatching
                    GComponent panelObjCom = panelObj.asCom;
                    if (panelObjCom != null)
                    {
                        panelObjCom.fairyBatching = true;
                    }

                    _isInited = true;
                }
            }
            catch (Exception e)
            {
                Log.E(e);
                throw;
            }
        }
        
        public void Show()
        {
            try
            {
                if (!IsShowing)
                {
                    FairyGUI.GRoot.inst.AddChild(this.ContentPane);
                    _ui.AdjustModalLayer();
                }
                else
                {
                    if (!this.IsTop) _ui.BringToFront(this);
                }

                if (ShowAnim != null)
                {
                    ShowAnim.OnCompleted(OpenAnimFinished, true);
                    ShowAnim.Run(UIRunner.Def);
                }
                else
                {
                    OpenAnimFinished(null);
                }
            }
            catch (Exception e)
            {
                Log.E($"UIPackName={UIPackName} UIResName={UIResName} e={e}");
                throw;
            }
        }

        public void Hide()
        {
            if (!IsShowing) return;
            if (HideAnim != null)
            {
                HideAnim.OnCompleted(HideAnimFinished);
                HideAnim.Run(UIRunner.Def);
            }
            else
            {
                HideAnimFinished(null);
            }
        }

        public void Refresh()
        {
            Show();
        }

        public void HideImmediately()
        {
            // ContentPane.visible = false;
            if (ContentPane.parent != null)
            {
                // UIKit.
                FairyGUI.GRoot.inst.RemoveChild(this.ContentPane);
                _ui.AdjustModalLayer();

                _ui?.DispatchEventWith(UIEvents.UIHide, this);
                // UIKit.LayerManager.RemoveChild(this);
            }

            this.OnHide();
        }

        public void Dispose()
        {
            if (!IsDotDel)
            {
                this.HideImmediately();
                this.ContentPane.Dispose();
                this.OnDestroy();
            }
            else
            {
                Log.E("当前panel标记为不可删除，name=" + this.UIResName);
            }
        }


        /// <summary>
        /// 打开动画播放完成
        /// </summary>
        /// <param name="task"></param>
        private void OpenAnimFinished(ITask task)
        {
            this.OnShow();
            _ui?.DispatchEventWith(UIEvents.UIShow, this);
        }

        /// <summary>
        /// 关闭动画播放完成
        /// </summary>
        /// <param name="task"></param>
        private void HideAnimFinished(ITask task)
        {
            this.HideImmediately();
        }

        #region 接口

        /// <summary>
        /// 界面初始化的时候
        /// </summary>
        protected virtual void OnInit()
        {
        }

        /// <summary>
        /// 显示界面显示完成
        /// </summary>
        /// <param name="param"></param>
        protected virtual void OnShow()
        {
        }

        /// <summary>
        /// 界面隐藏的时候
        /// </summary>
        protected virtual void OnHide()
        {
        }

        /// <summary>
        /// 界面销毁的时候
        /// </summary>
        protected virtual void OnDestroy()
        {
        }

        #endregion
    }
}