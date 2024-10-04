using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using Object = UnityEngine.Object;

namespace Neeto
{
    public static class AddressLibrary
    {
        [QuickAction]
        public static void Script()
        {
            var script = ScriptGenerator.FindScript(typeof(AddressLibrary));
            var groups = GetAllAddressableLabels();

            Debug.Log("Generating Addressables references");
            var startTime = System.DateTime.Now;


            // get generated region
            var variableCache = new HashSet<string>();
            var changeCache = new Dictionary<string, string>();
            var signatureBuilder = new StringBuilder();
            var refs = typeof(AddressLibrary).Assembly.GetReferencedAssemblies().Select(asm => asm.ToString());

            var sb = new StringBuilder();
            sb.Append("\n");

            // interate Addressables groups
            foreach (var label in groups)
            {
                if (label.Equals("default"))
                    continue;

                // generate class definition
                variableCache.Clear();
                sb.AppendLine($"\t\tpublic static class {ScriptGenerator.Codify(label)}\n\t\t{{");
                foreach (var location in Addressables.LoadResourceLocationsAsync(label).WaitForCompletion())
                {
                    var assetPath = location.InternalId;
                    var assetName = Path.GetFileNameWithoutExtension(assetPath);
                    var variableName = assetName = ScriptGenerator.Codify(assetName);

                    if (!refs.Contains(AssetDatabase.LoadAssetAtPath<Object>(assetPath).GetType().Assembly.FullName))
                    {
                        continue;
                    }


                    if (variableCache.Contains(assetName))
                    {
                        // try to resolve with type specifier...
                        var newName = variableName += "_" + location.ResourceType.Name;

                        int c = 0;
                        while (variableCache.Contains(newName))
                        {
                            newName = variableName + "_" + c++;

                            //Debug.LogError($"Variable Collision '{variableName}' aborting...", asset);
                            //return;
                        }
                    }
                    variableCache.Add(variableName);

                    // TODO
                    // detect signature change for automatic fixes or bugs

                    var typeName = location.ResourceType.FullName switch
                    {
                        "UnityEngine.ResourceManagement.ResourceProviders.SceneInstance" => nameof(SceneAddress),
                        _ => $"Asset<{location.ResourceType.FullName}>"
                    };

                    sb.AppendLine($"\t\t\tpublic static readonly {typeName} {variableName} = new {typeName}(\"{location.PrimaryKey}\", \"{assetPath}\");");
                }
                sb.AppendLine("\t\t}\n");
            }

            // write signatures dictionary
            {

                signatureBuilder.Insert(0,
        @"
#if UNITY_EDITOR
        public static readonly Dictionary<string, string> signatures = new Dictionary<string, string>
        {
");
                signatureBuilder.Append(
        @"        };
#endif
");
                sb.Insert(0, signatureBuilder);
            }

            //ScriptGenerator.OverwriteRegion(script, "GENERATED", sb.ToString());
            var elapsed = (System.DateTime.Now - startTime).TotalMilliseconds;
            Debug.Log($"Successfully generated Addressables references after {elapsed} ms", script);

            ScriptGenerator.OverwriteRegion(script, "GENERATED", sb.ToString());

        }

        public static List<string> GetAllAddressableGroups()
        {
            // Find all AddressableAssetGroup assets in the project
            string[] guids = AssetDatabase.FindAssets("t: AddressableAssetGroup");
            var groupNames = new List<string>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var group = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (group != null)
                {
                    groupNames.Add(group.name);
                }
            }

            return groupNames;
        }
        public static List<string> GetAllAddressableLabels()
        {
            // Find all AddressableAssetGroup assets in the project
            string[] guids = AssetDatabase.FindAssets("t: AddressableAssetSettings");

            var obj = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guids[0]));

            var labels = obj.GetType().GetMethod("GetLabels").Invoke(obj, null) as List<string>;

            return labels;
        }

        #region STRUCTURES
        public struct Address<T> : IAddress<T> where T : Object
        {
            public string primaryKey { get; }
            public string assetPath { get; }

            private T _cached;
            private int _referenceCount;
            private AsyncOperationHandle<T> _handle;

            public Address(string primaryKey, string assetPath = "")
            {
                this.primaryKey = primaryKey;
                this.assetPath = assetPath;
                _cached = default;
                _referenceCount = 0;
                _handle = default;
            }

            private void Retain()
            {
                _referenceCount++;
            }

            public void Release()
            {
                if (_referenceCount > 0)
                {
                    _referenceCount--;
                    if (_referenceCount == 0)
                    {
                        Addressables.Release(_handle);
                        _cached = default;
                        _handle = default;
                    }
                }
            }

            public T Load()
            {
                if (_cached != null)
                {
                    Retain();
                    return _cached;
                }

                _handle = Addressables.LoadAssetAsync<T>(primaryKey);
                _cached = _handle.WaitForCompletion();
                Retain();
                return _cached;
            }

            public async Task<T> LoadAsync()
            {
                if (_cached != null)
                {
                    Retain();
                    return _cached;
                }

                _handle = Addressables.LoadAssetAsync<T>(primaryKey);
                _cached = await _handle.Task;
                Retain();
                return _cached;
            }
        }
        public interface IAddress
        {
            public string primaryKey { get; }
        }
        public interface IAddress<T> : IAddress // : IAsset<T> where T : Object
        {
            public T Load();
        }
        public struct SceneAddress : IAddress<SceneInstance>
        {
            public string primaryKey { get; }
            public string assetPath { get; }

            public SceneAddress(string _primaryKey, string _assetPath = "")
            {
                primaryKey = _primaryKey;
                assetPath = _assetPath;
            }

            public SceneInstance Load()
            {
                return Addressables.LoadSceneAsync(assetPath, UnityEngine.SceneManagement.LoadSceneMode.Additive).WaitForCompletion();
            }
        }
        #endregion

        #region GENERATED

#if UNITY_EDITOR
        public static readonly Dictionary<string, string> signatures = new Dictionary<string, string>
        {
        };
#endif

		public static class Spells
		{
			public static readonly Address<UnityEngine.GameObject> Updraft = new Address<UnityEngine.GameObject>("Assets/Gylfie/Prefabs/Updraft.prefab", "Assets/Interwoven/Prefabs/Spells/Prefabs/Updraft.prefab");
			public static readonly Address<UnityEngine.GameObject> Gust = new Address<UnityEngine.GameObject>("Assets/Gylfie/Prefabs/Gust.prefab", "Assets/Interwoven/Prefabs/Spells/Prefabs/Gust.prefab");
		}

		public static class Utils
		{
		}

		public static class CharacterControllers
		{
		}

		public static class ColorPalettes
		{
		}

		public static class Colors
		{
		}

		public static class ComboSystems
		{
		}

		public static class Animations
		{
		}

		public static class SwordAnimations
		{
		}

		public static class SpellAnimations
		{
		}

		public static class LocomotionAnimations
		{
		}

		public static class CreatureAnimations
		{
		}

		public static class ReactAnims
		{
		}

		public static class Prefabs
		{
			public static readonly Address<UnityEngine.GameObject> Character_Dialogue = new Address<UnityEngine.GameObject>("Assets/Gylfie/Dialogue/Character_Dialogue.prefab", "Assets/Interwoven/Dialogue/Character_Dialogue.prefab");
			public static readonly Address<UnityEngine.GameObject> Overhead_Dialogue_Trigger = new Address<UnityEngine.GameObject>("Assets/Gylfie/Scripts/CharacterLogic/CharacterUI/Overhead_Dialogue_Trigger.prefab", "Assets/Interwoven/Scripts/CharacterLogic/CharacterUI/Overhead_Dialogue_Trigger.prefab");
			public static readonly Address<UnityEngine.GameObject> Scene_Path = new Address<UnityEngine.GameObject>("Assets/Gylfie/Levels/02_Alomere/NPC/Prefabs/Scene_Path.prefab", "Assets/Interwoven/Levels/02_Alomere/NPC/Prefabs/Scene_Path.prefab");
			public static readonly Address<UnityEngine.GameObject> Updraft = new Address<UnityEngine.GameObject>("Assets/Gylfie/Prefabs/Updraft.prefab", "Assets/Interwoven/Prefabs/Spells/Prefabs/Updraft.prefab");
			public static readonly Address<UnityEngine.GameObject> Gust = new Address<UnityEngine.GameObject>("Assets/Gylfie/Prefabs/Gust.prefab", "Assets/Interwoven/Prefabs/Spells/Prefabs/Gust.prefab");
		}

		public static class Variables
		{
		}

		public static class Scenes
		{
		}

		public static class PostProcessing
		{
		}

		public static class RagdollLogic
		{
		}

		public static class UI
		{
			public static readonly Address<UnityEngine.Texture2D> icon_graphics = new Address<UnityEngine.Texture2D>("icon_graphics", "Assets/Interwoven/UI/Icons/icon_graphics.png");
			public static readonly Address<UnityEngine.Sprite> icon_graphics_Sprite = new Address<UnityEngine.Sprite>("icon_graphics", "Assets/Interwoven/UI/Icons/icon_graphics.png");
			public static readonly Address<UnityEngine.Texture2D> icon_controller = new Address<UnityEngine.Texture2D>("Vector", "Assets/Interwoven/UI/Icons/icon_controller.png");
			public static readonly Address<UnityEngine.Sprite> icon_controller_Sprite = new Address<UnityEngine.Sprite>("Vector", "Assets/Interwoven/UI/Icons/icon_controller.png");
			public static readonly Address<UnityEngine.Texture2D> icon_accessibility = new Address<UnityEngine.Texture2D>("icon_accessibility", "Assets/Interwoven/UI/Icons/icon_accessibility.png");
			public static readonly Address<UnityEngine.Sprite> icon_accessibility_Sprite = new Address<UnityEngine.Sprite>("icon_accessibility", "Assets/Interwoven/UI/Icons/icon_accessibility.png");
			public static readonly Address<UnityEngine.Texture2D> icon_sound = new Address<UnityEngine.Texture2D>("icon_sound", "Assets/Interwoven/UI/Icons/icon_sound.png");
			public static readonly Address<UnityEngine.Sprite> icon_sound_Sprite = new Address<UnityEngine.Sprite>("icon_sound", "Assets/Interwoven/UI/Icons/icon_sound.png");
			public static readonly Address<TMPro.TMP_FontAsset> Normal_Font = new Address<TMPro.TMP_FontAsset>("font_normal", "Assets/Interwoven/UI/Fonts/Normal Font.asset");
			public static readonly Address<UnityEngine.Material> Normal_Font_Material = new Address<UnityEngine.Material>("font_normal", "Assets/Interwoven/UI/Fonts/Normal Font.asset");
			public static readonly Address<UnityEngine.Texture2D> Normal_Font_Texture2D = new Address<UnityEngine.Texture2D>("font_normal", "Assets/Interwoven/UI/Fonts/Normal Font.asset");
			public static readonly Address<TMPro.TMP_FontAsset> Title_Font = new Address<TMPro.TMP_FontAsset>("font_title", "Assets/Interwoven/UI/Fonts/Title Font.asset");
			public static readonly Address<UnityEngine.Texture2D> Title_Font_Texture2D = new Address<UnityEngine.Texture2D>("font_title", "Assets/Interwoven/UI/Fonts/Title Font.asset");
			public static readonly Address<UnityEngine.Material> Title_Font_Material = new Address<UnityEngine.Material>("font_title", "Assets/Interwoven/UI/Fonts/Title Font.asset");
			public static readonly Address<UnityEngine.Texture2D> tab_left_pressed = new Address<UnityEngine.Texture2D>("tab_left_pressed", "Assets/Interwoven/UI/Elements/Tabs/tab_left_pressed.png");
			public static readonly Address<UnityEngine.Sprite> tab_left_pressed_Sprite = new Address<UnityEngine.Sprite>("tab_left_pressed", "Assets/Interwoven/UI/Elements/Tabs/tab_left_pressed.png");
			public static readonly Address<UnityEngine.Texture2D> tab_square_pressed = new Address<UnityEngine.Texture2D>("tab_square_pressed", "Assets/Interwoven/UI/Elements/Tabs/tab_square_pressed.png");
			public static readonly Address<UnityEngine.Sprite> tab_square_pressed_Sprite = new Address<UnityEngine.Sprite>("tab_square_pressed", "Assets/Interwoven/UI/Elements/Tabs/tab_square_pressed.png");
			public static readonly Address<UnityEngine.Texture2D> tab_right_pressed = new Address<UnityEngine.Texture2D>("tab_right_pressed", "Assets/Interwoven/UI/Elements/Tabs/tab_right_pressed.png");
			public static readonly Address<UnityEngine.Sprite> tab_right_pressed_Sprite = new Address<UnityEngine.Sprite>("tab_right_pressed", "Assets/Interwoven/UI/Elements/Tabs/tab_right_pressed.png");
			public static readonly Address<UnityEngine.Texture2D> tab_left = new Address<UnityEngine.Texture2D>("tab_left", "Assets/Interwoven/UI/Elements/Tabs/tab_left.png");
			public static readonly Address<UnityEngine.Sprite> tab_left_Sprite = new Address<UnityEngine.Sprite>("tab_left", "Assets/Interwoven/UI/Elements/Tabs/tab_left.png");
			public static readonly Address<UnityEngine.Texture2D> tab_square = new Address<UnityEngine.Texture2D>("tab_square", "Assets/Interwoven/UI/Elements/Tabs/tab_square.png");
			public static readonly Address<UnityEngine.Sprite> tab_square_Sprite = new Address<UnityEngine.Sprite>("tab_square", "Assets/Interwoven/UI/Elements/Tabs/tab_square.png");
			public static readonly Address<UnityEngine.Texture2D> tab_right = new Address<UnityEngine.Texture2D>("tab_right", "Assets/Interwoven/UI/Elements/Tabs/tab_right.png");
			public static readonly Address<UnityEngine.Sprite> tab_right_Sprite = new Address<UnityEngine.Sprite>("tab_right", "Assets/Interwoven/UI/Elements/Tabs/tab_right.png");
			public static readonly Address<UnityEngine.Texture2D> button_01_pressed = new Address<UnityEngine.Texture2D>("button_pressed", "Assets/Interwoven/UI/Elements/Buttons/button_01_pressed.png");
			public static readonly Address<UnityEngine.Sprite> button_01_pressed_Sprite = new Address<UnityEngine.Sprite>("button_pressed", "Assets/Interwoven/UI/Elements/Buttons/button_01_pressed.png");
			public static readonly Address<UnityEngine.Texture2D> Pause_Menu_Gradient = new Address<UnityEngine.Texture2D>("Pause Menu Gradient", "Assets/Interwoven/UI/Crabby/Pause Menu Gradient.png");
			public static readonly Address<UnityEngine.Sprite> Pause_Menu_Gradient_Sprite = new Address<UnityEngine.Sprite>("Pause Menu Gradient", "Assets/Interwoven/UI/Crabby/Pause Menu Gradient.png");
			public static readonly Address<UnityEngine.Texture2D> button_01 = new Address<UnityEngine.Texture2D>("button", "Assets/Interwoven/UI/Elements/Buttons/button_01.png");
			public static readonly Address<UnityEngine.Sprite> button_01_Sprite = new Address<UnityEngine.Sprite>("button", "Assets/Interwoven/UI/Elements/Buttons/button_01.png");
			public static readonly Address<UnityEngine.Texture2D> gradient_top = new Address<UnityEngine.Texture2D>("gradient_top", "Assets/Interwoven/UI/Crabby/gradient_top.png");
			public static readonly Address<UnityEngine.Sprite> gradient_top_Sprite = new Address<UnityEngine.Sprite>("gradient_top", "Assets/Interwoven/UI/Crabby/gradient_top.png");
			public static readonly Address<UnityEngine.Texture2D> rectangle_gradient = new Address<UnityEngine.Texture2D>("rectangle_gradient", "Assets/Interwoven/UI/Crabby/rectangle_gradient.png");
			public static readonly Address<UnityEngine.Sprite> rectangle_gradient_Sprite = new Address<UnityEngine.Sprite>("rectangle_gradient", "Assets/Interwoven/UI/Crabby/rectangle_gradient.png");
			public static readonly Address<UnityEngine.Texture2D> rectangle_grandient_from_left = new Address<UnityEngine.Texture2D>("rectangle_grandient_from_left", "Assets/Interwoven/UI/Crabby/rectangle_grandient_from_left.png");
			public static readonly Address<UnityEngine.Sprite> rectangle_grandient_from_left_Sprite = new Address<UnityEngine.Sprite>("rectangle_grandient_from_left", "Assets/Interwoven/UI/Crabby/rectangle_grandient_from_left.png");
			public static readonly Address<UnityEngine.Texture2D> gradient_left = new Address<UnityEngine.Texture2D>("gradient_left", "Assets/Interwoven/UI/Crabby/gradient_left.png");
			public static readonly Address<UnityEngine.Sprite> gradient_left_Sprite = new Address<UnityEngine.Sprite>("gradient_left", "Assets/Interwoven/UI/Crabby/gradient_left.png");
			public static readonly Address<UnityEngine.GameObject> Dialogue_Manager = new Address<UnityEngine.GameObject>("Assets/Gylfie/Dialogue/Dialogue Manager.prefab", "Assets/Interwoven/Dialogue/Dialogue Manager.prefab");
			public static readonly Address<UnityEngine.Texture2D> heart = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/heart.png", "Assets/Interwoven/UI/Texture/GUI_Textures/heart.png");
			public static readonly Address<UnityEngine.Sprite> heart_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/heart.png", "Assets/Interwoven/UI/Texture/GUI_Textures/heart.png");
			public static readonly Address<UnityEngine.Texture2D> hazard_sign = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/hazard_sign.png", "Assets/Interwoven/UI/Texture/GUI_Textures/hazard_sign.png");
			public static readonly Address<UnityEngine.Sprite> hazard_sign_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/hazard_sign.png", "Assets/Interwoven/UI/Texture/GUI_Textures/hazard_sign.png");
			public static readonly Address<UnityEngine.Texture2D> cog = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/cog.png", "Assets/Interwoven/UI/Texture/GUI_Textures/cog.png");
			public static readonly Address<UnityEngine.Sprite> cog_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/cog.png", "Assets/Interwoven/UI/Texture/GUI_Textures/cog.png");
			public static readonly Address<UnityEngine.Texture2D> interdiction = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/interdiction.png", "Assets/Interwoven/UI/Texture/GUI_Textures/interdiction.png");
			public static readonly Address<UnityEngine.Sprite> interdiction_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/interdiction.png", "Assets/Interwoven/UI/Texture/GUI_Textures/interdiction.png");
			public static readonly Address<UnityEngine.Texture2D> piercing_sword = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/game-icons.net/piercing_sword.png", "Assets/Interwoven/UI/Texture/Gameplay_Textures/piercing_sword.png");
			public static readonly Address<UnityEngine.Sprite> piercing_sword_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/game-icons.net/piercing_sword.png", "Assets/Interwoven/UI/Texture/Gameplay_Textures/piercing_sword.png");
			public static readonly Address<UnityEngine.Texture2D> keyboard = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/keyboard.png", "Assets/Interwoven/UI/Texture/GUI_Textures/keyboard.png");
			public static readonly Address<UnityEngine.Sprite> keyboard_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/keyboard.png", "Assets/Interwoven/UI/Texture/GUI_Textures/keyboard.png");
			public static readonly Address<UnityEngine.Texture2D> joystick = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/joystick.png", "Assets/Interwoven/UI/Texture/GUI_Textures/joystick.png");
			public static readonly Address<UnityEngine.Sprite> joystick_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/joystick.png", "Assets/Interwoven/UI/Texture/GUI_Textures/joystick.png");
			public static readonly Address<UnityEngine.Texture2D> paint_brush = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/paint-brush.png", "Assets/Interwoven/UI/Texture/GUI_Textures/paint-brush.png");
			public static readonly Address<UnityEngine.Sprite> paint_brush_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/paint-brush.png", "Assets/Interwoven/UI/Texture/GUI_Textures/paint-brush.png");
			public static readonly Address<UnityEngine.Texture2D> exit = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/exit.png", "Assets/Interwoven/UI/Texture/GUI_Textures/exit.png");
			public static readonly Address<UnityEngine.Sprite> exit_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/exit.png", "Assets/Interwoven/UI/Texture/GUI_Textures/exit.png");
			public static readonly Address<UnityEngine.Texture2D> signal = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/signal.png", "Assets/Interwoven/UI/Texture/GUI_Textures/signal.png");
			public static readonly Address<UnityEngine.Sprite> signal_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/signal.png", "Assets/Interwoven/UI/Texture/GUI_Textures/signal.png");
			public static readonly Address<UnityEngine.Texture2D> info = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/info.png", "Assets/Interwoven/UI/Texture/GUI_Textures/info.png");
			public static readonly Address<UnityEngine.Sprite> info_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/info.png", "Assets/Interwoven/UI/Texture/GUI_Textures/info.png");
			public static readonly Address<UnityEngine.Texture2D> recycle = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/recycle.png", "Assets/Interwoven/UI/Texture/GUI_Textures/recycle.png");
			public static readonly Address<UnityEngine.Sprite> recycle_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/recycle.png", "Assets/Interwoven/UI/Texture/GUI_Textures/recycle.png");
			public static readonly Address<UnityEngine.Texture2D> pc = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/pc.png", "Assets/Interwoven/UI/Texture/GUI_Textures/pc.png");
			public static readonly Address<UnityEngine.Sprite> pc_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/pc.png", "Assets/Interwoven/UI/Texture/GUI_Textures/pc.png");
			public static readonly Address<UnityEngine.Texture2D> console_controller = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/console-controller.png", "Assets/Interwoven/UI/Texture/GUI_Textures/console-controller.png");
			public static readonly Address<UnityEngine.Sprite> console_controller_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/console-controller.png", "Assets/Interwoven/UI/Texture/GUI_Textures/console-controller.png");
			public static readonly Address<UnityEngine.Texture2D> check_mark = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/check_mark.png", "Assets/Interwoven/UI/Texture/GUI_Textures/check_mark.png");
			public static readonly Address<UnityEngine.Sprite> check_mark_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/check_mark.png", "Assets/Interwoven/UI/Texture/GUI_Textures/check_mark.png");
			public static readonly Address<UnityEngine.Texture2D> player_previous = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/player_previous.png", "Assets/Interwoven/UI/Texture/GUI_Textures/player_previous.png");
			public static readonly Address<UnityEngine.Sprite> player_previous_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/player_previous.png", "Assets/Interwoven/UI/Texture/GUI_Textures/player_previous.png");
			public static readonly Address<UnityEngine.Texture2D> gladius = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/game-icons.net/gladius.png", "Assets/Interwoven/UI/Texture/Gameplay_Textures/gladius.png");
			public static readonly Address<UnityEngine.Sprite> gladius_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/game-icons.net/gladius.png", "Assets/Interwoven/UI/Texture/Gameplay_Textures/gladius.png");
			public static readonly Address<UnityEngine.Texture2D> help = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/help.png", "Assets/Interwoven/UI/Texture/GUI_Textures/help.png");
			public static readonly Address<UnityEngine.Sprite> help_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/help.png", "Assets/Interwoven/UI/Texture/GUI_Textures/help.png");
			public static readonly Address<UnityEngine.Texture2D> settings_knobs = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/settings-knobs.png", "Assets/Interwoven/UI/Texture/GUI_Textures/settings-knobs.png");
			public static readonly Address<UnityEngine.Sprite> settings_knobs_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/settings-knobs.png", "Assets/Interwoven/UI/Texture/GUI_Textures/settings-knobs.png");
			public static readonly Address<UnityEngine.Texture2D> padlock = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/padlock.png", "Assets/Interwoven/UI/Texture/GUI_Textures/padlock.png");
			public static readonly Address<UnityEngine.Sprite> padlock_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/padlock.png", "Assets/Interwoven/UI/Texture/GUI_Textures/padlock.png");
			public static readonly Address<UnityEngine.Texture2D> path_distance = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/game-icons.net/path_distance.png", "Assets/Interwoven/UI/Texture/Gameplay_Textures/path_distance.png");
			public static readonly Address<UnityEngine.Sprite> path_distance_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/game-icons.net/path_distance.png", "Assets/Interwoven/UI/Texture/Gameplay_Textures/path_distance.png");
			public static readonly Address<UnityEngine.Texture2D> expand = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/expand.png", "Assets/Interwoven/UI/Texture/GUI_Textures/expand.png");
			public static readonly Address<UnityEngine.Sprite> expand_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/expand.png", "Assets/Interwoven/UI/Texture/GUI_Textures/expand.png");
			public static readonly Address<UnityEngine.Texture2D> shield_reflect = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/game-icons.net/shield_reflect.png", "Assets/Interwoven/UI/Texture/Gameplay_Textures/shield_reflect.png");
			public static readonly Address<UnityEngine.Sprite> shield_reflect_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/game-icons.net/shield_reflect.png", "Assets/Interwoven/UI/Texture/Gameplay_Textures/shield_reflect.png");
			public static readonly Address<UnityEngine.Texture2D> shield = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/game-icons.net/shield.png", "Assets/Interwoven/UI/Texture/Gameplay_Textures/shield.png");
			public static readonly Address<UnityEngine.Sprite> shield_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/game-icons.net/shield.png", "Assets/Interwoven/UI/Texture/Gameplay_Textures/shield.png");
			public static readonly Address<UnityEngine.Texture2D> talk = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/game-icons.net/talk.png", "Assets/Interwoven/UI/Texture/Gameplay_Textures/talk.png");
			public static readonly Address<UnityEngine.Sprite> talk_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/game-icons.net/talk.png", "Assets/Interwoven/UI/Texture/Gameplay_Textures/talk.png");
			public static readonly Address<UnityEngine.Texture2D> tornado = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/game-icons.net/tornado.png", "Assets/Interwoven/UI/Texture/Gameplay_Textures/tornado.png");
			public static readonly Address<UnityEngine.Sprite> tornado_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/game-icons.net/tornado.png", "Assets/Interwoven/UI/Texture/Gameplay_Textures/tornado.png");
			public static readonly Address<UnityEngine.Texture2D> wind_slap = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/game-icons.net/wind_slap.png", "Assets/Interwoven/UI/Texture/Gameplay_Textures/wind_slap.png");
			public static readonly Address<UnityEngine.Sprite> wind_slap_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/game-icons.net/wind_slap.png", "Assets/Interwoven/UI/Texture/Gameplay_Textures/wind_slap.png");
			public static readonly Address<UnityEngine.GameObject> Worldspace_Input_Event = new Address<UnityEngine.GameObject>("Assets/Gylfie/UI/HUD/Worldspace Input Event.prefab", "Assets/Interwoven/UI/HUD/Worldspace Input Event.prefab");
			public static readonly Address<UnityEngine.GameObject> InteractablePrompt = new Address<UnityEngine.GameObject>("Assets/Gylfie/_scripts/GameLogic/InteractablePrompt.prefab", "Assets/Interwoven/Scripts/Game/InteractablePrompt.prefab");
			public static readonly Address<UnityEngine.Texture2D> padlock_open = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/padlock-open.png", "Assets/Interwoven/UI/Texture/GUI_Textures/padlock-open.png");
			public static readonly Address<UnityEngine.Sprite> padlock_open_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/padlock-open.png", "Assets/Interwoven/UI/Texture/GUI_Textures/padlock-open.png");
			public static readonly Address<UnityEngine.Texture2D> power_button = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/power-button.png", "Assets/Interwoven/UI/Texture/GUI_Textures/power-button.png");
			public static readonly Address<UnityEngine.Sprite> power_button_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/power-button.png", "Assets/Interwoven/UI/Texture/GUI_Textures/power-button.png");
			public static readonly Address<UnityEngine.Texture2D> transform = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Icons/GUIIcons/transform.png", "Assets/Interwoven/UI/Texture/GUI_Textures/transform.png");
			public static readonly Address<UnityEngine.Sprite> transform_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Icons/GUIIcons/transform.png", "Assets/Interwoven/UI/Texture/GUI_Textures/transform.png");
			public static readonly Address<UnityEngine.Texture2D> refresh = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/refresh.png", "Assets/Interwoven/UI/Texture/GUI_Textures/refresh.png");
			public static readonly Address<UnityEngine.Sprite> refresh_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/refresh.png", "Assets/Interwoven/UI/Texture/GUI_Textures/refresh.png");
			public static readonly Address<UnityEngine.Texture2D> cancel = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/cancel.png", "Assets/Interwoven/UI/Texture/GUI_Textures/cancel.png");
			public static readonly Address<UnityEngine.Sprite> cancel_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/cancel.png", "Assets/Interwoven/UI/Texture/GUI_Textures/cancel.png");
			public static readonly Address<UnityEngine.Texture2D> plain_padlock = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/plain-padlock.png", "Assets/Interwoven/UI/Texture/GUI_Textures/plain-padlock.png");
			public static readonly Address<UnityEngine.Sprite> plain_padlock_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/plain-padlock.png", "Assets/Interwoven/UI/Texture/GUI_Textures/plain-padlock.png");
			public static readonly Address<UnityEngine.Texture2D> vr_headset = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Icons/GUIIcons/vr-headset.png", "Assets/Interwoven/UI/Texture/GUI_Textures/vr-headset.png");
			public static readonly Address<UnityEngine.Sprite> vr_headset_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Icons/GUIIcons/vr-headset.png", "Assets/Interwoven/UI/Texture/GUI_Textures/vr-headset.png");
			public static readonly Address<UnityEngine.Texture2D> position_marker = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/position_marker.png", "Assets/Interwoven/UI/Texture/GUI_Textures/position_marker.png");
			public static readonly Address<UnityEngine.Sprite> position_marker_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/position_marker.png", "Assets/Interwoven/UI/Texture/GUI_Textures/position_marker.png");
			public static readonly Address<UnityEngine.Texture2D> enter = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/enter.png", "Assets/Interwoven/UI/Texture/GUI_Textures/enter.png");
			public static readonly Address<UnityEngine.Sprite> enter_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/enter.png", "Assets/Interwoven/UI/Texture/GUI_Textures/enter.png");
			public static readonly Address<UnityEngine.Texture2D> button_finger = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/button-finger.png", "Assets/Interwoven/UI/Texture/GUI_Textures/button-finger.png");
			public static readonly Address<UnityEngine.Sprite> button_finger_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/button-finger.png", "Assets/Interwoven/UI/Texture/GUI_Textures/button-finger.png");
			public static readonly Address<UnityEngine.Texture2D> magnifying_glass = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/magnifying_glass.png", "Assets/Interwoven/UI/Texture/GUI_Textures/magnifying_glass.png");
			public static readonly Address<UnityEngine.Sprite> magnifying_glass_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/magnifying_glass.png", "Assets/Interwoven/UI/Texture/GUI_Textures/magnifying_glass.png");
			public static readonly Address<UnityEngine.Texture2D> confirmed = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/confirmed.png", "Assets/Interwoven/UI/Texture/GUI_Textures/confirmed.png");
			public static readonly Address<UnityEngine.Sprite> confirmed_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/confirmed.png", "Assets/Interwoven/UI/Texture/GUI_Textures/confirmed.png");
			public static readonly Address<UnityEngine.Texture2D> return_arrow = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/return_arrow.png", "Assets/Interwoven/UI/Texture/GUI_Textures/return_arrow.png");
			public static readonly Address<UnityEngine.Sprite> return_arrow_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/return_arrow.png", "Assets/Interwoven/UI/Texture/GUI_Textures/return_arrow.png");
			public static readonly Address<UnityEngine.Texture2D> heart_bottle = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/heart_bottle.png", "Assets/Interwoven/UI/Texture/GUI_Textures/heart_bottle.png");
			public static readonly Address<UnityEngine.Sprite> heart_bottle_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/heart_bottle.png", "Assets/Interwoven/UI/Texture/GUI_Textures/heart_bottle.png");
			public static readonly Address<UnityEngine.Texture2D> pin = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/pin.png", "Assets/Interwoven/UI/Texture/GUI_Textures/pin.png");
			public static readonly Address<UnityEngine.Sprite> pin_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/pin.png", "Assets/Interwoven/UI/Texture/GUI_Textures/pin.png");
			public static readonly Address<UnityEngine.Texture2D> binoculars = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/binoculars.png", "Assets/Interwoven/UI/Texture/GUI_Textures/binoculars.png");
			public static readonly Address<UnityEngine.Sprite> binoculars_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/binoculars.png", "Assets/Interwoven/UI/Texture/GUI_Textures/binoculars.png");
			public static readonly Address<UnityEngine.Texture2D> expander = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/expander.png", "Assets/Interwoven/UI/Texture/GUI_Textures/expander.png");
			public static readonly Address<UnityEngine.Sprite> expander_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/expander.png", "Assets/Interwoven/UI/Texture/GUI_Textures/expander.png");
			public static readonly Address<UnityEngine.Texture2D> folder = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/folder.png", "Assets/Interwoven/UI/Texture/GUI_Textures/folder.png");
			public static readonly Address<UnityEngine.Sprite> folder_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/folder.png", "Assets/Interwoven/UI/Texture/GUI_Textures/folder.png");
			public static readonly Address<UnityEngine.Texture2D> mushroom_cloud = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/mushroom-cloud.png", "Assets/Interwoven/UI/Texture/GUI_Textures/mushroom-cloud.png");
			public static readonly Address<UnityEngine.Sprite> mushroom_cloud_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/mushroom-cloud.png", "Assets/Interwoven/UI/Texture/GUI_Textures/mushroom-cloud.png");
			public static readonly Address<UnityEngine.Texture2D> contract = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/contract.png", "Assets/Interwoven/UI/Texture/GUI_Textures/contract.png");
			public static readonly Address<UnityEngine.Sprite> contract_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/contract.png", "Assets/Interwoven/UI/Texture/GUI_Textures/contract.png");
			public static readonly Address<UnityEngine.Texture2D> halt = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/halt.png", "Assets/Interwoven/UI/Texture/GUI_Textures/halt.png");
			public static readonly Address<UnityEngine.Sprite> halt_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/halt.png", "Assets/Interwoven/UI/Texture/GUI_Textures/halt.png");
			public static readonly Address<UnityEngine.Texture2D> high_shot = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/high_shot.png", "Assets/Interwoven/UI/Texture/GUI_Textures/high_shot.png");
			public static readonly Address<UnityEngine.Sprite> high_shot_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/high_shot.png", "Assets/Interwoven/UI/Texture/GUI_Textures/high_shot.png");
			public static readonly Address<UnityEngine.Texture2D> cursor = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/cursor.png", "Assets/Interwoven/UI/Texture/GUI_Textures/cursor.png");
			public static readonly Address<UnityEngine.Sprite> cursor_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/cursor.png", "Assets/Interwoven/UI/Texture/GUI_Textures/cursor.png");
			public static readonly Address<UnityEngine.GUISkin> INW_GUI_Skin = new Address<UnityEngine.GUISkin>("Assets/Gylfie/Scripts/UIScripts/GUI/IW_GUI_Skin.guiskin", "Assets/Interwoven/UI/GUI/INW_GUI_Skin.guiskin");
			public static readonly Address<UnityEngine.GameObject> AgentUI = new Address<UnityEngine.GameObject>("AgentUI", "Assets/Interwoven/Scripts/CharacterLogic/CharacterUI/AgentUI.prefab");
			public static readonly Address<UnityEngine.Texture2D> Circle_81x = new Address<UnityEngine.Texture2D>("Circle_81x", "Assets/Interwoven/UI/Crabby/Circle_81x.png");
			public static readonly Address<UnityEngine.Sprite> Circle_81x_Sprite = new Address<UnityEngine.Sprite>("Circle_81x", "Assets/Interwoven/UI/Crabby/Circle_81x.png");
			public static readonly Address<UnityEngine.Texture2D> open_book = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/open-book.png", "Assets/Interwoven/UI/Texture/GUI_Textures/open-book.png");
			public static readonly Address<UnityEngine.Sprite> open_book_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/open-book.png", "Assets/Interwoven/UI/Texture/GUI_Textures/open-book.png");
			public static readonly Address<UnityEngine.Texture2D> stopwatch = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/stopwatch.png", "Assets/Interwoven/UI/Texture/GUI_Textures/stopwatch.png");
			public static readonly Address<UnityEngine.Sprite> stopwatch_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/stopwatch.png", "Assets/Interwoven/UI/Texture/GUI_Textures/stopwatch.png");
			public static readonly Address<UnityEngine.Texture2D> click = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/click.png", "Assets/Interwoven/UI/Texture/GUI_Textures/click.png");
			public static readonly Address<UnityEngine.Sprite> click_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/click.png", "Assets/Interwoven/UI/Texture/GUI_Textures/click.png");
			public static readonly Address<UnityEngine.Texture2D> retro_controller = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/retro-controller.png", "Assets/Interwoven/UI/Texture/GUI_Textures/retro-controller.png");
			public static readonly Address<UnityEngine.Sprite> retro_controller_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/retro-controller.png", "Assets/Interwoven/UI/Texture/GUI_Textures/retro-controller.png");
			public static readonly Address<UnityEngine.Texture2D> folder_full = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/folder_full.png", "Assets/Interwoven/UI/Texture/GUI_Textures/folder_full.png");
			public static readonly Address<UnityEngine.Sprite> folder_full_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/folder_full.png", "Assets/Interwoven/UI/Texture/GUI_Textures/folder_full.png");
			public static readonly Address<UnityEngine.Texture2D> riposte = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/riposte.png", "Assets/Interwoven/UI/Texture/GUI_Textures/riposte.png");
			public static readonly Address<UnityEngine.Sprite> riposte_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/riposte.png", "Assets/Interwoven/UI/Texture/GUI_Textures/riposte.png");
			public static readonly Address<UnityEngine.Texture2D> cycle = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/cycle.png", "Assets/Interwoven/UI/Texture/GUI_Textures/cycle.png");
			public static readonly Address<UnityEngine.Sprite> cycle_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/cycle.png", "Assets/Interwoven/UI/Texture/GUI_Textures/cycle.png");
			public static readonly Address<UnityEngine.Texture2D> backward_time = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/backward_time.png", "Assets/Interwoven/UI/Texture/GUI_Textures/backward_time.png");
			public static readonly Address<UnityEngine.Sprite> backward_time_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/backward_time.png", "Assets/Interwoven/UI/Texture/GUI_Textures/backward_time.png");
			public static readonly Address<UnityEngine.Texture2D> dial_padlock = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/dial-padlock.png", "Assets/Interwoven/UI/Texture/GUI_Textures/dial-padlock.png");
			public static readonly Address<UnityEngine.Sprite> dial_padlock_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/dial-padlock.png", "Assets/Interwoven/UI/Texture/GUI_Textures/dial-padlock.png");
			public static readonly Address<UnityEngine.Texture2D> infinity = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/infinity.png", "Assets/Interwoven/UI/Texture/GUI_Textures/infinity.png");
			public static readonly Address<UnityEngine.Sprite> infinity_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/infinity.png", "Assets/Interwoven/UI/Texture/GUI_Textures/infinity.png");
			public static readonly Address<UnityEngine.Texture2D> game_console = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/game-console.png", "Assets/Interwoven/UI/Texture/GUI_Textures/game-console.png");
			public static readonly Address<UnityEngine.Sprite> game_console_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/game-console.png", "Assets/Interwoven/UI/Texture/GUI_Textures/game-console.png");
			public static readonly Address<UnityEngine.Texture2D> mouse = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/mouse.png", "Assets/Interwoven/UI/Texture/GUI_Textures/mouse.png");
			public static readonly Address<UnityEngine.Sprite> mouse_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/mouse.png", "Assets/Interwoven/UI/Texture/GUI_Textures/mouse.png");
			public static readonly Address<UnityEngine.Texture2D> gamepad = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/gamepad.png", "Assets/Interwoven/UI/Texture/GUI_Textures/gamepad.png");
			public static readonly Address<UnityEngine.Sprite> gamepad_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/gamepad.png", "Assets/Interwoven/UI/Texture/GUI_Textures/gamepad.png");
			public static readonly Address<UnityEngine.Texture2D> move = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/move.png", "Assets/Interwoven/UI/Texture/GUI_Textures/move.png");
			public static readonly Address<UnityEngine.Sprite> move_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/move.png", "Assets/Interwoven/UI/Texture/GUI_Textures/move.png");
			public static readonly Address<UnityEngine.Texture2D> hamburger_menu = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/hamburger-menu.png", "Assets/Interwoven/UI/Texture/GUI_Textures/hamburger-menu.png");
			public static readonly Address<UnityEngine.Sprite> hamburger_menu_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/hamburger-menu.png", "Assets/Interwoven/UI/Texture/GUI_Textures/hamburger-menu.png");
			public static readonly Address<UnityEngine.Texture2D> health_cross = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/health_cross.png", "Assets/Interwoven/UI/Texture/GUI_Textures/health_cross.png");
			public static readonly Address<UnityEngine.Sprite> health_cross_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/health_cross.png", "Assets/Interwoven/UI/Texture/GUI_Textures/health_cross.png");
			public static readonly Address<UnityEngine.Texture2D> save = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/save.png", "Assets/Interwoven/UI/Texture/GUI_Textures/save.png");
			public static readonly Address<UnityEngine.Sprite> save_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/save.png", "Assets/Interwoven/UI/Texture/GUI_Textures/save.png");
			public static readonly Address<UnityEngine.Texture2D> chat_bubble = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/chat_bubble.png", "Assets/Interwoven/UI/Texture/GUI_Textures/chat_bubble.png");
			public static readonly Address<UnityEngine.Sprite> chat_bubble_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/chat_bubble.png", "Assets/Interwoven/UI/Texture/GUI_Textures/chat_bubble.png");
			public static readonly Address<UnityEngine.Texture2D> github_logo = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/github-logo.png", "Assets/Interwoven/UI/Texture/GUI_Textures/github-logo.png");
			public static readonly Address<UnityEngine.Sprite> github_logo_Sprite = new Address<UnityEngine.Sprite>("Assets/Gylfie/UI/Texture/GUI_Textures/github-logo.png", "Assets/Interwoven/UI/Texture/GUI_Textures/github-logo.png");
			public static readonly Address<UnityEngine.Texture2D> steam_icon = new Address<UnityEngine.Texture2D>("Assets/Gylfie/UI/Texture/GUI_Textures/steam-icon 1.png", "Assets/Interwoven/UI/Texture/GUI_Textures/steam-icon.png");
			public static readonly SceneAddress AreYouSure = new SceneAddress("AreYouSure", "Assets/Interwoven/Scripts/Menu/AreYouSure.unity");
		}

		public static class Icons
		{
			public static readonly Address<UnityEngine.Texture2D> icon_graphics = new Address<UnityEngine.Texture2D>("icon_graphics", "Assets/Interwoven/UI/Icons/icon_graphics.png");
			public static readonly Address<UnityEngine.Sprite> icon_graphics_Sprite = new Address<UnityEngine.Sprite>("icon_graphics", "Assets/Interwoven/UI/Icons/icon_graphics.png");
			public static readonly Address<UnityEngine.Texture2D> icon_controller = new Address<UnityEngine.Texture2D>("Vector", "Assets/Interwoven/UI/Icons/icon_controller.png");
			public static readonly Address<UnityEngine.Sprite> icon_controller_Sprite = new Address<UnityEngine.Sprite>("Vector", "Assets/Interwoven/UI/Icons/icon_controller.png");
			public static readonly Address<UnityEngine.Texture2D> icon_accessibility = new Address<UnityEngine.Texture2D>("icon_accessibility", "Assets/Interwoven/UI/Icons/icon_accessibility.png");
			public static readonly Address<UnityEngine.Sprite> icon_accessibility_Sprite = new Address<UnityEngine.Sprite>("icon_accessibility", "Assets/Interwoven/UI/Icons/icon_accessibility.png");
			public static readonly Address<UnityEngine.Texture2D> icon_sound = new Address<UnityEngine.Texture2D>("icon_sound", "Assets/Interwoven/UI/Icons/icon_sound.png");
			public static readonly Address<UnityEngine.Sprite> icon_sound_Sprite = new Address<UnityEngine.Sprite>("icon_sound", "Assets/Interwoven/UI/Icons/icon_sound.png");
		}

		public static class Fonts
		{
			public static readonly Address<TMPro.TMP_FontAsset> Normal_Font = new Address<TMPro.TMP_FontAsset>("font_normal", "Assets/Interwoven/UI/Fonts/Normal Font.asset");
			public static readonly Address<UnityEngine.Material> Normal_Font_Material = new Address<UnityEngine.Material>("font_normal", "Assets/Interwoven/UI/Fonts/Normal Font.asset");
			public static readonly Address<UnityEngine.Texture2D> Normal_Font_Texture2D = new Address<UnityEngine.Texture2D>("font_normal", "Assets/Interwoven/UI/Fonts/Normal Font.asset");
			public static readonly Address<TMPro.TMP_FontAsset> Title_Font = new Address<TMPro.TMP_FontAsset>("font_title", "Assets/Interwoven/UI/Fonts/Title Font.asset");
			public static readonly Address<UnityEngine.Texture2D> Title_Font_Texture2D = new Address<UnityEngine.Texture2D>("font_title", "Assets/Interwoven/UI/Fonts/Title Font.asset");
			public static readonly Address<UnityEngine.Material> Title_Font_Material = new Address<UnityEngine.Material>("font_title", "Assets/Interwoven/UI/Fonts/Title Font.asset");
		}

		public static class Skybox
		{
			public static readonly Address<UnityEngine.Material> Day_ish_Skybox = new Address<UnityEngine.Material>("Day ish Skybox", "Assets/Interwoven/Levels/General/Skybox/Day ish Skybox.mat");
			public static readonly Address<UnityEngine.Material> Night_Warm_Skybox = new Address<UnityEngine.Material>("Night Warm Skybox", "Assets/Interwoven/Levels/General/Skybox/Night Warm Skybox.mat");
			public static readonly Address<UnityEngine.Material> Night_Skybox = new Address<UnityEngine.Material>("Night Skybox", "Assets/Interwoven/Levels/General/Skybox/Night Skybox.mat");
		}

		public static class Toolbar
		{
			public static readonly Address<UnityEngine.Texture2D> tab_left_pressed = new Address<UnityEngine.Texture2D>("tab_left_pressed", "Assets/Interwoven/UI/Elements/Tabs/tab_left_pressed.png");
			public static readonly Address<UnityEngine.Sprite> tab_left_pressed_Sprite = new Address<UnityEngine.Sprite>("tab_left_pressed", "Assets/Interwoven/UI/Elements/Tabs/tab_left_pressed.png");
			public static readonly Address<UnityEngine.Texture2D> tab_square_pressed = new Address<UnityEngine.Texture2D>("tab_square_pressed", "Assets/Interwoven/UI/Elements/Tabs/tab_square_pressed.png");
			public static readonly Address<UnityEngine.Sprite> tab_square_pressed_Sprite = new Address<UnityEngine.Sprite>("tab_square_pressed", "Assets/Interwoven/UI/Elements/Tabs/tab_square_pressed.png");
			public static readonly Address<UnityEngine.Texture2D> tab_right_pressed = new Address<UnityEngine.Texture2D>("tab_right_pressed", "Assets/Interwoven/UI/Elements/Tabs/tab_right_pressed.png");
			public static readonly Address<UnityEngine.Sprite> tab_right_pressed_Sprite = new Address<UnityEngine.Sprite>("tab_right_pressed", "Assets/Interwoven/UI/Elements/Tabs/tab_right_pressed.png");
			public static readonly Address<UnityEngine.Texture2D> tab_left = new Address<UnityEngine.Texture2D>("tab_left", "Assets/Interwoven/UI/Elements/Tabs/tab_left.png");
			public static readonly Address<UnityEngine.Sprite> tab_left_Sprite = new Address<UnityEngine.Sprite>("tab_left", "Assets/Interwoven/UI/Elements/Tabs/tab_left.png");
			public static readonly Address<UnityEngine.Texture2D> tab_square = new Address<UnityEngine.Texture2D>("tab_square", "Assets/Interwoven/UI/Elements/Tabs/tab_square.png");
			public static readonly Address<UnityEngine.Sprite> tab_square_Sprite = new Address<UnityEngine.Sprite>("tab_square", "Assets/Interwoven/UI/Elements/Tabs/tab_square.png");
			public static readonly Address<UnityEngine.Texture2D> tab_right = new Address<UnityEngine.Texture2D>("tab_right", "Assets/Interwoven/UI/Elements/Tabs/tab_right.png");
			public static readonly Address<UnityEngine.Sprite> tab_right_Sprite = new Address<UnityEngine.Sprite>("tab_right", "Assets/Interwoven/UI/Elements/Tabs/tab_right.png");
		}

		public static class Button
		{
			public static readonly Address<UnityEngine.Texture2D> tab_left_pressed = new Address<UnityEngine.Texture2D>("tab_left_pressed", "Assets/Interwoven/UI/Elements/Tabs/tab_left_pressed.png");
			public static readonly Address<UnityEngine.Sprite> tab_left_pressed_Sprite = new Address<UnityEngine.Sprite>("tab_left_pressed", "Assets/Interwoven/UI/Elements/Tabs/tab_left_pressed.png");
			public static readonly Address<UnityEngine.Texture2D> tab_square_pressed = new Address<UnityEngine.Texture2D>("tab_square_pressed", "Assets/Interwoven/UI/Elements/Tabs/tab_square_pressed.png");
			public static readonly Address<UnityEngine.Sprite> tab_square_pressed_Sprite = new Address<UnityEngine.Sprite>("tab_square_pressed", "Assets/Interwoven/UI/Elements/Tabs/tab_square_pressed.png");
			public static readonly Address<UnityEngine.Texture2D> tab_right_pressed = new Address<UnityEngine.Texture2D>("tab_right_pressed", "Assets/Interwoven/UI/Elements/Tabs/tab_right_pressed.png");
			public static readonly Address<UnityEngine.Sprite> tab_right_pressed_Sprite = new Address<UnityEngine.Sprite>("tab_right_pressed", "Assets/Interwoven/UI/Elements/Tabs/tab_right_pressed.png");
			public static readonly Address<UnityEngine.Texture2D> tab_left = new Address<UnityEngine.Texture2D>("tab_left", "Assets/Interwoven/UI/Elements/Tabs/tab_left.png");
			public static readonly Address<UnityEngine.Sprite> tab_left_Sprite = new Address<UnityEngine.Sprite>("tab_left", "Assets/Interwoven/UI/Elements/Tabs/tab_left.png");
			public static readonly Address<UnityEngine.Texture2D> tab_square = new Address<UnityEngine.Texture2D>("tab_square", "Assets/Interwoven/UI/Elements/Tabs/tab_square.png");
			public static readonly Address<UnityEngine.Sprite> tab_square_Sprite = new Address<UnityEngine.Sprite>("tab_square", "Assets/Interwoven/UI/Elements/Tabs/tab_square.png");
			public static readonly Address<UnityEngine.Texture2D> tab_right = new Address<UnityEngine.Texture2D>("tab_right", "Assets/Interwoven/UI/Elements/Tabs/tab_right.png");
			public static readonly Address<UnityEngine.Sprite> tab_right_Sprite = new Address<UnityEngine.Sprite>("tab_right", "Assets/Interwoven/UI/Elements/Tabs/tab_right.png");
			public static readonly Address<UnityEngine.Texture2D> button_01_pressed = new Address<UnityEngine.Texture2D>("button_pressed", "Assets/Interwoven/UI/Elements/Buttons/button_01_pressed.png");
			public static readonly Address<UnityEngine.Sprite> button_01_pressed_Sprite = new Address<UnityEngine.Sprite>("button_pressed", "Assets/Interwoven/UI/Elements/Buttons/button_01_pressed.png");
			public static readonly Address<UnityEngine.Texture2D> button_01 = new Address<UnityEngine.Texture2D>("button", "Assets/Interwoven/UI/Elements/Buttons/button_01.png");
			public static readonly Address<UnityEngine.Sprite> button_01_Sprite = new Address<UnityEngine.Sprite>("button", "Assets/Interwoven/UI/Elements/Buttons/button_01.png");
		}

		public static class LUT
		{
			public static readonly Address<UnityEngine.Texture2D> Pause_Menu_Gradient = new Address<UnityEngine.Texture2D>("Pause Menu Gradient", "Assets/Interwoven/UI/Crabby/Pause Menu Gradient.png");
			public static readonly Address<UnityEngine.Sprite> Pause_Menu_Gradient_Sprite = new Address<UnityEngine.Sprite>("Pause Menu Gradient", "Assets/Interwoven/UI/Crabby/Pause Menu Gradient.png");
			public static readonly Address<UnityEngine.Texture2D> gradient_top = new Address<UnityEngine.Texture2D>("gradient_top", "Assets/Interwoven/UI/Crabby/gradient_top.png");
			public static readonly Address<UnityEngine.Sprite> gradient_top_Sprite = new Address<UnityEngine.Sprite>("gradient_top", "Assets/Interwoven/UI/Crabby/gradient_top.png");
			public static readonly Address<UnityEngine.Texture2D> gradient_left = new Address<UnityEngine.Texture2D>("gradient_left", "Assets/Interwoven/UI/Crabby/gradient_left.png");
			public static readonly Address<UnityEngine.Sprite> gradient_left_Sprite = new Address<UnityEngine.Sprite>("gradient_left", "Assets/Interwoven/UI/Crabby/gradient_left.png");
		}

		public static class Selection
		{
			public static readonly Address<UnityEngine.Texture2D> rectangle_gradient = new Address<UnityEngine.Texture2D>("rectangle_gradient", "Assets/Interwoven/UI/Crabby/rectangle_gradient.png");
			public static readonly Address<UnityEngine.Sprite> rectangle_gradient_Sprite = new Address<UnityEngine.Sprite>("rectangle_gradient", "Assets/Interwoven/UI/Crabby/rectangle_gradient.png");
			public static readonly Address<UnityEngine.Texture2D> rectangle_grandient_from_left = new Address<UnityEngine.Texture2D>("rectangle_grandient_from_left", "Assets/Interwoven/UI/Crabby/rectangle_grandient_from_left.png");
			public static readonly Address<UnityEngine.Sprite> rectangle_grandient_from_left_Sprite = new Address<UnityEngine.Sprite>("rectangle_grandient_from_left", "Assets/Interwoven/UI/Crabby/rectangle_grandient_from_left.png");
		}

		public static class Physics
		{
			public static readonly Address<UnityEngine.PhysicMaterial> Friction_Off = new Address<UnityEngine.PhysicMaterial>("Friction Off", "Assets/Interwoven/Scripts/Physics/PhysicMaterials/Friction Off.physicMaterial");
			public static readonly Address<UnityEngine.PhysicMaterial> Friction_On = new Address<UnityEngine.PhysicMaterial>("Friction On", "Assets/Interwoven/Scripts/Physics/PhysicMaterials/Friction On.physicMaterial");
		}

		public static class Shaders
		{
			public static readonly Address<UnityEngine.Shader> SpritesheetUnlit = new Address<UnityEngine.Shader>("Assets/Gylfie/Graphics/SpriteSheets/SpritesheetUnlit.shadergraph", "Assets/Interwoven/Graphics/VFX/Hitspark/SpritesheetUnlit.shadergraph");
		}

		public static class VFX
		{
			public static readonly Address<UnityEngine.GameObject> Hitspark = new Address<UnityEngine.GameObject>("Assets/Gylfie/Graphics/VFX/Hitspark/Hitspark.prefab", "Assets/Interwoven/Graphics/VFX/Hitspark/Hitspark.prefab");
		}

		public static class Input
		{
			public static readonly Address<UnityEngine.InputSystem.InputActionAsset> Controls = new Address<UnityEngine.InputSystem.InputActionAsset>("Assets/Gylfie/Input/Controls.inputactions", "Assets/Interwoven/Input/Controls.inputactions");
			public static readonly Address<UnityEngine.InputSystem.InputActionReference> Controls_InputActionReference = new Address<UnityEngine.InputSystem.InputActionReference>("Assets/Gylfie/Input/Controls.inputactions", "Assets/Interwoven/Input/Controls.inputactions");
		}

		public static class Cutscene
		{
		}

#endregion GENERATED
    }
}