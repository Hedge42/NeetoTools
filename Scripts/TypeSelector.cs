using UnityEngine;
using System;

#if UNITY_EDITOR
#endif

namespace Matchwork
{
    /// <summary>
    /// Display a list of types implementing T
    /// <br/>Add [Parameterless] or [ExcludeTypes] to filter further
    /// </summary>
    [Serializable]
    public class TypeSelector<T>
    {
        [SerializeField] private string typeName;
        public string TypeName => typeName;

        public static implicit operator Type(TypeSelector<T> obj) => obj.type;
        public Type baseType => typeof(T);
        public Type type => _type ??= Type.GetType(typeName);
        private Type _type;
    }

    /// <summary> Select only types with parameterless constructors </summary>
    public class ParameterlessAttribute : Attribute { }

    /// <summary> Exclude these baseTypes 
    /// 
    /// <see cref="DynEditor"/>.
    /// </summary>
    public class ExcludeTypesAttribute : Attribute
    {
        public Type[] exclude { get; private set; }
        public ExcludeTypesAttribute(params Type[] exclude) => this.exclude = exclude;
    }
}