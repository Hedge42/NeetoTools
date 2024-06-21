using UnityEngine;
using System;
using UnityEditor;
using UnityDropdown.Editor;
//using Steamworks;

namespace Matchwork
{
    public static class WindowCommands
    {
        const string GOTO = "Go to/";
        const string BUILD = "Build/";
        const string GIT = "git/";

        static Texture2D nuke;

        [InitializeOnLoadMethod]
        static void Init()
        {
            //EditorApplication.delayCall += () =>
            //{
            //    nuke = ResourceGenerator.GUI_Textures_mushroom_cloud.Load();
            //};

            MToolbar.onRightMostToolbarGUI += () =>
            {
                if (MToolbar.IconPopupLayout(MTexture.nuke, "♥"))
                {
                    var menu = ActionMenu.Create(

                        // go to
                        (FindScript, GOTO + nameof(FindScript)),
                        (OpenBuildSettings, GOTO + nameof(OpenBuildSettings)),
                        (OpenNavigation, GOTO + nameof(OpenNavigation)),
                        (OpenOcclusion, GOTO + nameof(OpenOcclusion)),
                        (OpenPlayerSettings, GOTO + nameof(OpenPlayerSettings)),
                        (OpenProfiler, GOTO + nameof(OpenProfiler)),
                        (OpenAddressableGroups, GOTO + nameof(OpenAddressableGroups)),
                        (OpenLightingSettings, GOTO + nameof(OpenLightingSettings)),
                        (OpenGraphicsSettings, GOTO + nameof(OpenGraphicsSettings)),
                        (OpenTimeline, GOTO + nameof(OpenTimeline)),
                        (OpenPackageManager, GOTO + nameof(OpenPackageManager)),
                        (OpenDefineSymbols, GOTO + nameof(OpenDefineSymbols)),
                        (QuickLoadWindow.OpenEditorWindow, GOTO + "Quick-Loader"),

                        // git
                        (MGit.GitFetch, GIT + "Fetch"),
                        (MGit.GitCommit, GIT + "Commit"),
                        (MGit.GitLog, GIT + "Log"),

                        // build
                        (MBuild.BuildSteamDemo, BUILD + "Steam Demo"),
                        (MBuild.BuildSteamRelease, BUILD + "Steam Release")

                        ); ;

                    menu.ExpandAllFolders();
                    menu.ShowAsContext();
                }
            };
        }

        static void OpenBuildSettings()
        {
            EditorWindow.GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
        }
        static void OpenNavigation()
        {
            EditorWindow.GetWindow(Type.GetType("UnityEditor.NavMeshEditorWindow,UnityEditor"));
        }
        static void OpenOcclusion()
        {
            EditorWindow.GetWindow(Type.GetType("UnityEditor.OcclusionCullingWindow,UnityEditor"));
        }
        static void OpenPlayerSettings()
        {
            SettingsService.OpenProjectSettings("Project/Player");
        }
        static void OpenProfiler()
        {
            EditorWindow.GetWindow(Type.GetType("UnityEditor.ProfilerWindow,UnityEditor"));
        }
        static void OpenAddressableGroups()
        {
            EditorWindow.GetWindow(Type.GetType("UnityEditor.AddressableAssets.GUI.AddressableAssetsWindow,Unity.Addressables.Editor"));
        }
        static void OpenLightingSettings()
        {
            EditorWindow.GetWindow(Type.GetType("UnityEditor.LightingWindow,UnityEditor"));
        }
        static void OpenGraphicsSettings()
        {
            SettingsService.OpenProjectSettings("Project/Graphics");
        }
        static void OpenTimeline()
        {
            EditorWindow.GetWindow(Type.GetType("UnityEditor.Timeline.TimelineWindow,Unity.Timeline.Editor"));
        }
        static void OpenPackageManager()
        {
            EditorWindow.GetWindow(Type.GetType("UnityEditor.PackageManager.UI.PackageManagerWindow,UnityEditor"));
        }
        static void OpenDefineSymbols()
        {
            // Open the Project Settings window
            SettingsService.OpenProjectSettings("Project/Player");

            // Use reflection to select the "Other Settings" section in the Player Settings
            var window = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var w in window)
            {
                if (w.GetType().Name == "PlayerSettingsEditor")
                {
                    var target = w.GetType().GetProperty("target").GetValue(w, null);
                    var prop = target.GetType().GetProperty("scriptingDefineSymbolsForGroup");
                    prop.SetValue(target, BuildTargetGroup.Standalone, null);
                    break;
                }
            }
        }
        static void FindScript()
        {
            var found = AssetDatabase.FindAssets($"t:script {nameof(WindowCommands)}");
            if (found.Length > 0)
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(found[0])));
            }
        }
    }
}
