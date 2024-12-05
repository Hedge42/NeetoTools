using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Neeto
{
    [Serializable]
    public struct GraphicsSettings
    {
        static readonly Preference<GraphicsSettings> pref;

        public Vector2Int dimensions;
        public FullScreenMode fullScreen;
        public int frameRate;
        public bool unlockedFrameRate;
        public bool vsync;

        public void SetDefaults()
        {
            dimensions = new(1920, 1080);
            fullScreen = FullScreenMode.FullScreenWindow;
            frameRate = Application.targetFrameRate;
            unlockedFrameRate = frameRate > 0;
            vsync = false;
        }
        public void Apply()
        {
            QualitySettings.vSyncCount = vsync ? 1 : 0;
            Application.targetFrameRate = unlockedFrameRate ? -1 : frameRate;
            Screen.SetResolution(dimensions.x, dimensions.y, fullScreen);
        }
        public void Read() => pref.Read().CopyTo(this);
        public void Write() => pref.Write(this);
        public void Load()
        {
            Read();
            Apply();
        }


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
