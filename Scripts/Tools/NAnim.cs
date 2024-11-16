using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Neeto
{
    public static class NAnim
    {
        public static async UniTaskVoid InterpolateAsync(Playable playable, float start, float end, CancellationToken token)
        {
            playable.Pause();
            while (playable.IsValid())
            {
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, token);
                var nextTime = (float)playable.GetTime() + Time.deltaTime * (float)playable.GetSpeed();
                playable.SetTime(Mathf.Clamp(nextTime, start, end));
            }
        }
        public static async UniTaskVoid DebugWeightsAsync(Playable mixer, CancellationToken token)
        {
            StringBuilder sb = new();
            while (!token.IsCancellationRequested)
            {
                await UniTask.Yield(token);
                Debug.Log(DebugWeights(mixer, sb));
            }
        }
        public static async UniTaskVoid DebugWeightsAsync(Func<Playable> getMixer, CancellationToken token)
        {
            StringBuilder sb = new();
            while (!token.IsCancellationRequested)
            {
                await UniTask.Yield(token);
                Debug.Log(DebugWeights(getMixer(), sb));
            }
        }
        public static string DebugWeights(Playable playable, StringBuilder sb = null)
        {
            sb ??= new();
            sb.Clear();
            sb.Append('[');
            var count = playable.GetInputCount();
            var sum = 0f;
            for (int i = 0; i < count; i++)
            {
                var w = playable.GetInputWeight(i);
                sum += w;
                sb.Append(w.ToString("f2"));
                if (i < count - 1)
                    sb.Append(',');
            }
            sb.Append($"]({sum:f2})");
            return sb.ToString();
        }

        public static void DisconnectAndDestroy(this Playable mixer, int input)
        {
            var count = mixer.GetInputCount();
            if (input >= count)
                return;

            var sub = mixer.GetInput(input);
            mixer.DisconnectInput(input);
            sub.DestroyRecursive();
            mixer.GetGraph().Evaluate();
        }
        public static void DestroyRecursive(this Playable playable)
        {
            if (!playable.IsValid())
                return;

            // destroy all sub-playables, starting at the leafs
            for (int i = 0; i < playable.GetInputCount(); i++)
                playable.GetInput(i).DestroyRecursive();

            // destroy the leaf playable
            playable.Destroy();
        }

#if UNITY_EDITOR
        public static void TPose(this GameObject gameObject, string search = "l:T-Pose")
        {
            // requires asset with 'T-Pose' label, first one found will be used
            //if (NGUI.LoadFirstAsset<AnimationClip>(search) is AnimationClip tPose)
            //{
            //    tPose.SampleAnimation(gameObject, 0f);
            //    EditorUtility.SetDirty(gameObject);
            //}
            //else
            //{
            //    Debug.LogError("No AnimationClip with label matching 'T-Pose'");
            //}

            gameObject.GetComponent<Animator>()?.TPose();
        }
        public static void TPose(this Animator animator)
        {
            animator.Rebind();
            animator.Update(0f); // Force immediate reset to T-pose or default pose
            EditorUtility.SetDirty(Selection.activeGameObject);
        }
#endif
    }
}
