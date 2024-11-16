using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#if UNITY_EDITOR
#endif
using UnityEditor;

namespace Neeto
{
    public static class NTask
    {
        public static CancellationTokenSource Refresh(this CancellationTokenSource source)
        {
            Kill(source);

            source = new CancellationTokenSource();

            return source;
        }
        public static CancellationTokenSource Refresh(this CancellationTokenSource source, out CancellationToken token)
        {
            Kill(source);

            source = new CancellationTokenSource();
            token = source.Token;

            return source;
        }
        public static CancellationTokenSource Refresh(this CancellationTokenSource source, Component component)
        {
            source.Refresh();
            source.RegisterRaiseCancelOnDestroy(component);
            return source;
        }
        public static CancellationTokenSource Refresh(this CancellationTokenSource source, GameObject gameOobject)
        {
            source.Refresh();
            source.RegisterRaiseCancelOnDestroy(gameOobject);
            return source;
        }
        public static void Kill(this CancellationTokenSource source)
        {
            try
            {
                source?.Cancel();
                source?.Dispose();
            }
            catch { }
        }
        public static void Restart(this CancellationTokenSource source, params UniTask[] tasks)
        {
            source.Refresh(out var token);

            foreach (var t in tasks)
                t.AttachExternalCancellation(token).Forget();
        }

        public static UniTask Combine(params UniTask[] tasks)
        {
            return UniTask.WhenAll(tasks);
        }
        public static UniTask Combine(this UniTask task, UniTask other)
        {
            return UniTask.WhenAll(task, other);
        }


        public static void OnComplete(this UniTask task, UniTaskStatus status, Action action)
        {
            task.onComplete(status, action).Forget();
        }
        private static async UniTask onComplete(this UniTask task, UniTaskStatus status, Action action)
        {
            await task;
            if (task.Status == status)
                action();
        }

        public static UniTask RunAsync(this IEnumerator routine, CancellationToken token)
        {
            return routine.ToUniTask(cancellationToken: token).SuppressCancellationThrow();
        }

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

        public static void CreateLoop(PlayerLoopTiming timing, Action action, ref Routine state)
        {
            state.Switch(LoopAsync(timing, action));
        }
        public static void CreateLoop(this Action action, PlayerLoopTiming timing, ref Routine state)
        {
            state.Switch(LoopAsync(timing, action));
        }

        static async UniTask LoopAsync(PlayerLoopTiming timing, Action action, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await UniTask.Yield(timing, token);
                action.Invoke();
            }
        }
        static async UniTask LoopAsync(PlayerLoopTiming timing, Action action)
        {
            while (true)
            {
                await UniTask.Yield(timing);
                action.Invoke();
            }
        }

        public static async UniTask DelayAsync(float seconds, Action completed, CancellationToken token, PlayerLoopTiming timing)
        {
            var elapsed = 0f;

            while (elapsed < seconds)
            {
                await UniTask.Yield(timing, token);
                elapsed += timing.GetDeltaTime();
            }

            completed?.Invoke();
        }
        public static void DelayCall(Action delayedAction)
        {
#if UNITY_EDITOR
            EditorApplication.delayCall += () =>
            {
#endif

                delayedAction();

#if UNITY_EDITOR

            };
#endif
        }
        public static T DelayCall<T>(Func<T> func)
        {

            T value = default;
            bool flag = false;

#if UNITY_EDITOR
            EditorApplication.delayCall += () =>
            {
#endif

                value = func();

#if UNITY_EDITOR
            };
#endif

            if (!Application.isEditor)
            {
                return value = func();
            }

            return flag ? value : func();
        }
        public static Routine Delay(this Action action, float seconds)
        {
            return new Routine().Switch(action.DelayAsync(seconds));
        }
        public static async UniTask DelayAsync(this Action completed, float seconds, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            var elapsed = 0f;

            while (elapsed < seconds)
            {
                await UniTask.Yield(timing);
                elapsed += timing.GetDeltaTime();
            }

            completed?.Invoke();
        }
        public static async UniTask Fade(float rangeStart, float rangeEnd, float value, float duration, Action<float> setValue, CancellationToken token)
        {
            var elapsed = Mathf.InverseLerp(rangeStart, rangeEnd, value);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                setValue(Mathf.Lerp(rangeStart, rangeEnd, elapsed / duration));
                await UniTask.Yield(token);
            }
        }
        public static async UniTask Then(this UniTask task, Action action, CancellationToken token)
        {
            await UniTask.WaitUntil(() => task.Status.IsCompleted(), cancellationToken: token);
            action.Invoke();
        }
        public static async UniTask Then(this UniTask task, Action action)
        {
            await UniTask.WaitUntil(() => task.Status.IsCompleted());
            action.Invoke();
        }
        public static async UniTaskVoid LazyUpdate(PlayerLoopTiming timing, int frameSkip, CancellationToken token, Action action)
        {
            while (!token.IsCancellationRequested)
            {
                action?.Invoke();

                for (int i = 0; i < frameSkip; i++)
                    await UniTask.Yield(timing, token);
            }
        }
        public static async UniTaskVoid LazyUpdate(PlayerLoopTiming timing, float delay, bool ignoreTimeScale, CancellationToken token, Action action)
        {
            int ms = delay.ToMilliseconds();
            while (!token.IsCancellationRequested)
            {
                action?.Invoke();
                await UniTask.Delay(ms, ignoreTimeScale, timing, token);
            }
        }
        public static async UniTaskVoid InputBuffer(float duration, Func<bool> canAct, Action action, CancellationToken token)
        {
            var elapsed = 0f;

            while (elapsed < duration)
            {
                if (canAct())
                {
                    action();
                    return;
                }

                elapsed += Time.fixedDeltaTime;
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
            }
        }
        public static UniTask EmptyTask() => UniTask.CompletedTask;
        public static async UniTask Frame() => await UniTask.Yield();
        public static async UniTask DelayOrCondition(TimeSpan duration, Func<bool> condition, CancellationToken token)
        {
            var elapsed = 0f;
            var ignoreTimeScale = false;
            var timing = PlayerLoopTiming.Update;
            Func<bool> cancel = () => token.IsCancellationRequested || elapsed >= (float)duration.TotalSeconds || condition();
            while (!cancel())
            {
                await UniTask.Yield(timing, token);
                elapsed += timing.GetDeltaTime(ignoreTimeScale);
            }
        }
        public static UniTask.Awaiter GetAwaiter(this CancellationToken token)
        {
            return token.GetAwaiter();
        }
        public static async UniTask WhileActiveAsync(this GameObject g)
        {
            await UniTask.WaitWhile(() => g.activeInHierarchy);
        }
        public static async UniTask StartQueueAsync(ConcurrentQueue<UniTask> queue, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                while (queue.TryDequeue(out var task))
                {
                    await task;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        public static async UniTask StartQueueAsync(Queue<UniTask> queue, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await UniTask.Yield(token);
                await UniTask.WaitUntil(() => queue.Count > 0, PlayerLoopTiming.Update, token);

                await queue.Peek().AttachExternalCancellation(token);
                var x = queue.Dequeue();
            }
        }
        public static async UniTask WithTimeout(this UniTask task, float seconds, bool ignoreTimeScale = true)
        {
            var cts = new CancellationTokenSource();

            try
            {
                await UniTask.WhenAny(
                    task.AttachExternalCancellation(cts.Token),
                    UniTask.Delay(TimeSpan.FromSeconds(seconds), ignoreTimeScale, cancellationToken: cts.Token)
                    );
            }
            finally
            {
                cts.Kill();
            }
        }
        public static Routine ExecuteAfter(this Action action, float seconds)
        {
            async UniTask DelayAsync()
            {
                await UniTask.Delay(TimeSpan.FromSeconds(seconds));
                action?.Invoke();
            }
            return new Routine().Switch(DelayAsync());
        }
        public static Routine ExecuteAfter<T>(this Action<T> action, float seconds, T data)
        {
            async UniTask DelayAsync()
            {
                await UniTask.Delay(TimeSpan.FromSeconds(seconds));
                action?.Invoke(data);
            }
            return new Routine().Switch(DelayAsync());
        }


        public static void EveryFrame(this CancellationToken token, MonoBehaviour source, Action onUpdate, Func<bool> until = null, Action onCancel = null, Action onCancelOrComplete = null, Action onComplete = null)
        {
            EveryFrameAsync(token, source, onUpdate, until, onCancel, onCancelOrComplete, onComplete).Forget();
        }
        public static async UniTask EveryFrameAsync(this CancellationToken token, MonoBehaviour source, Action onUpdate, Func<bool> until = null, Action onCancel = null, Action onCancelOrComplete = null, Action onComplete = null)
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


        public static async UniTask<bool> NextFrame(this MonoBehaviour mono, CancellationToken token)
        {
            try
            {
                await UniTask.WaitForEndOfFrame(mono, token);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        public static async UniTask LerpAsync(float duration, PlayerLoopTiming loop, CancellationToken token, Action<float> Set)
        {
            for (var elapsed = 0f; elapsed < duration;)
            {
                await UniTask.Yield(loop, token);
                elapsed += loop.GetDeltaTime();
                Set(Mathf.Clamp01(elapsed / duration));

                if (elapsed >= duration)
                    return;
            }
        }

        public static async UniTask LerpAsync(float startTime, float duration, PlayerLoopTiming loop, bool ignoreTimeScale, CancellationToken token, Action<float> Set)
        {
            for (var elapsed = startTime; elapsed < duration;)
            {
                await UniTask.Yield(loop, token);
                elapsed += loop.GetDeltaTime(ignoreTimeScale);
                Set(Mathf.Clamp01(elapsed / duration));

                if (elapsed >= duration)
                    return;
            }
        }
        public static async UniTask LerpAsync(float duration, PlayerLoopTiming loop, bool ignoreTimeScale, CancellationToken token, Action<float> Set)
        {
            for (var elapsed = 0f; elapsed < duration;)
            {
                await UniTask.Yield(loop, token);
                elapsed += loop.GetDeltaTime(ignoreTimeScale);
                Set(Mathf.Clamp01(elapsed / duration));

                if (elapsed >= duration)
                    return;
            }
        }

        public static async UniTask<bool> Yield(this CancellationToken token, PlayerLoopTiming loop = PlayerLoopTiming.Update)
        {
            try
            {
                await UniTask.Yield(loop, token);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }
    }
}