using System;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Neeto
{
    /// <summary>
    /// automatically cancel a routine when a new one is started
    /// </summary>
    public struct Routine
    {
        public UniTask task;
        public Token token { get; private set; }
        public UniTaskStatus status => task.Status;

        public event Action stopped;

        public void Kill() => token--;

        void Stop()
        {
            Kill();
            stopped?.Invoke();
        }
        public void Refresh() => token++;



        public Routine Switch(params UniTask[] tasks)
        {
            task = UniTask.WhenAll(tasks)
                .AttachExternalCancellation(++token)
                .AttachExternalCancellation(Token.global)
                .ContinueWith(Stop)
                .Preserve();

            task.Forget();
            return this;
        }
        public Routine Switch(Func<CancellationToken, UniTask> GetTask)
        {
            var task = GetTask(++token)
                .AttachExternalCancellation(Token.global)
                .ContinueWith(Stop)
                .Preserve();

            task.Forget();
            return this;
        }
        public Routine Switch(UniTask _task, Action __onComplete)
        {
            task = _task
                .AttachExternalCancellation(++token)
                .AttachExternalCancellation(Token.global)
                .ContinueWith(__onComplete)
                .ContinueWith(Stop)
                .Preserve();

            task.Forget();
            return this;
        }
        public Routine Switch(UniTask _task, CancellationToken _token)
        {
            task = _task
                .AttachExternalCancellation(++token)
                .AttachExternalCancellation(Token.global)
                .AttachExternalCancellation(_token)
                .ContinueWith(Stop)
                .Preserve();

            task.Forget();
            return this;
        }




        public static Routine Run(params UniTask[] tasks)
        {
            return new Routine().Switch(tasks);
        }
        public async UniTask SwitchAsync(params UniTask[] tasks)
        {
            //cts = cts.Refresh();
            await (task = Prepare(tasks));
        }
        UniTask Prepare(params UniTask[] tasks)
        {
            return UniTask.WhenAll(tasks)
                .AttachExternalCancellation(++token)
                .AttachExternalCancellation(Token.global)
                .ContinueWith(Stop)
                .Preserve();
        }
        public async UniTask Wait()
        {
            await task;
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

        public static Routine operator %(Routine state, UniTask task) => state.Switch(task);
        public static Routine operator +(Routine state, UniTask task)
        {
            task.AttachExternalCancellation(state.token)
                .Preserve()
                .Forget();
            return state;
        }
    }
}