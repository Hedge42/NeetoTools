using UnityEngine;

namespace Neeto
{
    public static class NColor
    {
        /// <summary>RGB applied first, then HSV, then finally alpha</summary>
        public static Color With(this Color color, float? r = null, float? g = null, float? b = null, float? a = null, float? h = null, float? s = null, float? v = null)
        {
            var result = new Color(r ?? color.r, g ?? color.g, b ?? color.b);
            Color.RGBToHSV(result, out var H, out var S, out var V);
            result = Color.HSVToRGB(h ?? H, s ?? S, v ?? V);
            result.a = a ?? color.a;
            return result;
        }
        /// <summary>returns #RRGGBBAA</summary>
        public static string ToHexRGBA(this Color color, int i)
        {
            int R = Mathf.RoundToInt(color.r * 255);
            int G = Mathf.RoundToInt(color.g * 255);
            int B = Mathf.RoundToInt(color.b * 255);
            int A = Mathf.RoundToInt(color.a * 255);
            return $"#{R:X1}{G:X1}{B:X1}{A:X1}";
        }
        /// <summary>returns #RRGGBB</summary>
        public static string ToHexRGB(this Color color)
        {
            int R = Mathf.RoundToInt(color.r * 255);
            int G = Mathf.RoundToInt(color.g * 255);
            int B = Mathf.RoundToInt(color.b * 255);
            return $"#{R:X1}{G:X1}{B:X1}";
        }
    }
}