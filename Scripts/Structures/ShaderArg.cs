using Neeto;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.VFX;
using static System.Convert;
using System.Threading;

namespace Neeto
{
    public interface IShaderArg
    {
        public void Apply(Material _);
        public void Apply(VisualEffect _);
    }
    public interface IShaderAnimation
    {
        public void Start(Material m);
        public void Start(VisualEffect f);
        public void Stop();
    }

    [Serializable]
    public abstract class ShaderArg : IShaderArg, ISerializationCallbackReceiver
    {
        public string name;
        [Disabled] public int id;

        public abstract void Apply(Material _);
        public abstract void Apply(VisualEffect _);

        public void OnAfterDeserialize() => id = Shader.PropertyToID(name);
        public void OnBeforeSerialize() { }
    }
    [Serializable]
    public class ShaderArg<T> : ShaderArg
    {
        public static implicit operator T(ShaderArg<T> arg) => arg.value;

        public T value;

        public override void Apply(Material _) => _.Apply(this);
        public override void Apply(VisualEffect _) => _.Apply(this);
    }
    public static class ShaderArgumentExtensions
    {
        public static void Apply<T>(this VisualEffect _, ShaderArg<T> arg)
        {
            // questionable...
            switch (typeof(T).Name)
            {
                case nameof(Single):
                    _.SetFloat(arg.id, ToSingle(arg.value));
                    break;
                //case nameof(Double):
                //    _.SetBool(arg.id, ToBoolean(arg.value));
                //    break;
                case nameof(Int32):
                    _.SetInt(arg.id, ToInt32(arg.value));
                    break;
                case nameof(Vector2):
                    _.SetVector3(arg.id, (Vector2)ChangeType(arg.value, typeof(Vector2)));
                    break;
                case nameof(Vector3):
                    _.SetVector3(arg.id, (Vector3)ChangeType(arg.value, typeof(Vector3)));
                    break;
                case nameof(Color):
                case nameof(Quaternion):
                case nameof(Vector4):
                    _.SetVector4(arg.id, (Vector4)ChangeType(arg.value, typeof(Vector4)));
                    break;
                case nameof(AnimationCurve):
                    _.SetAnimationCurve(arg.id, arg.value as AnimationCurve);
                    break;
                case nameof(Texture2D):
                    _.SetTexture(arg.id, arg.value as Texture2D);
                    break;
                default:
                    ThrowArgument(typeof(T));
                    break;
            }
        }
        public static void Apply<T>(this Material _, ShaderArg<T> arg)
        {
            switch (typeof(T).Name)
            {
                case nameof(Single):
                    _.SetFloat(arg.id, ToSingle(arg.value));
                    break;
                case nameof(Int32):
                    _.SetInt(arg.id, ToInt32(arg.value));
                    break;
                case nameof(Vector2):
                case nameof(Vector3):
                    _.SetVector(arg.id, (Vector3)ChangeType(arg.value, typeof(Vector3)));
                    break;
                case nameof(Vector4):
                case nameof(Color):
                case nameof(Quaternion):
                    _.SetVector(arg.id, (Vector4)(object)arg.value);
                    break;
                case nameof(Texture2D):
                    _.SetTexture(arg.id, arg.value as Texture2D);
                    break;
                default:
                    ThrowArgument(typeof(T));
                    break;
            }
        }

        static void ThrowArgument(Type type)
        {
            throw new System.NotImplementedException($"Support for type {type} not implemented");
        }
    }


    [Serializable]
    public abstract class ShaderAnimation<T> : ShaderArg<T>, IShaderAnimation
    {
        [Range(0, 1)]
        public float t;
        public T start;
        public T end;
        [Min(.01f)]
        public float duration = 1f;
        public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

        Token token;

        public void Start(Material m) => Loop(m, ++token).Forget();
        public void Start(VisualEffect vfx) => Loop(vfx, ++token).Forget();
        public void Stop() => token.Cancel();

        async UniTask Loop(Material mat, CancellationToken token)
        {
            var elapsed = 0f;

            while (elapsed < duration)
            {
                await UniTask.Yield(token);
                elapsed += Time.deltaTime;
                t = Mathf.Clamp01(elapsed / duration);
                t = curve.Evaluate(t);
                value = Lerp(start, end, t);
                Apply(mat);
            }
        }
        async UniTask Loop(VisualEffect vfx, CancellationToken token)
        {
            var elapsed = 0f;

            while (elapsed < duration)
            {
                await UniTask.Yield(token);
                elapsed += Time.deltaTime;
                t = Mathf.Clamp01(elapsed / duration);
                t = curve.Evaluate(t);
                value = Lerp(start, end, t);
                Apply(vfx);
            }
        }

        public abstract T Lerp(T start, T end, float t);
    }
    [Serializable]
    public class ShaderFloatAnimation : ShaderAnimation<float>
    {
        public override float Lerp(float start, float end, float t) => Mathf.Lerp(start, end, t);
    }
    [Serializable]
    public class ShaderBool : ShaderArg<int>
    {
        public bool Value
        {
            get => value == 1;
            set => base.value = value ? 1 : 0;
        }
    }
    [Serializable] public class ShaderFloat : ShaderArg<float> { }
    [Serializable] public class ShaderInt : ShaderArg<int> { }
    [Serializable] public class ShaderColor : ShaderArg<Color> { }
    [Serializable] public class ShaderVector2 : ShaderArg<Vector2> { }
    [Serializable] public class ShaderVector3 : ShaderArg<Vector3> { }
    [Serializable] public class ShaderTexture : ShaderArg<Texture2D> { }
    [Serializable]
    public class ShaderColorAnimation : ShaderAnimation<Color>
    {
        public override Color Lerp(Color start, Color end, float t) => Color.Lerp(start, end, t);
    }

}