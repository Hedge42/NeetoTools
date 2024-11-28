using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Neeto
{
    public static class TaskExtensions
    {
        #region AGGREGATION

        #endregion

        #region DELAY
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
        public static async UniTask DelayAsync(this Action completed, float seconds, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            var elapsed = 0f;

            while (elapsed < seconds)
            {
                await UniTask.Yield(timing);
                elapsed += Loop.GetDeltaTime(timing);
            }

            completed?.Invoke();
        }
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
        #endregion

        #region INTERPOLATION
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
        public static async UniTask LerpAsync(this CancellationToken token, float startTime, float duration, PlayerLoopTiming loop, bool ignoreTimeScale, Action<float> Set)
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
        public static async UniTask LerpAsync(this CancellationToken token, float duration, PlayerLoopTiming loop, bool ignoreTimeScale, Action<float> Set)
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
        #endregion

        #region EXTENSIONS
        public static async UniTask WithContinuation(this UniTask task, Action<UniTaskStatus> action)
        {
            task = task.Preserve();
            try
            {
                await task;
            }
            finally
            {
                action(task.Status);
            }
        }
        public static UniTask RunAsync(this IEnumerator routine, CancellationToken token)
        {
            return routine.ToUniTask(cancellationToken: token).SuppressCancellationThrow();
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
        #endregion

    }
}