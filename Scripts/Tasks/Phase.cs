using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace Neeto
{
    public struct Phase
    {
        public static Routine Create(Action<float> onUpdate, float duration)
        {
            return new Routine(async token => await StartAsync(onUpdate, duration, token));
        }
        public static Routine Start(Action<float> onUpdate, float duration)
        {
            return Create(onUpdate, duration).Resume();
        }
        public static async UniTask StartAsync(Action<float> onUpdate, float duration, CancellationToken token)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                await UniTask.Yield(token);
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                onUpdate(t);
            }
        }
    }
}