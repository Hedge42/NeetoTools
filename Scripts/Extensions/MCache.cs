using UnityEngine;
using System;

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Matchwork
{
    public static class MCache
    {
        static HashSet<object> _all;
        public static HashSet<object> all => _all ??= new();

        static Dictionary<object, object> _dic;
        public static Dictionary<object, object> dic => _dic ??= new();

        private static readonly ConcurrentDictionary<object, object> cache = new ConcurrentDictionary<object, object>();


        public static T Cache<T>(this Func<T> factory)
        {
            if (!cache.TryGetValue(factory, out var value))
            {
                value = factory();
                cache.AddOrUpdate(factory, value, (_, _) => factory);
            }

            if (value is T Value)
                return Value;

            return default(T);
        }
        public static T AsCache<T>(this object obj, Func<T> factory)
        {
            if (cache.TryGetValue(obj, out var cachedValue))
            {
                return (T)cachedValue;
            }

            var result = factory();
            cache[factory] = result;
            return result;
        }

        public static T Get<T>(object key, Func<T> factory = null)
        {
            var result = dic.TryGetValue(key, out var value);

            if (result && value is T)
            {
                return (T)value;
            }

            if (!result || value == null)
            {
                value = factory();
                dic.SetValue(key, value);
                return (T)value;
            }

            return default(T);
        }
        public static T Get<T>(Func<T> factory) => FunctionCache<T>.Get(factory);
        static class FunctionCache<T>
        {
            static Dictionary<Func<T>, T> _cache;
            public static Dictionary<Func<T>, T> cache => _cache ??= new Dictionary<Func<T>, T>();
            public static T Get(Func<T> factory)
            {
                //Debug.Log($"target? {factory.Target}");
                if (cache.TryGetValue(factory, out var value))
                    return value;

                value = factory();
                cache.Add(factory, value);
                return value;
            }
        }
    }
}