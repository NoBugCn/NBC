using System;
using UnityEngine;

namespace NBC
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public class AutoFindAttribute : Attribute
    {
        public string Name;
    }
}