using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


public static class MTexture
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
            inspect = ResourceGenerator.GUI_Textures_magnifying_glass.Load();
            save = ResourceGenerator.GUI_Textures_save.Load();
            click = ResourceGenerator.GUI_Textures_click.Load();
            folder_full = ResourceGenerator.GUI_Textures_folder_full.Load();
            folder = ResourceGenerator.GUI_Textures_folder.Load();
            info = ResourceGenerator.GUI_Textures_info.Load();
            confirm = ResourceGenerator.GUI_Textures_confirmed.Load();
            cancel = ResourceGenerator.GUI_Textures_cancel.Load();
            expand = ResourceGenerator.GUI_Textures_expand.Load();
            contract = ResourceGenerator.GUI_Textures_contract.Load();
            undo = ResourceGenerator.GUI_Textures_backward_time.Load();
            time = ResourceGenerator.GUI_Textures_stopwatch.Load();
            locked = ResourceGenerator.GUI_Textures_padlock.Load();
            unlocked = ResourceGenerator.GUI_Textures_padlock_open.Load();//.Multiply(Color.white.WithA(.5f));
            nuke = ResourceGenerator.GUI_Textures_mushroom_cloud.Load();
            settings_cog = ResourceGenerator.GUI_Textures_cog.Load();
            settings_knobs = ResourceGenerator.GUI_Textures_settings_knobs.Load();
            git = ResourceGenerator.GUI_Textures_github_logo.Load();
            steam = ResourceGenerator.GUI_Textures_steam_icon.Load();
            shadow = Color.black.With(a: .123f).AsTexturePixel(); // wtf is a setting
            highlight = Color.white.With(a: .069f).AsTexturePixel();
        };
    }
#endif
}
