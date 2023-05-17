using System;
using System.Linq;
using System.Reflection;
using FairyGUI;
using UnityEngine;

namespace NBC
{
    public static class UIExtension
    {
        public static void AutoFindAllField<T>(this T self) where T : UIPanel
        {
            var fields = self.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var attrs = field.GetCustomAttributes<AutoFindAttribute>().ToArray();
                if (attrs.Length <= 0) continue;
                var findInfo = attrs[0];
                var name = string.IsNullOrEmpty(findInfo.Name) ? field.Name : findInfo.Name;
                object obj;
                var type = field.FieldType;
                if (field.FieldType == typeof(FairyGUI.Controller))
                {
                    obj = self.ContentPane.GetController(name);
                }
                else if (field.FieldType == typeof(FairyGUI.Transition))
                {
                    obj = self.ContentPane.GetTransition(name);
                }
                else
                {
                    obj = self.ContentPane.GetChild(name);
                }

                if (obj == null)
                {
                    throw new Exception("查找子物体失败" + "type=" + field.FieldType + "/name=" + findInfo.Name);
                }

                // Log.I(self.UIResName + "查找子物体" + "type=" + field.FieldType + "/name=" + findInfo.Name);
                field.SetValue(self, obj);
            }
        }

        /// <summary>
        /// 自动注册点击事件
        /// </summary>
        /// <param name="self"></param>
        /// <param name="btnStartName">FGUI中按钮以什么开头</param>
        /// <param name="onClick"></param>
        /// <typeparam name="T"></typeparam>
        public static void AutoAddClick<T>(this T self, Action<GComponent> onClick, string btnStartName = "Btn")
            where T : UIPanel
        {
            for (int i = 0; i < self.ContentPane.numChildren; i++)
            {
                FairyGUI.GObject gObject = self.ContentPane.GetChildAt(i);
                if (gObject.name.StartsWith(btnStartName))
                {
                    gObject.onClick.Add(a => { onClick?.Invoke(a.sender as GComponent); });
                }
            }
        }

        public static void AutoClearClick<T>(this T self, string btnStartName = "Btn")
            where T : UIPanel
        {
            for (int i = 0; i < self.ContentPane.numChildren; i++)
            {
                FairyGUI.GObject gObject = self.ContentPane.GetChildAt(i);
                if (gObject.name.StartsWith(btnStartName))
                {
                    gObject.onClick.Clear();
                }
            }
        }


        /// <summary>
        /// 自动注册点击事件
        /// </summary>
        /// <param name="self"></param>
        /// <param name="btnStartName"></param>
        /// <param name="onClick"></param>
        /// <typeparam name="T"></typeparam>
        public static void AutoAddClick(this GComponent self, Action<GComponent> onClick, string btnStartName = "Btn")
        {
            for (int i = 0; i < self.numChildren; i++)
            {
                FairyGUI.GObject gObject = self.GetChildAt(i);
                if (gObject.name.StartsWith(btnStartName))
                {
                    gObject.onClick.Add(a => { onClick?.Invoke(a.sender as GComponent); });
                }
            }
        }

        public static void AutoSetClick(this GComponent self, Action<GComponent> onClick, string btnStartName = "Btn")
        {
            for (int i = 0; i < self.numChildren; i++)
            {
                FairyGUI.GObject gObject = self.GetChildAt(i);
                if (gObject.name.StartsWith(btnStartName))
                {
                    gObject.onClick.Set(a => { onClick?.Invoke(a.sender as GComponent); });
                }
            }
        }

        public static void AutoClearClick(this GComponent self, string btnStartName = "Btn")
        {
            for (int i = 0; i < self.numChildren; i++)
            {
                FairyGUI.GObject gObject = self.GetChildAt(i);
                if (gObject.name.StartsWith(btnStartName))
                {
                    gObject.onClick.Clear();
                }
            }
        }

        #region 安全距离

        private static bool _isInitArea = false;

        private static void InitArea()
        {
            var safeArea = Screen.safeArea;
            Rect norSafeArea = Rect.zero;
            var width = GRoot.inst.width;
            norSafeArea.width = width;
            norSafeArea.height = safeArea.height / safeArea.width * width;

            var y = Screen.height - Screen.safeArea.yMax;
            var x = Screen.width - Screen.safeArea.xMax;

            norSafeArea.x = (x / Screen.width) * GRoot.inst.width;
            norSafeArea.y = (y / Screen.height) * GRoot.inst.height;
            if (norSafeArea.y < 1)
            {
                norSafeArea.y = GRoot.inst.size.y - norSafeArea.height;
            }

            UIConst.SafeArea = norSafeArea;
            _isInitArea = true;
        }

        /// <summary>
        /// 设置对象安全距离
        /// </summary>
        /// <param name="self"></param>
        public static void ToSafeArea(this GObject self)
        {
            if (!UIConst.OpenSafeArea) return;
            if (!_isInitArea) InitArea();
            var safeArea = UIConst.SafeArea;
            if (Math.Abs(self.size.x - safeArea.width) < 1)
            {
                self.SetSize(safeArea.width, safeArea.height);

                self.position = new Vector3(0, safeArea.y);
            }
        }

        public static Vector2 ToSafeArea(this Vector2 vector2)
        {
            if (!UIConst.OpenSafeArea) return vector2;
            if (!_isInitArea) InitArea();
            return vector2;
        }

        public static Vector3 ToSafeArea(this Vector3 vector3)
        {
            if (!UIConst.OpenSafeArea) return vector3;
            if (!_isInitArea) InitArea();
            return vector3;
        }

        #endregion
    }
}