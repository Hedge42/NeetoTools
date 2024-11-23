using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace Neeto
{
    public struct Routine
    {
        public static implicit operator Routine(Func<CancellationToken, UniTask> t) => new(t);
        public static implicit operator Func<CancellationToken, UniTask>(Routine r) => r.func;

        Token token;
        Func<CancellationToken, UniTask> func;

        public Routine(Func<CancellationToken, UniTask> func)
        {
            this.token = Token.Create();
            this.func = func;
        }
        public Routine Switch(Func<CancellationToken, UniTask> func)
        {
            this.func = func;
            return this;
        }
        public void Pause() => token.Disable();
        public Routine Resume()
        {
            func(++token).Forget();
            return this;
        }
        public UniTask.Awaiter GetAwaiter() => func(++token).GetAwaiter();
    }
}