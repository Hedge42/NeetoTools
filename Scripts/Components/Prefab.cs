using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Neeto
{
    public class Prefab : MonoBehaviour
    {
        static readonly Dictionary<GameObject, Queue<GameObject>> registry = new();

        public static bool HasInstances(GameObject prefab)
        {
            return registry.TryGetValue(prefab, out var pool) 
                && pool.Count > 0;
        }

        public static void Return(GameObject prefab, GameObject instance)
        {
            if (!prefab || !instance)
                return;

            var pool = registry.ContainsKey(prefab)
                ? registry[prefab]
                : new();

            pool.Enqueue(instance);
        }
        public static GameObject Spawn(GameObject prefab)
        {
            return HasInstances(prefab)
                ? registry[prefab].Dequeue()
                : GameObject.Instantiate(prefab);
        }

        public static T Spawn<T>(T prefab) where T : Component
        {
            return Spawn(prefab.gameObject)?.GetComponent<T>();
        }
        public static void Return<T>(T prefab, T instance) where T : Component
        {
            Return(prefab.gameObject, instance.gameObject);
        }
    }
}