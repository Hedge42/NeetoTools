using UnityEngine;

#if UNITY_EDITOR
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