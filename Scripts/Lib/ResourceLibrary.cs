using UnityEngine;
using System;
using Object = UnityEngine.Object;

namespace Neeto
{
    public static class ResourceLibrary
    {
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

        #region GENERATED
        public static readonly Resource<NeetoSettings> NeetoSettings = new("NeetoSettings");
        public static readonly Resource<UnityEngine.TextAsset> ScriptGenerator = new("ScriptGenerator");
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
        public static readonly Resource<UnityEngine.GameObject> Adv_Cliff_1A1 = new("Adv_Cliff_1A1");
        public static readonly Resource<UnityEngine.GameObject> Adv_Cliff_1A2 = new("Adv_Cliff_1A2");
        public static readonly Resource<UnityEngine.Texture2D> Materials_1_Rock_B_Normal = new("Materials/1_Rock_B Normal");
        public static readonly Resource<UnityEngine.Texture2D> Materials_1_Rock_B = new("Materials/1_Rock_B");
        public static readonly Resource<UnityEngine.Texture2D> Materials_1_Rock_B_MAT_A_AO = new("Materials/1_Rock_B_MAT_A AO");
        public static readonly Resource<UnityEngine.Texture2D> Materials_1_Rock_B_MAT_A_Metallic_2 = new("Materials/1_Rock_B_MAT_A Metallic 2");
        //public static readonly Resource<UnityEngine.Texture2D> Materials_1_Rock_B_MAT_A_Metallic = new ("Materials/1_Rock_B_MAT_A Metallic");
        //public static readonly Resource<UnityEngine.Texture2D> Materials_1_Rock_B_MAT_A_Secondary = new ("Materials/1_Rock_B_MAT_A Secondary");
        //public static readonly Resource<UnityEngine.Material> Materials_1_Rock_B_MAT_A = new ("Materials/1_Rock_B_MAT_A");
        //public static readonly Resource<UnityEngine.Texture2D> Materials_1_Rock_B_MAT_A = new ("Materials/1_Rock_B_MAT_A");
        //public static readonly Resource<UnityEngine.Texture2D> Materials_ADV_Cliff_1_AO_2 = new ("Materials/ADV_Cliff_1_AO_2");
        //public static readonly Resource<UnityEngine.Texture2D> Materials_ADV_Cliff_1_AO_occlusion = new ("Materials/ADV_Cliff_1_AO_occlusion");
        //public static readonly Resource<UnityEngine.Texture2D> Materials_ADV_Cliff_1_Metallic = new ("Materials/ADV_Cliff_1_Metallic");
        //public static readonly Resource<UnityEngine.Texture2D> Materials_ADV_Cliff_1_Normal_normals = new ("Materials/ADV_Cliff_1_Normal_normals");
        //public static readonly Resource<UnityEngine.Material> Materials_Adv_Cliff_1A1 = new ("Materials/Adv_Cliff_1A1");
        //public static readonly Resource<UnityEngine.Texture2D> Materials_Adv_Cliff_1A1 = new ("Materials/Adv_Cliff_1A1");
        //public static readonly Resource<UnityEngine.Material> Materials_Adv_Cliff_2 = new ("Materials/Adv_Cliff_2");
        //public static readonly Resource<UnityEngine.Texture2D> Materials_Concrete_Normal_EX = new ("Materials/Concrete_Normal_EX");
        //public static readonly Resource<UnityEngine.Texture2D> Materials_Rock_Test_1 = new ("Materials/Rock Test 1");
        //public static readonly Resource<UnityEngine.Texture2D> Materials_Rock_Test_2 = new ("Materials/Rock Test 2");
        //public static readonly Resource<UnityEngine.Texture2D> Materials_Rock_2_Height_heights = new ("Materials/Rock_2_Height_heights");
        //public static readonly Resource<UnityEngine.Texture2D> Materials_Rock_2_normal_normals = new ("Materials/Rock_2_normal_normals");
        //public static readonly Resource<UnityEngine.Texture2D> Materials_Smooth_1.01 = new ("Materials/Smooth 1.01");
        //public static readonly Resource<UnityEngine.GameObject> Rock_A1 = new ("Rock_A1");
        //public static readonly Resource<UnityEngine.Texture2D> Easy_performant_outline_EP_Outline_logo = new ("Easy performant outline/EP Outline logo");
        public static readonly Resource<UnityEngine.Material> Easy_performant_outline_Materials__BasicBlit = new("Easy performant outline/Materials/_BasicBlit");
        public static readonly Resource<UnityEngine.Material> Easy_performant_outline_Materials__Blur = new("Easy performant outline/Materials/_Blur");
        public static readonly Resource<UnityEngine.Material> Easy_performant_outline_Materials__ClearStencil = new("Easy performant outline/Materials/_ClearStencil");
        public static readonly Resource<UnityEngine.Material> Easy_performant_outline_Materials__Dilate = new("Easy performant outline/Materials/_Dilate");
        public static readonly Resource<UnityEngine.Material> Easy_performant_outline_Materials__EdgeDilate = new("Easy performant outline/Materials/_EdgeDilate");
        public static readonly Resource<UnityEngine.Material> Easy_performant_outline_Materials__FinalBlit = new("Easy performant outline/Materials/_FinalBlit");
        public static readonly Resource<UnityEngine.Material> Easy_performant_outline_Materials__Obstacle = new("Easy performant outline/Materials/_Obstacle");
        public static readonly Resource<UnityEngine.Material> Easy_performant_outline_Materials__Outline = new("Easy performant outline/Materials/_Outline");
        public static readonly Resource<UnityEngine.Material> Easy_performant_outline_Materials__OutlineMask = new("Easy performant outline/Materials/_OutlineMask");
        public static readonly Resource<UnityEngine.Material> Easy_performant_outline_Materials__PartialBlit = new("Easy performant outline/Materials/_PartialBlit");
        public static readonly Resource<UnityEngine.Material> Easy_performant_outline_Materials__TransparentBlit = new("Easy performant outline/Materials/_TransparentBlit");
        public static readonly Resource<UnityEngine.Material> Easy_performant_outline_Materials__ZPrepass = new("Easy performant outline/Materials/_ZPrepass");
        public static readonly Resource<UnityEngine.Shader> Easy_performant_outline_Shaders_BasicBlit = new("Easy performant outline/Shaders/BasicBlit");
        public static readonly Resource<UnityEngine.Shader> Easy_performant_outline_Shaders_Blur = new("Easy performant outline/Shaders/Blur");
        public static readonly Resource<UnityEngine.Shader> Easy_performant_outline_Shaders_ClearStencil = new("Easy performant outline/Shaders/ClearStencil");
        public static readonly Resource<UnityEngine.Shader> Easy_performant_outline_Shaders_Dilate = new("Easy performant outline/Shaders/Dilate");
        public static readonly Resource<UnityEngine.Shader> Easy_performant_outline_Shaders_EdgeDilate = new("Easy performant outline/Shaders/EdgeDilate");
        public static readonly Resource<UnityEngine.Shader> Easy_performant_outline_Shaders_Fills_ColorFill = new("Easy performant outline/Shaders/Fills/ColorFill");
        public static readonly Resource<UnityEngine.Shader> Easy_performant_outline_Shaders_Fills_Dots = new("Easy performant outline/Shaders/Fills/Dots");
        public static readonly Resource<UnityEngine.Shader> Easy_performant_outline_Shaders_Fills_EmptyFill = new("Easy performant outline/Shaders/Fills/EmptyFill");
        public static readonly Resource<UnityEngine.Shader> Easy_performant_outline_Shaders_Fills_FillMask = new("Easy performant outline/Shaders/Fills/FillMask");
        public static readonly Resource<UnityEngine.Shader> Easy_performant_outline_Shaders_Fills_Fresnel = new("Easy performant outline/Shaders/Fills/Fresnel");
        public static readonly Resource<UnityEngine.Shader> Easy_performant_outline_Shaders_Fills_Interlaced = new("Easy performant outline/Shaders/Fills/Interlaced");
        public static readonly Resource<UnityEngine.Shader> Easy_performant_outline_Shaders_FinalBlit = new("Easy performant outline/Shaders/FinalBlit");
        public static readonly Resource<UnityEditor.ShaderInclude> Easy_performant_outline_Shaders_MiskCG = new("Easy performant outline/Shaders/MiskCG");
        public static readonly Resource<UnityEngine.Shader> Easy_performant_outline_Shaders_Obstacle = new("Easy performant outline/Shaders/Obstacle");
        public static readonly Resource<UnityEngine.Shader> Easy_performant_outline_Shaders_Outline = new("Easy performant outline/Shaders/Outline");
        public static readonly Resource<UnityEngine.Shader> Easy_performant_outline_Shaders_OutlineMask = new("Easy performant outline/Shaders/OutlineMask");
        public static readonly Resource<UnityEngine.Shader> Easy_performant_outline_Shaders_PartialBlit = new("Easy performant outline/Shaders/PartialBlit");
        public static readonly Resource<UnityEngine.Shader> Easy_performant_outline_Shaders_TransparentBlit = new("Easy performant outline/Shaders/TransparentBlit");
        public static readonly Resource<UnityEngine.Shader> Easy_performant_outline_Shaders_ZPrepass = new("Easy performant outline/Shaders/ZPrepass");
        //public static readonly Resource<ExtEvents.PackageSettings> ExtEvents_PackageSettings = new ("ExtEvents_PackageSettings");
        //public static readonly Resource<DG.Tweening.Core.DOTweenSettings> DOTweenSettings = new("DOTweenSettings");
        public static readonly Resource<TMPro.TMP_ColorGradient> Color_Gradient_Presets_Blue_to_Purple___Vertical = new("Color Gradient Presets/Blue to Purple - Vertical");
        public static readonly Resource<TMPro.TMP_ColorGradient> Color_Gradient_Presets_Dark_to_Light_Green___Vertical = new("Color Gradient Presets/Dark to Light Green - Vertical");
        public static readonly Resource<TMPro.TMP_ColorGradient> Color_Gradient_Presets_Light_to_Dark_Green___Vertical = new("Color Gradient Presets/Light to Dark Green - Vertical");
        public static readonly Resource<TMPro.TMP_ColorGradient> Color_Gradient_Presets_Yellow_to_Orange___Vertical = new("Color Gradient Presets/Yellow to Orange - Vertical");
        //public static readonly Resource<UnityEngine.Material> Fonts_&_Materials_Anton_SDF___Drop_Shadow = new ("Fonts & Materials/Anton SDF - Drop Shadow");
        //public static readonly Resource<UnityEngine.Material> Fonts_&_Materials_Anton_SDF___Outline = new ("Fonts & Materials/Anton SDF - Outline");
        //public static readonly Resource<UnityEngine.Material> Fonts_&_Materials_Anton_SDF___Sunny_Days = new ("Fonts & Materials/Anton SDF - Sunny Days");
        //public static readonly Resource<TMPro.TMP_FontAsset> Fonts_&_Materials_Anton_SDF = new ("Fonts & Materials/Anton SDF");
        //public static readonly Resource<UnityEngine.Material> Fonts_&_Materials_Bangers_SDF___Drop_Shadow = new ("Fonts & Materials/Bangers SDF - Drop Shadow");
        //public static readonly Resource<UnityEngine.Material> Fonts_&_Materials_Bangers_SDF___Outline = new ("Fonts & Materials/Bangers SDF - Outline");
        //public static readonly Resource<UnityEngine.Material> Fonts_&_Materials_Bangers_SDF_Glow = new ("Fonts & Materials/Bangers SDF Glow");
        //public static readonly Resource<UnityEngine.Material> Fonts_&_Materials_Bangers_SDF_Logo = new ("Fonts & Materials/Bangers SDF Logo");
        //public static readonly Resource<TMPro.TMP_FontAsset> Fonts_&_Materials_Bangers_SDF = new ("Fonts & Materials/Bangers SDF");
        //public static readonly Resource<TMPro.TMP_FontAsset> Fonts_&_Materials_Electronic_Highway_Sign_SDF = new ("Fonts & Materials/Electronic Highway Sign SDF");
        //public static readonly Resource<UnityEngine.Material> Fonts_&_Materials_LiberationSans_SDF___Metalic_Green = new ("Fonts & Materials/LiberationSans SDF - Metalic Green");
        //public static readonly Resource<UnityEngine.Material> Fonts_&_Materials_LiberationSans_SDF___Overlay = new ("Fonts & Materials/LiberationSans SDF - Overlay");
        //public static readonly Resource<UnityEngine.Material> Fonts_&_Materials_LiberationSans_SDF___Soft_Mask = new ("Fonts & Materials/LiberationSans SDF - Soft Mask");
        //public static readonly Resource<TMPro.TMP_FontAsset> Fonts_&_Materials_Oswald_Bold_SDF = new ("Fonts & Materials/Oswald Bold SDF");
        //public static readonly Resource<UnityEngine.Material> Fonts_&_Materials_Roboto_Bold_SDF___Drop_Shadow = new ("Fonts & Materials/Roboto-Bold SDF - Drop Shadow");
        //public static readonly Resource<UnityEngine.Material> Fonts_&_Materials_Roboto_Bold_SDF___Surface = new ("Fonts & Materials/Roboto-Bold SDF - Surface");
        //public static readonly Resource<TMPro.TMP_FontAsset> Fonts_&_Materials_Roboto_Bold_SDF = new ("Fonts & Materials/Roboto-Bold SDF");
        //public static readonly Resource<TMPro.TMP_SpriteAsset> Sprite_Assets_Default_Sprite_Asset = new ("Sprite Assets/Default Sprite Asset");
        //public static readonly Resource<TMPro.TMP_SpriteAsset> Sprite_Assets_DropCap_Numbers = new ("Sprite Assets/DropCap Numbers");
        //public static readonly Resource<UnityEngine.Material> Fonts_&_Materials_LiberationSans_SDF___Drop_Shadow = new ("Fonts & Materials/LiberationSans SDF - Drop Shadow");
        //public static readonly Resource<TMPro.TMP_FontAsset> Fonts_&_Materials_LiberationSans_SDF___Fallback = new ("Fonts & Materials/LiberationSans SDF - Fallback");
        //public static readonly Resource<UnityEngine.Material> Fonts_&_Materials_LiberationSans_SDF___Outline = new ("Fonts & Materials/LiberationSans SDF - Outline");
        //public static readonly Resource<TMPro.TMP_FontAsset> Fonts_&_Materials_LiberationSans_SDF = new ("Fonts & Materials/LiberationSans SDF");
        //public static readonly Resource<UnityEngine.TextAsset> LineBreaking_Following_Characters = new ("LineBreaking Following Characters");
        //public static readonly Resource<UnityEngine.TextAsset> LineBreaking_Leading_Characters = new ("LineBreaking Leading Characters");
        //public static readonly Resource<TMPro.TMP_SpriteAsset> Sprite_Assets_EmojiOne = new ("Sprite Assets/EmojiOne");
        //public static readonly Resource<TMPro.TMP_StyleSheet> Style_Sheets_Default_Style_Sheet = new ("Style Sheets/Default Style Sheet");
        //public static readonly Resource<TMPro.TMP_Settings> TMP_Settings = new ("TMP Settings");
        //public static readonly Resource<UniHumanoid.HumanPoseClip> UniHumanoid_T_Pose.pose = new ("UniHumanoid/T-Pose.pose");
        //public static readonly Resource<UnityEngine.Material> TestMToon = new ("TestMToon");
        //public static readonly Resource<UnityEngine.Material> TestStandard = new ("TestStandard");
        //public static readonly Resource<UnityEngine.Material> TestUniUnlit = new ("TestUniUnlit");
        //public static readonly Resource<UnityEngine.Material> TestUnlitColor = new ("TestUnlitColor");
        //public static readonly Resource<UnityEngine.Material> TestUnlitCutout = new ("TestUnlitCutout");
        //public static readonly Resource<UnityEngine.Material> TestUnlitTexture = new ("TestUnlitTexture");
        //public static readonly Resource<UnityEngine.Material> TestUnlitTransparent = new ("TestUnlitTransparent");
        //public static readonly Resource<UnityEngine.Shader> UniGLTF_NormalMapExporter = new ("UniGLTF/NormalMapExporter");
        //public static readonly Resource<UnityEngine.Shader> UniGLTF_StandardMapExporter = new ("UniGLTF/StandardMapExporter");
        //public static readonly Resource<UnityEngine.Shader> UniGLTF_StandardMapImporter = new ("UniGLTF/StandardMapImporter");
        //public static readonly Resource<UnityEngine.Shader> UniGLTF_UniUnlit = new ("UniGLTF/UniUnlit");
        //public static readonly Resource<UnityEngine.Shader> Shaders_MToon = new ("Shaders/MToon");
        //public static readonly Resource<UnityEditor.ShaderInclude> Shaders_MToonCore = new ("Shaders/MToonCore");
        //public static readonly Resource<UnityEditor.ShaderInclude> Shaders_MToonSM3 = new ("Shaders/MToonSM3");
        //public static readonly Resource<UnityEditor.ShaderInclude> Shaders_MToonSM4 = new ("Shaders/MToonSM4");
        //public static readonly Resource<UnityEngine.Shader> VRM10_vrmc_materials_mtoon = new ("VRM10/vrmc_materials_mtoon");
        public static readonly Resource<UnityEditor.ShaderInclude> VRM10_vrmc_materials_mtoon_attribute = new("VRM10/vrmc_materials_mtoon_attribute");
        public static readonly Resource<UnityEditor.ShaderInclude> VRM10_vrmc_materials_mtoon_define = new("VRM10/vrmc_materials_mtoon_define");
        public static readonly Resource<UnityEditor.ShaderInclude> VRM10_vrmc_materials_mtoon_forward_fragment = new("VRM10/vrmc_materials_mtoon_forward_fragment");
        public static readonly Resource<UnityEditor.ShaderInclude> VRM10_vrmc_materials_mtoon_forward_vertex = new("VRM10/vrmc_materials_mtoon_forward_vertex");
        public static readonly Resource<UnityEditor.ShaderInclude> VRM10_vrmc_materials_mtoon_geometry_alpha = new("VRM10/vrmc_materials_mtoon_geometry_alpha");
        public static readonly Resource<UnityEditor.ShaderInclude> VRM10_vrmc_materials_mtoon_geometry_normal = new("VRM10/vrmc_materials_mtoon_geometry_normal");
        public static readonly Resource<UnityEditor.ShaderInclude> VRM10_vrmc_materials_mtoon_geometry_uv = new("VRM10/vrmc_materials_mtoon_geometry_uv");
        public static readonly Resource<UnityEditor.ShaderInclude> VRM10_vrmc_materials_mtoon_geometry_vertex = new("VRM10/vrmc_materials_mtoon_geometry_vertex");
        public static readonly Resource<UnityEditor.ShaderInclude> VRM10_vrmc_materials_mtoon_input = new("VRM10/vrmc_materials_mtoon_input");
        public static readonly Resource<UnityEditor.ShaderInclude> VRM10_vrmc_materials_mtoon_lighting_mtoon = new("VRM10/vrmc_materials_mtoon_lighting_mtoon");
        public static readonly Resource<UnityEditor.ShaderInclude> VRM10_vrmc_materials_mtoon_lighting_unity = new("VRM10/vrmc_materials_mtoon_lighting_unity");
        public static readonly Resource<UnityEditor.ShaderInclude> VRM10_vrmc_materials_mtoon_utility = new("VRM10/vrmc_materials_mtoon_utility");
        #endregion
    }
}