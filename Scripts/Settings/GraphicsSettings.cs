using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Neeto
{
    [Serializable]
    public class GraphicsSettings : SettingsModule
    {
        public static GraphicsSettings instance { get; private set; } = new Lazy<GraphicsSettings>(Load<GraphicsSettings>);

        //public Resolution resolution;
        public Vector2Int dimensions;
        public FullScreenMode fullScreen;
        public int refreshRate;
        public bool unlockedFrameRate;
        public bool vsync;

        public override void SetDefaults()
        {
            dimensions = new(1920, 1080);
            fullScreen = FullScreenMode.FullScreenWindow;
            refreshRate = (int)Screen.currentResolution.refreshRateRatio.numerator;
            unlockedFrameRate = false;
            vsync = false;
        }
        public override void Apply()
        {
            QualitySettings.vSyncCount = vsync ? 1 : 0;
            Application.targetFrameRate = unlockedFrameRate ? -1 : refreshRate;
            Screen.SetResolution(dimensions.x, dimensions.y, fullScreen);
        }
        public void Load() => (instance = Load<GraphicsSettings>()).Apply();

        public static IEnumerable<string> GetResolutionLabels()
        {
            return Screen.resolutions.Select(NiceLabel);
        }
        public static string NiceLabel(Resolution resolution)
        {
            return resolution.ToString().Split('@')[0].Trim();
        }
        public static void ApplyResolution(Resolution resolution, bool? fullScreen = null)
        {
            Screen.SetResolution(resolution.width, resolution.height, fullScreen ?? Screen.fullScreen);
        }
        public static Resolution ParseResolution(string str)
        {
            var e = str.Split('x').Select(_ => int.Parse(_.Trim())).ToArray();
            return new Resolution { width = e[0], height = e[1] };
        }
        public static int GetIndex(Resolution resolution)
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
        public static int GetIndex(Vector2Int dim)
        {
            for (int i = 0; i < Screen.resolutions.Length; i++)
            {
                if (Screen.resolutions[i].width == dim.x &&
                    Screen.resolutions[i].height == dim.y)
                {
                    return i;
                }
            }
            return 0;
        }
    }


}
