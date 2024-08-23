using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEditor.AddressableAssets;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using System.IO;
using Neeto;
using System.Text;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Neeto
{
    [Serializable]
    public class AdressablesGenerator : IScriptGenerator
    {
        public string Script()
        {
            var script = Resources.Load<TextAsset>(nameof(ScriptGenerator));
            var groups = AddressableAssetSettingsDefaultObject.Settings.GetLabels();

            Debug.Log("Generating Addressables references");
            var startTime = System.DateTime.Now;


            // get generated region
            var variableCache = new HashSet<string>();
            var changeCache = new Dictionary<string, string>();
            var signatureBuilder = new StringBuilder();

            var sb = new StringBuilder();
            sb.Append("\n");

            // interate Addressables groups
            foreach (var label in groups)
            {
                if (label.Equals("default"))
                    continue;

                // generate class definition
                variableCache.Clear();
                sb.AppendLine($"\t\tpublic static class {label}\n\t\t{{");
                foreach (var location in Addressables.LoadResourceLocationsAsync(label).WaitForCompletion())
                {

                    var assetPath = location.InternalId;
                    var assetName = Path.GetFileNameWithoutExtension(assetPath);
                    var variableName = assetName = ScriptGenerator.Codify(assetName);


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
                        "UnityEngine.ResourceManagement.ResourceProviders.SceneInstance" => nameof(AddressableSceneAsset),
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

            return sb.ToString();
        }
    }
}