using UnityEngine;

#if UNITY_EDITOR
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