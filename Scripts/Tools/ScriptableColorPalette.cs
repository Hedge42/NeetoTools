using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Neeto;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = MenuPath.Neeto + nameof(ScriptableColorPalette))]
public class ScriptableColorPalette : ScriptableObject
{
    [SerializeReference, Polymorphic] public IColor[] colors;
}

public interface IColor
{
    public Color color { get; }
}
[Serializable]
public class ColorReference : IColor
{
    [field: SerializeField] public Color color { get; set; } = Color.gray;
}
[Serializable]
public class HDRColorReference : IColor
{
    [field: SerializeField, ColorUsage(false, true)]
    public Color color { get; set; } = Color.gray;
}
