using UnityEngine;
using System;

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

public static class NColor
{
    public static Color With(this Color src, float? r = null, float? g = null, float? b = null, float? a = null)
    {
        return new Color
        {
            r = r ?? src.r,
            g = g ?? src.g,
            b = b ?? src.b,
            a = a ?? src.a
        };
    }
    public static Color WithA(this Color color, float a)
    {
        color.a = a;
        return color;
    }
    public static Color WithR(this Color color, float r)
    {
        color.r = r;
        return color;
    }
    public static Color WithRGB(this Color color, float rgb)
    {
        color.r = color.g = color.b = rgb;
        return color;
    }
    public static Color WithG(this Color color, float g)
    {
        color.g = g;
        return color;
    }
    public static Color WithB(this Color color, float b)
    {
        color.b = b;
        return color;
    }
    public static Color WithH(this Color color, float value)
    {
        Color.RGBToHSV(color, out var h, out var s, out var v);
        return Color.HSVToRGB(value, s, v).WithA(color.a);
    }
    public static Color WithS(this Color color, float value)
    {
        Color.RGBToHSV(color, out var h, out var s, out var v);
        return Color.HSVToRGB(h, value, v).WithA(color.a);
    }
    public static Color WithV(this Color color, float value)
    {
        Color.RGBToHSV(color, out var h, out var s, out var v);
        return Color.HSVToRGB(h, s, value).WithA(color.a);
    }
    public static string AsHex(this Color color, bool rgba = false)
    {
        // thanks chatGPT ♥

        // Multiply each component by 255 as Color components are from 0 to 1.
        int red = Mathf.RoundToInt(color.r * 255);
        int green = Mathf.RoundToInt(color.g * 255);
        int blue = Mathf.RoundToInt(color.b * 255);
        int alpha = Mathf.RoundToInt(color.a * 255);

        // Convert the integers to hex strings and combine them.

        return rgba
            ? $"#{red:X1}{green:X1}{blue:X1}{alpha:X1}"
            : $"#{red:X1}{green:X1}{blue:X1}";
    }
}
