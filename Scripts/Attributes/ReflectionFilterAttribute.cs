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
    public class ReflectionFilterAttribute : Attribute
    {
        public readonly string source;
        public ReflectionFilterAttribute(string _) => source = _;
    }
}