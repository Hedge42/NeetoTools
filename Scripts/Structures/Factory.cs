using System;
using System.Collections.Generic;

namespace Neeto
{
    public static class Factory
    {
        static Dictionary<object, object> _dic;
        static Dictionary<object, object> dic => _dic ??= new();

        public static T Cache<T>(object key, Func<T> factory)
        {
            if (dic.TryGetValue(key, out var value) && value is T result)
            {
                return result;
            }

            // else create object and cache
            dic.SetValue(key, result = factory());
            return result;
        }
    }
}