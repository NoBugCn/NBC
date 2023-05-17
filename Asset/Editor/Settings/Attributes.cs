using System;

namespace NBC.Asset.Editor
{
    /// <summary>
    /// 类型绑定
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class BindAttribute : Attribute
    {
        public object BindObject;

        public BindAttribute(object obj)
        {
            this.BindObject = obj;
        }
    }

    /// <summary>
    /// 类标记Id
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class IdAttribute : Attribute
    {
        public int Id;

        public IdAttribute(int id)
        {
            Id = id;
        }
    }
}