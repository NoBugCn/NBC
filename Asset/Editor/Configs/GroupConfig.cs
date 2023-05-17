using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NBC.Asset.Editor
{
    [Serializable]
    public class GroupConfig : ISelectTag
    {
        public string Name;
        [Header("是否启用")] public bool Enable = true;
        [Header("打包模式")] public BundleMode BundleMode = BundleMode.Single;
        [Header("寻址模式")] public AddressMode AddressMode = AddressMode.None;
        [Header("标签(,分隔)")] public string Tags;
        [Header("收集源")] public List<Object> Collectors = new List<Object>();
        [Header("过滤器规则")] public FilterEnum FilterEnum = FilterEnum.All;
        public string Filter = "*";


        public string ShowTags
        {
            get => Tags;
            set => Tags = value;
        }
    }
}