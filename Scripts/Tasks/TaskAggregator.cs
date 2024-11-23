using Cysharp.Threading.Tasks;

namespace Neeto
{
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
}