using System;

namespace StaticMemberEnum
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class StaticMemberEnumAttribute : Attribute
    {
    }
}
