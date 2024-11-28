using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

#if UNITY_EDITOR
#endif


namespace Neeto
{
    [ExecuteInEditMode]
    public class LoadingSpriteIcon : MonoBehaviour
    {
        public SpriteRenderer Renderer;
        [Min(0f)]
        public float fadeDuration;

        public float alpha
        {
            get => Renderer.color.a;
            set => Renderer.color = Renderer.color.With(a: value);
        }
        Token token;

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
            token.Cancel();
        }
        public void Enable(bool enabled)
        {
            EnableAsync(enabled, ++token).Forget();
        }
        async UniTask EnableAsync(bool enabled, CancellationToken token)
        {
            var elapsed = 0f;
            var end = enabled ? 1f : 0f;
            var start = alpha;
            do
            {
                await UniTask.Yield(token);
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / fadeDuration);
                alpha = Mathf.Lerp(start, end, t);
            }
            while (elapsed < fadeDuration);
        }
    }
}