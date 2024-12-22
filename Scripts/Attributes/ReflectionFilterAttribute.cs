using Mono.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Neeto
{

    public class ReflectionFilterAttribute : System.Attribute
    {
        public readonly string memberFactory;
        public readonly string memberFilter;

        public ReflectionFilterAttribute(string filterMethod = null, string factoryMethod = null)
        {
            this.memberFactory = factoryMethod;
            this.memberFilter = filterMethod;
        }
    }

}