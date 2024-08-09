using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using Cysharp.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Neeto
{
    [ExecuteInEditMode]
    public class LoadingSpriteIcon : MonoBehaviour
    {
        public new SpriteRenderer renderer;
        [Min(0f)]
        public float fadeDuration;

        public float alpha
        {
            get => renderer.color.a;
            set => renderer.color = renderer.color.WithA(value);
        }
        NTask state;

        private void OnEnable()
        {
            Enable(true);
        }
        private void OnDisable()
        {
            Enable(false);
        }
        private void OnDestroy()
        {
            state.Kill();
        }
        public void Enable(bool enabled)
        {
            state %= EnableAsync(enabled);
        }
        async UniTask EnableAsync(bool enabled)
        {
            var elapsed = 0f;
            var end = enabled ? 1f : 0f;
            var start = alpha;
            do
            {
                await UniTask.Yield();
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / fadeDuration);
                alpha = Mathf.Lerp(start, end, t);
            }
            while (elapsed < fadeDuration);
        }
    }
}