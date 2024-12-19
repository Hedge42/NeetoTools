using Neeto;
using System;
using UnityEngine;
using static Neeto.CollisionType;

namespace Neeto
{
    /// <summary>
    /// Sends collision data to parent-controller <see cref="CollisionSystem"/>
    /// </summary>
    public class SubCollider : MonoBehaviour
    {
        public MonoBehaviour behaviour;
        public event Action<CollisionInfo> callback;
        protected Collider Collider;

        void Awake() => Collider = GetComponent<Collider>();
        protected void Raise(CollisionInfo info)
        {
            if (!behaviour || behaviour.enabled)
            {
                callback?.Invoke(info);
            }
        }


        public static void Create(CollisionType type, Collider collider, MonoBehaviour behaviour, Action<CollisionInfo> callback)
        {
            if (!collider)
                return;

            var _ = collider.gameObject;
            var sub = type switch
            {
                CollisionEnter => _.AddComponent<CollisionEnterComponent>(),
                CollisionExit => _.AddComponent<CollisionExitComponent>(),
                CollisionStay => _.AddComponent<CollisionStayComponent>(),

                TriggerEnter => _.AddComponent<TriggerEnterComponent>(),
                TriggerExit => _.AddComponent<TriggerExitComponent>(),
                TriggerStay => _.AddComponent<TriggerStayComponent>(),

                _ => _.AddComponent<SubCollider>()
            };

            sub.behaviour = behaviour;
            sub.callback = callback;
        }
    }
}