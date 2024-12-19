using UnityEngine;

namespace Neeto
{
    public class CollisionExitComponent : SubCollider
    {
        void OnCollisionExit(Collision collision)
            => Raise(new(behaviour, Collider, collision, CollisionType.CollisionExit));
    }
}