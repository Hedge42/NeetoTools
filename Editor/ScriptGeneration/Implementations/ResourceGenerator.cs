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
        [Note("Whitelist types that will be included, separated by ';'\nAll possible types will be used if left empty.")]
        public string typeList;

        public void Write(ScriptGenerator gen)
        {
            var sb = new StringBuilder();
            var source = (gen.script as MonoScript).GetClass().Assembly;

            bool isWhitelist = typeList.Trim().Length > 0;
            var types = typeList.Split(';');

            // find all resource asset paths
            var resourcePaths = AssetDatabase.FindAssets("t:Object", new[] { "Assets" })
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(path => path.Contains("/Resources/"));// && Directory.Exists(Path.Combine(Neeto.Menu.project, path)));

            Debug.Log(resourcePaths.Count());

            foreach (var assetPath in resourcePaths)
            {
                // get path relative to resource folder
                var resourcePath = assetPath.AsTextAfter("/Resources/");
                var ext = Path.GetExtension(resourcePath);
                if (ext.IsEmpty())
                    continue; // don't include folders
                resourcePath = resourcePath.Replace(Path.GetExtension(resourcePath), ""); // removeExtension
                //var resourceName = resourcePath.Replace('-', '_').Replace('/', '_').Replace(' ', '_'); // make variable safe
                var resourceName = ScriptGenerator.Codify(resourcePath);

                try
                {
                    var obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));

                    if (!ScriptGenerator.IsValidReference(source, obj.GetType().Assembly))
                        continue;

                    if (isWhitelist && !types.Any(name => obj.GetType().FullName.Contains(name)))
                        continue;

                    var type = obj.GetType();
                    if (type.Equals(typeof(MonoScript)))
                        type = typeof(TextAsset);

                    var line = $"\t\tpublic static readonly Resource<{type.FullName}> {resourceName} = new (\"{resourcePath}\");";
                    sb.AppendLine(line);
                }
                catch
                {
                    Debug.LogError($"Error loading resource at path '{assetPath}'");
                }
                //Debug.Log(line);
            }
            gen.WriteContent(sb.ToString());
        }
    }

   
}