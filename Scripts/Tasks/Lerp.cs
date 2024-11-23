using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace Neeto
{
    public struct Lerp
    {

        public static Routine Max(float max, Func<float> get, Action<float> set, CancellationToken? token = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            return new(Token =>
                UniTask.Create(async () =>
                {
                    float t;
                    do
                    {
                        await UniTask.Yield(timing, Token, true);
                        var y = get();
                        set(t = Mathf.Min(max, y));
                    } while (t < max);
                }),
                token);
        }
        public static Routine Linear(float duration, Action<float> update, CancellationToken? token = null)
        {
            return new Routine(t => Linear(duration, t, update), token);
        }
        public static async UniTask Linear(float duration, CancellationToken token, Action<float> update, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                await UniTask.Yield(timing, token, true);
                elapsed += timing.GetDeltaTime();
                var t = Mathf.Clamp01(elapsed / duration);
                update(t);
            }
        }
    }
}