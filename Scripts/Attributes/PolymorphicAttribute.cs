using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Matchwork
{
    public class PolymorphicAttribute : PropertyAttribute
    {
        public Type[] refTypes { get; }
        public Module? module { get; }

        public PolymorphicAttribute(Type[] refTypes = null, string load = null)
        {
            this.refTypes = refTypes;
            this.module = load;
        }
    }
    public class SearchAttribute : PropertyAttribute
    {
        public string module;
        public Type[] inheriting;
        public SearchAttribute(Type[] inheriting)
        {
            this.inheriting = inheriting;
        }
        public SearchAttribute(Module module)
        {
            this.module = module;
        }
    }
    public class BaseTypesAttribute : Attribute
    {
        public Type[] baseTypes;
        public BaseTypesAttribute(Type[] searchTypes)
        {
            this.baseTypes = searchTypes;
        }
        public IEnumerable<Type> Filter(IEnumerable<Type> types)
        {
            return types.Where(baseTypes.Contains);
        }
    }
    public class ModuleAttribute : Attribute 
    {
        public Module? module;
        public ModuleAttribute(Module module)
        {
            this.module = module;
        }
    }
}