using System;

namespace EPYSLTEXCore.Infrastructure.CustomeAttribute
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ChildEntityAttribute : Attribute
    {
    }
}
