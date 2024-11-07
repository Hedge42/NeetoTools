using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Neeto
{
    public interface IBoolSource
    {
        bool GetValue();
    }
    [Serializable]
    public struct BoolSource : IBoolSource
    {
        public static implicit operator bool(BoolSource source) => source.GetValue();

        [LeftToggle]
        public bool invert;

        [SerializeReference, Polymorphic]
        public IBoolSource source;

        public bool GetValue() => source.GetValue() != invert;
    }
    [Serializable]
    public struct BoolPropertySource : IBoolSource
    {
        public SerializedProperty<bool> property;

        public bool GetValue() => property.Value;
    }

    [Serializable]
    public class IsBehaviourActive : IBoolSource
    {
        public Behaviour behaviour;
        public bool GetValue() => behaviour.isActiveAndEnabled;
    }
    [Serializable]
    public class IsNavMeshAgentStopped : IBoolSource
    {
        public NavMeshAgent NavMeshAgent;
        public bool GetValue() => NavMeshAgent.isStopped;
    }
}
