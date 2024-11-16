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
    public class MeshVolumeGizmo : MonoBehaviour
    {
        //[GetComponent] public BoxCollider boxCollider;

        public Mesh mesh;

        public Color gizmoColor = Color.cyan.With(a: .2f);

        private void OnDrawGizmos()
        {
            if (!enabled)
                return;

            var temp = Gizmos.color;
            Gizmos.color = gizmoColor;
            Gizmos.DrawMesh(mesh, transform.position, transform.rotation, transform.lossyScale);
            Gizmos.color = temp;
        }
    }
}