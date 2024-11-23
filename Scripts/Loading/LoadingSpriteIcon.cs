using UnityEngine;
using Cysharp.Threading.Tasks;

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
        Routine state;

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