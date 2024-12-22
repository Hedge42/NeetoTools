using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEditor;
using System.Text;
using System;
using System.Linq;
using Object = UnityEngine.Object;
using Cysharp.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
#endif

namespace Neeto
{
    public static class AddressablesDatabase
    {
        #region GENERATED
		public static readonly AddressableAsset<UnityEngine.Texture2D> LUT_1px_png = new("ad75c300c917b414eb560b5be4abda76");
		public static readonly AddressableAsset<UnityEngine.Texture2D> LUT_1px_clear_png = new("6f9fd996685284f499257dd8e7391c56");
		public static readonly AddressableAsset<UnityEngine.Texture2D> LUT_particle_png = new("6905b9172d260f543a0b889d44c70da0");
		public static readonly AddressableAsset<UnityEngine.Texture2D> LUT_particle_soft_256_png = new("c2af1838bbb0cf946895f08736ef6f69");
		public static readonly AddressableAsset<UnityEngine.Texture2D> LUT_Gradients_gradient_64_bottom_to_top_png = new("f0739717802caa04c8b4fef66ad1f018");
		public static readonly AddressableAsset<UnityEngine.Texture2D> LUT_Gradients_gradient_64_left_to_right_png = new("359c8fb110d119f42901ee3ac02900d2");
		public static readonly AddressableAsset<UnityEngine.Texture2D> LUT_Gradients_gradient_64_right_to_left_png = new("039e218f48bc3f64fb117d1c579c229e");
		public static readonly AddressableAsset<UnityEngine.Texture2D> LUT_Gradients_gradient_64_top_to_bottom_png = new("d78225edb944c304d8f8ee69c46c9090");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_backward_time_png = new("a6698cd21590e28409d3124ccc72f81b");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_binoculars_png = new("2ff4ec649e86a464ab810af5c5e2b7ca");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_button_finger_png = new("d1708e95cfc904c46bb49b7e8060d875");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_cancel_png = new("d878633ab77abe440b1fd94ea87daa48");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_chat_bubble_png = new("bdb8716f93c12aa4b915abd9bae140e5");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_check_mark_png = new("dc609012dd39df34b83b8784ad0cc87a");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_click_png = new("a050b9af7ebc4434eb7f4701aef6c277");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_cog_png = new("8753e0012cf4f1d4182d20fd423c2175");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_confirmed_png = new("d95987caf5904e543bd5b04341000f75");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_console_controller_png = new("5c04d47d9dfa14f44a940c3787067b81");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_contract_png = new("3357b810cb759f14f9d8fbbb50cb2b95");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_cursor_png = new("45a61806064812e47a43446cdf4063be");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_cycle_png = new("a728c62543c6c6a49847e0b75126bdf9");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_d_Favorite_colored_png = new("67f454d61327fd844a6d7de6b8c464ac");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_d_Favorite_colored_Modified_png = new("a0aef060e8d14ac4d8b7112278baa2e4");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_d_Texture_Icon_png = new("2ae91a75ed7f279468394a318f091603");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_d_Texture_Icon_Modified_png = new("ad7df3ccf8f27b349abb5173a6d1333e");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_d_cs_Script_Icon_png = new("a4dccc8b8cb8f424996dae0284667845");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_d_cs_Script_Icon_Modified_png = new("5c1e1bbe7751e4b4f9fbc446f389c320");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_dial_padlock_png = new("563450e518930514ab0adb27e6b51b02");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_enter_png = new("21f74b445ae632c4d84020cd1bcf602d");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_exit_png = new("2eba1b4037f340e4d80251f33328d2ac");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_expand_png = new("ddf5a32d221bfe8469cffbf057aa9da0");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_expander_png = new("e78148ecb59bade478621f7d23b6018f");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_folder_png = new("750c9e429907f4544bdc53802bedba5b");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_folder_full_png = new("1fcdaccc6a034ec43b0c3f41887f0cc2");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_game_console_png = new("b15b9ae2adafc154696548084147204d");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_gamepad_png = new("45af7b2c1c4bfe441ae381afd78c4c65");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_github_logo_png = new("572b9d7d1d565894b9b07412b4972809");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_halt_png = new("cbbd9816c6a00b84b81334de6bc719ff");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_hamburger_menu_png = new("8ce4f5bb04648df45a4d29684e0e1817");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_hazard_sign_png = new("3f1e2d8de6f8cd543b6d4108d9646504");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_health_cross_png = new("cf8863c54bc576e4fba447e7b8c65967");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_heart_png = new("2b2caf1c45327134188b0bc773bd736d");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_heart_bottle_png = new("0c84a5b7c429dc342a52591ed8049970");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_help_png = new("53efacd0acc741a4d8895e1cb92ae945");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_high_shot_png = new("429742ae25d42f5418bea4c9584fb8f5");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_infinity_png = new("29c0bc294c46afd488fa667bb170d5ff");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_info_png = new("1aa8e592f4faf364f80b17a000f3b879");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_interdiction_png = new("44523175b0dd981458f6feaa4b93c733");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_joystick_png = new("038ee2950d723fa41909c73fb4bdba46");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_keyboard_png = new("3ff57df7ccdb7b34491acde5ea5abf26");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_magnifying_glass_png = new("090d7baf29ce8f24d86c239f5f7a8275");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_moon_png = new("499e1af3d7b2e524580cee923f54d1d7");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_mouse_png = new("734a1f2df546457438d22cd7d4254795");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_move_png = new("30ffad2260432d64c9e3933e6bd9e28b");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_mushroom_cloud_png = new("4ef867e666d122449973ed9088981846");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_open_book_png = new("242b51d75ef9e544fbf441af7b91b512");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_padlock_open_png = new("5ba66aa958a3ceb4d9e0ce9374bf0aae");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_padlock_png = new("f104f44bb621f944e81dc1efbe9a8cbc");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_paint_brush_png = new("439931ccc50e026438104eb20203dbd8");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_pc_png = new("7389ab85799624c44977f3bec94927d7");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_pin_png = new("6a66db5bb8d13b94a910f9bb91b2b3f4");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_plain_padlock_png = new("ae063166efc78694e8a53d00a4cbaec4");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_player_previous_png = new("614a560b056789a4aa44e2f0b0f27608");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_position_marker_png = new("afa830dbca6098146811ddaf00777ccb");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_power_button_png = new("3722e8e0654cc4746ae3ba71d05d4422");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_recycle_png = new("dc353321bfb517f45a9058d8b0486ac8");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_refresh_png = new("07ed60687a4e4b3499430427710af1ba");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_retro_controller_png = new("030260276d2eaf746874486807d44f41");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_return_arrow_png = new("6e72f77a4342a7448938685b24eedc33");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_riposte_png = new("ab4730ff6e241f64d92824306c6e77c9");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_save_png = new("6682ae44575f53b4b9b8b0df67f61de9");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_settings_knobs_png = new("ba064f2e043d46f4791d0a054e44bc05");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_signal_png = new("b9dba02bf8e320c4e8f3ce2c2b77b6fa");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_speaker_png = new("26483de06691d374ab12cd315f0b3fb0");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_steam_icon_png = new("3e84b2a062060064bb567641e989c291");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_stopwatch_png = new("0993a06db01f4614db730325e3a603de");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_transform_png = new("5ef70bcb7c16d1a45b3b346e2a9f5332");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_vr_headset_png = new("b551cd77b15cb534fb880c33e209132b");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_world_africa_europe_png = new("291d7f15e349d054582b597a47a1e593");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_world_america_png = new("49a83879d0b7cfe4a91aa9b60e08a30e");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_world_asia_oceania_png = new("163643563bb7fe84e99c930793656013");
		public static readonly AddressableAsset<UnityEngine.Texture2D> Textures_world_png = new("093dbd026aa6f2349bf8dba760c5ce6e");
		public static readonly AddressableAsset<UnityEngine.UIElements.PanelSettings> UI_PanelSettings_asset = new("6d3170b83d1c13a448b1434808d435de");
		public static readonly AddressableAsset<UnityEngine.UIElements.VisualTreeAsset> UI_QuickContext_UXML_uxml = new("4c8a8c6f5e364104eb46981ea5096884");
#endregion GENERATED

#if UNITY_EDITOR
        [ScriptGenerator]
        static void GenerateReferences() =>
            GenerateReferences(NGUI.FindScript(GenerateReferences));

        public static void GenerateReferences(TextAsset script)
        {
            StringBuilder sb = new();
            var path = NGUI.GetAssetDirectory(script);
            var guids = GetAddressableGuids().Where(guid => AssetDatabase.GUIDToAssetPath(guid).Contains(path));

            foreach (var guid in guids)
            {
                var asset = Addressables.LoadAssetAsync<Object>(guid).WaitForCompletion();
                var typeName = asset.GetType().FullName;
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var fieldName = ScriptGenerator.Codify(assetPath.Remove(0, path.Length));

                sb.AppendLine($"\t\tpublic static readonly AddressableAsset<{typeName}> {fieldName} = new(\"{guid}\");");
            }


            ScriptGenerator.OverwriteRegion(script, "GENERATED", sb.ToString());
        }

        static IEnumerable<AddressableAssetEntry> GetAssetEntries(Func<AddressableAssetEntry, bool> entryFilter)
        {
            var list = new List<AddressableAssetEntry>();
            AddressableAssetSettingsDefaultObject.Settings.GetAllAssets(list, false, entryFilter: entryFilter);
            return list;
        }
        public static IEnumerable<T> GetAssetEntries<T>() where T : Object
        {
            return GetAssetEntries(_ => typeof(T).IsAssignableFrom(_.TargetAsset.GetType()))
                .Select(_ => _.TargetAsset as T);
        }

        public static IEnumerable<string> GetAddressableGuids()
        {
            var list = new List<AddressableAssetEntry>();
            AddressableAssetSettingsDefaultObject.Settings.GetAllAssets(list, false);
            return list.Select(entry => entry.guid);
        }
        public static IEnumerable<T> GetAssetEntries<T>(string assetPath) where T : Object
        {
            return GetAssetEntries(entry =>
                entry.AssetPath.Contains(assetPath)
                    && typeof(T).IsAssignableFrom(entry.GetType()))
                .Select(entry => entry.TargetAsset as T);
        }
#endif
    }

    public struct AddressableAsset<T> where T : Object
    {
        public readonly string guid;

        public AddressableAsset(string guid) =>
            this.guid = guid;

        public UniTask<T> LoadAsync() =>
            Addressables.LoadAssetAsync<T>(guid).ToUniTask();
    }
};
