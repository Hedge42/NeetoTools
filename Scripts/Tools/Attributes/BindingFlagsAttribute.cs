//using Cysharp.Threading.Tasks;
using System;
using System.Reflection;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
public class BindingFlagsAttribute : Attribute
{
    public BindingFlags flags { get; set; }
    public BindingFlagsAttribute(BindingFlags flags) => this.flags = flags;
}
