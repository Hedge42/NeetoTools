using UnityEngine;
using Neeto;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using System;
using UnityEngine.Events;

namespace Neeto
{
    using static Neeto.CollisionType;
    public class CollisionMessager : MonoBehaviour
    {
        [SerializeReference, Polymorphic]
        public ICollisionEvent Event;

        public new Collider collider;

        void Awake()
        {
            if (Event != null)
            {
                Event.Create(collider, this);
            }
        }

        public interface ICollisionEvent
        {
            void Create(Collider collider, MonoBehaviour mainObject);
            void Raise(CollisionInfo info);
        }

        [Serializable]
        public class CollisionEnterInfo : ICollisionEvent
        {
            public UnityEvent<CollisionInfo> Event;

            public void Create(Collider collider, MonoBehaviour mainObject)
                => SubCollider.Create(CollisionType.CollisionEnter, collider, mainObject, Raise);

            public void Raise(CollisionInfo info)
                    => Event?.Invoke(info);
        }
        [Serializable]
        public class TriggerStayInfo : ICollisionEvent
        {
            public UnityEvent<CollisionInfo> Event;

            public void Create(Collider collider, MonoBehaviour mainObject)
                => SubCollider.Create(TriggerStay, collider, mainObject, Raise);

            public void Raise(CollisionInfo info)
                    => Event?.Invoke(info);
        }

        [Serializable]
        public class TriggerEnterInfo : ICollisionEvent
        {
            public UnityEvent<CollisionInfo> Event;
            public void Create(Collider collider, MonoBehaviour mainObject)
                => SubCollider.Create(TriggerEnter, collider, mainObject, Raise);
            public void Raise(CollisionInfo info)
                => Event?.Invoke(info);
        }

        [Serializable]
        public class CollisionIsEntered : ICollisionEvent
        {
            public UnityEvent<bool> Event;
            public bool invert;

            public void Create(Collider collider, MonoBehaviour mainObject)
            {
                SubCollider.Create(CollisionEnter, collider, mainObject, Raise);
                SubCollider.Create(CollisionExit, collider, mainObject, Raise);
            }
            public void Raise(CollisionInfo info)
            {
                if (info.type == CollisionEnter)
                {
                    Event?.Invoke(true != invert);
                }
                else if (info.type == CollisionExit)
                {
                    Event?.Invoke(false != invert);
                }
            }
        }

        [Serializable]
        public class TriggerIsEntered : ICollisionEvent
        {
            public UnityEvent<bool> Event;
            public bool invert;
            public void Create(Collider collider, MonoBehaviour mainObject)
            {
                SubCollider.Create(TriggerEnter, collider, mainObject, Raise);
                SubCollider.Create(TriggerExit, collider, mainObject, Raise);
            }
            public void Raise(CollisionInfo info)
            {
                if (info.type == CollisionType.TriggerEnter)
                {
                    Event?.Invoke(true != invert);
                }
                else if (info.type == CollisionType.TriggerExit)
                {
                    Event?.Invoke(false != invert);
                }
            }
        }
    }
}