using UnityEngine;

namespace Neeto
{
    public class CollisionStayComponent : SubCollider
    {
        void OnCollisionStay(Collision collision)
            => Raise(new(behaviour, Collider, collision, CollisionType.CollisionStay));
    }
}