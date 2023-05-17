using System;

namespace NBC.Asset.Editor
{
    /// <summary>
    /// 自定义名称
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public sealed class DisplayNameAttribute : Attribute
    {
        public DisplayNameAttribute(string name)
        {
            showName = name;
        }

        public string showName;
    }
}