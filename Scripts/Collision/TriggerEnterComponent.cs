using UnityEngine;

namespace Neeto
{
    public class TriggerEnterComponent : SubCollider
    {
        void OnTriggerEnter(Collider other)
        {
            if (behaviour.enabled)
                Raise(new(behaviour, Collider, other, CollisionType.TriggerEnter));
        }
    }
}