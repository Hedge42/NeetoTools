using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using System.CodeDom.Compiler;
using UnityEditor;
#endif

namespace Neeto
{
    public static class ScriptGenerator
    {
        private static readonly Regex InvalidCharsRgx = new Regex(@"[^a-zA-Z0-9]", RegexOptions.Compiled);
        private static readonly Regex StartingCharsRgx = new Regex(@"^[^a-zA-Z]*", RegexOptions.Compiled);

        public static string GenerateTags()
        {
            var sb = new StringBuilder();
            foreach (var tag in UnityEditorInternal.InternalEditorUtility.tags)
            {
                sb.AppendLine($"\tpublic const string {tag.Replace(' ', '_')} = \"{tag}\";");
            }
            return sb.ToString();
        }
        public static string GenerateSortingLayers()
        {
            var sb = new StringBuilder("public static class SortingLibrary\n{");
            foreach (var layer in SortingLayer.layers)
            {
                sb.AppendLine($"\tpublic const int {layer.name.Replace(' ', '_')} = {layer.value};");
            }
            sb.AppendLine("}");
            return sb.ToString();
        }
        public static string GenerateAddressables()
        {
            // Use reflection to access AddressableAssetSettingsDefaultObject
            //var sourceAssembly = (gen.script as MonoScript).GetClass().Assembly;
            var settingsType = System.Type.GetType("UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject, Unity.Addressables.Editor");
            var settingsProperty = settingsType?.GetProperty("Settings", BindingFlags.Static | BindingFlags.Public);
            var settings = settingsProperty?.GetValue(null);
            var getLabelsMethod = settings?.GetType().GetMethod("GetLabels");

            var groups = (IEnumerable<string>)getLabelsMethod?.Invoke(settings, null);

            Debug.Log("Generating Addressables references");
            var startTime = System.DateTime.Now;

            var variableCache = new HashSet<string>();
            var changeCache = new Dictionary<string, string>();
            var signatureBuilder = new StringBuilder();
            var sb = new StringBuilder();
            sb.Append("\n");

            // Iterate over Addressables groups
            foreach (var label in groups)
            {
                if (label.Equals("default"))
                    continue;

                // Generate class definition
                variableCache.Clear();
                sb.AppendLine($"\t\tpublic static class {label}\n\t\t{{");

                foreach (var location in Addressables.LoadResourceLocationsAsync(label).WaitForCompletion())
                {
                    // check that this type will be valid in the script
                    var targetAssembly = location.ResourceType.Assembly;
                    //if (!ScriptGenerator.IsValidReference(sourceAssembly, targetAssembly))
                    //continue;

                    var assetPath = location.InternalId;
                    var assetName = Path.GetFileNameWithoutExtension(assetPath);
                    var variableName = assetName = ScriptGenerator.Codify(assetName);

                    if (variableCache.Contains(assetName))
                    {
                        var newName = variableName += "_" + location.ResourceType.Name;
                        int c = 0;
                        while (variableCache.Contains(newName))
                        {
                            newName = variableName + "_" + c++;
                        }
                    }

                    variableCache.Add(variableName);

                    var typeName = location.ResourceType.FullName switch
                    {
                        "UnityEngine.ResourceManagement.ResourceProviders.SceneInstance" => "AddressableSceneAsset",
                        _ => $"Asset<{location.ResourceType.FullName}>"
                    };

                    sb.AppendLine($"\t\t\tpublic static readonly {typeName} {variableName} = new {typeName}(\"{location.PrimaryKey}\", \"{assetPath}\");");
                }
                sb.AppendLine("\t\t}\n");
            }

            // Write signatures dictionary
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

            var elapsed = (System.DateTime.Now - startTime).TotalMilliseconds;
            Debug.Log($"Successfully generated Addressables references after {elapsed} ms");

            return sb.ToString();
        }
        public static string GeneratePhysicsLayers()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 32; i++)
            {
                var name = LayerMask.LayerToName(i);
                if (name.IsEmpty())
                    continue;

                sb.AppendLine($"\t\tpublic const int {name.Replace(' ', '_')} = {1 << i};");
            }
            return sb.ToString();
        }
        public static string GenerateResources<T>() where T : Object
        {
            var sb = new StringBuilder();

            // find all resource asset paths
            var resourcePaths = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { "Assets" })
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(path => path.Contains("/Resources/"));

            foreach (var assetPath in resourcePaths)
            {
                // get path relative to resource folder
                var resourcePath = assetPath.AsTextAfter("/Resources/").WithoutExtension();
                var resourceName = Codify(resourcePath);

                try
                {
                    var line = $"\t\tpublic static readonly Resource<{typeof(T).FullName}> {resourceName} = new (\"{resourcePath}\");";
                    sb.AppendLine(line);
                }
                catch
                {
                    Debug.LogError($"Error loading resource at path '{assetPath}'");
                }
            }
            return sb.ToString();
        }



        public static IEnumerable<string> AddressableAssetPaths(string folderPath = null)
        {
            /*
             uses reflection so an editor assembly is not referenced 
             */

            var settingsType = Type.GetType("UnityEditor.AddressableAssets.Settings.AddressableAssetSettings, Unity.Addressables.Editor");
            if (settingsType == null)
            {
                Debug.LogError("AddressableAssetSettings type not found. Ensure the Addressables package is installed.");
                yield break;
            }

            var defaultSettingsProperty = settingsType.GetProperty("DefaultObject", BindingFlags.Public | BindingFlags.Static);
            var settingsObject = defaultSettingsProperty?.GetValue(null);
            if (settingsObject == null)
            {
                Debug.LogError("Could not retrieve AddressableAssetSettingsDefaultObject.");
                yield break;
            }

            var settings = settingsObject.GetType().GetProperty("Settings", BindingFlags.Public | BindingFlags.Instance)?.GetValue(settingsObject);
            if (settings == null)
            {
                Debug.LogError("AddressableAssetSettings not found.");
                yield break;
            }

            var groupsProperty = settings.GetType().GetProperty("groups", BindingFlags.Public | BindingFlags.Instance);
            var groups = groupsProperty?.GetValue(settings) as IEnumerable<object>;
            if (groups == null) yield break;

            foreach (var group in groups)
            {
                var entriesProperty = group.GetType().GetProperty("entries", BindingFlags.Public | BindingFlags.Instance);
                var entries = entriesProperty?.GetValue(group) as IEnumerable<object>;
                if (entries == null) continue;

                foreach (var entry in entries)
                {
                    var assetPathProperty = entry.GetType().GetProperty("AssetPath", BindingFlags.Public | BindingFlags.Instance);
                    var assetPath = assetPathProperty?.GetValue(entry) as string;
                    if (!assetPath.IsEmpty()
                        && (folderPath.IsEmpty() || assetPath.Contains(folderPath)))
                    {
                        yield return assetPath;
                    }
                }
            }
        }

        public static void OverwriteRegion(TextAsset script, string region, string content)
        {
            if (!script)
            {
                Debug.LogError($"Null script passed to OverwriteRegion");
                return;
            }    

            string startRegion = $"#region {region}";
            string endRegion = "#endregion";
            string scriptText = script.text;
            int startRegionIndex = scriptText.IndexOf(startRegion);
            int endRegionIndex = scriptText.IndexOf(endRegion, startRegionIndex);
            if (startRegionIndex == -1 || endRegionIndex == -1)
            {
                Debug.LogError($"Region {region} not found in script.");
                return;
            }
            // Calculate the indices for the actual content between the region tags
            int contentStartIndex = scriptText.IndexOf('\n', startRegionIndex) + 1;
            int contentEndIndex = endRegionIndex;
            if (contentStartIndex < 0 || contentEndIndex < 0 || contentEndIndex < contentStartIndex)
            {
                Debug.LogError("Invalid region format in script.");
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(scriptText.Substring(0, contentStartIndex));
            sb.Append(content);
            sb.Append(scriptText.Substring(contentEndIndex));
            SetText(script, sb.ToString());
        }
        public static void SetText(TextAsset script, string content)
        {
            // Record undo for asset modification
            Undo.RecordObject(script, $"Overwrite TextAsset '{script.name}'");
            // Update the asset
            File.WriteAllText(AssetDatabase.GetAssetPath(script), content);
            // Refresh the asset database
            AssetDatabase.Refresh();
        }
        public static string Codify(string name)
        {
            // Check if the variable name starts with a valid character.
            // If not, prepend an underscore
            if (!StartingCharsRgx.IsMatch(name))
            {
                name = "_" + name;
            }
            // Replace invalid characters with underscore
            name = InvalidCharsRgx.Replace(name, "_");
            // Check if the variable name is a reserved keyword.
            // If it is, append an underscore.
            if (CodeDomProvider.CreateProvider("C#").IsValidIdentifier(name) == false)
            {
                name += "_";
            }
            return name;
        }
        public static string NormalizeUnixLineEndings(string input)
        {
            return input.Replace("\r\n", "\n").Replace("\r", "\n");
        }
        public static string NormalizeWindowsLineEndings(string input)
        {
            // First normalize to Unix-style, then convert to Windows-style
            string unixNormalized = NormalizeUnixLineEndings(input);
            return unixNormalized.Replace("\n", "\r\n");
        }

        public static bool IsValidReference(Assembly source, Assembly target)
        {
            if (source == null || target == null)
                return false;


            // Check if the script assembly directly references the target assembly
            return source.GetReferencedAssemblies().Any(a => a.FullName == target.FullName);
        }

    }

#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class ScriptGeneratorAttribute : Attribute
    {
#if UNITY_EDITOR
        [QuickMenu("Run Script Generation", order = 696969)]
        public static void RunScriptGeneration() => 
            TypeCache.GetMethodsWithAttribute<ScriptGeneratorAttribute>()
                .Foreach(method => method.Invoke(null, null));
#endif
    }
}