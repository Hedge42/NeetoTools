using Neeto;
using UnityEngine;
using UnityEngine.Events;

namespace Neeto
{
    using static CollisionType;
    /// <summary>
    /// Recieve collision events from multiple colliders <br/>
    /// Parent-controller for <see cref="SubCollider"/>
    /// </summary>
    public class CollisionSystem : MonoBehaviour
    {
        public bool debug;

        [SerializeField, SyncedProperty(nameof(collidersEnabled))]
        bool _collidersEnabled;
        public bool collidersEnabled
        {
            get => _collidersEnabled;
            set => SetEnabled(value);
        }

        public CollisionType collisionType;

        [SerializeField, SyncedProperty(nameof(objectLayer))]
        Layer _objectLayer;
        public Layer objectLayer
        {
            get => _objectLayer;
            set => SetLayer(value);
        }
        [SerializeField, SyncedProperty(nameof(useTriggers))]

        bool _useTriggers;
        public bool useTriggers
        {
            get => _useTriggers;
            set => SetTriggers(value);
        }

        public LayerMask eventMask;
        public Collider[] colliders;

        public UnityEvent<CollisionInfo> callback;


        void Awake()
        {
            foreach (var collider in colliders)
            {
                SubCollider.Create(collisionType, collider, this, Callback);
            }
        }
        void Start() { }

        void Callback(CollisionInfo info)
        {
            if (!enabled)
                return;

            if (!eventMask.Evaluate(info.other.gameObject.layer))
                return;

            callback?.Invoke(info);
        }

        public void SetEnabled(bool value)
        {
            _collidersEnabled = value;
            foreach (var _ in colliders)
            {
                if (_)
                    _.enabled = value;
            }
        }
        public void SetTriggers(bool value)
        {
            _useTriggers = value;
            foreach (var _ in colliders)
            {
                if (_)
                    _.isTrigger = value;
            }
        }
        public void SetLayer(int value)
        {
            _objectLayer = value;
            foreach (var _ in colliders)
            {
                if (_)
                    _.gameObject.layer = value;
            }
        }
    }
}