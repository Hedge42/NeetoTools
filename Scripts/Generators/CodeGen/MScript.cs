#if UNITY_EDITOR
using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Neeto
{
    public static class MScript
    {
        private static readonly Regex InvalidCharsRgx = new Regex(@"[^a-zA-Z0-9]", RegexOptions.Compiled);
        private static readonly Regex StartingCharsRgx = new Regex(@"^[^a-zA-Z]*", RegexOptions.Compiled);

        public static T LoadFirstInstance<T>() where T : UnityEngine.Object
        {
            var guid = System.Linq.Enumerable.FirstOrDefault(AssetDatabase.FindAssets($"t:{typeof(T).Name}"));
            var path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        public static string MakeValidVariableName(string name)
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
            //if (CodeDomProvider.CreateProvider("C#").IsValidIdentifier(name) == false)
            //{
            //    name += "_";
            //}

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
        
        public static void OverwriteRegion(TextAsset script, string regionName, string newContent)
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
    }
}

#endif