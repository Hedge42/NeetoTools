using System;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Neeto
{
    public static class ResourceTextures
    {
        [QuickAction]
        public static void Script()
        {
            var sb = new StringBuilder();
            foreach (var tex in AssetDatabase.FindAssets("t: Texture2D")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(assetPath => assetPath.Contains("/Resources/"))
                .Select(assetPath => AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath)))
            {
                var resourcePath = ResourceLibrary.AssetPathToResourcePath(AssetDatabase.GetAssetPath(tex));
                if (resourcePath.Length == 0)
                    continue;
                sb.AppendLine($"public static readonly ResourceLibrary.Resource<Texture2D> {ScriptGenerator.Codify(resourcePath)} = new (\"{resourcePath}\");");
            }

            var script = ScriptGenerator.FindScript(typeof(ResourceTextures));
            ScriptGenerator.OverwriteRegion(script, "GENERATED", sb.ToString());
        }

        #region GENERATED
public static readonly ResourceLibrary.Resource<Texture2D> Textures_backward_time_png = new ("Textures/backward_time.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_binoculars_png = new ("Textures/binoculars.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_button_finger_png = new ("Textures/button-finger.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_cancel_png = new ("Textures/cancel.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_chat_bubble_png = new ("Textures/chat_bubble.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_check_mark_png = new ("Textures/check_mark.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_click_png = new ("Textures/click.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_cog_png = new ("Textures/cog.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_confirmed_png = new ("Textures/confirmed.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_console_controller_png = new ("Textures/console-controller.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_contract_png = new ("Textures/contract.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_cursor_png = new ("Textures/cursor.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_cycle_png = new ("Textures/cycle.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_dial_padlock_png = new ("Textures/dial-padlock.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_enter_png = new ("Textures/enter.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_exit_png = new ("Textures/exit.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_expand_png = new ("Textures/expand.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_expander_png = new ("Textures/expander.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_folder_png = new ("Textures/folder.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_folder_full_png = new ("Textures/folder_full.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_game_console_png = new ("Textures/game-console.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_gamepad_png = new ("Textures/gamepad.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_github_logo_png = new ("Textures/github-logo.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_halt_png = new ("Textures/halt.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_hamburger_menu_png = new ("Textures/hamburger-menu.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_hazard_sign_png = new ("Textures/hazard_sign.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_health_cross_png = new ("Textures/health_cross.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_heart_png = new ("Textures/heart.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_heart_bottle_png = new ("Textures/heart_bottle.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_help_png = new ("Textures/help.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_high_shot_png = new ("Textures/high_shot.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_infinity_png = new ("Textures/infinity.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_info_png = new ("Textures/info.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_interdiction_png = new ("Textures/interdiction.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_joystick_png = new ("Textures/joystick.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_keyboard_png = new ("Textures/keyboard.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_magnifying_glass_png = new ("Textures/magnifying_glass.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_mouse_png = new ("Textures/mouse.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_move_png = new ("Textures/move.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_mushroom_cloud_png = new ("Textures/mushroom-cloud.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_open_book_png = new ("Textures/open-book.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_padlock_open_png = new ("Textures/padlock-open.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_padlock_png = new ("Textures/padlock.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_paint_brush_png = new ("Textures/paint-brush.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_pc_png = new ("Textures/pc.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_pin_png = new ("Textures/pin.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_plain_padlock_png = new ("Textures/plain-padlock.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_player_previous_png = new ("Textures/player_previous.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_position_marker_png = new ("Textures/position_marker.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_power_button_png = new ("Textures/power-button.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_recycle_png = new ("Textures/recycle.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_refresh_png = new ("Textures/refresh.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_retro_controller_png = new ("Textures/retro-controller.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_return_arrow_png = new ("Textures/return_arrow.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_riposte_png = new ("Textures/riposte.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_save_png = new ("Textures/save.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_settings_knobs_png = new ("Textures/settings-knobs.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_signal_png = new ("Textures/signal.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_steam_icon_png = new ("Textures/steam-icon.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_stopwatch_png = new ("Textures/stopwatch.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_transform_png = new ("Textures/transform.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_vr_headset_png = new ("Textures/vr-headset.png");
public static readonly ResourceLibrary.Resource<Texture2D> Easy_performant_outline_EP_Outline_logo_png = new ("Easy performant outline/EP Outline logo.png");
public static readonly ResourceLibrary.Resource<Texture2D> Materials_1_Rock_B_Normal_png = new ("Materials/1_Rock_B Normal.png");
public static readonly ResourceLibrary.Resource<Texture2D> Materials_1_Rock_B_png = new ("Materials/1_Rock_B.png");
public static readonly ResourceLibrary.Resource<Texture2D> Materials_1_Rock_B_MAT_A_AO_png = new ("Materials/1_Rock_B_MAT_A AO.png");
public static readonly ResourceLibrary.Resource<Texture2D> Materials_1_Rock_B_MAT_A_Metallic_2_png = new ("Materials/1_Rock_B_MAT_A Metallic 2.png");
public static readonly ResourceLibrary.Resource<Texture2D> Materials_1_Rock_B_MAT_A_Metallic_png = new ("Materials/1_Rock_B_MAT_A Metallic.png");
public static readonly ResourceLibrary.Resource<Texture2D> Materials_1_Rock_B_MAT_A_Secondary_png = new ("Materials/1_Rock_B_MAT_A Secondary.png");
public static readonly ResourceLibrary.Resource<Texture2D> Materials_1_Rock_B_MAT_A_png = new ("Materials/1_Rock_B_MAT_A.png");
public static readonly ResourceLibrary.Resource<Texture2D> Materials_ADV_Cliff_1_AO_2_png = new ("Materials/ADV_Cliff_1_AO_2.png");
public static readonly ResourceLibrary.Resource<Texture2D> Materials_ADV_Cliff_1_AO_occlusion_png = new ("Materials/ADV_Cliff_1_AO_occlusion.png");
public static readonly ResourceLibrary.Resource<Texture2D> Materials_ADV_Cliff_1_Metallic_png = new ("Materials/ADV_Cliff_1_Metallic.png");
public static readonly ResourceLibrary.Resource<Texture2D> Materials_ADV_Cliff_1_Normal_normals_png = new ("Materials/ADV_Cliff_1_Normal_normals.png");
public static readonly ResourceLibrary.Resource<Texture2D> Materials_Adv_Cliff_1A1_png = new ("Materials/Adv_Cliff_1A1.png");
public static readonly ResourceLibrary.Resource<Texture2D> Materials_Concrete_Normal_EX_png = new ("Materials/Concrete_Normal_EX.png");
public static readonly ResourceLibrary.Resource<Texture2D> Materials_Rock_Test_1_png = new ("Materials/Rock Test 1.png");
public static readonly ResourceLibrary.Resource<Texture2D> Materials_Rock_Test_2_png = new ("Materials/Rock Test 2.png");
public static readonly ResourceLibrary.Resource<Texture2D> Materials_Rock_2_Height_heights_png = new ("Materials/Rock_2_Height_heights.png");
public static readonly ResourceLibrary.Resource<Texture2D> Materials_Rock_2_normal_normals_png = new ("Materials/Rock_2_normal_normals.png");
public static readonly ResourceLibrary.Resource<Texture2D> Materials_Smooth_1_01_png = new ("Materials/Smooth 1.01.png");
public static readonly ResourceLibrary.Resource<Texture2D> Fonts___Materials_Anton_SDF_asset = new ("Fonts & Materials/Anton SDF.asset");
public static readonly ResourceLibrary.Resource<Texture2D> Fonts___Materials_Bangers_SDF_asset = new ("Fonts & Materials/Bangers SDF.asset");
public static readonly ResourceLibrary.Resource<Texture2D> Fonts___Materials_Electronic_Highway_Sign_SDF_asset = new ("Fonts & Materials/Electronic Highway Sign SDF.asset");
public static readonly ResourceLibrary.Resource<Texture2D> Fonts___Materials_Oswald_Bold_SDF_asset = new ("Fonts & Materials/Oswald Bold SDF.asset");
public static readonly ResourceLibrary.Resource<Texture2D> Fonts___Materials_Roboto_Bold_SDF_asset = new ("Fonts & Materials/Roboto-Bold SDF.asset");
public static readonly ResourceLibrary.Resource<Texture2D> Fonts___Materials_LiberationSans_SDF___Fallback_asset = new ("Fonts & Materials/LiberationSans SDF - Fallback.asset");
public static readonly ResourceLibrary.Resource<Texture2D> Fonts___Materials_LiberationSans_SDF_asset = new ("Fonts & Materials/LiberationSans SDF.asset");
public static readonly ResourceLibrary.Resource<Texture2D> blue_png = new ("blue.png");
public static readonly ResourceLibrary.Resource<Texture2D> green_png = new ("green.png");
public static readonly ResourceLibrary.Resource<Texture2D> personal_actionTreeBackground_png = new ("personal/actionTreeBackground.png");
public static readonly ResourceLibrary.Resource<Texture2D> personal_actionTreeBackgroundWithoutBorder_png = new ("personal/actionTreeBackgroundWithoutBorder.png");
public static readonly ResourceLibrary.Resource<Texture2D> personal_foldoutBackground_png = new ("personal/foldoutBackground.png");
public static readonly ResourceLibrary.Resource<Texture2D> personal_propertiesBackground_png = new ("personal/propertiesBackground.png");
public static readonly ResourceLibrary.Resource<Texture2D> pink_png = new ("pink.png");
public static readonly ResourceLibrary.Resource<Texture2D> pro_actionTreeBackground_png = new ("pro/actionTreeBackground.png");
public static readonly ResourceLibrary.Resource<Texture2D> pro_actionTreeBackgroundWithoutBorder_png = new ("pro/actionTreeBackgroundWithoutBorder.png");
public static readonly ResourceLibrary.Resource<Texture2D> pro_foldoutBackground_png = new ("pro/foldoutBackground.png");
public static readonly ResourceLibrary.Resource<Texture2D> pro_propertiesBackground_png = new ("pro/propertiesBackground.png");
public static readonly ResourceLibrary.Resource<Texture2D> yellow_png = new ("yellow.png");
public static readonly ResourceLibrary.Resource<Texture2D> Cursors_cutCursor_add_png = new ("Cursors/cutCursor-add.png");
public static readonly ResourceLibrary.Resource<Texture2D> Cursors_cutCursor_png = new ("Cursors/cutCursor.png");
public static readonly ResourceLibrary.Resource<Texture2D> Textures_GridBox_Default_png = new ("Textures/GridBox_Default.png");
public static readonly ResourceLibrary.Resource<Texture2D> FlatSkin_Font_Roboto_Black_ttf = new ("FlatSkin/Font/Roboto-Black.ttf");
public static readonly ResourceLibrary.Resource<Texture2D> FlatSkin_Font_Roboto_BlackItalic_ttf = new ("FlatSkin/Font/Roboto-BlackItalic.ttf");
public static readonly ResourceLibrary.Resource<Texture2D> FlatSkin_Font_Roboto_Bold_ttf = new ("FlatSkin/Font/Roboto-Bold.ttf");
public static readonly ResourceLibrary.Resource<Texture2D> FlatSkin_Font_Roboto_BoldItalic_ttf = new ("FlatSkin/Font/Roboto-BoldItalic.ttf");
public static readonly ResourceLibrary.Resource<Texture2D> FlatSkin_Font_Roboto_Italic_ttf = new ("FlatSkin/Font/Roboto-Italic.ttf");
public static readonly ResourceLibrary.Resource<Texture2D> FlatSkin_Font_Roboto_Light_ttf = new ("FlatSkin/Font/Roboto-Light.ttf");
public static readonly ResourceLibrary.Resource<Texture2D> FlatSkin_Font_Roboto_LightItalic_ttf = new ("FlatSkin/Font/Roboto-LightItalic.ttf");
public static readonly ResourceLibrary.Resource<Texture2D> FlatSkin_Font_Roboto_Medium_ttf = new ("FlatSkin/Font/Roboto-Medium.ttf");
public static readonly ResourceLibrary.Resource<Texture2D> FlatSkin_Font_Roboto_MediumItalic_ttf = new ("FlatSkin/Font/Roboto-MediumItalic.ttf");
public static readonly ResourceLibrary.Resource<Texture2D> FlatSkin_Font_Roboto_Regular_ttf = new ("FlatSkin/Font/Roboto-Regular.ttf");
public static readonly ResourceLibrary.Resource<Texture2D> FlatSkin_Font_Roboto_Thin_ttf = new ("FlatSkin/Font/Roboto-Thin.ttf");
public static readonly ResourceLibrary.Resource<Texture2D> FlatSkin_Font_Roboto_ThinItalic_ttf = new ("FlatSkin/Font/Roboto-ThinItalic.ttf");
public static readonly ResourceLibrary.Resource<Texture2D> FlatSkin_SearchSmallDownOff_png = new ("FlatSkin/SearchSmallDownOff.png");
public static readonly ResourceLibrary.Resource<Texture2D> FlatSkin_SearchSmallDownOff_2x_png = new ("FlatSkin/SearchSmallDownOff@2x.png");
public static readonly ResourceLibrary.Resource<Texture2D> FlatSkin_SearchSmallDownOn_png = new ("FlatSkin/SearchSmallDownOn.png");
public static readonly ResourceLibrary.Resource<Texture2D> FlatSkin_SearchSmallDownOn_2x_png = new ("FlatSkin/SearchSmallDownOn@2x.png");
public static readonly ResourceLibrary.Resource<Texture2D> GraphView_Nodes_BlackboardFieldExposed_png = new ("GraphView/Nodes/BlackboardFieldExposed.png");
public static readonly ResourceLibrary.Resource<Texture2D> GraphView_Nodes_BlackboardFieldPillBackground_png = new ("GraphView/Nodes/BlackboardFieldPillBackground.png");
public static readonly ResourceLibrary.Resource<Texture2D> GraphView_Nodes_NodeChevronDown_png = new ("GraphView/Nodes/NodeChevronDown.png");
public static readonly ResourceLibrary.Resource<Texture2D> GraphView_Nodes_NodeChevronDown_2x_png = new ("GraphView/Nodes/NodeChevronDown@2x.png");
public static readonly ResourceLibrary.Resource<Texture2D> GraphView_Nodes_NodeChevronLeft_png = new ("GraphView/Nodes/NodeChevronLeft.png");
public static readonly ResourceLibrary.Resource<Texture2D> GraphView_Nodes_NodeChevronLeft_2x_png = new ("GraphView/Nodes/NodeChevronLeft@2x.png");
public static readonly ResourceLibrary.Resource<Texture2D> GraphView_Nodes_NodeChevronRight_png = new ("GraphView/Nodes/NodeChevronRight.png");
public static readonly ResourceLibrary.Resource<Texture2D> GraphView_Nodes_NodeChevronRight_2x_png = new ("GraphView/Nodes/NodeChevronRight@2x.png");
public static readonly ResourceLibrary.Resource<Texture2D> GraphView_Nodes_NodeChevronUp_png = new ("GraphView/Nodes/NodeChevronUp.png");
public static readonly ResourceLibrary.Resource<Texture2D> GraphView_Nodes_NodeChevronUp_2x_png = new ("GraphView/Nodes/NodeChevronUp@2x.png");
public static readonly ResourceLibrary.Resource<Texture2D> GraphView_Nodes_PreviewCollapse_png = new ("GraphView/Nodes/PreviewCollapse.png");
public static readonly ResourceLibrary.Resource<Texture2D> GraphView_Nodes_PreviewCollapse_2x_png = new ("GraphView/Nodes/PreviewCollapse@2x.png");
public static readonly ResourceLibrary.Resource<Texture2D> GraphView_Nodes_PreviewExpand_png = new ("GraphView/Nodes/PreviewExpand.png");
public static readonly ResourceLibrary.Resource<Texture2D> GraphView_Nodes_PreviewExpand_2x_png = new ("GraphView/Nodes/PreviewExpand@2x.png");
public static readonly ResourceLibrary.Resource<Texture2D> Icons_settings_button_png = new ("Icons/settings_button.png");
public static readonly ResourceLibrary.Resource<Texture2D> Icons_Settings_Flyout_9slice_png = new ("Icons/Settings_Flyout_9slice.png");
public static readonly ResourceLibrary.Resource<Texture2D> Icons_Settings_Flyout_9slice_2x_png = new ("Icons/Settings_Flyout_9slice@2x.png");
public static readonly ResourceLibrary.Resource<Texture2D> Icons_settingsbutton_png = new ("Icons/settingsbutton.png");
public static readonly ResourceLibrary.Resource<Texture2D> Icons_SettingsIcons_png = new ("Icons/SettingsIcons.png");
public static readonly ResourceLibrary.Resource<Texture2D> Icons_SettingsIcons_2x_png = new ("Icons/SettingsIcons@2x.png");
public static readonly ResourceLibrary.Resource<Texture2D> Icons_SettingsIcons_hover_png = new ("Icons/SettingsIcons_hover.png");
public static readonly ResourceLibrary.Resource<Texture2D> Icons_SettingsIcons_hover_2x_png = new ("Icons/SettingsIcons_hover@2x.png");
public static readonly ResourceLibrary.Resource<Texture2D> Icons_sg_graph_icon_png = new ("Icons/sg_graph_icon.png");
public static readonly ResourceLibrary.Resource<Texture2D> Icons_sg_graph_icon_gray_dark_png = new ("Icons/sg_graph_icon_gray_dark.png");
public static readonly ResourceLibrary.Resource<Texture2D> Icons_sg_graph_icon_gray_light_png = new ("Icons/sg_graph_icon_gray_light.png");
public static readonly ResourceLibrary.Resource<Texture2D> Icons_sg_subgraph_icon_png = new ("Icons/sg_subgraph_icon.png");
public static readonly ResourceLibrary.Resource<Texture2D> Icons_sg_subgraph_icon_gray_dark_png = new ("Icons/sg_subgraph_icon_gray_dark.png");
public static readonly ResourceLibrary.Resource<Texture2D> Icons_sg_subgraph_icon_gray_light_png = new ("Icons/sg_subgraph_icon_gray_light.png");
public static readonly ResourceLibrary.Resource<Texture2D> Cinemachine_png = new ("Cinemachine.png");
public static readonly ResourceLibrary.Resource<Texture2D> InspectorIcons_FreeformLight_png = new ("InspectorIcons/FreeformLight.png");
public static readonly ResourceLibrary.Resource<Texture2D> InspectorIcons_GlobalLight_png = new ("InspectorIcons/GlobalLight.png");
public static readonly ResourceLibrary.Resource<Texture2D> InspectorIcons_ParametricLight_png = new ("InspectorIcons/ParametricLight.png");
public static readonly ResourceLibrary.Resource<Texture2D> InspectorIcons_PointLight_png = new ("InspectorIcons/PointLight.png");
public static readonly ResourceLibrary.Resource<Texture2D> InspectorIcons_SpriteLight_png = new ("InspectorIcons/SpriteLight.png");
public static readonly ResourceLibrary.Resource<Texture2D> LightCapBottomLeft_png = new ("LightCapBottomLeft.png");
public static readonly ResourceLibrary.Resource<Texture2D> LightCapBottomRight_png = new ("LightCapBottomRight.png");
public static readonly ResourceLibrary.Resource<Texture2D> LightCapDown_png = new ("LightCapDown.png");
public static readonly ResourceLibrary.Resource<Texture2D> LightCapTopLeft_png = new ("LightCapTopLeft.png");
public static readonly ResourceLibrary.Resource<Texture2D> LightCapTopRight_png = new ("LightCapTopRight.png");
public static readonly ResourceLibrary.Resource<Texture2D> LightCapUp_png = new ("LightCapUp.png");
public static readonly ResourceLibrary.Resource<Texture2D> PixelPerfectCamera_png = new ("PixelPerfectCamera.png");
public static readonly ResourceLibrary.Resource<Texture2D> SceneViewIcons_FreeformLight_png = new ("SceneViewIcons/FreeformLight.png");
public static readonly ResourceLibrary.Resource<Texture2D> SceneViewIcons_GlobalLight_png = new ("SceneViewIcons/GlobalLight.png");
public static readonly ResourceLibrary.Resource<Texture2D> SceneViewIcons_ParametricLight_png = new ("SceneViewIcons/ParametricLight.png");
public static readonly ResourceLibrary.Resource<Texture2D> SceneViewIcons_PointLight_png = new ("SceneViewIcons/PointLight.png");
public static readonly ResourceLibrary.Resource<Texture2D> SceneViewIcons_SpriteLight_png = new ("SceneViewIcons/SpriteLight.png");
public static readonly ResourceLibrary.Resource<Texture2D> Sparkle_png = new ("Sparkle.png");
public static readonly ResourceLibrary.Resource<Texture2D> Path_pointHovered_png = new ("Path/pointHovered.png");
public static readonly ResourceLibrary.Resource<Texture2D> Path_pointNormal_png = new ("Path/pointNormal.png");
public static readonly ResourceLibrary.Resource<Texture2D> Path_pointPreview_png = new ("Path/pointPreview.png");
public static readonly ResourceLibrary.Resource<Texture2D> Path_pointRemovePreview_png = new ("Path/pointRemovePreview.png");
public static readonly ResourceLibrary.Resource<Texture2D> Path_pointSelected_png = new ("Path/pointSelected.png");
public static readonly ResourceLibrary.Resource<Texture2D> Path_tangentNormal_png = new ("Path/tangentNormal.png");
public static readonly ResourceLibrary.Resource<Texture2D> ShapeTool_png = new ("ShapeTool.png");
public static readonly ResourceLibrary.Resource<Texture2D> ShapeToolPro_png = new ("ShapeToolPro.png");
public static readonly ResourceLibrary.Resource<Texture2D> TangentBroken_png = new ("TangentBroken.png");
public static readonly ResourceLibrary.Resource<Texture2D> TangentBrokenPro_png = new ("TangentBrokenPro.png");
public static readonly ResourceLibrary.Resource<Texture2D> TangentContinuous_png = new ("TangentContinuous.png");
public static readonly ResourceLibrary.Resource<Texture2D> TangentContinuousPro_png = new ("TangentContinuousPro.png");
public static readonly ResourceLibrary.Resource<Texture2D> TangentLinear_png = new ("TangentLinear.png");
public static readonly ResourceLibrary.Resource<Texture2D> TangentLinearPro_png = new ("TangentLinearPro.png");
public static readonly ResourceLibrary.Resource<Texture2D> DecalGizmo_png = new ("DecalGizmo.png");
#endregion GENERATED
    }
}