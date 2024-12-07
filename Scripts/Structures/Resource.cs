using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Neeto
{
    public static class Resource
    {
        static void GenerateTextures()
        {
            //ScriptGenerator.
        }

        #region Textures
        public static readonly Resource<UnityEngine.Texture2D> LUT_1px = new("LUT/1px");
        public static readonly Resource<UnityEngine.Texture2D> LUT_1px_clear = new("LUT/1px_clear");
        public static readonly Resource<UnityEngine.Texture2D> LUT_Gradients_gradient_64_bottom_to_top = new("LUT/Gradients/gradient-64-bottom-to-top");
        public static readonly Resource<UnityEngine.Texture2D> LUT_Gradients_gradient_64_left_to_right = new("LUT/Gradients/gradient-64-left-to-right");
        public static readonly Resource<UnityEngine.Texture2D> LUT_Gradients_gradient_64_right_to_left = new("LUT/Gradients/gradient-64-right-to-left");
        public static readonly Resource<UnityEngine.Texture2D> LUT_Gradients_gradient_64_top_to_bottom = new("LUT/Gradients/gradient-64-top-to-bottom");
        public static readonly Resource<UnityEngine.Texture2D> LUT_particle = new("LUT/particle");
        public static readonly Resource<UnityEngine.Texture2D> LUT_particle_soft_256 = new("LUT/particle_soft_256");
        public static readonly Resource<UnityEngine.Texture2D> Textures_backward_time = new("Textures/backward_time");
        public static readonly Resource<UnityEngine.Texture2D> Textures_binoculars = new("Textures/binoculars");
        public static readonly Resource<UnityEngine.Texture2D> Textures_button_finger = new("Textures/button-finger");
        public static readonly Resource<UnityEngine.Texture2D> Textures_cancel = new("Textures/cancel");
        public static readonly Resource<UnityEngine.Texture2D> Textures_chat_bubble = new("Textures/chat_bubble");
        public static readonly Resource<UnityEngine.Texture2D> Textures_check_mark = new("Textures/check_mark");
        public static readonly Resource<UnityEngine.Texture2D> Textures_click = new("Textures/click");
        public static readonly Resource<UnityEngine.Texture2D> Textures_cog = new("Textures/cog");
        public static readonly Resource<UnityEngine.Texture2D> Textures_confirmed = new("Textures/confirmed");
        public static readonly Resource<UnityEngine.Texture2D> Textures_console_controller = new("Textures/console-controller");
        public static readonly Resource<UnityEngine.Texture2D> Textures_contract = new("Textures/contract");
        public static readonly Resource<UnityEngine.Texture2D> Textures_cursor = new("Textures/cursor");
        public static readonly Resource<UnityEngine.Texture2D> Textures_cycle = new("Textures/cycle");
        public static readonly Resource<UnityEngine.Texture2D> Textures_dial_padlock = new("Textures/dial-padlock");
        public static readonly Resource<UnityEngine.Texture2D> Textures_enter = new("Textures/enter");
        public static readonly Resource<UnityEngine.Texture2D> Textures_exit = new("Textures/exit");
        public static readonly Resource<UnityEngine.Texture2D> Textures_expand = new("Textures/expand");
        public static readonly Resource<UnityEngine.Texture2D> Textures_expander = new("Textures/expander");
        public static readonly Resource<UnityEngine.Texture2D> Textures_folder = new("Textures/folder");
        public static readonly Resource<UnityEngine.Texture2D> Textures_folder_full = new("Textures/folder_full");
        public static readonly Resource<UnityEngine.Texture2D> Textures_game_console = new("Textures/game-console");
        public static readonly Resource<UnityEngine.Texture2D> Textures_gamepad = new("Textures/gamepad");
        public static readonly Resource<UnityEngine.Texture2D> Textures_github_logo = new("Textures/github-logo");
        public static readonly Resource<UnityEngine.Texture2D> Textures_halt = new("Textures/halt");
        public static readonly Resource<UnityEngine.Texture2D> Textures_hamburger_menu = new("Textures/hamburger-menu");
        public static readonly Resource<UnityEngine.Texture2D> Textures_hazard_sign = new("Textures/hazard_sign");
        public static readonly Resource<UnityEngine.Texture2D> Textures_health_cross = new("Textures/health_cross");
        public static readonly Resource<UnityEngine.Texture2D> Textures_heart = new("Textures/heart");
        public static readonly Resource<UnityEngine.Texture2D> Textures_heart_bottle = new("Textures/heart_bottle");
        public static readonly Resource<UnityEngine.Texture2D> Textures_help = new("Textures/help");
        public static readonly Resource<UnityEngine.Texture2D> Textures_high_shot = new("Textures/high_shot");
        public static readonly Resource<UnityEngine.Texture2D> Textures_infinity = new("Textures/infinity");
        public static readonly Resource<UnityEngine.Texture2D> Textures_info = new("Textures/info");
        public static readonly Resource<UnityEngine.Texture2D> Textures_interdiction = new("Textures/interdiction");
        public static readonly Resource<UnityEngine.Texture2D> Textures_joystick = new("Textures/joystick");
        public static readonly Resource<UnityEngine.Texture2D> Textures_keyboard = new("Textures/keyboard");
        public static readonly Resource<UnityEngine.Texture2D> Textures_magnifying_glass = new("Textures/magnifying_glass");
        public static readonly Resource<UnityEngine.Texture2D> Textures_mouse = new("Textures/mouse");
        public static readonly Resource<UnityEngine.Texture2D> Textures_move = new("Textures/move");
        public static readonly Resource<UnityEngine.Texture2D> Textures_mushroom_cloud = new("Textures/mushroom-cloud");
        public static readonly Resource<UnityEngine.Texture2D> Textures_open_book = new("Textures/open-book");
        public static readonly Resource<UnityEngine.Texture2D> Textures_padlock_open = new("Textures/padlock-open");
        public static readonly Resource<UnityEngine.Texture2D> Textures_padlock = new("Textures/padlock");
        public static readonly Resource<UnityEngine.Texture2D> Textures_paint_brush = new("Textures/paint-brush");
        public static readonly Resource<UnityEngine.Texture2D> Textures_pc = new("Textures/pc");
        public static readonly Resource<UnityEngine.Texture2D> Textures_pin = new("Textures/pin");
        public static readonly Resource<UnityEngine.Texture2D> Textures_plain_padlock = new("Textures/plain-padlock");
        public static readonly Resource<UnityEngine.Texture2D> Textures_player_previous = new("Textures/player_previous");
        public static readonly Resource<UnityEngine.Texture2D> Textures_position_marker = new("Textures/position_marker");
        public static readonly Resource<UnityEngine.Texture2D> Textures_power_button = new("Textures/power-button");
        public static readonly Resource<UnityEngine.Texture2D> Textures_recycle = new("Textures/recycle");
        public static readonly Resource<UnityEngine.Texture2D> Textures_refresh = new("Textures/refresh");
        public static readonly Resource<UnityEngine.Texture2D> Textures_retro_controller = new("Textures/retro-controller");
        public static readonly Resource<UnityEngine.Texture2D> Textures_return_arrow = new("Textures/return_arrow");
        public static readonly Resource<UnityEngine.Texture2D> Textures_riposte = new("Textures/riposte");
        public static readonly Resource<UnityEngine.Texture2D> Textures_save = new("Textures/save");
        public static readonly Resource<UnityEngine.Texture2D> Textures_settings_knobs = new("Textures/settings-knobs");
        public static readonly Resource<UnityEngine.Texture2D> Textures_signal = new("Textures/signal");
        public static readonly Resource<UnityEngine.Texture2D> Textures_steam_icon = new("Textures/steam-icon");
        public static readonly Resource<UnityEngine.Texture2D> Textures_stopwatch = new("Textures/stopwatch");
        public static readonly Resource<UnityEngine.Texture2D> Textures_transform = new("Textures/transform");
        public static readonly Resource<UnityEngine.Texture2D> Textures_vr_headset = new("Textures/vr-headset");
        #endregion
    }

    [Serializable]
    public struct Resource<T> where T : Object
    {
        public string resourcePath;
        public T asset;

        public Resource(string resourcePath)
        {
            this.resourcePath = resourcePath;
            this.asset = null;
        }

        public T Load()
        {
            return asset ??= Resources.Load(resourcePath) as T;
        }
    }
}
