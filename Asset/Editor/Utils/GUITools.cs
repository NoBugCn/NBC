using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NBC.Asset.Editor
{
    public static class GUITools
    {
        public static GUIStyle DefLabelStyle = new GUIStyle(GUI.skin.label)
        {
            richText = true,
            alignment = TextAnchor.MiddleCenter
        };

        public static GUIStyle GetStyle(string styleName)
        {
            GUIStyle s = GUI.skin.FindStyle(styleName);
            if (s == null) s = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
            if (s == null)
            {
                Debug.LogError("Missing built-in gui style: " + styleName);
                s = new GUIStyle();
            }

            return s;
        }

        public static bool EnumGUILayout(string title, object target, string fieldName)
        {
            var t = target.GetType();
            var field = GetFieldInfo(t, fieldName);
            var displayNameInfo = GetDisplayNameInfo(field);
            var value = field.GetValue(target);
            var newValue = value;

            var names = displayNameInfo.Keys.ToList();
            var values = displayNameInfo.Values.ToList();
            var showName = value.ToString();

            var mask = values.FindIndex(n => n.Name == showName);
            if (mask < 0) mask = 0;
            var newMask = EditorGUILayout.Popup(title, mask, names.ToArray());
            if (newMask != mask)
            {
                var newInfo = values[newMask];
                if (newInfo != null)
                {
                    newValue = Enum.Parse(value.GetType(), newInfo.Name, true);
                }
            }

            if (value != newValue)
            {
                field.SetValue(target, newValue);
                return true;
            }

            return false;
        }

        private static Dictionary<Type, Dictionary<string, FieldInfo>> _fieldInfoDic =
            new Dictionary<Type, Dictionary<string, FieldInfo>>();

        private static FieldInfo GetFieldInfo(Type t, string fieldName)
        {
            //得到字段的值,只能得到public类型的字典的值
            FieldInfo[] fieldInfos = t.GetFields();
            FieldInfo showFieldInfo = null;
            foreach (var fieldInfo in fieldInfos)
            {
                if (fieldInfo.Name == fieldName)
                {
                    showFieldInfo = fieldInfo;
                    break;
                }
            }

            if (showFieldInfo != null)
            {
                if (!_fieldInfoDic.TryGetValue(t, out var dictionary))
                {
                    dictionary = new Dictionary<string, FieldInfo>();
                    _fieldInfoDic[t] = new Dictionary<string, FieldInfo>();
                }

                dictionary[fieldName] = showFieldInfo;
            }

            return showFieldInfo;
        }

        private static Dictionary<Type, Dictionary<string, FieldInfo>> _displayNameInfoDic =
            new Dictionary<Type, Dictionary<string, FieldInfo>>();

        private static Dictionary<string, FieldInfo> GetDisplayNameInfo(FieldInfo fieldInfo)
        {
            var fieldType = fieldInfo.FieldType;

            if (_displayNameInfoDic.TryGetValue(fieldType, out var dictionary))
            {
                return dictionary;
            }

            var fields = fieldType.GetFields();
            Dictionary<string, FieldInfo> showFieldInfos = new Dictionary<string, FieldInfo>();
            foreach (var f in fields)
            {
                if (f.FieldType != fieldType) continue;
                var menuNameAttr = f.GetCustomAttribute<DisplayNameAttribute>();
                var showName = menuNameAttr != null ? menuNameAttr.showName : f.Name;
                showFieldInfos[showName] = f;
            }

            _displayNameInfoDic[fieldType] = showFieldInfos;
            return showFieldInfos;
        }
    }
}