using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Neeto
{
    public class Interpolator : MonoBehaviour
    {
        [SerializeReference, Polymorphic, ReorderableList]
        public IInterpolator[] interpolators = new[] { new MaterialColorInterpolator() };

        public void Interpolate(float t)
        {
            if (!enabled)
                return;

            foreach (var i in interpolators)
                i.Interpolate(t);
        }
    }

    public interface IInterpolator
    {
        void Interpolate(float t);
    }
    [Serializable]
    public class PostExposureInterpolator : IInterpolator
    {
        public bool enabled = true;
        public Volume Volume;
        public float a = 0f;
        public float b = 1f;

        ColorAdjustments adj;

        public void Interpolate(float t)
        {
            if (!enabled) return;
            if (!adj) enabled = Volume.profile.TryGet(out adj);
            if (!enabled) return;

            adj.postExposure.value = Mathf.Lerp(a, b, t);
        }
    }
    [Serializable]
    public class FloatEventInterpolator : IInterpolator
    {
        public bool enabled = true;
        public float a = 0f;
        public float b = 1f;
        public UnityEvent<float> output;

        public void Interpolate(float t)
        {
            if (enabled)
                output?.Invoke(Mathf.Lerp(a, b, t));
        }
    }
    [Serializable]
    public class Vector2EventInterpolator : IInterpolator
    {
        public bool enabled = true;
        public Vector2 a = Vector2.right;
        public Vector2 b = Vector2.up;
        public UnityEvent<Vector2> output;

        public void Interpolate(float t)
        {
            if (enabled)
                output?.Invoke(Vector2.Lerp(a, b, t));
        }
    }
    [Serializable]
    public class Vector3EventInterpolator : IInterpolator
    {
        public bool enabled = true;
        public Vector3 a = Vector3.right;
        public Vector3 b = Vector3.up;
        public UnityEvent<Vector3> output;

        public void Interpolate(float t)
        {
            if (enabled)
                output?.Invoke(Vector3.Lerp(a, b, t));
        }
    }
    [Serializable]
    public class ColorEventInterpolator : IInterpolator
    {
        public bool enabled = true;
        public Color a = Color.black;
        public Color b = Color.white;
        public UnityEvent<Color> output;

        public void Interpolate(float t)
        {
            if (enabled)
                output?.Invoke(Color.Lerp(a, b, t));
        }
    }
    [Serializable]
    public class MaterialColorInterpolator : IInterpolator
    {
        public bool enabled = true;
        public Material material;
        public string propertyName = "_Color";
        public Color a = Color.black;
        public Color b = Color.white;

        public void Interpolate(float t)
        {
            if (enabled)
                material.SetColor(propertyName, Color.Lerp(a, b, t));
        }
    }
    [Serializable]
    public class MaterialFloatInterpolator : IInterpolator
    {
        public bool enabled = true;
        public Material material;
        public string propertyName = "_Intensity";
        public float a = 0;
        public float b = 1;

        public void Interpolate(float t)
        {
            if (enabled)
                material.SetFloat(propertyName, Mathf.Lerp(a, b, t));
        }
    }
}