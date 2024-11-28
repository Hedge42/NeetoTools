using Cysharp.Threading.Tasks;
using System;
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
            monitor.MonitorAsync(++token).Forget();
        }
        void OnDisable()
        {
            token.Cancel();
        }
    }

    public interface IValueMonitor
    {
        UniTaskVoid MonitorAsync(CancellationToken token);
    }

    [Serializable]
    public class FloatMonitorEvent : IValueMonitor
    {
        public SerializedProperty<float> property;
        public UnityEvent<float> output;

        public UniTaskVoid MonitorAsync(CancellationToken token)
        {
            Loop.Start(token, PlayerLoopTiming.LastPostLateUpdate, () => output?.Invoke(property.Value));
            return default;
        }
    }

    [Serializable]
    public class FloatMonitorSwitch : IValueMonitor
    {
        public SerializedProperty<float> property;

        public UnityEvent<float> output;

        public UniTaskVoid MonitorAsync(CancellationToken token)
        {
            Loop.Start(token, PlayerLoopTiming.LastPostLateUpdate, () => output?.Invoke(property.Value));
            return default;
        }
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

        public UniTaskVoid MonitorAsync(CancellationToken token)
        {
            int lastIndex = -1;

            Loop.Void(() =>
            {
                var value = property.Value;
                for (int i = 0; i < elements.Length; i++)
                {
                    if (elements[i].comparison.Evaluate(value))
                    {
                        if (lastIndex != i)
                        {
                            lastIndex = i;
                            elements[i].entered?.Invoke();
                        }
                        break;
                    }
                }
            },
            token, PlayerLoopTiming.LastPostLateUpdate);
            return default;
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

        public UniTaskVoid MonitorAsync(CancellationToken token)
        {
            bool? lastValue = sendInitial ? null : property.Value;
            Loop.Void(() =>
            {
                var newValue = property.Value;

                if (lastValue != newValue)
                {
                    if (newValue)
                        onTrue?.Invoke();
                    else
                        onFalse?.Invoke();
                }
            }, 
            token, PlayerLoopTiming.PostLateUpdate);
            return default;
        }
    }
}
