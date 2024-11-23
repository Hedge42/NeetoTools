using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace Neeto
{
    public class Tween
    {
        public Func<float> get;
        public Action<float> set;
        public Func<float, bool> exitCondition;
        public PlayerLoopTiming timing = PlayerLoopTiming.Update;

        public static void Void(Func<float> get, Action<float> set, Func<float, bool> exitCondition, CancellationToken token, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            UniTask.Void(async () =>
            {
                var t = get();
                do
                {
                    await UniTask.Yield(timing, token, true);
                    set(t = get());
                }
                while (exitCondition(t));
            });
        }
    }
}