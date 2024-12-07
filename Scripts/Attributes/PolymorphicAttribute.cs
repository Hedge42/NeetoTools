using Cysharp.Threading.Tasks;
using Neeto;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
#endif

public class PolymorphicAttribute : PropertyAttribute
{
    public Type[] include { get; }
    public Type[] exclude { get; }
    public Type Default { get; }
    public BindingFlags flags { get; }

    const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    public PolymorphicAttribute(Type Default = null, Type[] include = null, Type[] exclude = null, BindingFlags flags = FLAGS)
    {
        this.exclude = exclude;
        this.include = include;
        this.flags = flags;
        this.Default = Default;
    }
}

[InitializeOnLoad]
public class ScriptAttribute : Attribute
{
    public Type type { get; }

    /// <param name="_type">The first type in the script which has an associated MonoScript</param>
    public ScriptAttribute(Type _type) => type = _type;

#if UNITY_EDITOR
    static ScriptAttribute()
    {
        instances = new();
        var types = TypeCache.GetTypesWithAttribute<ScriptAttribute>();
        var scripts = NGUI.LoadAssets<MonoScript>();
        foreach (var type in types)
        {
            var attribute = type.GetCustomAttribute<ScriptAttribute>();
            foreach (var script in scripts)
            {
                if (script.GetClass() == attribute.type)
                {
                    instances.Add(type, script);
                }
            }
        }
    }
    static Dictionary<Type, MonoScript> instances { get; }

    public static bool TryFindScript(object obj, out MonoScript script)
    {
        script = null;
        return obj != null && instances.TryGetValue(obj.GetType(), out script);
    }
#endif
}