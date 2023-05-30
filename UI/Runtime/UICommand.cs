using System;
using System.Collections.Generic;

namespace NBC
{
    internal enum UICmdType
    {
        Show,
        Hide,
        Del
    }

    internal enum UICommandState
    {
        None,
        Wait,
        Done,
    }

    internal class UICommand
    {
        
        #region 静态

        private static UIManager _uiKit;
        private static readonly Queue<UICommand> _pools = new();
        private static readonly List<UICommand> _curCmd = new();

        public static void Init(UIManager uiKit)
        {
            _uiKit = uiKit;
            MonoManager.Inst.OnUpdate += OnUpdate;
        }

        public static bool HasCmd(Type type, UICmdType cmdType)
        {
            return _curCmd.Exists(a => a.UIType == type && a.CmdType == cmdType);
        }

        public static UICommand GetCmd(UICmdType uiCmdType)
        {
            var cmd = _pools.Count > 0 ? _pools.Dequeue() : new UICommand();
            cmd.CmdType = uiCmdType;
            return cmd;
        }

        public static void RevertCmd(UICommand cmd)
        {
            cmd.RestData();
            _pools.Enqueue(cmd);
        }

        private static void OnUpdate()
        {
            if (_curCmd.Count < 1) return;
            var cur = _curCmd[0];
            if (cur.State == UICommandState.None)
            {
                cur.State = UICommandState.Wait;
                switch (cur.CmdType)
                {
                    case UICmdType.Show:
                        cur.Show();
                        break;
                    case UICmdType.Hide:
                        cur.Hide();
                        break;
                    case UICmdType.Del:
                        cur.Del();
                        break;
                }
            }
            else if (cur.State == UICommandState.Done)
            {
                var cmd = _curCmd[0];
                _curCmd.RemoveAt(0);
                if (cmd != null) RevertCmd(cmd);
            }
        }

        #endregion

        public UICmdType CmdType;
        public Type UIType;
        public Action<IUIPanel> Callback;
        public object Param;
        public IUIPanel Panel;
        public string UIName;
        
        public UICommandState State;

        public void RestData()
        {
            Param = null;
            Panel = null;
            State = UICommandState.None;
            Callback = null;
            UIType = null;
        }

        public UICommand SetCallback(Action<IUIPanel> callback)
        {
            Callback = callback;
            return this;
        }

        public UICommand SetUIData(Type uiType, string uiName, object param)
        {
            UIType = uiType;
            UIName = uiName;
            Param = param;
            return this;
        }


        public void Run()
        {
            State = UICommandState.None;
            _curCmd.Add(this);
        }

        public void Show()
        {
            if (UIName == "PlotPanel")
            {
                
            }
            IUIPanel panel = _uiKit.GetUI(UIName);
            if (panel == null)
            {
                panel = Activator.CreateInstance(UIType) as IUIPanel;
                if (panel != null)
                {
                    panel.SetUIManager(_uiKit);
                    panel.SetData(Param);
                    panel.Init();
                    _uiKit.AddUI(UIName, panel);
                }
            }

            State = UICommandState.Done;
            _uiKit.ShowUI(UIName, Param);
            Callback?.Invoke(panel);
        }

        public void Hide()
        {
            State = UICommandState.Done;
            IUIPanel wind = _uiKit.GetUI(UIName);
            if (wind == null)
            {
                Log.W($"要隐藏的界面不存在:{UIName}");
                return;
            }

            if (!wind.IsShowing)
            {
                Log.W($"要隐藏的界面未打开{UIName}");
                return;
            }

            wind.Hide();
        }

        public void Del()
        {
            State = UICommandState.Done;
            _uiKit.RemoveUI(UIName);
        }
    }
}