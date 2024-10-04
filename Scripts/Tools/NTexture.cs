using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Neeto
{
    public static class NTexture
    {
        public static Texture2D inspect { get; private set; }
        public static Texture2D save { get; private set; }
        public static Texture2D click { get; private set; }
        public static Texture2D folder { get; private set; }
        public static Texture2D folder_full { get; private set; }
        public static Texture2D info { get; private set; }
        public static Texture2D confirm { get; private set; }
        public static Texture2D cancel { get; private set; }
        public static Texture2D expand { get; private set; }
        public static Texture2D contract { get; private set; }
        public static Texture2D undo { get; private set; }
        public static Texture2D time { get; private set; }
        public static Texture2D locked { get; private set; }
        public static Texture2D unlocked { get; private set; }
        public static Texture2D nuke { get; private set; }
        public static Texture2D settings_cog { get; private set; }
        public static Texture2D settings_knobs { get; private set; }
        public static Texture2D git { get; private set; }
        public static Texture2D steam { get; private set; }
        public static Texture2D shadow { get; private set; }
        public static Texture2D highlight { get; private set; }

        //public static Texture2D folder { get; private set; }
        //public static Texture2D circle { get; private set; }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void Load()
        {
            EditorApplication.delayCall += () =>
            {
                //inspect = ResourceLibrary.Textures_magnifying_glass.Load();
                //save = ResourceLibrary.Textures_save.Load();
                //click = ResourceLibrary.Textures_click.Load();
                //folder_full = ResourceLibrary.Textures_folder_full.Load();
                //folder = ResourceLibrary.Textures_folder.Load();
                //info = ResourceLibrary.Textures_info.Load();
                //confirm = ResourceLibrary.Textures_confirmed.Load();
                //cancel = ResourceLibrary.Textures_cancel.Load();
                //expand = ResourceLibrary.Textures_expand.Load();
                //contract = ResourceLibrary.Textures_contract.Load();
                //undo = ResourceLibrary.Textures_backward_time.Load();
                //time = ResourceLibrary.Textures_stopwatch.Load();
                //locked = ResourceLibrary.Textures_padlock.Load();
                //unlocked = ResourceLibrary.Textures_padlock_open.Load();//.Multiply(Color.white.WithA(.5f));
                //nuke = ResourceLibrary.Textures_mushroom_cloud.Load();
                //settings_cog = ResourceLibrary.Textures_cog.Load();
                //settings_knobs = ResourceLibrary.Textures_settings_knobs.Load();
                //git = ResourceLibrary.Textures_github_logo.Load();
                //steam = ResourceLibrary.Textures_steam_icon.Load();
                //shadow = Color.black.With(a: .123f).AsTexturePixel(); // wtf is a setting
                //highlight = Color.white.With(a: .069f).AsTexturePixel();
            };
        }
#endif
    }
}