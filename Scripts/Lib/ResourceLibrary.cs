using UnityEngine;
using System;
using Object = UnityEngine.Object;
using System.Text;
using UnityEditor;
using System.IO;
using System.Linq;
using Unity.Collections;

namespace Neeto
{
    public static class ResourceLibrary
    {
#if UNITY_EDITOR
        [QuickAction]
        public static void Generate()
        {
            var sb = new StringBuilder();

            // find all resource asset paths
            var resourcePaths = AssetDatabase.FindAssets("t:Object", new[] { "Assets" })
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(path => path.Contains("/Resources/"));// && Directory.Exists(Path.Combine(MPath.project, path)));

            Debug.Log(resourcePaths.Count());

            var refs = typeof(ResourceLibrary).Assembly.GetReferencedAssemblies().Select(a => a.ToString());

            foreach (var assetPath in resourcePaths)
            {
                // get path relative to resource folder
                var resourcePath = assetPath.AsTextAfter("/Resources/");
                var ext = Path.GetExtension(resourcePath);
                if (ext.IsEmpty())
                    continue; // don't include folders

                var variableName = ScriptGenerator.Codify(resourcePath);

                try
                {
                    var obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));

                    if (!refs.Contains(obj.GetType().Assembly.FullName))
                    {
                        continue;
                    }

                    var type = obj.GetType();
                    if (type.Equals(typeof(MonoScript)))
                        type = typeof(TextAsset);

                    var line = $"public static readonly Resource<{type.FullName}> {variableName} = new (\"{resourcePath}\");";
                    sb.AppendLine(line);
                }
                catch
                {
                    Debug.LogError($"Error loading resource at path '{assetPath}'");
                }
            }

            var script = AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets" })
                .Select(guid => AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid)))
                .FirstOrDefault(script => script.GetClass() != null && script.GetClass().Equals(typeof(ResourceLibrary)));

            ScriptGenerator.OverwriteRegion(script, "GENERATED", sb.ToString());
        }
#endif

        public static string AssetPathToResourcePath(string assetPath)
        {
            return assetPath.AsTextAfter("/Resources/");
        }

        [Serializable]
        public struct Resource<T> where T : Object
        {
            public string resourcePath;

            public Resource(string resourcePath)
            {
                this.resourcePath = resourcePath;
            }

            public T Load()
            {
                return Resources.Load(resourcePath) as T;
            }
        }

        #region GENERATED
	public const string Untagged = "Untagged";
	public const string Respawn = "Respawn";
	public const string Finish = "Finish";
	public const string EditorOnly = "EditorOnly";
	public const string MainCamera = "MainCamera";
	public const string Player = "Player";
	public const string GameController = "GameController";
	public const string Rig = "Rig";
	public const string Weapon = "Weapon";
	public const string Fire = "Fire";
	public const string Gameplay = "Gameplay";
	public const string Cutscene = "Cutscene";
#endregion
    }
}