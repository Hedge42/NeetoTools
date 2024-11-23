using Cysharp.Threading.Tasks;
using Neeto;
using System;
using System.Threading;
using UnityEngine;

namespace Neeto
{
    public struct Loop
    {
        public static Routine Create(Action update, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            return new(token => UntilCancelled(update, token, timing));
        }

        /// <summary>Until cancelled</summary>
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

        public static Routine Until(Func<bool> exitCondition, CancellationToken? token = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            return new(t => UniTask.WaitUntil(exitCondition, timing, t, true), token);
        }
        public static Routine Until(Func<bool> exitCondition, Action update, CancellationToken? token = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            return new(t => UntilAsync(exitCondition, update, t, timing), token);
        }
        public static Routine While(Func<bool> continueCondition, Action update, CancellationToken? token = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            return new Routine(t => WhileAsync(continueCondition, update, t), token);
        }
        public static Routine UntilCancelled(Action update, CancellationToken? token = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            return new(t => UntilCancelled(update, t, timing), token);
        }
        public static Routine ForSeconds(float duration, Action update, CancellationToken? token = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            return new(t => ForSeconds(duration, update, t), token);
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

        public static void Update(CancellationToken token, MonoBehaviour source, Action onUpdate, Func<bool> until = null, Action onCancel = null, Action onCancelOrComplete = null, Action onComplete = null)
        {
            UpdateAsync(token, source, onUpdate, until, onCancel, onCancelOrComplete, onComplete).Forget();
        }
        public static async UniTask UpdateAsync(CancellationToken token, MonoBehaviour source, Action onUpdate, Func<bool> until = null, Action onCancel = null, Action onCancelOrComplete = null, Action onComplete = null)
        {
            try
            {
                while (until == null || until() == false)
                {
                    await UniTask.WaitForEndOfFrame(source, token);
                    onUpdate?.Invoke();
                }
                onComplete?.Invoke();
            }
            catch (OperationCanceledException)
            {
                onCancel?.Invoke();
            }
            finally
            {
                onCancelOrComplete?.Invoke();
            }
        }

        public static async UniTaskVoid UpdateAsync(PlayerLoopTiming timing, int frameSkip, CancellationToken token, Action action)
        {
            while (!token.IsCancellationRequested)
            {
                action?.Invoke();

                for (int i = 0; i < frameSkip; i++)
                    await UniTask.Yield(timing, token);
            }
        }
        public static async UniTaskVoid UpdateAsync(PlayerLoopTiming timing, float delay, bool ignoreTimeScale, CancellationToken token, Action action)
        {
            int ms = delay.ToMilliseconds();
            while (!token.IsCancellationRequested)
            {
                action?.Invoke();
                await UniTask.Delay(ms, ignoreTimeScale, timing, token);
            }
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