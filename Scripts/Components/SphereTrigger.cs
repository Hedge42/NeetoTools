using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Neeto;

public class SphereTrigger : MonoBehaviour
{
    public new SphereCollider collider;
    public Color passiveColor = Color.cyan.With(a: .2f);
    public Color activeColor = Color.red.With(a: .2f);
    public float radius => collider.radius;
    public bool isTriggered => enabled && colliders.Count > 0;

    HashSet<Collider> colliders = new();

    public bool debug;

    void Start() { } // just so we can disable
    void OnDrawGizmos()
    {
        if (!enabled)
            return;

        collider ??= GetComponent<SphereCollider>();

        Gizmos.color = colliders.Any() ? activeColor : passiveColor;
        var r = radius * transform.lossyScale.x;
        Gizmos.DrawSphere(transform.position, r);
        Gizmos.color = Gizmos.color.With(a: 1f);
        Gizmos.DrawWireSphere(transform.position, r);
    }
    void OnTriggerEnter(Collider other)
    {
        colliders.Add(other);

        Debug.Log($"foot enter '{other.name}'");
    }
    void OnTriggerExit(Collider other)
    {
        colliders.Remove(other);

        Debug.Log($"foot exit '{other.name}'");
    }
}
