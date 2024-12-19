using UnityEngine;

namespace Neeto
{
    public class CollisionEnterComponent : SubCollider
    {
        void OnCollisionEnter(Collision collision)
            => Raise(new(behaviour, Collider, collision, CollisionType.CollisionEnter));
    }
}