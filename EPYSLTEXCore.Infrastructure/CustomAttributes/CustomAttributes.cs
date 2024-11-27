using System;

namespace EPYSLEMSCore.Infrastructure.CustomeAttribute
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ChildEntityAttribute : Attribute
    {
    }
}
