using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Neeto
{
    public struct Delay
    {
        public static async UniTask Seconds(float seconds, CancellationToken token, PlayerLoopTiming timing = PlayerLoopTiming.Update, bool ignoreTimescale = false)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(seconds), ignoreTimescale, timing, token, true);
        }
        public static async UniTask Frame(MonoBehaviour mono, CancellationToken token)
        {
            await UniTask.WaitForEndOfFrame(mono, token, true);
        }
        public static void Update(Action action, CancellationToken token)
        {
            UniTask.Void(async () =>
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token, true);
                action();
            });
        }
        public static async UniTask Update(CancellationToken token)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, token, true);
        }
        public static void Editor(Action action)
        {
#if UNITY_EDITOR
            EditorApplication.delayCall += () =>
            {
#endif

                action();

#if UNITY_EDITOR

            };
#endif
        }
        public static async UniTask Editor(Action action, CancellationToken token)
        {
            var flag = false;
            Editor(() => flag = true);
            await UniTask.WaitUntil(() => flag, PlayerLoopTiming.Update, token, true);
            action();
        }
    }
}