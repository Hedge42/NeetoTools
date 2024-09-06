#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using Neeto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public static partial class EditorCommands
{

    [MenuItem(MenuPath.Open + "Build Settings", priority = Neeto.MenuOrder.Low)]
    static void OpenBuildSettings()
    {
        EditorWindow.GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
    }

    [MenuItem(MenuPath.Open + "Navigation Window")]
    static void OpenNavigation()
    {
        EditorWindow.GetWindow(Type.GetType("UnityEditor.NavMeshEditorWindow,UnityEditor"));
    }

    [MenuItem(MenuPath.Open + "Occlusion Window")]
    static void OpenOcclusion()
    {
        EditorWindow.GetWindow(Type.GetType("UnityEditor.OcclusionCullingWindow,UnityEditor"));
    }

    [MenuItem(MenuPath.Open + "Player Settings", priority = Neeto.MenuOrder.Low)]
    static void OpenPlayerSettings()
    {
        SettingsService.OpenProjectSettings("Project/Player");
    }

    [MenuItem(MenuPath.Open + "Profiler Window")]
    static void OpenProfiler()
    {
        EditorWindow.GetWindow(Type.GetType("UnityEditor.ProfilerWindow,UnityEditor"));
    }

    [MenuItem(MenuPath.Open + "Addressable Groups", priority = Neeto.MenuOrder.Low)]
    static void OpenAddressableGroups()
    {
        EditorWindow.GetWindow(Type.GetType("UnityEditor.AddressableAssets.GUI.AddressableAssetsWindow,Unity.Addressables.Editor"));
    }

    [MenuItem(MenuPath.Open + "Lighting Settings", priority = Neeto.MenuOrder.Low)]
    static void OpenLightingSettings()
    {
        EditorWindow.GetWindow(Type.GetType("UnityEditor.LightingWindow,UnityEditor"));
    }

    [MenuItem(MenuPath.Open + "Graphics Settings", priority = Neeto.MenuOrder.Low)]
    static void OpenGraphicsSettings()
    {
        SettingsService.OpenProjectSettings("Project/Graphics");
    }

    [MenuItem(MenuPath.Open + "Timeline")]
    static void OpenTimeline()
    {
        EditorWindow.GetWindow(Type.GetType("UnityEditor.Timeline.TimelineWindow,Unity.Timeline.Editor"));
    }

    [MenuItem(MenuPath.Open + "Package Manager")]
    static void OpenPackageManager()
    {
        EditorWindow.GetWindow(Type.GetType("UnityEditor.PackageManager.UI.PackageManagerWindow,UnityEditor"));
    }

    [MenuItem(MenuPath.Open + "Physics Settings", priority = Neeto.MenuOrder.Low)]
    public static void OpenPhysicsSettings()
    {
        SettingsService.OpenProjectSettings("Project/Physics");
    }
    [MenuItem(MenuPath.Open + "Neeto Settings", priority = Neeto.MenuOrder.Low)]
    public static void OpenNeetoSettings()
    {
        SettingsService.OpenProjectSettings("Project/Neeto");
    }

    [MenuItem(MenuPath.Open + "Persistent Data Directory", priority = Neeto.MenuOrder.Bottom)]
    public static void OpenPersistentDataDirectory()
    {
        EditorUtility.OpenWithDefaultApp(Application.persistentDataPath);
    }
    [MenuItem(MenuPath.Open + "Project Directory", priority = Neeto.MenuOrder.Bottom)]
    public static void OpenProjectDirectory()
    {
        EditorUtility.OpenWithDefaultApp(SystemPath.Project);
    }

    [MenuItem(MenuPath.Run + nameof(SnapToMouse))]
    public static void SnapToMouse()
    {
        if (!Selection.activeGameObject)
            return;

        var transform = Selection.activeGameObject.transform;
        var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        var qti = QueryTriggerInteraction.Ignore;
        var dist = Mathf.Infinity;
        var layers = LayerLibrary.Environment;
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

    [MenuItem(MenuPath.Run + nameof(SnapToNavMesh))]
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

    [MenuItem(MenuPath.Run + nameof(SnapToGround))]
    public static void SnapToGround()
    {
        if (!Selection.activeGameObject)
            return;

        var qti = QueryTriggerInteraction.Ignore;
        var layers = LayerLibrary.Environment;
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

    [MenuItem(MenuPath.Run + "Display default editor overrides")]
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

    [MenuItem(MenuPath.Assets + "Log Object Type", false)]
    public static void LogObjectType()
    {
        var obj = Selection.activeObject;
        Debug.Log($"{obj.name} type:{obj.GetType()}");
    }

    [MenuItem(MenuPath.Assets + "Log Object Type", true)]
    public static bool CanLogObjectType()
    {
        return Selection.activeObject != null;
    }
}
#endif