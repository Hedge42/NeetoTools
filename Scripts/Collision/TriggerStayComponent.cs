using UnityEngine;

namespace Neeto
{
    public class TriggerStayComponent : SubCollider
    {
        void OnTriggerStay(Collider other)
            => Raise(new(behaviour, Collider, other, CollisionType.TriggerStay));
    }
}