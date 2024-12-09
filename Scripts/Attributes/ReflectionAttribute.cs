using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Neeto
{

    /* WhiteList for reflection scripts
     * ONLY use methods contained in these types
     */
    public class ReflectionAttribute : Attribute
    {
        public Type scope { get; }
        public Type returnType { get; }

        /// <summary> Describe what this does </summary>
		public ReflectionAttribute(Type scope = null, Type returnType = null)
        {
            this.scope = scope;
            this.returnType = returnType;
        }
    }
}