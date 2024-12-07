using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.VFX;

[Serializable]
public abstract class ShaderAnimation<T> : ShaderValue<T>, IShaderAnimation
{
    [Range(0, 1)]
    public float t;
    public T start;
    public T end;
    [Min(.01f)]
    public float duration = 1f;

    public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

    Token token;

    public void Start(Material m) => Loop(m).Fire(++token);
    public void Start(VisualEffect vfx) => Loop(vfx).Fire(++token);
    public void Stop() => token.Cancel();

    async UniTask Loop(Material mat)
    {
        var elapsed = 0f;

        while (elapsed < duration)
        {
            t = Mathf.Clamp01(elapsed / duration);
            t = curve.Evaluate(t);
            value = Lerp(start, end, t);
            Apply(mat);
            await UniTask.Yield();
            elapsed += Time.deltaTime;
        }
    }
    async UniTask Loop(VisualEffect vfx)
    {
        var elapsed = 0f;

        while (elapsed < duration)
        {
            t = Mathf.Clamp01(elapsed / duration);
            t = curve.Evaluate(t);
            value = Lerp(start, end, t);
            Apply(vfx);
            await UniTask.Yield();
            elapsed += Time.deltaTime;
        }
    }

    public abstract T Lerp(T start, T end, float t);
}

public interface IShaderAnimation
{
    public void Start(Material m);
    public void Start(VisualEffect f);
    public void Stop();
}

[Serializable]
public class ShaderFloatAnimation : ShaderAnimation<float>
{
    public override float Lerp(float start, float end, float t) => Mathf.Lerp(start, end, t);
}

[Serializable]
public class ShaderColorAnimation : ShaderAnimation<Color>
{
    public override Color Lerp(Color start, Color end, float t) => Color.Lerp(start, end, t);
}

