using Cysharp.Threading.Tasks;
using Matchwork;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityToolbarExtender;
using UnityDropdown;
using UnityDropdown.Editor;

public static class MBuild
{
    // if neither demo or release are checked, build will not connect to steam
    public const string DEMO = "DEMO"; // uses demo steam app id
    public const string RELEASE = "RELEASE"; // uses full game steam app id
    public const string DEV = "DEV"; // includes cheats
    public const string INTERNAL = "INTERNAL";

    static void PingScript()
    {
        if (MEdit.TryFindFirstAsset($"t:script {nameof(MBuild)}", out var script))
            EditorGUIUtility.PingObject(script);
    }
    static void OpenBuildFolder()
    {
        Application.OpenURL(Path.Combine(Application.dataPath, "../Builds"));
    }
    static void OpenBuildSettings()
    {
        EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
    }

    public static string GenerateBuildLocation()
    {
        return EditorUserBuildSettings.GetBuildLocation(EditorUserBuildSettings.activeBuildTarget);
    }
    public static string GetBuildPath(string name)
    {
        // Define the root builds folder
        string buildsRoot = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Builds");

        //Debug.Log("Builds root: " + buildsRoot);
        Debug.Log(GenerateBuildLocation());

        // Ensure the builds root folder exists
        if (!Directory.Exists(buildsRoot))
        {
            Directory.CreateDirectory(buildsRoot);
        }

        // Generate a folder name based on the current date
        string dateFolderName = $"{name}_{DateTime.Now.ToString("yy.MM.dd")}";
        string buildFolderPath = Path.Combine(buildsRoot, dateFolderName);

        // Ensure the dated build folder exists
        if (!Directory.Exists(buildFolderPath))
        {
            Directory.CreateDirectory(buildFolderPath);
        }

        return buildFolderPath;
    }
    public static string Define(bool include, params string[] defines)
    {
        var prev = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);

        // Add the new define symbol
        var list = prev.Split(';').ToList();

        foreach (var d in defines)
        {
            if (include && !list.Contains(d))
                list.Add(d);
            else if (!include && list.Contains(d))
                list.Remove(d);
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, string.Join(";", list));

        return prev;
    }
    public static string Define(string defines)
    {
        var prev = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
        return prev;
    }

    public static void BuildSteamRelease()
    {
        var prev = MBuild.Define(true, MBuild.RELEASE);
        MBuild.Define(false, MBuild.INTERNAL, MBuild.DEV, MBuild.DEMO);
        MBuild.Build("interwoven");
        MBuild.Define(prev);
    }
    public static void BuildSteamDemo()
    {
        var prev = MBuild.Define(true, MBuild.DEMO);
        MBuild.Define(false, MBuild.INTERNAL, MBuild.DEV);
        MBuild.Build("interwoven_demo");
        MBuild.Define(prev);
    }

    // release standard
    public static void BuildRelease()
    {
        var prev = Define(true, RELEASE);
        Define(false, DEMO, DEV, INTERNAL);
        Build();
        Define(prev);
    }
    // release with cheats
    public static void BuildReleaseDev()
    {
        var prev = Define(false, DEMO);
        Define(true, RELEASE, DEV, INTERNAL);
        Build(BuildOptions.Development);
        Define(prev);
    }

    // demo no cheats
    public static void BuildDemo()
    {
        var prev = Define(false, RELEASE, INTERNAL, DEV);
        Define(true, DEMO);
        Build();
        Define(prev);
    }
    // demo with cheats
    public static void BuildDemoDev()
    {
        var prev = Define(false, RELEASE, INTERNAL);
        Define(true, DEMO, DEV);
        Build(BuildOptions.Development);
        Define(prev);
    }

    // internal no cheats
    public static void BuildInternal()
    {
        var prev = Define(true, INTERNAL);
        Define(false, RELEASE, DEMO, DEV);
        Build("interwoven");
        Define(prev);
    }
    // internal with cheats
    public static void BuildInternalDev()
    {
        var prev = Define(true, INTERNAL, DEV);
        Define(false, RELEASE, DEMO);
        Build("interwoven", BuildOptions.Development);
        Define(prev);
    }

    public static string GetBuildPath()
    {
        // Define the root builds folder
        string buildsRoot = Path.Combine(Application.dataPath, "../Builds/");

        // Ensure the builds root folder exists
        if (!Directory.Exists(buildsRoot))
        {
            Directory.CreateDirectory(buildsRoot);
        }

        // Generate a folder name based on the current date
        string dateFolderName = DateTime.Now.ToString("yy.MM.dd.mm");
        string buildFolderPath = Path.Combine(buildsRoot, $"interwoven" + dateFolderName);

        // Ensure the dated build folder exists
        if (!Directory.Exists(buildFolderPath))
        {
            Directory.CreateDirectory(buildFolderPath);
        }

        return buildFolderPath;
    }
    public static void Build(string name, BuildOptions options = BuildOptions.None)
    {
        var path = Path.Combine($"{GetBuildPath()}{name}/{name}.exe");

        Build(new BuildPlayerOptions()
        {
            scenes = MScene.buildScenePaths.ToArray(),
            locationPathName = path,
            target = EditorUserBuildSettings.activeBuildTarget,
            options = options
        });
    }
    public static void Build(BuildOptions options = BuildOptions.None)
    {
        var name = "interwoven";
        var path = Path.Combine($"{GetBuildPath()}{name}/{name}.exe");

        Build(new BuildPlayerOptions()
        {
            scenes = MScene.buildScenePaths.ToArray(),
            locationPathName = path,
            target = EditorUserBuildSettings.activeBuildTarget,
            options = options
        });
    }
    public static void Build(BuildPlayerOptions options)
    {
        BuildReport report = BuildPipeline.BuildPlayer(options);

        // Check the build result
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded.");
            Application.OpenURL(options.locationPathName.GetDirectoryPath());
        }
        else
        {
            Debug.LogError("Build failed.");
        }
    }
}
