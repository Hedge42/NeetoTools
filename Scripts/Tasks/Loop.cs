using Cysharp.Threading.Tasks;
using Neeto;
using System;
using System.Threading;
using UnityEngine;

namespace Neeto
{
    public struct Loop
    {
        public bool isRunning { get; private set; }
        public PlayerLoopTiming timing { get; private set; }
        Token token { get; set; }
        Action update { get; set; }

        public static Loop Create(PlayerLoopTiming timing, Action update)
        {
            var loop = new Loop();
            loop.timing = timing;
            loop.update = update;
            return loop;
        }
        public void Start() => StartAsync().Forget();
        public void Stop() => token.Disable();
        public async UniTask StartAsync()
        {
            var token = ++this.token;
            for (; ; )
            {
                await UniTask.Yield(timing, token, true);
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