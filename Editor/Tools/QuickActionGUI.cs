using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Callbacks;
using UnityDropdown.Editor;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Neeto
{
    /// <summary>
    /// Draws extra icons in the editor, see <see cref="QuickActionAttribute"/>
    /// </summary>
    [InitializeOnLoad]
    public static class QuickActionGUI
    {
        class QuickActionMap : Dictionary<string, List<DropdownItem<Action>>> { }

        const int SIZE = 16;
        static bool enabled => NeetoSettings.instance.showQuickInspect;
        static QuickActionMap projectMap;
        static Dictionary<int, List<DropdownItem<Action>>> hierarchyMap = new();
        static GUIContent content; // magnifying-glass icon with "inspect" tooltip
        static HashSet<int> sceneIDs; // instanceIDs of Scene headers in hierarchy
        static CancellationTokenSource cts;

        static QuickActionGUI()
        {
            // add to Hierarchy and Project item GUIs
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
            EditorApplication.projectWindowItemOnGUI += OnProjectGUI;

            // store scene IDs so they are ignored in OnHierarchyGUI
            EditorSceneManager.sceneOpened += (_, _) => UpdateSceneIDs();
            EditorSceneManager.sceneClosed += (scene) => UpdateSceneIDs();

            // initialization with engine calls always requires delay call
            EditorApplication.delayCall += () =>
            {
                content = EditorGUIUtility.IconContent("HoverBar_Down");
                UpdateSceneIDs();
            };
        }

        static void OnHierarchyGUI(int instanceID, Rect rect)
        {
            if (!enabled || rect.y == 0) // first item is scene asset
                return;

            else if (content == null || sceneIDs == null || sceneIDs.Contains(instanceID)) // ignore scene headers
                return;

            else if (GUI.Button(rect.With(xMin: rect.xMax - SIZE, height: SIZE), content, EditorStyles.iconButton))
            {
                if (!hierarchyMap.TryGetValue(instanceID, out var items))
                {
                    var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
                    hierarchyMap[instanceID] = items = new()
                    {
                        GetInspectAction(gameObject)
                    };
                    items.AddRange(GetGameObjectActions(gameObject));
                }

                new DropdownMenu<Action>(items,
                    _ => _.Invoke())
                    .ShowAsContext();
            }
        }
        
        static void OnProjectGUI(string guid, Rect rect)
        {
            if (!enabled || content == null)
                return;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            var ext = Path.GetExtension(path);
            switch (ext)
            {
                case ".prefab":
                case ".asset":
                case ".png":
                case ".jpg":
                case ".fbx":
                case ".lighting":
                case ".mat":
                case ".md":
                    break;
                case ".cs":
                    if (projectMap != null && projectMap.ContainsKey(Path.GetFileName(path)))
                        break;
                    else
                        return;
                default:
                    return;
            }

            if (GUI.Button(rect.With(xMin: rect.xMax - SIZE, height: SIZE), content, EditorStyles.iconButton))
            {
                EditorApplication.delayCall += () =>
                {
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(path)
                             ?? AssetDatabase.LoadAssetAtPath<ModelImporter>(path); // whyy

                    // always display Inspect (Properties) option
                    var items = new List<DropdownItem<Action>>
                    {
                        GetInspectAction(asset)
                    };

                    // look for static [QuickAction] after reflection
                    if (asset is MonoScript script) // run from script file populated by reflection
                    {
                        foreach (var item in projectMap[$"{script.name}.cs"])
                        {
                            items.Add(item);
                        }
                    }
                    else // look for Object instance [QuickAction]
                    {
                        items.AddRange(GetObjectActions(asset));
                    }

                    new DropdownMenu<Action>(items,
                        _ => _.Invoke())
                        .ShowAsContext();
                };
            }
        }

        [DidReloadScripts]
        static void OnScriptsLoaded()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = new();
            var token = cts.Token;

            // ensures Unity is fully initialized before starting
            EditorApplication.delayCall += delegate
            {
                Debug.Log("Starting QuickEvent processing...");
                Task.Run(() => ProcessQuickActions(), token);
                //ProcessQuickActions();
            };
        }
        static void ProcessQuickActions()
        {
            var timer = new Stopwatch();
            timer.Start();

            var map = new QuickActionMap();
            // Find all EditorWindow types
            foreach (var type in TypeCache.GetTypesDerivedFrom<EditorWindow>())
            {
                Add(map, $"{type.Name}.cs", $"Open {type.Name}", () => EditorWindow.GetWindow(type, false));
            }

            // Find all ScriptableObject types (excluding Editor subclasses)
            foreach (var type in TypeCache.GetTypesDerivedFrom<ScriptableObject>())
            {
                if (type.IsPublic && !type.IsAbstract && !type.IsSubclassOf(typeof(Editor)) && !type.IsSubclassOf(typeof(EditorWindow)))
                {
                    Add(map, $"{type.Name}.cs", $"Create new {type.Name}", () => CreateScriptableObjectPanel(type));
                }
            }

            // Find all methods with [QuickAction] attribute
            foreach (var method in TypeCache.GetMethodsWithAttribute<QuickActionAttribute>())
            {
                if (!method.IsStatic)
                    continue;

                var attribute = method.GetCustomAttribute<QuickActionAttribute>();
                var type = method.DeclaringType;
                Add(map, attribute.file ?? $"{type.Name}.cs", $"{type.Name}.{method.Name}()", () => method.Invoke(null, null));
            }

            // run on main thread
            EditorApplication.delayCall += delegate
            {
                QuickActionGUI.projectMap = map;
                timer.Stop();
                Debug.Log($"QuickEvent processing completed in {timer.ElapsedMilliseconds}ms with ({map.Count}) files.");
            };
        }
        static void UpdateSceneIDs()
        {
            var ids = new List<int>();

            // Iterate over all loaded scenes and get their instance IDs
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                // the Scene header's instanceID is its HashCode!
                // https://discussions.unity.com/t/how-do-i-get-the-loaded-scene-from-the-instanceid-in-editorapplication-hierarchywindowitemongui/634684/4
                ids.Add(EditorSceneManager.GetSceneAt(i).GetHashCode());
            }

            sceneIDs = ids.ToHashSet();
        }
        static DropdownItem<Action> GetInspectAction(Object obj)
        {
            return new DropdownItem<Action>(() => EditorUtility.OpenPropertyEditor(obj), "Inspect");
        }
        static List<DropdownItem<Action>> GetObjectActions(Object obj)
        {
            var type = obj.GetType();
            return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                       .Where(method => method.GetCustomAttribute<QuickActionAttribute>() != null)
                       .Select(method => new DropdownItem<Action>(() => method.Invoke(method.IsStatic ? null : obj, null), $"{type.Name}.{method.Name}()"))
                       .ToList();
        }
        static List<DropdownItem<Action>> GetGameObjectActions(GameObject gameObject)
        {
            var items = new List<DropdownItem<Action>>();

            // add mono actions
            foreach (var mono in gameObject.GetComponents<MonoBehaviour>())
            {
                if (!mono) // missing script
                {
                    Debug.LogError($"'{gameObject.name}' has a missing script!", gameObject);
                    continue;
                }

                items.AddRange(GetObjectActions(mono));
            }

            // add prefab actions
            if (PrefabUtility.GetPrefabAssetType(gameObject) != PrefabAssetType.NotAPrefab)
            {
                // ping prefab asset
                items.Add(new DropdownItem<Action>(
                    delegate
                    {
                        var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
                        var asset = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(Object));
                        EditorGUIUtility.PingObject(asset);
                    },
                    "Ping Prefab's Asset"
                ));

                /* ping prefab root
                 * helpful when prefabs are stacked in the hierarchy
                 */
                var root = PrefabUtility.GetNearestPrefabInstanceRoot(gameObject);
                if (root != gameObject)
                {
                    items.Add(new DropdownItem<Action>(
                        delegate
                        {
                            EditorGUIUtility.PingObject(root);
                        },
                        "Ping Prefab's Root"
                    ));
                }
            }

            return items;
        }
        static void CreateScriptableObject(Type type)
        {
            // Find the script asset associated with this ScriptableObject type
            var script = MonoScript.FromScriptableObject(ScriptableObject.CreateInstance(type));
            var scriptPath = AssetDatabase.GetAssetPath(script);
            var defaultDirectory = System.IO.Path.GetDirectoryName(scriptPath);

            // Create a new instance
            var instance = ScriptableObject.CreateInstance(type);

            // Determine the save location
            var path = defaultDirectory;
            var fileName = type.Name + ".asset";
            var assetPath = AssetDatabase.GenerateUniqueAssetPath(System.IO.Path.Combine(path, fileName));

            // Create the asset in the specified path
            AssetDatabase.CreateAsset(instance, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

            Debug.Log($"ScriptableObject '{fileName}' created at: {assetPath}", asset);
        }
        static void CreateScriptableObjectPanel(Type type)
        {
            // Find the script asset associated with this ScriptableObject type
            var script = MonoScript.FromScriptableObject(ScriptableObject.CreateInstance(type));
            var scriptPath = AssetDatabase.GetAssetPath(script);
            var defaultDirectory = System.IO.Path.GetDirectoryName(scriptPath);

            // Show save file dialog
            var defaultName = type.Name + ".asset";
            var path = EditorUtility.SaveFilePanelInProject("Save ScriptableObject", defaultName, "asset", "Please enter a file name to save the ScriptableObject to", defaultDirectory);

            if (string.IsNullOrEmpty(path))
            {
                // User canceled the dialog
                return;
            }

            // Create a new instance
            var instance = ScriptableObject.CreateInstance(type);

            // Create the asset in the specified path
            AssetDatabase.CreateAsset(instance, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            Debug.Log($"ScriptableObject '{type.Name}' created at: {path}", asset);
        }
        static void Add(Dictionary<string, List<DropdownItem<Action>>> map, string key, string label, Action action)
        {
            if (!map.TryGetValue(key, out var list))
            {
                map.Add(key, list = new());
            }
            list.Add(new(action, label));
        }
    }
}