using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Debug = UnityEngine.Debug;
using UnityEngine.VFX;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Neeto
{
    public class SphereVolumeGizmo : MonoBehaviour
	{
        [GetComponent] public SphereCollider sphereCollider;

        public Color gizmoColor = Color.cyan.With(a: .15f);

        private void OnDrawGizmos()
        {
            if (!enabled)
                return;


            var temp = Gizmos.color;
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(sphereCollider.transform.position, sphereCollider.radius);
            Gizmos.color = temp;
        }

        private void Start()
        {
            
        }
    }
}