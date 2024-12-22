using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Neeto
{
    [DisallowMultipleComponent]
    public abstract class Prefab : MonoBehaviour
    {
        static readonly Dictionary<GameObject, List<GameObject>> registry = new();

        protected virtual void OnDestroy()
        {
            registry[gameObject].Remove(gameObject);
            registry.Remove(gameObject);
        }
        public static void Return(GameObject instance)
        {
            if (registry.TryGetValue(instance, out var list))
                list.Add(instance);
        }
        public static GameObject Instantiate(GameObject prefab)
        {
            if (!registry.TryGetValue(prefab, out var pool))
                pool = registry[prefab] = new();

            var instance = pool.Count > 0
                ? registry[prefab].Pop()
                : GameObject.Instantiate(prefab);

            registry[instance] = pool;
            return instance;
        }
        public static new T Instantiate<T>(T prefab) where T : Component
        {
            return Instantiate(prefab.gameObject)?.GetComponent<T>();
        }

    }
}