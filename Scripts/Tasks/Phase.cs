using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace Neeto
{
    public struct Phase
    {
        public float start;
        public float end;
        public float duration;
        public PlayerLoopTiming timing;

        public Phase(float _duration, PlayerLoopTiming _timing = PlayerLoopTiming.Update)
        {
            start = 0;
            end = 1;
            duration = _duration;
            timing = _timing;
        }
        public Phase(float _start, float _end, float _duration, PlayerLoopTiming _timing = PlayerLoopTiming.Update)
        {
            start = _start;
            end = _end;
            duration = _duration;
            timing = _timing;
        }


        public static void MoveTowards(CancellationToken token, float target, float speed, PlayerLoopTiming timing, Func<float> get, Action<float> set)
        {
            MoveTowardsAsync(token, target, speed, timing, get, set).Forget();
        }
        public static async UniTask MoveTowardsAsync(CancellationToken token, float target, float speed, PlayerLoopTiming timing, Func<float> get, Action<float> set)
        {
            for (; ; )
            {
                await UniTask.Yield(timing, token, true);

                var value = get();

                if (value >= target - Mathf.Epsilon)
                    break;

                set(Mathf.MoveTowards(value, target, timing.GetDeltaTime() * speed));
            }
        }
        public async UniTask Start(CancellationToken token, Action<float> update)
        {
            await StartAsync(start, end, duration, timing, token, update);
        }
        public static async UniTask StartAsync(float duration, PlayerLoopTiming timing, CancellationToken token, Action<float> onT)
        {
            var elapsed = 0f;
            do
            {
                await UniTask.Yield(timing, token, true);
                elapsed += timing.GetDeltaTime();
                onT(Mathf.Clamp01(elapsed / duration));
            }
            while (elapsed < duration);
        }
        public static async UniTask StartAsync(float start, float end, float duration, PlayerLoopTiming timing, CancellationToken token, Action<float> update)
        {
            await StartAsync(duration, timing, token, t => update(Engine.Remap(t, 0, 1, start, end, true)));
        }
        public static void Start(float duration, PlayerLoopTiming timing, CancellationToken token, Action<float> onT) => StartAsync(duration, timing, token, onT).Forget();
        public static void Start(float start, float end, float duration, PlayerLoopTiming timing, CancellationToken token, Action<float> update) => StartAsync(start, end, duration, timing, token, update).Forget();
    }
}