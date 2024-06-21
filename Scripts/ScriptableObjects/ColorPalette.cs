using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Matchwork
{
    [CreateAssetMenu(menuName = MPath.Var + nameof(ColorPalette))]
    public class ColorPalette : ScriptableObject
    {
        public Color[] colors;

        public static readonly Color shadow = new Color(0, 0, 0, .15f);
    }

    public interface IColorPalette
    {
        Color[] colors { get; }
    }
    [Serializable]
    public class ColorPalette_ : IColorPalette
    {
        [field: SerializeField]
        public Color[] colors { get; set; } = new Color[]
        {
            Color.red,
            Color.green,
            Color.blue,
        };
    }
    [Serializable]
    public class ColorPaletteHDR : IColorPalette
    {
        [field: SerializeField, ColorUsage(false, true)]
        public Color[] colors { get; set; } = new Color[]
        {
            Color.red,
            Color.green,
            Color.blue,
        };
    }
}