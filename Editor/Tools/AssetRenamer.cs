using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using System.IO;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Neeto
{

    #region EDITOR
#if UNITY_EDITOR


    public class AssetRenamer : EditorWindow
    {
        string search;
        string replace;

        int selected;
        Vector2 scroll;
        Texture2D bg;

        [MenuItem(MenuPath.Open + nameof(AssetRenamer), priority = MenuOrder.Bottom)]
        public static void Open()
        {
            EditorWindow.GetWindow<AssetRenamer>();
        }

        void OnGUI()
        {
            var guids = Selection.assetGUIDs;

            EditorGUILayout.LabelField($"Edit ({guids.Length}) asset names...");

            search = EditorGUILayout.TextField("Search for", search);
            replace = EditorGUILayout.TextField("Replace with", replace);

            if (!search.HasContents())
                return;

            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.indentLevel++;
            foreach (var asset in Selection.objects)
            {
                var result = asset.name.Replace(search, replace);
                if (!result.Equals(search))
                    GUILayout.Box(new GUIContent(result));
            }
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Replace Names") && !string.IsNullOrWhiteSpace(search))
            {
                Execute(search, replace);
            }
        }

        public static void Execute(string search, string replace)
        {
            var selectedAssets = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets);
            foreach (var asset in selectedAssets)
            {
                var path = AssetDatabase.GetAssetPath(asset);
                var assetName = Path.GetFileNameWithoutExtension(path);
                if (assetName.Contains(search))
                {
                    var newName = assetName.Replace(search, replace);
                    AssetDatabase.RenameAsset(path, newName);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static readonly string[] remove =
        {
            "Instance",
            "Variant"
        };

        [MenuItem(MenuPath.Open + "Fix Asset Names")]
        public static void Fix()
        {
            var selectedAssets = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets);
            foreach (var asset in selectedAssets)
            {
                var path = AssetDatabase.GetAssetPath(asset);
                var assetName = Path.GetFileNameWithoutExtension(path);

                var newName = new string(assetName);

                foreach (var r in remove)
                    newName = newName.Replace(r, "");
                newName = Regex.Replace(newName, "[^A-Za-z]+$", "");

                // Adjust name formatting to add underscores and ensure numbers are two-digit
                newName = Regex.Replace(newName, "([A-Z])(?=[a-z])", "_$1").TrimStart('_');
                newName = Regex.Replace(newName, "([0-9]+)", "_$1"); // Add underscore before numbers
                newName = Regex.Replace(newName, "(_)+", "_"); // Remove consecutive underscores
                newName = Regex.Replace(newName, "(_)([0-9]+)$", match => $"_{int.Parse(match.Groups[2].Value):D2}"); // Ensure trailing numbers are two digits

                // Ensure the new name is unique
                var finalName = GetUniqueName(newName, path);


                Undo.RecordObject(asset, "Rename asset");
                AssetDatabase.RenameAsset(path, finalName);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static string GetUniqueName(string baseName, string assetPath)
        {
            var directory = Path.GetDirectoryName(assetPath);
            var extension = Path.GetExtension(assetPath);
            var uniqueName = baseName;
            var counter = 1;

            while (AssetDatabase.LoadAssetAtPath($"{directory}/{uniqueName}{extension}", typeof(UnityEngine.Object)) != null)
            {
                // Adjust this to ensure a unique name without unnecessarily incrementing if the base name is already unique.
                uniqueName = $"{baseName}_{counter:D2}";
                counter++;
            }

            return uniqueName.TrimEnd('_'); // Ensure no trailing underscore
        }
    }
#endif
    #endregion
}
