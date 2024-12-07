using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using System;
using UnityEngine;
using UnityEngine.VFX;



[Serializable]
public abstract class ShaderValue<T> : IShaderValue, ISerializationCallbackReceiver
{
    public static implicit operator T(ShaderValue<T> arg) => arg.value;
    public string name;
    [Disabled] public int id;
    public T value;

    public void Apply(VisualEffect _)
    {
        switch (value)
        {
            case float value:
                _.SetFloat(id, value);
                break;
            case int value:
                _.SetInt(id, value);
                break;
            case bool value:
                _.SetBool(id, value);
                break;
            case AnimationCurve value:
                _.SetAnimationCurve(id, value);
                break;
            case Gradient value:
                _.SetGradient(id, value);
                break;
            case Mesh value:
                _.SetMesh(id, value);
                break;
            case Vector2 value:
                _.SetVector2(id, value);
                break;
            case Vector3 value:
                _.SetVector3(id, value);
                break;
            case Vector4 value:
                _.SetVector4(id, value);
                break;
            case Texture2D value:
                _.SetTexture(id, value);
                break;

            default:
                throw new NotSupportedException(typeof(T).Name);
        }
    }
    public void Apply(Material _)
    {
        switch (value)
        {
            case float value:
                _.SetFloat(id, value);
                break;
            case bool value:
                _.SetInt(id, value ? 1 : 0);
                break;
            case int value:
                _.SetInt(id, value);
                break;
            case Vector4 value:
                _.SetVector(id, value);
                break;
            case Color value:
                _.SetColor(id, value);
                break;
            case Texture2D value:
                _.SetTexture(id, value);
                break;

            default:
                throw new NotSupportedException(typeof(T).Name);
        }
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize() => id = Shader.PropertyToID(name);
    void ISerializationCallbackReceiver.OnBeforeSerialize() { }
}

public interface IShaderValue
{
    void Apply(Material _);
    void Apply(VisualEffect _);
}

public static class ShaderValueExtension
{
    public static void Apply(this VisualEffect _, IShaderValue value) => value.Apply(_);
    public static void Apply(this Material _, IShaderValue value) => value.Apply(_);
}


[Serializable] public class ShaderBool : ShaderValue<int> { public bool Value { get => value == 1; set => base.value = value ? 1 : 0; } }
[Serializable] public class ShaderGradient : ShaderValue<Gradient> { }
[Serializable] public class ShaderFloat : ShaderValue<float> { }
[Serializable] public class ShaderInt : ShaderValue<int> { }
[Serializable] public class ShaderColor : ShaderValue<Color> { }
[Serializable] public class ShaderVector2 : ShaderValue<Vector2> { }
[Serializable] public class ShaderVector3 : ShaderValue<Vector3> { }
[Serializable] public class ShaderTexture : ShaderValue<Texture2D> { }

