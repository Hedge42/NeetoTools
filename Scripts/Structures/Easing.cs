using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Neeto
{
    // source: https://easings.net/
    public static class Easing
    {
        public enum Method
        {
            EaseInSine,
            EaseOutSine,
            EaseInCirc,
            EaseOutCirc,
            EaseInBack,
            EaseOutBack,
            EaseInElastic,
            EaseOutElastic,
            EaseInBounce,
            EaseOutBounce
        }
        public static Func<float, float> GetFunc(this Method method)
        {
            switch (method)
            {
                case Method.EaseInSine:
                    return EaseInSine;
                case Method.EaseOutSine:
                    return EaseOutSine;
                case Method.EaseInCirc:
                    return EaseInCirc;
                case Method.EaseOutCirc:
                    return EaseOutCirc;
                case Method.EaseInBack:
                    return EaseInBack;
                case Method.EaseOutBack:
                    return EaseOutBack;
                case Method.EaseInElastic:
                    return EaseInElastic;
                case Method.EaseOutElastic:
                    return EaseOutElastic;
                case Method.EaseInBounce:
                    return EaseInBounce;
                case Method.EaseOutBounce:
                    return EaseOutBounce;

                default: throw new System.NotImplementedException();
            }
        }

        static async UniTask Ease(Func<PlayerLoopTiming, float> deltaTime, float start, float end, float duration, Method method, Action<float> action, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken? token = null, Func<bool> isPaused = null)
        {
            var Token = token ?? global::Token.global;


            var elapsed = 0f;
            var range = end - start;
            var func = method.GetFunc();

            await UniTask.Yield(timing, Token);
            while ((elapsed += deltaTime(timing)) < duration)
            {
                if (isPaused != null)
                    await UniTask.WaitWhile(isPaused, timing, Token);

                action(start + range * func(elapsed / duration));
                await UniTask.Yield(timing, Token);
            }
            action(end);
        }
        public static async UniTask Ease(float start, float end, float duration, Method method, Action<float> action, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken? token = null, Func<bool> isPaused = null)
        {
            await Ease(NTask.GetDeltaTime, start, end, duration, method, action, timing, token, isPaused);
        }
        public static async UniTask EaseUnscaled(float start, float end, float duration, Method method, Action<float> action, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken? token = null, Func<bool> isPaused = null)
        {
            await Ease(NTask.GetUnscaledDeltaTime, start, end, duration, method, action, timing, token, isPaused);
        }
        public static float EaseInSine(float t)
        {
            return 1 - Mathf.Cos(t * Mathf.PI / 2f);
        }
        public static float EaseOutSine(float t)
        {
            return Mathf.Sin(t * Mathf.PI / 2f);
        }

        public static float EaseInCirc(float t)
        {
            return 1 - Mathf.Sqrt(1 - Mathf.Pow(t, 2));
        }
        public static float EaseOutCirc(float t)
        {
            return Mathf.Sqrt(1 - Mathf.Pow(t - 1, 2));
        }

        public static float EaseInBack(float t)
        {
            var c1 = 1.70158f;
            var c2 = c1 + 1;

            return c2 * t * t * t - c1 * t * t;
        }
        public static float EaseOutBack(float t)
        {
            var c1 = 1.70158f;
            var c2 = c1 + 1;

            return 1 + c2 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
        }

        public static float EaseInElastic(float t)
        {
            float c = (2 * Mathf.PI) / 3;
            return t == 0
                ? 0
                : t == 1
                ? 1
                : -Mathf.Pow(2, 10 * t - 10) * Mathf.Sin((t * 10 - 10.75f) * c);
        }
        public static float EaseOutElastic(float t)
        {
            float c = (2 * Mathf.PI) / 3;

            return t == 0
              ? 0
              : t == 1
              ? 1
              : Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * c) + 1;
        }
        public static float EaseOutBounce(float t)
        {
            float n1 = 7.5625f;
            float d1 = 2.75f;

            if (t < 1 / d1)
            {
                return n1 * t * t;
            }
            else if (t < 2 / d1)
            {
                return n1 * (t -= 1.5f / d1) * t + 0.75f;
            }
            else if (t < 2.5 / d1)
            {
                return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            }
            else
            {
                return n1 * (t -= 2.625f / d1) * t + 0.984375f;
            }
        }
        public static float EaseInBounce(float t)
        {
            return 1 - EaseOutBounce(1 - t);
        }


    }
}