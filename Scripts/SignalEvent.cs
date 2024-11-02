using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Neeto
{
    public class SignalEvent : MonoBehaviour
    {
        public bool resetTime = true;
        [SerializeReference, Polymorphic]
        public ISignalType signal = new CurveSignal();

        public UnityEvent<float> output;

        float time;

        void OnEnable()
        {
            if (resetTime)
                time = 0f;
        }
        void Update()
        {
            output?.Invoke(signal.Process(Time.time));
        }
    }
    public interface ISignalType
    {
        float Process(float time);
    }
    [Serializable]
    public class CosineSignal : ISignalType
    {
        public float frequency = 1f;
        public Vector2 remap = Vector2.up;
        public float Process(float time)
        {
            return Mathf.Cos(time * frequency * Mathf.PI * 2f).Remap(-1, 1, remap.x, remap.y);
        }
    }
    [Serializable]
    public class CurveSignal : ISignalType
    {
        public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f).With(postWrapMode: WrapMode.Loop);
        public float freq, amp = 1f;
        public float Process(float time)
        {
            return amp * curve.Evaluate(time * freq);
        }
    }

    public static class CurveExtensions
    {
        public static AnimationCurve With(this AnimationCurve curve, WrapMode? preWrapMode = null, WrapMode? postWrapMode = null)
        {
            if (preWrapMode is WrapMode PreWrapMode)
            {
                curve.preWrapMode = PreWrapMode;
            }
            if (postWrapMode is WrapMode PostWrapMode)
            {
                curve.preWrapMode = PostWrapMode;
            }

            return curve;
        }
    }
}
