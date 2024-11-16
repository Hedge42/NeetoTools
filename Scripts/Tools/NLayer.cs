using System.Text;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Neeto
{
    [Serializable]
    public static class NLayer
    {
        public static string[] GetLayerNames()
        {
            var list = new List<string>();
            for (int i = 0; i < 32; i++)
            {
                var name = LayerMask.LayerToName(i);
                if (!name.IsEmpty())
                    list.Add(name);
            }
            return list.ToArray();
        }
        public static void SetLayer(this Collider[] colliders, int layer)
        {
            foreach (var collider in colliders)
            {
                if (collider.gameObject.layer != layer)
                    collider.gameObject.layer = layer;
            }
        }
        public static bool Evaluate(this LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }
        public static bool Evaluate(this LayerMask mask, GameObject gameObject)
        {
            return mask.Evaluate(gameObject.layer);
        }
        public static bool Evaluate(this LayerMask mask, Collider collider)
        {
            return mask.Evaluate(collider.gameObject.layer);
        }
    }
}