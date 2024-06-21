#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using System.Text.RegularExpressions;
using UnityEditor.AddressableAssets;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using System.IO;
using UnityEngine.AddressableAssets;

namespace Matchwork
{
    public class AddressablesReferenceGenerator : ScriptableObject
    {
        [SerializeField, Disabled, Conditional(nameof(hasAsset)), SyncedProperty(nameof(outputPath))]
        private string _outputPath;
        public string outputPath
        {
            get => _asset
                    ? _outputPath = AssetDatabase.GetAssetPath(_asset)
                    : _outputPath;

            set => _outputPath = value;
        }

        public TextAsset _asset;
        bool hasAsset => _asset != null;

        [MenuItem(MPath.Commands + nameof(GenerateAssetReferences), priority = MPath.BOTTOM, validate = false)]
        public static void GenerateAssetReferences()
        {
            Assets.Generate(AddressableAssetSettingsDefaultObject.Settings.GetLabels());
        }
        [MenuItem(MPath.Main + nameof(GenerateAssetReferences), priority = MPath.BOTTOM, validate = true)]
        public static bool CanGenerateAssetReferences()
        {
            var obj = MScript.LoadFirstInstance<AddressablesReferenceGenerator>();
            return obj != null && obj._asset != null;
        }

        [MenuItem(MPath.Utils + nameof(SetAsAddressable), validate = false)]
        public static void SetAsAddressable()
        {
            var selected = UnityEditor.Selection.objects;
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            foreach (var obj in selected)
            {
                var path = AssetDatabase.GetAssetPath(obj);
                var guid = AssetDatabase.GUIDFromAssetPath(path).ToString();
                if (settings.FindAssetEntry(guid) == null)
                    settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
            }
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
        }
        [MenuItem(MPath.Utils + nameof(SetAsAddressable), validate = true)]
        public static bool CanSetAsAddressable()
        {
            return UnityEditor.Selection.objects.Length > 0;
        }

        public static string GetFirstName(string keyword, string fileContent)
        {
            // thanks ChatGPT ♥

            // The regular expression matches a class declaration in C#
            // This simple version does not handle cases with comments, strings containing keyword 'class', etc.
            var classPattern = @$"{keyword}\s+([_a-zA-Z][_a-zA-Z0-9]*)";
            var match = Regex.Match(fileContent, classPattern);
            if (match.Success)
            {
                return match.Groups[1].Value; // The first group captures the class name
            }

            return null; // Return null if no class declaration is found
        }

        public static List<T> LoadAssets<T>(Func<T, bool> onValidate = null) where T : UnityEngine.Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            var assets = new List<T>();
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<T>(path);

                if (onValidate == null || onValidate(obj))
                {
                    assets.Add(obj);
                }
            }
            return assets;
        }
        public static List<TextAsset> LoadScripts()
        {
            return LoadAssets<TextAsset>(t => Path.GetExtension(AssetDatabase.GetAssetPath(t)).Equals(".cs"));
        }

        [MenuItem(MPath.Utils + nameof(CreateAssetReferenceGenerator))]
        public static void CreateAssetReferenceGenerator()
        {
            MEdit.CreateAsset<AddressablesReferenceGenerator>();
        }
    }
}
#endif