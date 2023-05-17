using System;
using UnityEngine;

namespace NBC.Asset
{
    public class Mono : MonoBehaviour
    {
        public static void AddUpdate(Action action)
        {
            Inst.OnUpdate += action;
        }

        public static void RemoveUpdate(Action action)
        {
            Inst.OnUpdate -= action;
        }

        private event Action OnUpdate;

        private static bool IsQuiting { get; set; }

        private static Mono _inst;

        private static Mono Inst => _inst;

        protected void OnApplicationQuit()
        {
            IsQuiting = true;
        }

        protected void Awake()
        {
            if (_inst != null)
            {
                DestroyImmediate(this.gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            _inst = this;
        }

        protected void Update()
        {
            OnUpdate?.Invoke();
        }
    }
}