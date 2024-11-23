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
            monitor.MonitorAsync(token.Refresh()).Forget();
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
    public class FloatMonitorEvent : IValueMonitor
    {
        public SerializedProperty<float> property;
        public UnityEvent<float> output;

        public async UniTaskVoid MonitorAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, token);
                output?.Invoke(property.Value);
            }
        }
    }

    [Serializable]
    public class FloatMonitorSwitch : IValueMonitor
    {
        public SerializedProperty<float> property;

        public UnityEvent<float> output;

        public async UniTaskVoid MonitorAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, token);
                output?.Invoke(property.Value);
            }
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

        public async UniTaskVoid MonitorAsync(CancellationToken token)
        {
            int lastIndex = -1;

            while(!token.IsCancellationRequested)
            {
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, token);
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
