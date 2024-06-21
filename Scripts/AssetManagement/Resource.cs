using System;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public struct Resource<T> where T : Object
{
    public string resourcePath;

    public Resource(string resourcePath)
    {
        this.resourcePath = resourcePath;
    }

    public T Load()
    {
        return Resources.Load(resourcePath) as T;
    }
}

public static class ResourceGenerator
{
    [MenuItem(MPath.Commands + nameof(GenerateResources), priority = MPath.BOTTOM)]
    public static void GenerateResources()
    {
        var script = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Neeto/Scripts/AssetManagement/Resource.cs");
        var resourcePaths = AssetDatabase.FindAssets("t:Object", new[] { "Assets" })
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Where(path => path.Contains("/Resources/"));// && Directory.Exists(Path.Combine(MPath.project, path)));

        var resourceDirectories = resourcePaths.Where(path => Directory.Exists(Path.Combine(MPath.project, path)));

        var sb = new StringBuilder();
        foreach (var assetPath in resourcePaths)
        {
            var obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            var typeName = obj.GetType().FullName;
            var resourcePath = assetPath.AsTextAfter("/Resources/");
            var ext = Path.GetExtension(resourcePath);
            if (ext.IsEmpty())
                continue;
            resourcePath = resourcePath.Replace(Path.GetExtension(resourcePath), ""); // removeExtension
            var resourceName = resourcePath.Replace('-', '_').Replace('/', '_');

            var line = $"public static readonly Resource<{typeName}> {resourceName} = new (\"{resourcePath}\");";
            sb.AppendLine(line);
            Debug.Log(line);
        }

        MScript.OverwriteRegion(script, "GENERATED", sb.ToString());
    }

    #region GENERATED
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_backward_time = new ("GUI_Textures/backward_time");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_binoculars = new ("GUI_Textures/binoculars");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_button_finger = new ("GUI_Textures/button-finger");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_cancel = new ("GUI_Textures/cancel");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_chat_bubble = new ("GUI_Textures/chat_bubble");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_check_mark = new ("GUI_Textures/check_mark");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_click = new ("GUI_Textures/click");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_cog = new ("GUI_Textures/cog");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_confirmed = new ("GUI_Textures/confirmed");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_console_controller = new ("GUI_Textures/console-controller");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_contract = new ("GUI_Textures/contract");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_cursor = new ("GUI_Textures/cursor");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_cycle = new ("GUI_Textures/cycle");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_dial_padlock = new ("GUI_Textures/dial-padlock");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_enter = new ("GUI_Textures/enter");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_exit = new ("GUI_Textures/exit");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_expand = new ("GUI_Textures/expand");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_expander = new ("GUI_Textures/expander");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_folder = new ("GUI_Textures/folder");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_folder_full = new ("GUI_Textures/folder_full");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_game_console = new ("GUI_Textures/game-console");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_gamepad = new ("GUI_Textures/gamepad");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_github_logo = new ("GUI_Textures/github-logo");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_halt = new ("GUI_Textures/halt");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_hamburger_menu = new ("GUI_Textures/hamburger-menu");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_hazard_sign = new ("GUI_Textures/hazard_sign");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_health_cross = new ("GUI_Textures/health_cross");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_heart = new ("GUI_Textures/heart");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_heart_bottle = new ("GUI_Textures/heart_bottle");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_help = new ("GUI_Textures/help");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_high_shot = new ("GUI_Textures/high_shot");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_infinity = new ("GUI_Textures/infinity");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_info = new ("GUI_Textures/info");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_interdiction = new ("GUI_Textures/interdiction");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_joystick = new ("GUI_Textures/joystick");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_keyboard = new ("GUI_Textures/keyboard");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_magnifying_glass = new ("GUI_Textures/magnifying_glass");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_mouse = new ("GUI_Textures/mouse");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_move = new ("GUI_Textures/move");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_mushroom_cloud = new ("GUI_Textures/mushroom-cloud");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_open_book = new ("GUI_Textures/open-book");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_padlock_open = new ("GUI_Textures/padlock-open");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_padlock = new ("GUI_Textures/padlock");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_paint_brush = new ("GUI_Textures/paint-brush");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_pc = new ("GUI_Textures/pc");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_pin = new ("GUI_Textures/pin");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_plain_padlock = new ("GUI_Textures/plain-padlock");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_player_previous = new ("GUI_Textures/player_previous");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_position_marker = new ("GUI_Textures/position_marker");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_power_button = new ("GUI_Textures/power-button");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_recycle = new ("GUI_Textures/recycle");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_refresh = new ("GUI_Textures/refresh");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_retro_controller = new ("GUI_Textures/retro-controller");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_return_arrow = new ("GUI_Textures/return_arrow");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_riposte = new ("GUI_Textures/riposte");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_save = new ("GUI_Textures/save");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_settings_knobs = new ("GUI_Textures/settings-knobs");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_signal = new ("GUI_Textures/signal");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_steam_icon = new ("GUI_Textures/steam-icon");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_stopwatch = new ("GUI_Textures/stopwatch");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_transform = new ("GUI_Textures/transform");
public static readonly Resource<UnityEngine.Texture2D> GUI_Textures_vr_headset = new ("GUI_Textures/vr-headset");
#endregion


}

