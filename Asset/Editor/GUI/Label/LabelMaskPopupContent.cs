using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace NBC.Asset.Editor
{
    public class LabelMaskPopupContent : PopupWindowContent
    {
        private GUIStyle _toggleMixed;

        // private readonly GUIContent _manageLabelsButtonContent =
        //     EditorGUIUtility.TrIconContent("_Popup@2x", "Manage Labels");

        private readonly GUIStyle _toolbarButtonStyle = "RL FooterButton";

        private readonly GUIStyle _hintLabelStyle;

        private readonly SearchField _searchField;
        private string _searchValue;
        private readonly List<GUIStyle> _searchStyles;
        private Rect _activatorRect;
        private float _labelToggleControlRectHeight;
        private float _labelToggleControlRectWidth;
        private string _controlToFocus;

        private int _lastItemCount = -1;
        private Vector2 _rect;

        private List<string> _labelsAll;
        private readonly List<string> _labels;

        private ISelectTag _selectTag;

        public LabelMaskPopupContent(Rect activatorRect, ISelectTag selectTag)
        {
            _selectTag = selectTag;
            _labels = new List<string>();
            _labels.AddRange(selectTag.ShowTags.Split(","));
            _labels.Remove(string.Empty);
            _searchField = new SearchField();
            _activatorRect = activatorRect;
            _searchStyles = new List<GUIStyle>
            {
                GUITools.GetStyle("ToolbarSeachTextField"),
                GUITools.GetStyle("ToolbarSeachCancelButton"),
                GUITools.GetStyle("ToolbarSeachCancelButtonEmpty")
            };

            _hintLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Italic,
                richText = true
            };
        }
        
        public override Vector2 GetWindowSize()
        {
            if (_labelsAll == null)
                _labelsAll = GetLabelNamesOrderedSelectedFirst();

            if (_lastItemCount != _labelsAll.Count)
            {
                int maxLen = 0;
                string maxStr = "";
                for (int i = 0; i < _labelsAll.Count; i++)
                {
                    var len = _labelsAll[i].Length;
                    if (len > maxLen)
                    {
                        maxLen = len;
                        maxStr = _labelsAll[i];
                    }
                }

                var content = new GUIContent(maxStr);
                var size = GUI.skin.toggle.CalcSize(content);
                _labelToggleControlRectHeight = Mathf.Ceil(size.y + GUI.skin.toggle.margin.top);
                _labelToggleControlRectWidth = size.x + 16;

                float width = Mathf.Clamp(Mathf.Max(size.x, _activatorRect.width), 170, 500);
                float labelAreaHeight = _labelsAll.Count *
                                        (_labelToggleControlRectHeight + GUI.skin.toggle.margin.bottom);
                float toolbarAreaHeight = 30 + _hintLabelStyle.CalcHeight(new GUIContent(maxStr), width);
                float paddingHeight = 6;
                float height = labelAreaHeight + toolbarAreaHeight + paddingHeight;
                height = Mathf.Clamp(height, 50, 300);
                _rect = new Vector2(width, height);
                _lastItemCount = _labelsAll.Count;
            }

            return _rect;
        }

        void SetLabelForEntries(string label, bool value)
        {
            if (string.IsNullOrEmpty(label)) return;
            if (value)
            {
                if (!_labels.Contains(label))
                {
                    _labels.Add(label);
                }
            }
            else
            {
                _labels.Remove(label);
            }

            _selectTag.ShowTags = string.Join(",", _labels);
        }

        List<string> GetLabelNamesOrderedSelectedFirst()
        {
            var labels = new List<string>(Defs.UserTags.Count);
            labels.AddRange(Defs.UserTags);
            return labels;
        }

        Vector2 m_ScrollPosition;

        public override void OnGUI(Rect fullRect)
        {
            int count = -1;
            if (_labelsAll == null)
                _labelsAll = GetLabelNamesOrderedSelectedFirst();

            var areaRect = new Rect(fullRect.xMin + 3, fullRect.yMin + 3, fullRect.width - 6, fullRect.height - 6);
            GUILayout.BeginArea(areaRect);

            GUILayoutUtility.GetRect(areaRect.width, 1);
  
            Rect searchRect = EditorGUILayout.GetControlRect();
            _searchValue = _searchField.OnGUI(searchRect, _searchValue, _searchStyles[0], _searchStyles[1],
                _searchStyles[2]);

            EditorGUI.BeginDisabledGroup(true);
            string labelText;
            int searchLabelIndex = _labelsAll.IndexOf(_searchValue);
            if (searchLabelIndex >= 0)
            {
                count = _labels.Contains(_searchValue) ? 1 : 0;

                labelText = string.Format(
                    count > 0 ? Language.LabelHintSearchFoundIsDisabled : Language.LabelHintSearchFoundIsEnabled,
                    _searchValue);
            }
            else
            {
                labelText = !string.IsNullOrEmpty(_searchValue)
                    ? string.Format(Language.LabelHintSearchFoundIsEnabled, _searchValue)
                    : Language.LabelHintIdle;
            }

            Rect hintRect = EditorGUILayout.GetControlRect(true,
                _hintLabelStyle.CalcHeight(new GUIContent(labelText), fullRect.width), _hintLabelStyle);
            hintRect.x -= 3;
            hintRect.width += 6;
            hintRect.y -= 3;
            EditorGUI.LabelField(hintRect, new GUIContent(labelText), _hintLabelStyle);
            EditorGUI.EndDisabledGroup();

            if (Event.current.isKey &&
                (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter) &&
                _searchField.HasFocus())
            {
                if (!string.IsNullOrEmpty(_searchValue))
                {
                    if (searchLabelIndex >= 0)
                    {
                        SetLabelForEntries(_searchValue, count <= 0);
                    }
                    else
                    {
                        Defs.AddTag(_searchValue);
                        SetLabelForEntries(_searchValue, true);
                        _labelsAll.Insert(0, _searchValue);
                    }

                    _controlToFocus = _searchValue;
                    GUI.ScrollTo(new Rect(0, searchLabelIndex * 19, 0, 0));
                    _searchValue = "";
                    _lastItemCount = -1;

                    Event.current.Use();
                    GUIUtility.ExitGUI();
                    editorWindow.Repaint();
                }
            }

            var scrollViewHeight = areaRect.height - (hintRect.y + hintRect.height + 2);
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, false, false);
            Vector2 yPositionDrawRange = new Vector2(m_ScrollPosition.y - 19, m_ScrollPosition.y + scrollViewHeight);

            for (int i = 0; i < _labelsAll.Count; i++)
            {
                var labelName = _labelsAll[i];
                if (!string.IsNullOrEmpty(_searchValue))
                {
                    if (labelName.IndexOf(_searchValue, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                }

                var toggleRect = EditorGUILayout.GetControlRect(GUILayout.Width(_labelToggleControlRectWidth),
                    GUILayout.Height(_labelToggleControlRectHeight));
                if (toggleRect.height > 1)
                {
                    // only draw toggles if they are in view
                    if (toggleRect.y < yPositionDrawRange.x || toggleRect.y > yPositionDrawRange.y)
                        continue;
                }
                else
                {
                    continue;
                }

                var oldState = _labels.Contains(labelName);

                GUI.SetNextControlName(labelName);
                var newState = EditorGUI.ToggleLeft(toggleRect, new GUIContent(labelName), oldState);
                EditorGUI.showMixedValue = false;
                if (oldState != newState)
                {
                    SetLabelForEntries(labelName, newState);
                }
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();

            if (Event.current.type == EventType.Repaint &&
                _labelsAll != null && _controlToFocus != null)
            {
                GUI.FocusControl(_controlToFocus);
                _controlToFocus = null;
            }
        }
    }
}