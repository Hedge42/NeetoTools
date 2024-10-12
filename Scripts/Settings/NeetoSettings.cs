using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;
#endif

namespace Neeto
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class NeetoSettings : ScriptableObject
    {
        #region runtime

        public LayerMask snappingLayers;

        static NeetoSettings _instance;
        public static NeetoSettings instance => _instance ??= LoadOrCreate();
        public static NeetoSettings LoadOrCreate()
        {
            var settings = Resources.Load<NeetoSettings>(assetName);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<NeetoSettings>();
#if UNITY_EDITOR
                AssetDatabase.CreateAsset(settings, assetPath);
                AssetDatabase.SaveAssets();
#endif
            }
            return settings;
        }


        #endregion


        #region editor
#if UNITY_EDITOR
        public AssemblyDefinitionAsset[] reflectionScope;
        public bool experimentalEditorFeatures = true;
        public const string assetPath = "Assets/_Neeto/Resources/" + assetName + ".asset";
        public const string assetName = "NeetoSettings";

        [Note("Context menu items included in this list will be excluded from the custom context menu.")]
        public string[] contextBlacklist;

        public static GUIContent extrasGUIContent
        {
            get
            {
                var content = instance.experimentalEditorFeatures
                    ? EditorGUIUtility.IconContent("d_DebuggerAttached@2x")
                    : EditorGUIUtility.IconContent("d_DebuggerDisabled@2x");

                content.tooltip = "Enable Neeto's experimental editor features?\n\nThese are potentially buggy and may leave you confused.";
                return content;
            }
        }
        static NeetoSettings()
        {
            // workaround to access editor code since this is in a runtime assembly...
            var list = Assembly.Load("ToolbarExtender.Editor")
                .GetType("UnityToolbarExtender.ToolbarExtender")
                .GetField("RightToolbarGUI", BindingFlags.Static | BindingFlags.GetField | BindingFlags.Public)
                .GetValue(null) as List<Action>;

            // workaround to make sure this is drawn on the far right side
            EditorApplication.delayCall += () =>
            {
                list.Add(() =>
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(extrasGUIContent, EditorStyles.toolbarButton, GUILayout.Width(26)))
                    {
                        instance.experimentalEditorFeatures = !instance.experimentalEditorFeatures;
                        EditorApplication.RepaintHierarchyWindow();
                        EditorApplication.RepaintProjectWindow();
                    }
                    GUILayout.Space(4);
                });
            };
        }
        [QuickAction]
        public static void Open()
        {
            SettingsService.OpenProjectSettings($"Project/Neeto");
        }
        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(LoadOrCreate());
        }
        class NeetoSettingsProvider : SettingsProvider
        {
            Editor editor;
            public NeetoSettingsProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope) { }

            [SettingsProvider]
            public static SettingsProvider CreateMyCustomSettingsProvider()
            {
                return new NeetoSettingsProvider("Project/Neeto", SettingsScope.Project);
            }
            public override void OnActivate(string searchContext, VisualElement rootElement)
            {
                Editor.CreateCachedEditor(NeetoSettings.instance, null, ref editor);
            }
            public override void OnGUI(string searchContext)
            {
                editor.OnInspectorGUI();
            }
        }
#endif
        #endregion
    }
}