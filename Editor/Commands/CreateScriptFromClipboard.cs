using Neeto;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Neeto // naming ?? ?? ?? ??
{
    public class CreateScriptFromClipboard
    {
        [UnityEditor.MenuItem(MenuPath.Create + nameof(Neeto.CreateScriptFromClipboard))]
        public static void Execute()
        {
            string clipboardContent = GUIUtility.systemCopyBuffer;

            if (string.IsNullOrEmpty(clipboardContent))
            {
                Debug.LogError("Clipboard is empty.");
                return;
            }

            ScriptGenerator.TryGetFirstDefinitionName(GetFirstTypeName(clipboardContent), out string typeName);
            if (string.IsNullOrEmpty(typeName))
            {
                Debug.LogError("No valid type declaration found in the clipboard content.");
                return;
            }


            var folder = Selection.activeObject
                                        ? Path.GetDirectoryName(AssetDatabase.GetAssetPath(Selection.activeObject))
                                        : Application.dataPath;

            var path = Path.Combine(folder, typeName + ".cs");
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            Debug.Log($"path={path}");

            File.WriteAllText(path, clipboardContent);
            AssetDatabase.Refresh();

            var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            EditorGUIUtility.PingObject(asset);
            Application.OpenURL(path);
            Selection.activeObject = asset;
            Debug.Log($"Script {typeName}.cs created successfully at {path}", asset);
            Event.current.Use();
        }

        private static string GetFirstTypeName(string scriptContent)
        {
            // This regex looks for class, struct, or enum declarations
            var match = Regex.Match(scriptContent, @"\b(class|struct|enum)\s+(\w+)");
            return match.Success ? match.Groups[2].Value : null;
        }
    }
}