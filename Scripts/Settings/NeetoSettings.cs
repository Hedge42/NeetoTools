using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditorInternal;
using System;
using UnityEngine.AddressableAssets;
using Neeto;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NeetoSettings : ScriptableObject
{

#if UNITY_EDITOR
    public AssemblyDefinitionAsset[] reflectionScope;
    public bool overrideToolbarWindow;
    public bool showQuickInspect;
#endif

    [QuickAction]
    public static void Open()
    {
        SettingsService.OpenProjectSettings($"Project/Neeto");
    }

    public static NeetoSettings instance => _instance ??= LoadOrCreate();


    #region editor
    public const string assetPath = "Assets/_Neeto/Resources/" + assetName + ".asset";
    public const string assetName = "NeetoSettings";
    static NeetoSettings _instance;

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

#if UNITY_EDITOR
    public static SerializedObject GetSerializedSettings()
    {
        return new SerializedObject(LoadOrCreate());
    }
#endif

#if UNITY_EDITOR
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

