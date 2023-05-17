using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace NBC.Asset.Editor
{
    [Serializable]
    public class VerticalSplitter
    {
        [NonSerialized] Rect m_Rect = new Rect(0, 0, 0, 3);
        public Rect SplitterRect => m_Rect;

        [FormerlySerializedAs("m_currentPercent")] [SerializeField]
        float m_CurrentPercent;

        bool m_Resizing;
        float m_MinPercent;
        float m_MaxPercent;

        public VerticalSplitter(float percent = .6f, float minPer = .4f, float maxPer = .9f)
        {
            m_CurrentPercent = percent;
            m_MinPercent = minPer;
            m_MaxPercent = maxPer;
        }

        public bool OnGUI(Rect content, out Rect top, out Rect bot)
        {
            m_Rect.x = content.x;
            m_Rect.y = (int)(content.y + content.height * m_CurrentPercent);
            m_Rect.width = content.width;

            EditorGUIUtility.AddCursorRect(m_Rect, MouseCursor.ResizeVertical);
            if (Event.current.type == EventType.MouseDown && m_Rect.Contains(Event.current.mousePosition))
                m_Resizing = true;

            if (m_Resizing)
            {
                EditorGUIUtility.AddCursorRect(content, MouseCursor.ResizeVertical);

                var mousePosInRect = Event.current.mousePosition.y - content.y;
                m_CurrentPercent = Mathf.Clamp(mousePosInRect / content.height, m_MinPercent, m_MaxPercent);
                m_Rect.y = Mathf.Min((int)(content.y + content.height * m_CurrentPercent),
                    content.yMax - m_Rect.height);
                if (Event.current.type == EventType.MouseUp)
                    m_Resizing = false;
            }

            top = new Rect(content.x, content.y, content.width, m_Rect.yMin - content.yMin);
            bot = new Rect(content.x, m_Rect.yMax, content.width, content.yMax - m_Rect.yMax);
            return m_Resizing;
        }
    }


    [Serializable]
    public class HorizontalSplitter
    {
        [NonSerialized] Rect m_Rect = new Rect(0, 0, 3, 0);

        public Rect SplitterRect => m_Rect;

        [FormerlySerializedAs("m_currentPercent")] [SerializeField]
        float m_CurrentPercent;

        bool m_Resizing;
        float m_MinPercent;
        float m_MaxPercent;

        public HorizontalSplitter(float percent = .6f, float minPer = .4f, float maxPer = .9f)
        {
            m_CurrentPercent = percent;
            m_MinPercent = minPer;
            m_MaxPercent = maxPer;
        }

        public bool OnGUI(Rect content, out Rect left, out Rect right)
        {
            m_Rect.y = content.y;
            m_Rect.x = (int)(content.x + content.width * m_CurrentPercent);
            m_Rect.height = content.height;

            EditorGUIUtility.AddCursorRect(m_Rect, MouseCursor.ResizeHorizontal);
            if (Event.current.type == EventType.MouseDown && m_Rect.Contains(Event.current.mousePosition))
                m_Resizing = true;

            if (m_Resizing)
            {
                EditorGUIUtility.AddCursorRect(content, MouseCursor.ResizeHorizontal);

                var mousePosInRect = Event.current.mousePosition.x - content.x;
                m_CurrentPercent = Mathf.Clamp(mousePosInRect / content.width, m_MinPercent, m_MaxPercent);
                m_Rect.x = Mathf.Min((int)(content.x + content.width * m_CurrentPercent), content.xMax - m_Rect.width);
                if (Event.current.type == EventType.MouseUp)
                    m_Resizing = false;
            }
            left = new Rect(content.x, content.y, content.width * m_CurrentPercent, content.height);
            right = new Rect(m_Rect.xMax, content.y, content.width - content.width * m_CurrentPercent, content.height);
            return m_Resizing;
        }
    }
}