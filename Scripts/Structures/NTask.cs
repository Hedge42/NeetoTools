using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using System.Collections.Concurrent;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
#endif

namespace Neeto
{
    /// <summary>
    /// automatically cancel a routine when a new one is started
    /// </summary>
    public struct NTask
    {
        public UniTask task;
        public CancellationTokenSource cts;
        public CancellationToken token => cts.Token;
        public UniTaskStatus status => task.Status;

        public event Action stopped;

        public void Kill()
        {
            cts.Kill();
            cts = null;
        }
        void Stop()
        {
            Kill();
            stopped?.Invoke();
        }
        public void Refresh()
        {
            cts = cts.Refresh();
        }



        public NTask Switch(params UniTask[] tasks)
        {
            cts = cts.Refresh();

            task = UniTask.WhenAll(tasks)
                .AttachExternalCancellation(cts.Token)
                .AttachExternalCancellation(MCancel.global)
                .ContinueWith(Stop)
                .Preserve();

            task.Forget();
            return this;
        }
        public NTask Switch(Func<CancellationToken, UniTask> GetTask)
        {
            cts = cts.Refresh();

            var task = GetTask(cts.Token)
                .AttachExternalCancellation(MCancel.global)
                .ContinueWith(Stop)
                .Preserve();

            task.Forget();
            return this;
        }
        public NTask Switch(UniTask _task, Action __onComplete)
        {
            cts = cts.Refresh();

            task = _task
                .AttachExternalCancellation(cts.Token)
                .AttachExternalCancellation(MCancel.global)
                .ContinueWith(__onComplete)
                .ContinueWith(Stop)
                .Preserve();

            task.Forget();
            return this;
        }
        public NTask Switch(UniTask _task, CancellationToken _token)
        {
            cts = cts.Refresh();

            task = _task
                .AttachExternalCancellation(cts.Token)
                .AttachExternalCancellation(MCancel.global)
                .AttachExternalCancellation(_token)
                .ContinueWith(Stop)
                .Preserve();

            task.Forget();
            return this;
        }




        public NTask Repeat(params UniTask[] tasks)
        {
            cts = cts.Refresh();
            task = UniTask.WhenAll(tasks)
                .AttachExternalCancellation(cts.Token)
                .AttachExternalCancellation(MCancel.global)
                .Preserve();

            RepeatAsync(task).Forget();
            return this;
        }

        public static NTask Run(params UniTask[] tasks)
        {
            return new NTask().Switch(tasks);
        }

        public async UniTask RepeatAsync(UniTask task)
        {
            /* redundant condition but feels safer than while(true)
             */

            //cts = cts.Refresh();
            //task = UniTask.WhenAll(tasks)
            //    .AttachExternalCancellation(cts.Token)
            //    .AttachExternalCancellation(AsyncUtils.GlobalToken)
            //    .Preserve();

            //try
            //{
            while (!token.IsCancellationRequested)
            {
                await task;
            }
            //}
            //catch
            //{
            //    stopped?.Invoke();
            //}
        }
        public async UniTask SwitchAsync(params UniTask[] tasks)
        {
            //cts = cts.Refresh();
            await (task = Prepare(tasks));
        }
        UniTask Prepare(params UniTask[] tasks)
        {
            cts = cts.Refresh();
            return UniTask.WhenAll(tasks)
                .AttachExternalCancellation(cts.Token)
                .AttachExternalCancellation(MCancel.global)
                .ContinueWith(Stop)
                .Preserve();
        }
        public async UniTask Wait()
        {
            await task;
        }
        public void Add(UniTask task)
        {
            // don't cancel state if it's running
            bool cold = cts == null;
            if (cold)
            {
                cts = cts.Refresh();
                task = task.ContinueWith(Stop);
            }

            // compose and fire
            task = task.AttachExternalCancellation(cts.Token);
            task = task.AttachExternalCancellation(MCancel.global);
            task = task.Preserve();

            if (cold)
            {
                this.task = task;
            }

            task.Forget();
        }

        public async UniTask CompletedAsync(Action action = null)
        {
            await UniTask.WaitUntil(IsCompleted);
            action?.Invoke();
        }
        public bool IsCompleted()
        {
            return task.GetAwaiter().IsCompleted;
        }
        public void OnComplete(UniTaskStatus status, Action action)
        {
            StatusAsync().ContinueWith(_status =>
            {
                if (_status == status)
                    action();
            }).Forget();
        }
        private async UniTask<UniTaskStatus> StatusAsync()
        {
            await SafeCompletionAsync();
            return task.Status;
        }
        public async UniTask SafeCompletionAsync()
        {
            while (!task.GetAwaiter().IsCompleted)
                await UniTask.Yield();
        }

        public static NTask operator %(NTask state, UniTask task) => state.Switch(task);
        public static NTask operator +(NTask state, UniTask task)
        {
            task.AttachExternalCancellation(state.token)
                .Preserve()
                .Forget();
            return state;
        }
    }

    public static class MCancel
    {
        static CancellationTokenSource _global;
        public static CancellationTokenSource globalSource
        {
            get
            {
                if (!isSetup)
                    Setup();
                return _global;
            }
        }

        static CancellationTokenSource _scene;
        public static CancellationTokenSource sceneSource
        {
            get
            {
                if (!isSetup)
                    Setup();
                return _scene;
            }
        }

        public static CancellationToken scene => sceneSource.Token;
        public static CancellationToken global => globalSource.Token;

        static bool isSetup;

        static void Setup()
        {
            _global = _global.Refresh();
            _scene = _scene.Refresh();
            isSetup = true;
        }

        [RuntimeInitializeOnLoadMethod]
        static void OK()
        {
            AppHelper.onQuit += CancelGlobal;
            SceneManager.activeSceneChanged += RefreshSceneToken;
        }
        static void CancelGlobal()
        {
            globalSource.Kill();
            sceneSource.Kill();
            SceneManager.activeSceneChanged -= RefreshSceneToken;
            isSetup = false;
        }
        static void RefreshSceneToken(Scene from, Scene to)
        {
            _scene = _scene.Refresh();
        }

        public static CancellationToken GetGlobalToken()
        {
            return globalSource.Token;
        }

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

        public static void CreateLoop(PlayerLoopTiming timing, Action action, ref NTask state)
        {
            state.Switch(LoopAsync(timing, action));
        }
        public static void CreateLoop(this Action action, PlayerLoopTiming timing, ref NTask state)
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
    }


    public struct AsyncRunner
    {
        public UniTask task;
        public CancellationTokenSource cts;
        public CancellationToken token => cts.Token;
        public UniTaskStatus status => task.Status;

        public void Kill()
        {
            cts.Kill();
        }
        public void Run(UniTask _task, Component component = null)
        {
            cts ??= cts.Refresh();

            //this.cts = cts.Refresh();
            if (component) cts.RegisterRaiseCancelOnDestroy(component);

            task = _task.AttachExternalCancellation(cts.Token);
            task = task.AttachExternalCancellation(MCancel.global);
            task = task.Preserve();
            task.Forget();
        }
        public void OnComplete(UniTaskStatus status, Action action)
        {
            AwaitStatus().ContinueWith(_status =>
            {
                if (_status == status)
                    action();
            }).Forget();
        }
        private async UniTask<UniTaskStatus> AwaitStatus()
        {
            await task;
            return task.Status;
        }
    }

    public struct TaskAggregator
    {
        public UniTask task;

        public TaskAggregator Add(UniTask task) => this += task;
        public static TaskAggregator operator +(TaskAggregator tr, UniTask _task)
        {
            tr.task = UniTask.WhenAll(tr.task, _task).Preserve();
            return tr;
        }
        public UniTask.Awaiter GetAwaiter()
        {
            return task.GetAwaiter();
        }
    }

    public struct TaskCombiner
    {
        public UniTask task;
        public UniTaskStatus status => task.Status;

        private CancellationTokenSource cts;
        public CancellationToken token => (cts ??= new CancellationTokenSource()).Token;

        public void Cancel()
        {
            //cts = cts.Refresh();

            cts.Kill();
        }

        public void AddTokens(params CancellationToken[] tokens)
        {
            foreach (var t in tokens)
            {
                t.Register(Cancel);
                task = task.AttachExternalCancellation(t);
            }
        }

        public static TaskCombiner Create(IEnumerable<UniTask> tasks)
        {


            var result = new TaskCombiner();
            tasks = tasks.Select(t => t.AttachExternalCancellation(result.token));
            result.task = UniTask.WhenAll(tasks).Preserve();
            return result;
        }

        public static TaskCombiner Combine(params UniTask[] tasks)
        {
            return Combine(tasks.AsEnumerable());
        }
        public static TaskCombiner Combine(IEnumerable<UniTask> tasks)
        {
            var result = new TaskCombiner();

            var arr = tasks.ToArray();

            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = arr[i].AttachExternalCancellation(result.token);
            }

            result.task = UniTask.WhenAll(arr);

            return result;
        }


        public static TaskCombiner operator +(TaskCombiner tr, UniTask _task)
        {
            _task = _task.AttachExternalCancellation(tr.token);
            tr.task = UniTask.WhenAll(tr.task, _task).Preserve();
            return tr;
        }
    }
}