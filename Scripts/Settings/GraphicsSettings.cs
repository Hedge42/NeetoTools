using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Neeto
{
    public static class GraphicsSettings
    {
        public static string NiceLabel(this Resolution resolution)
        {
            return resolution.ToString().Split('@')[0].Trim();
        }
        public static IEnumerable<string> GetResolutionLabels()
        {
            return Screen.resolutions.Select(NiceLabel);
        }
        public static void ApplyResolution(this Resolution resolution, bool? fullScreen = null)
        {
            Screen.SetResolution(resolution.width, resolution.height, fullScreen ?? Screen.fullScreen);
        }
        public static Resolution ParseResolution(string str)
        {
            var e = str.Split('x').Select(_ => int.Parse(_.Trim())).ToArray();
            return new Resolution { width = e[0], height = e[1] };
        }
        public static int GetIndex(this Resolution resolution)
        {
            for (int i = 0; i < Screen.resolutions.Length; i++)
            {
                if (Screen.resolutions[i].width == resolution.width &&
                    Screen.resolutions[i].height == resolution.height)
                {
                    return i;
                }
            }
            return 0;
        }
        public static Resolution With(this Resolution Resolution, Vector2Int? dimensions = null, int? w = null, int? h = null)
        {
            if (dimensions is Vector2Int Dimensions)
            {
                Resolution.width = Dimensions.x;
                Resolution.height = Dimensions.y;
            }
            if (w is int W)
            {
                Resolution.width = W;
            }
            if (h is int H)
            {
                Resolution.height = H;
            }
            return Resolution;
        }
    }
}
