using System.Collections.Generic;
using UnityEngine;
using System;

namespace Neeto
{
    /// <summary>
    /// A simple data structure to do overlap spheres without additional allocations
    /// </summary>
    [Serializable]
    public class Sensor
    {
        public float radius = .5f;
        public LayerMask layer = -1;
        public QueryTriggerInteraction qti = QueryTriggerInteraction.Collide;

        const int BUFFER_SIZE = 32;
        public static Collider[] Buffer { get; } = new Collider[BUFFER_SIZE];
        public static int Hits { get; private set; }
        public int hits { get; private set; }


        public Sensor(float radius, LayerMask mask, QueryTriggerInteraction queryTrigger)
        {
            this.radius = radius;
            this.layer = mask;
            this.qti = queryTrigger;
        }

        public static int Sphere(Vector3 position, float radius, LayerMask layer, QueryTriggerInteraction qti)
        {
            /*
            do the overlap sphere and store the hit results in the buffer
             */

            return Hits = Physics.OverlapSphereNonAlloc(position, radius, Buffer, layer, qti);
        }
        public static int Sphere<T>(List<T> results, Vector3 position, float radius, LayerMask layer, QueryTriggerInteraction qti)
        {
            Sphere(position, radius, layer, qti);

            results.Clear();

            for (int i = 0; i < Hits; i++)
            {
                var result = Buffer[i].GetComponent<T>();
                if (result != null)
                    results.Add(result);
            }

            return Hits;
        }
        public static int Capsule(Vector3 p0, float rad, float height, LayerMask layer, QueryTriggerInteraction qti)
        {
            var p1 = p0 + Vector3.up * height;
            return Hits = Physics.OverlapCapsuleNonAlloc(p0, p1, rad, Buffer, layer, qti);
        }
        public static int Box(Vector3 position, Vector3 halfExtents, float height, Quaternion rotation, LayerMask layer, QueryTriggerInteraction qti)
        {
            return Hits = Physics.OverlapBoxNonAlloc(position, halfExtents, Buffer, rotation, layer, qti);
        }

        public static bool Any() => Hits > 0;
        public static void GetComponents<T>(List<T> results)
        {
            results.Clear();

            for (int i = 0; i < Hits; i++)
            {
                var result = Buffer[i].GetComponent<T>();
                if (result != null)
                    results.Add(result);
            }
        }

        public static ArraySegment<Collider> GetCollidersNonAlloc()
        {
            return new ArraySegment<Collider>(Buffer, 0, Hits);
        }

        public int Scan(Vector3 position)
        {
            return hits = Sphere(position, radius, layer, qti);
        }
        public int HitCount(LayerMask filter)
        {
            int result = 0;

            for (int i = 0; i < hits; i++)
                if (filter.Evaluate(Buffer[i].gameObject.layer))
                    result++;

            return result;
        }
        public int HitCount<T>()
        {
            int result = 0;

            for (int i = 0; i < hits; i++)
                if (Buffer[i].GetComponent<T>() != null)
                    result++;

            return result;
        }
        public bool Any<T>()
        {
            for (int i = 0; i < hits; i++)
                if (Buffer[i].GetComponent<T>() != null)
                    return true;

            return false;
        }
        public bool TaperedOverlapSphere(Vector3 origin, Vector3 direction, float distance, float startRadius, float endRadius, int steps, LayerMask layerMask, out Collider hit)
        {
            /*
             Do multiple overlap spheres with different orientations
             */

            direction.Normalize();
            float stepDistance = distance / steps;
            float radiusStep = (endRadius - startRadius) / steps;
            Vector3 currentPosition = origin;
            hit = null;

            for (int i = 0; i < steps; i++)
            {
                float currentRadius = startRadius + (radiusStep * i);
                currentPosition += direction * stepDistance;
                var hits = Physics.OverlapSphere(currentPosition, currentRadius, layerMask);
                if (hits.Length > 0)
                {
                    hit = hits[0];
                    return true;
                }
            }
            return false;
        }
    }
}