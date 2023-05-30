using FairyGUI;
using UnityEngine;

namespace NBC
{
    /// <summary>
    /// ui界面默认动画
    /// </summary>
    public class PanelAnimationDef : NTask
    {
        public enum AnimType
        {
            CenterScaleBig = 0,

            /// <summary>
            /// 上往中滑动--
            /// </summary>
            UpToSlide = 1,

            /// <summary>
            /// //下往中滑动
            /// </summary>
            DownToSlide = 2,

            /// <summary>
            /// 左往中--
            /// </summary>
            LeftToSlide = 3,

            /// <summary>
            /// 右往中--
            /// </summary>
            RightToSlide = 4,

            /// <summary>
            /// 透明度
            /// </summary>
            Fade = 5
        }

        private bool _isClose;
        private GComponent _node;
        private AnimType _animType;


        public PanelAnimationDef(GComponent node, AnimType animType = AnimType.CenterScaleBig, bool close = false)
        {
            this._node = node;
            _isClose = close;
            _animType = animType;
        }

        protected override void OnStart()
        {
            if (_animType == AnimType.CenterScaleBig)
            {
                var strat = _isClose ? Vector3.one : Vector3.zero;
                var end = _isClose ? Vector3.zero : Vector3.one;
                var easeType = _isClose ? EaseType.BackIn : EaseType.BackOut;

                GTween.To(strat, end, 0.5f)
                    .SetEase(easeType)
                    .SetTarget(this._node, TweenPropType.Scale)
                    .OnComplete(Finish);
            }
            else if (_animType == AnimType.UpToSlide || _animType == AnimType.DownToSlide)
            {
                var hight = GRoot.inst.viewHeight;
                var y = _animType == AnimType.UpToSlide ? -hight : hight;
                var strat = _isClose ? 0 : y;
                var end = _isClose ? y : 0;

                GTween.To(strat, end, 0.5f)
                    .SetEase(EaseType.CubicOut)
                    .SetTarget(this._node, TweenPropType.Y)
                    .OnComplete(Finish);
            }
            else if (_animType == AnimType.LeftToSlide || _animType == AnimType.RightToSlide)
            {
                var width = GRoot.inst.viewWidth;

                var x = _animType == AnimType.LeftToSlide ? -width : width;

                var strat = _isClose ? 0 : x;
                var end = _isClose ? x : 0;

                GTween.To(strat, end, 0.5f)
                    .SetEase(EaseType.CubicOut)
                    .SetTarget(this._node, TweenPropType.X)
                    .OnComplete(Finish);
            }
            else if (_animType == AnimType.Fade)
            {
                var s = this._isClose ? 1 : 0;
                var end = this._isClose ? 0 : 1;
                this._node.alpha = s;
                GTween.To(s, end, 0.5f)
                    .SetEase(EaseType.Linear)
                    .SetTarget(this._node, TweenPropType.Alpha)
                    .OnStart(() => { })
                    .OnComplete(Finish);
            }
        }
    }


    public static class UIPanelAnimation
    {
        public static NTask GetCenterScaleBig(IUIPanel panel, bool close = false)
        {
            return new PanelAnimationDef(panel.ContentPane, PanelAnimationDef.AnimType.CenterScaleBig, close);
        }

        public static NTask GetUpToSlide(IUIPanel panel, bool close = false)
        {
            return new PanelAnimationDef(panel.ContentPane, PanelAnimationDef.AnimType.UpToSlide, close);
        }

        public static NTask GetDownToSlide(IUIPanel panel, bool close = false)
        {
            return new PanelAnimationDef(panel.ContentPane, PanelAnimationDef.AnimType.DownToSlide, close);
        }

        public static NTask GetLeftToSlide(IUIPanel panel, bool close = false)
        {
            return new PanelAnimationDef(panel.ContentPane, PanelAnimationDef.AnimType.LeftToSlide, close);
        }

        public static NTask GetRightToSlide(IUIPanel panel, bool close = false)
        {
            return new PanelAnimationDef(panel.ContentPane, PanelAnimationDef.AnimType.RightToSlide, close);
        }

        public static NTask GetFade(IUIPanel panel, bool close = false)
        {
            return new PanelAnimationDef(panel.ContentPane, PanelAnimationDef.AnimType.Fade, close);
        }
    }
}