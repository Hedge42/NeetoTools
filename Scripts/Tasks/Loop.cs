using Cysharp.Threading.Tasks;
using Neeto;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Neeto
{
    public struct Loop
    {
        public static void Start(CancellationToken token, PlayerLoopTiming timing, Action onUpdate)
        {
            UniTask.Void(async () =>
            {
                for (; ; )
                {
                    await UniTask.Yield(timing, token, true);
                    onUpdate();
                }
            });
        }
        public static async UniTask StartAsync(CancellationToken token, PlayerLoopTiming timing, Action update)
        {
            for (; ; )
            {
                await UniTask.Yield(timing, token, true);
                update();
            }
        }
        public static void Interpolate(CancellationToken token, float duration, PlayerLoopTiming timing, Action<float> onT)
        {
            UniTask.Void(async () =>
            {
                var elapsed = 0f;
                for (; ; )
                {
                    await UniTask.Yield(timing, token, true);
                    elapsed += timing.GetDeltaTime();
                    onT.Invoke(Mathf.Clamp01(elapsed / duration));
                }
            });
        }

        public static void Void(Action update, CancellationToken token, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            UniTask.Void(async () =>
            {
                for (; ; )
                {
                    await UniTask.Yield(timing, token, true);
                    update();
                }
            });
        }

        public static async UniTask UntilCancelled(CancellationToken token)
        {
            await UniTask.WaitUntil(() => token.IsCancellationRequested);
        }
        public static async UniTask UntilCancelled(Action update, CancellationToken token, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            for (; ; )
            {
                await UniTask.Yield(timing);
                if (token.IsCancellationRequested)
                    return;
                update();
            }
        }
        public static async UniTask ForSeconds(float duration, Action update, CancellationToken token, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                await UniTask.Yield(timing, token, true);
                elapsed += GetDeltaTime(timing);
                update();
            }
        }
        public static async UniTask ForSeconds(float duration, CancellationToken token, Action update)
        {
            await ForSeconds(duration, token, PlayerLoopTiming.Update, update);
        }
        public static async UniTask ForSeconds(float duration, CancellationToken token, PlayerLoopTiming timing, Action update)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                await UniTask.Yield(timing, token, true);
                elapsed += GetDeltaTime(timing);
                update();
            }
        }
        public static async UniTask WhileAsync(Func<bool> continueCondition, Action update, CancellationToken token, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            for (; ; )
            {
                await UniTask.Yield(timing, token, true);
                if (!continueCondition())
                    return;
                update();
            }
        }
        public static async UniTask UntilAsync(Func<bool> exitCondition, Action update, CancellationToken token, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            for (; ; )
            {
                await UniTask.Yield(timing, token, true);
                if (exitCondition())
                    return;
                update();
            }
        }

        public static float GetDeltaTime(PlayerLoopTiming timing)
        {
            // only two fixedUpdate options (4 and 5)
            if (timing == PlayerLoopTiming.FixedUpdate || timing == PlayerLoopTiming.LastFixedUpdate)
                return Time.fixedDeltaTime;

            // initialization options are (0 and 1), all others are update timings
            else if ((int)timing > 1)
                return Time.deltaTime;
            // reasonable default? This will never be used, who am I kidding
            else
                return .1f;
        }
        public static float GetDeltaTime(PlayerLoopTiming timing, bool ignoreTimeScale = false)
        {
            // only two fixedUpdate options (4 and 5)
            if (timing == PlayerLoopTiming.FixedUpdate || timing == PlayerLoopTiming.LastFixedUpdate)
                return ignoreTimeScale
                    ? Time.fixedUnscaledDeltaTime
                    : Time.fixedDeltaTime;

            // initialization options are (0 and 1), all others are update timings
            else if ((int)timing > 1)
                return ignoreTimeScale
                    ? Time.unscaledDeltaTime
                    : Time.deltaTime;
            // reasonable default? This will never be used, who am I kidding
            else
                return .1f;
        }
        public static float GetUnscaledDeltaTime(PlayerLoopTiming timing)
        {
            // only two fixedUpdate options (4 and 5)
            if (timing == PlayerLoopTiming.FixedUpdate || timing == PlayerLoopTiming.LastFixedUpdate)
                return Time.fixedUnscaledDeltaTime;
            // initialization options are (0 and 1), all others are update timings
            else if ((int)timing > 1)
                return Time.unscaledDeltaTime;
            // reasonable default? This will never be used, who am I kidding
            else
                return .1f;
        }
    }

    public static class LoopExtensions
    {
        public static float GetDeltaTime(this PlayerLoopTiming timing)
        {
            // only two fixedUpdate options (4 and 5)
            if (timing == PlayerLoopTiming.FixedUpdate || timing == PlayerLoopTiming.LastFixedUpdate)
                return Time.fixedDeltaTime;

            // initialization options are (0 and 1), all others are update timings
            else if ((int)timing > 1)
                return Time.deltaTime;
            // reasonable default? This will never be used, who am I kidding
            else
                return .1f;
        }
        public static float GetDeltaTime(this PlayerLoopTiming timing, bool ignoreTimeScale = false)
        {
            // only two fixedUpdate options (4 and 5)
            if (timing == PlayerLoopTiming.FixedUpdate || timing == PlayerLoopTiming.LastFixedUpdate)
                return ignoreTimeScale
                    ? Time.fixedUnscaledDeltaTime
                    : Time.fixedDeltaTime;

            // initialization options are (0 and 1), all others are update timings
            else if ((int)timing > 1)
                return ignoreTimeScale
                    ? Time.unscaledDeltaTime
                    : Time.deltaTime;
            // reasonable default? This will never be used, who am I kidding
            else
                return .1f;
        }
        public static float GetUnscaledDeltaTime(this PlayerLoopTiming timing)
        {
            // only two fixedUpdate options (4 and 5)
            if (timing == PlayerLoopTiming.FixedUpdate || timing == PlayerLoopTiming.LastFixedUpdate)
                return Time.fixedUnscaledDeltaTime;
            // initialization options are (0 and 1), all others are update timings
            else if ((int)timing > 1)
                return Time.unscaledDeltaTime;
            // reasonable default? This will never be used, who am I kidding
            else
                return .1f;
        }
    }
}