using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace Neeto
{
    public struct Routine
    {
        public static implicit operator UniTask(Routine r) => r.StartAsync();
        public static implicit operator Routine(UniTask task) => Create(token => task.AttachExternalCancellation(token));
        public static implicit operator Routine(CancellationToken token) => new() { token = Token.Create(token) };
        public static implicit operator Routine(Func<CancellationToken, UniTask> t) => new() { func = t };
        public static implicit operator Func<CancellationToken, UniTask>(Routine r) => r.func;
        public static Routine operator %(Routine routine, Func<CancellationToken, UniTask> func) => routine.Switch(func);

        Token token;
        Func<CancellationToken, UniTask> func;


        public Routine(Func<CancellationToken, UniTask> _func)
        {
            func = _func;
            token = default;
        }
        public Routine(Func<CancellationToken, UniTask> _func, CancellationToken _token)
        {
            token = Token.Create(_token);
            func = _func;
        }

        public static Routine Create(CancellationToken token)
        {
            return new()
            {
                func = default,
                token = Token.Create(token)
            };
        }
        public static Routine Create(Func<CancellationToken, UniTask> _func) => _func;
        public static Routine Create(Func<CancellationToken, UniTask> _func, CancellationToken link) => new() { func = _func, token = link };

        public async UniTask SwitchAsync(Func<CancellationToken, UniTask> func)
        {
            this.func = func;
            await StartAsync();
        }
        public Routine Switch(Func<CancellationToken, UniTask> func, CancellationToken link)
        {
            this.func = func;
            token %= link;
            StartAsync().Forget();
            return this;
        }
        public Routine Switch(Func<CancellationToken, UniTask> func)
        {
            this.func = func;
            StartAsync().Forget();
            return this;
        }
        public async UniTask StartAsync(CancellationToken token)
        {
            this.token %= token;

            UnityEngine.Debug.Log($"ok");
            await StartAsync();
        }
        public UniTask StartAsync() => func(++token);
        public void Start() => StartAsync().Forget();
        public void Stop() => token.Disable();
        public UniTask.Awaiter GetAwaiter() => StartAsync().GetAwaiter();
    }
}