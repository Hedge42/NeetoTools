using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Neeto
{
    public static class NPhysics
    {
        public static void SnapToGround(this Rigidbody rb, LayerMask env)
        {
            var qti = QueryTriggerInteraction.Ignore;
            var pos = rb.position + Vector3.up * 2f; // start from above
            var dir = Vector3.down;
            if (Physics.Raycast(pos, dir, out var hit, 10f, env, qti))            {
                rb.position = hit.point;
            }
        }
    }
}
