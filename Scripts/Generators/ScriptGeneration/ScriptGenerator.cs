using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Reflection;

#if UNITY_EDITOR
using System.CodeDom.Compiler;
using UnityEditor;
#endif

namespace Neeto
{
    [InitializeOnLoad]
    [CreateAssetMenu(menuName = MENU.Neeto + nameof(ScriptGenerator), order = MENU.Mid)]
    public class ScriptGenerator : ScriptableObject
    {
#if UNITY_EDITOR
        [CustomEditor(typeof(ScriptGenerator))]
        public class ScriptGeneratorEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (GUILayout.Button("Generate"))
                {
                    (target as ScriptGenerator).Export();
                }
            }
        }
#endif


        [Tooltip("Text after #region")]
        public string region = "GENERATED";
        public TextAsset script;
        [SerializeReference, Polymorphic]
        public IScriptGenerator generator;
        Editor editor;
        [QuickAction]
        public void Export()
        {
            Undo.RecordObject(script, "Generate script");
            generator.Write(this);
        }
        public void WriteContent(string content)
        {
            OverwriteRegion(script, region, content);
        }
        static void OverwriteRegion(TextAsset script, string region, string content)
        {
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
        public static StringBuilder WithRegion(StringBuilder script, string regionName, string newContent)
        {
            string startRegion = $"#region {regionName}";
            string endRegion = "#endregion";
            string scriptText = script.ToString();
            int startRegionIndex = scriptText.IndexOf(startRegion);
            int endRegionIndex = scriptText.IndexOf(endRegion, startRegionIndex);
            if (startRegionIndex == -1 || endRegionIndex == -1)
            {
                Debug.LogError($"Region {regionName} not found in script.");
                return script;
            }
            // Calculate the indices for the actual content between the region tags
            int contentStartIndex = scriptText.IndexOf('\n', startRegionIndex) + 1;
            int contentEndIndex = endRegionIndex;
            if (contentStartIndex < 0 || contentEndIndex < 0 || contentEndIndex < contentStartIndex)
            {
                Debug.LogError("Invalid region format in script.");
                return script;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(scriptText.Substring(0, contentStartIndex));
            sb.Append(newContent);
            sb.Append(scriptText.Substring(contentEndIndex));
            return sb;
        }

        public static bool IsValidReference(Assembly source, Assembly target)
        {
            if (source == null || target == null)
                return false;

            // Check if the script assembly directly references the target assembly
            return source.GetReferencedAssemblies().Any(a => a.FullName == target.FullName);
        }
    }
}