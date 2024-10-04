using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Neeto
{
    public static class ScriptGenerator
    {
        public static void OverwriteRegion(TextAsset script, string region, string content)
        {
            string startRegion = $"#region {region}";
            string endRegion = $"#endregion {region}";
            string scriptText = script.text;

            if (!script.text.Contains(startRegion))
            {
                Debug.Log($"'{script.name}' does not contain region '{region}'", script);
                return;
            }


            int startRegionIndex = scriptText.IndexOf(startRegion);
            int endRegionIndex = scriptText.IndexOf(endRegion, startRegionIndex);

            if (startRegionIndex == -1 || endRegionIndex == -1)
            {
                Debug.LogError($"Region {region} not found in script '{script.name}'", script);
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
        public static string ReplaceRegion(string script, string region, string content)
        {
            string startRegion = $"#region {region}";
            string endRegion = "#endregion";

            int startRegionIndex = script.IndexOf(startRegion);
            int endRegionIndex = script.IndexOf(endRegion, startRegionIndex);

            if (startRegionIndex == -1 || endRegionIndex == -1)
            {
                Debug.LogError($"Region {region} not found in script.");
                return script;
            }

            // Calculate the indices for the actual content between the region tags
            int contentStartIndex = script.IndexOf('\n', startRegionIndex) + 1;
            int contentEndIndex = endRegionIndex;

            if (contentStartIndex < 0 || contentEndIndex < 0 || contentEndIndex < contentStartIndex)
            {
                Debug.LogError("Invalid region format in script.");
                return script;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(script.Substring(0, contentStartIndex));
            sb.Append(content);
            sb.Append(script.Substring(contentEndIndex));
            return sb.ToString();
        }

        private static readonly Regex InvalidCharsRgx = new Regex(@"[^a-zA-Z0-9]", RegexOptions.Compiled);
        private static readonly Regex StartingCharsRgx = new Regex(@"^[^a-zA-Z]*", RegexOptions.Compiled);

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
        public static MonoScript FindScript(Type type)
        {
            return AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets" })
                .Select(guid => AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid)))
                .FirstOrDefault(script => script.GetClass() != null && script.GetClass().Equals(type));
        }
    }
}