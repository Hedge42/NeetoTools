#if UNITY_EDITOR
using Neeto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public static partial class EditorHelper
{
    [QuickAction]
    [MenuItem(MENU.Open + "Package manifest", priority = Neeto.MENU.Bottom)]
    static void OpenPackageManifest()
    {
        EditorApplication.delayCall += () =>
        {
            EditorUtility.OpenWithDefaultApp(Path.Combine(Path.GetDirectoryName(Application.dataPath), "Packages", "manifest.json"));
        };
    }

    [MenuItem(MENU.Open + "Build Settings", priority = Neeto.MENU.Low)]
    static void OpenBuildSettings()
    {
        EditorWindow.GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
    }

    [MenuItem(MENU.Open + "Navigation Window")]
    static void OpenNavigation()
    {
        EditorWindow.GetWindow(Type.GetType("UnityEditor.NavMeshEditorWindow,UnityEditor"));
    }

    [MenuItem(MENU.Open + "Occlusion Window")]
    [QuickAction]
    static void OpenOcclusion()
    {
        EditorWindow.GetWindow(Type.GetType("UnityEditor.OcclusionCullingWindow,UnityEditor"));
    }

    [MenuItem(MENU.Open + "Player Settings", priority = Neeto.MENU.Low)]
    [QuickAction]
    static void OpenPlayerSettings()
    {
        SettingsService.OpenProjectSettings("Project/Player");
    }

    [MenuItem(MENU.Open + "Profiler Window")]
    static void OpenProfiler()
    {
        EditorWindow.GetWindow(Type.GetType("UnityEditor.ProfilerWindow,UnityEditor"));
    }

    [MenuItem(MENU.Open + "Addressable Groups", priority = Neeto.MENU.Low)]
    static void OpenAddressableGroups()
    {
        EditorWindow.GetWindow(Type.GetType("UnityEditor.AddressableAssets.GUI.AddressableAssetsWindow,Unity.Addressables.Editor"));
    }

    [MenuItem(MENU.Open + "Lighting Settings", priority = Neeto.MENU.Low)]
    static void OpenLightingSettings()
    {
        EditorWindow.GetWindow(Type.GetType("UnityEditor.LightingWindow,UnityEditor"));
    }

    [MenuItem(MENU.Open + "Graphics Settings", priority = Neeto.MENU.Low)]
    static void OpenGraphicsSettings()
    {
        SettingsService.OpenProjectSettings("Project/Graphics");
    }

    [MenuItem(MENU.Open + "Timeline")]
    static void OpenTimeline()
    {
        EditorWindow.GetWindow(Type.GetType("UnityEditor.Timeline.TimelineWindow,Unity.Timeline.Editor"));
    }

    [MenuItem(MENU.Open + "Package Manager")]
    static void OpenPackageManager()
    {
        EditorWindow.GetWindow(Type.GetType("UnityEditor.PackageManager.UI.PackageManagerWindow,UnityEditor"));
    }

    [MenuItem(MENU.Open + "Physics Settings", priority = Neeto.MENU.Low)]
    public static void OpenPhysicsSettings()
    {
        SettingsService.OpenProjectSettings("Project/Physics");
    }
    [MenuItem(MENU.Open + "Neeto Settings", priority = Neeto.MENU.Low)]
    public static void OpenNeetoSettings()
    {
        SettingsService.OpenProjectSettings("Project/Neeto");
    }

    [MenuItem(MENU.Open + "Persistent Data Directory", priority = Neeto.MENU.Bottom)]
    public static void OpenPersistentDataDirectory()
    {
        EditorUtility.OpenWithDefaultApp(Application.persistentDataPath);
    }
    [MenuItem(MENU.Open + "Project Directory", priority = Neeto.MENU.Bottom)]
    public static void OpenProjectDirectory()
    {
        EditorUtility.OpenWithDefaultApp(Path.GetDirectoryName(Application.dataPath));
    }

    [MenuItem(MENU.Run + nameof(SnapToMouse) + " #G")]
    public static void SnapToMouse()
    {
        if (!Selection.activeGameObject)
            return;

        var transform = Selection.activeGameObject.transform;
        var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        var qti = QueryTriggerInteraction.Ignore;
        var dist = Mathf.Infinity;
        var layers = NeetoSettings.instance.snappingLayers;
        Event.current.Use();

        // NOTE don't use NavMesh.Raycast, it doesn't seem to work
        var result = Physics.Raycast(ray, out var hit, dist, layers, qti);

        // stupid
        var output = $"RaycastMove transform '{transform.name}' {result.SuccessOrFail()}";
        Undo.RecordObject(transform, output);
        Debug.Log(output, transform);
        transform.position = hit.point;


        if (result)
        {
            transform.position = hit.point;
            Debug.Log(output + " (SUCCESS)");
        }
        else Debug.LogError(output + " (FAIL)", transform);
    }

    [MenuItem(MENU.Run + nameof(SnapToNavMesh))]
    public static void SnapToNavMesh()
    {
        if (!Selection.activeGameObject)
            return;

        var dist = 1f;
        var transforms = Selection.gameObjects.Select(g => g.transform).ToArray();
        var output = transforms.Length == 1
            ? $"Snap ({transforms[0].name}) to NavMesh"
            : $"Snap ({transforms.Length}) objects to NavMesh";
        Undo.RecordObjects(transforms, output);
        Debug.Log(output);
        foreach (var transform in transforms)
        {
            var result = NavMesh.SamplePosition(transform.position, out var hit, dist, NavMesh.AllAreas);

            if (result)
                transform.position = hit.position;
            else
                Debug.LogError($"NavMesh not found within ({dist:F1}) distance of ({transform.name})", transform);
        }
    }

    [MenuItem(MENU.Run + nameof(SnapToGround))]
    public static void SnapToGround()
    {
        if (!Selection.activeGameObject)
            return;

        var qti = QueryTriggerInteraction.Ignore;
        var layers = NeetoSettings.instance.snappingLayers;
        var transforms = Selection.gameObjects.Select(g => g.transform).ToArray();
        var output = transforms.Length == 1
            ? $"Snap ({transforms[0].name}) to ground"
            : $"Snap ({transforms.Length}) transforms to ground";

        Undo.RecordObjects(transforms, output);
        Debug.Log(output);
        foreach (var transform in transforms)
        {
            var pos = transform.position + Vector3.up * 2f; // start from above
            var dir = Vector3.down;
            var result = Physics.Raycast(pos, dir, out var hit, 10f, layers, qti);

            if (result)
                transform.position = hit.point;
            else
                Debug.LogError($"Physical collider not found underneath ({transform.name})", transform);
        }
    }

    [MenuItem(MENU.Debug + "Find Global Editors")]
    public static void DisplayDefaultEditorOverrides()
    {
        HashSet<Type> editorTypes = new HashSet<Type>();

        var asss = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var ass in asss)
        {
            var types = ass.DefinedTypes;
            foreach (var type in types)
            {
                var att = type.GetCustomAttributes<CustomEditor>();
                if (att == null || att.Count() == 0)
                    continue;

                foreach (var _att in att)
                {
                    var field = _att.GetType().GetField("m_InspectedType", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Default);
                    var editorType = (Type)field.GetValue(_att);

                    // what other types do people override?
                    if (editorType.Equals(typeof(MonoBehaviour)) || editorType.Equals(typeof(ScriptableObject)))
                    {
                        editorTypes.Add(type);
                    }
                }
            }
        }

        var output = $"Found {editorTypes.Count} editor scripts:\n"
            + string.Join('\n', editorTypes.Select(t => t.FullName));

        Debug.Log(output);
    }

    [MenuItem(MENU.Assets + "Log Object Type", false)]
    public static void LogObjectType()
    {
        var obj = Selection.activeObject;
        Debug.Log($"{obj.name} type:{obj.GetType()}");
    }

    [MenuItem(MENU.Assets + "Log Object Type", true)]
    public static bool CanLogObjectType()
    {
        return Selection.activeObject != null;
    }
}
#endif