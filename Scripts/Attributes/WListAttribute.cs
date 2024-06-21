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


namespace Matchwork
{

    /* WhiteList for reflection scripts
     * ONLY use methods contained in these types
     */
    public class WListAttribute : Attribute, IMethodFilter
    {
        public Type[] types { get; private set; }

        /// <summary> Describe what this does </summary>
		public WListAttribute(params Type[] types)
        {
            this.types = types;
        }

        public bool Filter(MethodInfo info)
        {
            foreach (var t in types)
            {
                if (types.Contains(t))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Meant to be used with GameAction to restrict classes to look in
    /// </summary>
    public class TypeFilterAttribute : Attribute
    {
        public Type[] types;

        public TypeFilterAttribute(params Type[] types)
        {
            this.types = types;
        }

        public bool Evaluate(Type type)
        {
            return types.Contains(type);
        }

        public static bool Evaluate(MemberInfo info, Type type)
        {
            var atr = info.GetCustomAttribute<TypeFilterAttribute>();
            return atr == null || atr.Evaluate(type);
        }
        public static IEnumerable<Type> Evaluate(MemberInfo info, IEnumerable<Type> types)
        {
            var atr = info.GetCustomAttribute<TypeFilterAttribute>();
            if (atr == null)
                return types;
            else
                return types.Where(t => atr.Evaluate(t));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /* BlackList for reflection scripts
     * do NOT use methods contained in these types
     */
    public class BListAttribute : Attribute, IMethodFilter
    {
        public Type[] types { get; private set; }

        /// <summary> Describe what this does </summary>
		public BListAttribute(params Type[] types)
        {
            this.types = types;
        }

        public bool Filter(MethodInfo info)
        {
            foreach (var t in types)
            {
                if (types.Contains(t))
                {
                    return true;
                }
            }

            return false;
        }
    }
}