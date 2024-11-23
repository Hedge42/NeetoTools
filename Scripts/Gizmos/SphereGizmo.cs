using UnityEngine;
using Neeto;

public class SphereGizmo : MonoBehaviour
{
    public Color color = Color.cyan.With(a: .2f);
    public float radius = .33f;
    public bool inheritScale;

    void OnDrawGizmos()
    {
        Gizmos.color = color;
        var r = radius;
        if (inheritScale)
            r *= transform.lossyScale.magnitude;
        Gizmos.DrawSphere(transform.position, r);
        Gizmos.color = color.With(a: 1f);
        Gizmos.DrawWireSphere(transform.position, r);
    }
}
