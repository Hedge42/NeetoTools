using System.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.IO;
using Neeto;
using System.Text;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;
using UnityEditor;

namespace Neeto
{
    [Serializable]
    public class AddressGenerator : IScriptGenerator
    {
        public void Write(ScriptGenerator gen)
        {
            // Use reflection to access AddressableAssetSettingsDefaultObject
            var sourceAssembly = (gen.script as MonoScript).GetClass().Assembly;
            var settingsType = Type.GetType("UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject, Unity.Addressables.Editor");
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
                    if (!ScriptGenerator.IsValidReference(sourceAssembly, targetAssembly))
                        continue;

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

            gen.WriteContent(sb.ToString());

            var elapsed = (System.DateTime.Now - startTime).TotalMilliseconds;
            Debug.Log($"Successfully generated Addressables references after {elapsed} ms");

        }
       
    }
}
