using UnityEngine;

namespace NBC
{
    public interface IUIPanel
    {
        FairyGUI.GComponent ContentPane { get; }

        // string UILayer { get; }
        /// <summary>
        /// 模块id
        /// </summary>
        int Id { get; }

        bool IsTop { get; }
        bool IsShowing { get; }
        bool IsCanVisible { get; }
        bool IsDotDel { get; }

        bool IsModal { get; }

        bool IsFrozen { get; set; }

        void SetUIManager(UIManager kit);
        void SetData(object args);
        object GetData();
        string[] GetDependPackages();
        System.Collections.Generic.Dictionary<string, string> GetLanguageConfig();
        void Init();
        void Show();
        void Hide();
        void HideImmediately();
        void Refresh();
        void Dispose();
    }
}