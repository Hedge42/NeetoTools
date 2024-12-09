using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Neeto
{
    public static class Factory
    {
        static readonly Dictionary<object, object> dic = new();

        public static T Cache<T>(object key, Func<T> factory)
        {
            if (dic.TryGetValue(key, out var value) && value is T result)
                return result;

            // else create object and cache
            dic.SetValue(key, result = factory());
            return result;
        }
        public static T Cache<T>(T existing, Func<T> func) where T : class
            => existing == null ? func() : existing;

        public static T CacheComponent<T>(this GameObject gameObject) where T : Component
            => Factory<T>.CacheComponent(gameObject);
        public static void AddToCache<T>(this T component, bool removeOnDestroy = false) where T : Component
            => Factory<T>.AddToCache(component, removeOnDestroy);
        public static void RemoveFromCache<T>(this T component) where T : Component
            => Factory<T>.RemoveFromCache(component);
    }
    public static class Factory<T> where T : Component
    {
        static Dictionary<GameObject, T> _dic;
        static Dictionary<GameObject, T> dic
        {
            get
            {
                if (_dic == null)
                {
                    _dic = new ();
                    SceneManager.activeSceneChanged += (_, _) => dic.Clear();
                }
                return _dic;
            }
        }

        public static T CacheComponent(GameObject gameObject, bool removeOnDestroy = false)
        {
            T component;
            if (dic.TryGetValue(gameObject, out component))
                return component;

            dic[gameObject] = component = gameObject.GetComponent<T>();
            if (removeOnDestroy)
                gameObject.OnDestroy(() => RemoveFromCache(component));
            return component;
        }
        public static void AddToCache(T component, bool removeOnDestroy = false)
        {
            var gameObject = component.gameObject;
            if (dic.ContainsKey(gameObject))
                return;

            dic.Add(gameObject, component);
            if (removeOnDestroy)
                gameObject.OnDestroy(() => RemoveFromCache(component));
        }
        public static void RemoveFromCache(T component)
            => dic.Remove(component.gameObject);
    }
}