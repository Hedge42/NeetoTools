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
    public class ReturnTypeAttribute : Attribute, IMethodFilter, ITypeFilter
    {
        public Type[] types { get; private set; }

        /// <summary> Describe what this does </summary>
		public ReturnTypeAttribute(params Type[] types)
        {
            this.types = types;
        }

        public bool Filter(MethodInfo info)
        {
            foreach (var t in types)
            {
                if (t.IsAssignableFrom(info.ReturnType))
                {
                    return true;
                }
            }

            return false;
        }

        public bool Filter(Type type)
        {
            if (types.Length == 0) return true;

            foreach (var t in types)
            {
                if (t.IsAssignableFrom(type))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public interface ITypeFilter
    {
        public abstract bool Filter(Type type);
    }
    public static class FilterExtension
    {
        public static bool Filter(this IEnumerable<ITypeFilter> filters, Type type)
        {
            foreach (var f in filters)
            {
                if (!f.Filter(type))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool Filter(this IEnumerable<IMethodFilter> filters, MethodInfo info)
        {
            foreach (var f in filters)
            {
                if (!f.Filter(info))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public interface IMethodFilter
    {
        public bool Filter(MethodInfo info);

    }
}