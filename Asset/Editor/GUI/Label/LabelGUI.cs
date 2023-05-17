using System;
using UnityEditor;
using UnityEngine;

namespace NBC.Asset.Editor
{
    public class LabelGUI
    {
        Rect buttonRect;

        public void OnGUI(ISelectTag selectTag)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(Language.Tags, GUILayout.Width(148));
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(string.Empty, selectTag.ShowTags);
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                PopupWindow.Show(buttonRect, new LabelMaskPopupContent(buttonRect, selectTag));
            }

            if (Event.current.type == EventType.Repaint)
            {
                buttonRect = GUILayoutUtility.GetLastRect();
            }

            GUILayout.EndHorizontal();
        }
    }
}