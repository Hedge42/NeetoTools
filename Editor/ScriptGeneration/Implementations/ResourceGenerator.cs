using System.Linq;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using System;
using Object = UnityEngine.Object;

namespace Neeto
{
    [Serializable]
    public struct ResourceGenerator : IScriptGenerator
    {
        public string Script()
        {
            var sb = new StringBuilder();

            // find all resource asset paths
            var resourcePaths = AssetDatabase.FindAssets("t:Object", new[] { "Assets" })
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(path => path.Contains("/Resources/"));// && Directory.Exists(Path.Combine(MPath.project, path)));

            foreach (var assetPath in resourcePaths)
            {
                // get path relative to resource folder
                var resourcePath = assetPath.AsTextAfter("/Resources/");
                var ext = Path.GetExtension(resourcePath);
                if (ext.IsEmpty())
                    continue; // don't include folders
                resourcePath = resourcePath.Replace(Path.GetExtension(resourcePath), ""); // removeExtension
                var resourceName = resourcePath.Replace('-', '_').Replace('/', '_').Replace(' ', '_'); // make variable safe

                var obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                var type = obj.GetType();
                if (type.Equals(typeof(MonoScript)))
                    type = typeof(TextAsset);

                var line = $"public static readonly Resource<{type.FullName}> {resourceName} = new (\"{resourcePath}\");";
                sb.AppendLine(line);
                //Debug.Log(line);
            }
            return sb.ToString();
        }
    }
}