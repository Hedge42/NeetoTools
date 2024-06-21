using Codice.CM.SEIDInfo;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Matchwork
{
    public static class Assets
    {
        /* 
		 * Code in this region is generated through the editor Matchwork → Generate Addressables References 
		 * 
		 * Do NOT modify
		 */


        #region GENERATED

#if UNITY_EDITOR
        public static readonly Dictionary<string, string> signatures = new Dictionary<string, string>
        {
        };
#endif

		public static class ScriptableObjects
		{
			public static readonly Asset<UnityEngine.TextAsset> README = new Asset<UnityEngine.TextAsset>("Assets/Neeto/README.md", "Assets/Neeto/README.md");
		}

#endregion GENERATED

#if UNITY_EDITOR

        public static void Generate(List<string> groups)
        {
            Debug.Log("Generating Addressables references");
            var startTime = System.DateTime.Now;

            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>("Assets/Neeto/Scripts/AssetManagement/Assets.cs");

            //AddressableAssetSettingsDefaultObject.Settings.Get

            // get generated region
            var startString = "#region GENERATED";
            var startIndex = asset.text.IndexOf(startString) + startString.Length;
            var endIndex = asset.text.IndexOf("#endregion");
            var affectedSubstring = asset.text.Substring(startIndex, endIndex - startIndex);
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
                    var variableName = assetName = MScript.MakeValidVariableName(assetName);


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
                        "UnityEngine.ResourceManagement.ResourceProviders.SceneInstance" =>  nameof(AddressableSceneAsset),
                        _ => $"Asset<{location.ResourceType.FullName}>"
                    };

                    sb.AppendLine($"\t\t\tpublic static readonly {typeName} {variableName} = new {typeName}(\"{location.PrimaryKey}\", \"{assetPath}\");");
                }
                sb.AppendLine("\t\t}\n");
            }

            // write signatures dictionary
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

            MScript.OverwriteRegion(asset, "GENERATED", sb.ToString());
            var elapsed = (System.DateTime.Now - startTime).TotalMilliseconds;
            Debug.Log($"Successfully generated Addressables references after {elapsed} ms", asset);
        }

        public static void ExportRegion(TextAsset script, string regionName, string newContent)
        {
            string startRegion = $"#region {regionName}";
            string endRegion = "#endregion";
            string scriptText = script.text;

            int startRegionIndex = scriptText.IndexOf(startRegion);
            int endRegionIndex = scriptText.IndexOf(endRegion, startRegionIndex);

            if (startRegionIndex == -1 || endRegionIndex == -1)
            {
                Debug.LogError($"Region {regionName} not found in script.");
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
            sb.Append(newContent);
            sb.Append(scriptText.Substring(contentEndIndex));

            // Record undo for asset modification
            Undo.RecordObject(script, $"Overwrite Region {regionName}");

            // Update the asset
            string outputPath = AssetDatabase.GetAssetPath(script);
            System.IO.File.WriteAllText(outputPath, sb.ToString());

            // Refresh the asset database
            AssetDatabase.Refresh();
        }
#endif
    }

    public interface IAsset
    {
        public string primaryKey { get; }
    }
    public interface IAsset<T> : IAsset // : IAsset<T> where T : Object
    {
        public T Load();
    }

    public struct AddressableSceneAsset : IAsset<SceneInstance>
    {
        public string primaryKey { get; }
        public string assetPath { get; }

        public AddressableSceneAsset(string _primaryKey, string _assetPath = "")
        {
            primaryKey = _primaryKey;
            assetPath = _assetPath;
        }

        public SceneInstance Load()
        {
            return Addressables.LoadSceneAsync(assetPath, UnityEngine.SceneManagement.LoadSceneMode.Additive).WaitForCompletion();
        }
    }
    public struct Asset<T> : IAsset<T> where T : Object
    {
        public string primaryKey { get; }
        public string assetPath { get; }

        private T _cached;
        private int _referenceCount;
        private AsyncOperationHandle<T> _handle;

        public Asset(string primaryKey, string assetPath = "")
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

    public static class AssetCache
    {
        static Dictionary<string, Object> dic = new();
        public static T CachedValue<T>(this IAsset<T> asset) where T : Object
        {
            if (!dic.TryGetValue(asset.primaryKey, out var instance))
            {
                dic.Add(asset.primaryKey, instance = asset.Load());
            }

            return (T)instance;
        }
    }
}

