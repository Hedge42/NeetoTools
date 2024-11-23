using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor.Build.Reporting;


#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Neeto
{
    [CreateAssetMenu(menuName = Menu.Neeto + nameof(Neeto.BuildPreset), order = Menu.High)]
    public class BuildPreset : ScriptableObject
    {
        #region Editor
#if UNITY_EDITOR
        [CustomEditor(typeof(BuildPreset))]
        public class ScriptableDefineSymbolsEditor : Editor
        {
            new BuildPreset target => base.target as BuildPreset;
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (GUILayout.Button("Apply to build"))
                {
                    target.Apply();
                }
                else if (GUILayout.Button("Fill from existing"))
                {
                    Undo.RecordObject(target, "Fill Define Symbols");
                    target.symbols = target.group.GetDefines().Select(str => new BuildSymbol { def = str }).ToArray();
                }
            }
        }
#endif
        #endregion

        public string buildName;

        [Disabled]
        public string preview;

        void UpdatePreview()
        {
            preview = BuildOps.GetBuildPath(buildName);

            NGUI.Dirty(this);
        }

        [Flags]
        public enum Flags
        {
            Explicit,
            Revert,
        }

        public Flags defineFlags;


        [Note("Should these be the ONLY defines?")]
        public bool explicitDefines;

        [Tooltip("Are these defines subject to ONLY the build?")]
        public bool temporaryDefines;

        public BuildOptions options = BuildOptions.Development;
        public BuildTargetGroup group = BuildTargetGroup.Standalone;

        public Flags sceneFlags;



        [SerializeReference]
        public BuildSymbol[] symbols;

        public void Apply()
        {
            var defs = group.GetDefines();

            foreach (var s in symbols)
            {
                if (s && !defs.Contains(s))
                    defs.Add(s);
                else if (!s && defs.Contains(s))
                    defs.Remove(s);
            }

            group.SetDefines(defs);
        }
    }

    [Serializable]
    public class BuildSymbol
    {
        #region Editor
        [CustomPropertyDrawer(typeof(BuildSymbol))]
        public class DefineSymbolDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                position = position.With(h: NGUI.LineHeight);

                EditorGUI.BeginProperty(position, label, property);

                var target = property.managedReferenceValue as BuildSymbol;
                target ??= new();

                position = EditorGUI.PrefixLabel(position, label);
                target.enabled = EditorGUI.Toggle(position.With(w: 18), target.enabled);
                target.def = EditorGUI.TextField(position.Move(xMin: 20), target.def);
                property.managedReferenceValue = target;

                EditorGUI.EndProperty();

            }
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return NGUI.FullLineHeight;
            }
        }
        #endregion

        #region Operators
        public static implicit operator string(BuildSymbol symbol) => symbol.def;
        public static implicit operator bool(BuildSymbol symbol) => symbol.enabled;
        public static implicit operator BuildSymbol(string symbol) => new BuildSymbol { def = symbol };
        #endregion

        public bool enabled = true;
        public string def;
    }

    public static partial class BuildOps
    {
        public static List<string> GetDefines(this BuildTargetGroup group)
        {
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').ToList();
        }
        public static void SetDefines(this BuildTargetGroup group, string defs)
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defs);
        }
        public static void SetDefines(this BuildTargetGroup group, List<string> defs)
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(';', defs));
        }
        public static void AddDefine(this BuildTargetGroup group, string def, bool include = true)
        {
            var defs = group.GetDefines();

            if (!include && defs.Contains(def))
                defs.Remove(def);
            else if (include && !defs.Contains(def))
                defs.Add(def);

            group.SetDefines(defs);
        }
        public static void AddDefines(this BuildTargetGroup group, params string[] defs)
        {
            group.AddDefines(defs.ToList(), true);
        }
        public static void AddDefines(this BuildTargetGroup group, List<string> symbols, bool include = true)
        {
            var defines = group.GetDefines();

            foreach (var _ in symbols)
            {
                if (include && !defines.Contains(_))
                    defines.Add(_);
                else if (!include && defines.Contains(_))
                    defines.Remove(_);
            }
            group.SetDefines(defines);
        }
        public static void AddDefines(this BuildTargetGroup group, List<string> include, Func<string, bool> predicate)
        {
            var defs = group.GetDefines();

            foreach (var def in include)
            {
                group.AddDefine(def, predicate.Invoke(def));
            }

            group.SetDefines(defs);
        }
        public static void RemoveDefines(this BuildTargetGroup group, List<string> symbols)
        {
            group.AddDefines(symbols, false);
        }
        public static void RemoveDefines(this BuildTargetGroup group, params string[] defs)
        {
            group.AddDefines(defs.ToList(), false);
        }

        public static string GetOrCreateBuildDirectory(string name)
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
            string buildFolderPath = Path.Combine(buildsRoot, $"{name}_{dateFolderName}");

            // Ensure the dated build folder exists
            if (!Directory.Exists(buildFolderPath))
            {
                Directory.CreateDirectory(buildFolderPath);
            }

            return buildFolderPath;
        }
        public static BuildTargetGroup target => BuildTargetGroup.Standalone;
        public static void OpenBuildFolder()
        {
            Application.OpenURL(Path.Combine(Application.dataPath, "../Builds"));
        }
        public static void OpenBuildSettings()
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
                scenes = SceneHelper.buildScenePaths.ToArray(),
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
                scenes = SceneHelper.buildScenePaths.ToArray(),
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
}