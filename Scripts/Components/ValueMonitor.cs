using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Neeto
{
    public class ValueMonitor : MonoBehaviour
    {
        [SerializeReference, Polymorphic]
        public IValueMonitor monitor;

        Token token;

        void OnEnable()
        {
            monitor.MonitorAsync(token.Update()).Forget();
        }
        void OnDisable()
        {
            token.Disable();
        }
    }

    public interface IValueMonitor
    {
        UniTaskVoid MonitorAsync(CancellationToken token);
    }

    [Serializable]
    public class FloatMonitorSequence : IValueMonitor
    {
        [Serializable]
        public struct Element
        {
            public FloatComparison comparison;
            public UnityEvent entered;
        }
        public SerializedProperty<float> property;
        public Element[] elements;

        public async UniTaskVoid MonitorAsync(CancellationToken token)
        {
            int? lastIndex = null;

            for (; ; )
            {
                var value = property.Value;
                for (int i = 0; i < elements.Length; i++)
                {
                    if (i != lastIndex && elements[i].comparison.Evaluate(value))
                    {
                        lastIndex = i;
                        elements[i].entered?.Invoke();
                    }
                }

                await UniTask.Yield(PlayerLoopTiming.EarlyUpdate);
            }
        }
    }

    [Serializable]
    public class BooleanMonitor : IValueMonitor
    {
        public bool sendInitial;

        [SerializeReference, Polymorphic]
        public SerializedProperty<bool> property;

        public UnityEvent onTrue;
        public UnityEvent onFalse;

        public async UniTaskVoid MonitorAsync(CancellationToken token)
        {

            bool? lastValue = sendInitial ? null : property.Value;
            for (; ; )
            {
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
                var newValue = property.Value;

                if (lastValue != newValue)
                {
                    if (newValue)
                    {
                        onTrue?.Invoke();
                    }
                    else
                    {
                        onFalse?.Invoke();
                    }
                }
            }
        }
    }
}
