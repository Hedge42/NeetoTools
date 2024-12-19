using UnityEngine;

namespace Neeto
{
    public class TriggerExitComponent : SubCollider
    {
        void OnTriggerExit(Collider other)
            => Raise(new(behaviour, Collider, other, CollisionType.TriggerExit));
    }
}