using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Neeto
{
    public interface IFloatSource
    {
        float GetValue();
    }

    [Serializable]
    public class FloatSource : IFloatSource
    {
        public static implicit operator float(FloatSource source) => source.GetValue();

        [LeftToggle]
        public bool shouldClamp;
        [LeftToggle]
        public bool isAbsolute;
        [LeftToggle]
        public bool shouldMultiply;
        public Vector2 clampRange = Vector2.up;
        public float multiplyAmount = 1f;

        [SerializeReference, Polymorphic]
        public IFloatSource source;
        public float GetValue()
        {
            var value = source.GetValue();

            if (shouldClamp)
                value = value.Clamp(clampRange);
            if (isAbsolute)
                value = value.Abs();
            if (shouldMultiply)
                value *= multiplyAmount;

            return value;
        }
    }

    [Serializable]
    public class FloatPropertySource : IFloatSource
    {
        public SerializedProperty<float> property;

        public float GetValue()
        {
            return property.Value;
        }
    }

    [Serializable]
    public class RigidbodySpeedSource : IFloatSource
    {
        public static implicit operator float(RigidbodySpeedSource source) => source.GetValue();

        [GetComponent] public Rigidbody Rigidbody;
        public bool isRelative = true;
        public Axis axis = Axis.z;
        public float GetValue()
        {
            var velocity = Rigidbody.velocity;
            if (isRelative)
                velocity = Rigidbody.InverseRotation() * velocity;

            return axis switch
            {
                Axis.z => velocity.z,
                Axis.y => velocity.y,
                Axis.x => velocity.x,
                _ => velocity.magnitude
            };
        }
    }

    [Serializable]
    public class NavMeshAgentSpeedSource : IFloatSource
    {
        public static implicit operator float(NavMeshAgentSpeedSource source) => source.GetValue();

        [GetComponent] public NavMeshAgent NavMeshAgent;
        public bool isRelative = true;
        public Axis axis = Axis.z;
        public float GetValue()
        {
            var velocity = NavMeshAgent.velocity;
            if (isRelative)
                velocity = NavMeshAgent.transform.rotation.Inverse() * velocity;
            
            return axis switch
            {
                Axis.z => velocity.z,
                Axis.y => velocity.y,
                Axis.x => velocity.x,
                _ => velocity.magnitude
            };
        }
    }
}
