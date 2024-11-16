using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace Neeto
{
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