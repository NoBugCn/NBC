using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NBC.Asset.Editor
{
    public class GroupInfoGUI
    {
        private bool _groupBaseInfoFold = true;

        private bool _groupCollectorsInfoFold = true;
        Vector2 _collectorsScrollPos;
        private CollectorWindow _window;
        private LabelGUI _labelGUI;

        public GroupInfoGUI(CollectorWindow window)
        {
            _window = window;
        }

        public void OnGUI(Rect rect)
        {
            if (_window == null) return;
            GUILayout.BeginArea(rect);
            if (_window.SelectGroupConfig == null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(Language.NoSelectGroup);
                GUILayout.EndArea();
                return;
            }

            DrawInfo();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            DrawCollectors();
            GUILayout.EndArea();
        }

        void DrawInfo()
        {
            GUI.color = new Color(0, 0, 0, 0.2f);
            GUILayout.BeginHorizontal(Styles.headerBoxStyle);
            GUI.color = Color.white;
            GUILayout.Label(
                $"<b><size=18>{(_groupBaseInfoFold ? "▼" : "▶")} 【{_window.SelectGroupConfig.Name}】 基础信息</size></b>");
            GUILayout.EndHorizontal();

            var lastRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.MouseDown && lastRect.Contains(Event.current.mousePosition))
            {
                _groupBaseInfoFold = !_groupBaseInfoFold;
                Event.current.Use();
            }

            if (!_groupBaseInfoFold) return;
            EditorGUILayout.Space();

            if (GUITools.EnumGUILayout(Language.GroupBundleMode, _window.SelectGroupConfig, "BundleMode"))
            {
                _window.UpdateAssets();
            }

            if (GUITools.EnumGUILayout(Language.GroupAddressMode, _window.SelectGroupConfig, "AddressMode"))
            {
                _window.UpdateAssets();
            }


            var newFilter =
                (FilterEnum)EditorGUILayout.EnumPopup(Language.Filter, _window.SelectGroupConfig.FilterEnum);

            if (newFilter != _window.SelectGroupConfig.FilterEnum)
            {
                _window.SelectGroupConfig.FilterEnum = newFilter;

                _window.UpdateAssets();
            }

            if (_window.SelectGroupConfig.FilterEnum == FilterEnum.Custom)
            {
                var newField =
                    EditorGUILayout.TextField(Language.FilterCustom, _window.SelectGroupConfig.Filter);
                if (newField != _window.SelectGroupConfig.Filter)
                {
                    _window.SelectGroupConfig.Filter = newField;
                    _window.UpdateAssets();
                }
            }


            DrawLabels();
        }

        void DrawLabels()
        {
            if (_labelGUI == null)
            {
                _labelGUI = new LabelGUI();
            }

            _labelGUI?.OnGUI(_window.SelectGroupConfig);
            // if (_labelGUI != null && _labelGUI.Lable != _window.SelectGroupConfig.Tags)
            // {
            //     _window.SelectGroupConfig.Tags = _labelGUI.Lable;
            // }
        }

        void DrawCollectors()
        {
            GUI.color = new Color(0, 0, 0, 0.2f);
            GUILayout.BeginHorizontal(Styles.headerBoxStyle);
            GUI.color = Color.white;
            GUILayout.Label(
                $"<b><size=18>{(_groupCollectorsInfoFold ? "▼" : "▶")} 【{_window.SelectGroupConfig.Name}】 收集源 ({_window.SelectGroupConfig.Collectors.Count})</size></b>");
            GUI.backgroundColor = default(Color);
            if (GUILayout.Button(Styles.Add, GUILayout.Width(40)))
            {
                _window.SelectGroupConfig.Collectors.Add(null);
            }

            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            var lastRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.MouseDown && lastRect.Contains(Event.current.mousePosition))
            {
                _groupCollectorsInfoFold = !_groupCollectorsInfoFold;
                Event.current.Use();
            }

            if (!_groupCollectorsInfoFold) return;
            EditorGUILayout.Space();

            _collectorsScrollPos = EditorGUILayout.BeginScrollView(_collectorsScrollPos);

            if (_window.SelectGroupConfig.Collectors == null)
            {
                _window.SelectGroupConfig.Collectors = new List<Object>();
            }

            for (int i = 0; i < _window.SelectGroupConfig.Collectors.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Format(Language.CollectorTitle, i), GUILayout.Width(60));
                var collector = _window.SelectGroupConfig.Collectors[i];
                var newObj = EditorGUILayout.ObjectField("", collector, typeof(Object), false);
                if (newObj != collector)
                {
                    _window.SelectGroupConfig.Collectors[i] = newObj;
                    _window.UpdateAssets();
                }

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    _window.SelectGroupConfig.Collectors.RemoveAt(i);
                    _window.UpdateAssets();
                }

                GUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }

            GUILayout.EndScrollView();
        }
    }
}