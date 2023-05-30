using UnityEngine;

namespace NBC
{
    public static class UIConst
    {
        public const string ServiceName = "NBC.UIKit";
        
        /// <summary>
        /// UI资源前缀
        /// </summary>
        public static string UIPackRootUrl = "";
        
        /// <summary>
        /// UI安全区域
        /// </summary>
        public static Rect SafeArea = Rect.zero;
        
        
        /// <summary>
        /// 开启UI安全区域
        /// </summary>
        internal static bool OpenSafeArea = false;
    }
}