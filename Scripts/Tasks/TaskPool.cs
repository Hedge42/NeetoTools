using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Neeto
{
    public class TaskPool
    {
        public UniTask task;
        public UniTaskStatus status => task.Status;

        Token token;

        List<Func<CancellationToken, UniTask>> tasks = new();

        public async UniTask AllAsync()
        {
            ++token;
            await UniTask.WhenAll(tasks.Select(t => t(token)));
        }
        public void Cancel() => token.Disable();

        public static TaskPool Create(params UniTask[] tasks)
        {
            var result = new TaskPool();
            result.tasks = new();
            foreach (var task in tasks)
            {
                result.tasks.Add(token => task.AttachExternalCancellation(token));
            }
            return result;
        }
        public static TaskPool operator +(TaskPool pool, UniTask _task)
        {
            pool.tasks.Add(token => _task.AttachExternalCancellation(token));
            return pool;
        }
        public static TaskPool operator +(TaskPool pool, Func<CancellationToken, UniTask> func)
        {
            pool.tasks.Add(func);
            return pool;
        }
    }

    public static class TaskPoolExtensions
    {
        public static UniTask Combine(params UniTask[] tasks)
        {
            return UniTask.WhenAll(tasks);
        }
        public static UniTask Combine(this UniTask task, UniTask other)
        {
            return UniTask.WhenAll(task, other);
        }
    }
}