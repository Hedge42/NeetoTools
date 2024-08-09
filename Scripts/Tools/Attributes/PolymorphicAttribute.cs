using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PolymorphicAttribute : PropertyAttribute
{
    public Type[] include { get; }
    public Type[] exclude { get; }
    public BindingFlags flags { get; }

    const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    public PolymorphicAttribute(Type[] include = null, Type[] exclude = null, BindingFlags flags = FLAGS)
    {
        this.exclude = exclude;
        this.include = include;
        this.flags = flags;
    }
}
