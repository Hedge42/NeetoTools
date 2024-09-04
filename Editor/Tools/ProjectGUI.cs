using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

using Settings = NeetoSettings;

namespace Neeto
{
    [InitializeOnLoad]
    public static class ProjectGUI
    {
        static bool enabled
        {
            get => Settings.instance.overrideProjectWindow;
            set => Settings.instance.overrideProjectWindow = value;
        }

        private static bool isDragging = false;
        private static ScriptableObject targetObject = null;
        static ProjectGUI()
        {
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowGUI;
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
            EditorApplication.update += OnEditorUpdate;
        }
        private static void OnProjectWindowGUI(string guid, Rect rect)
        {
            if (!enabled)
                return;

            var path = AssetDatabase.GUIDToAssetPath(guid);

            var ext = Path.GetExtension(path);
            //Directory.Exists(path);

            var isFolder = false;
            var isObject = false;
            var isText = false;
            //var isPrefab = false;

            switch (ext)
            {
                case ".prefab":
                    isObject = true;
                    //isPrefab = true;
                    break;
                case ".asset":
                case ".png":
                    isObject = true;
                    break;
                case ".fbx":
                case ".lighting":
                case ".mat":
                    isObject = true;
                    break;
                case ".cs":
                case ".txt":
                case ".json":
                    isText = true;
                    break;
                case "":
                    isFolder = true;
                    break;
                default:
                    return;
            }

            int size = 16;
            rect = new Rect(rect.xMax - size, rect.y, size, 16);

            if (isText || isObject || isFolder)
            {
                var content = new GUIContent();
                if (isText)
                {
                    content.tooltip = "Open in text editor";
                    content.image = NTexture.click;
                }
                else if (isFolder)
                {
                    content.tooltip = "Open in file explorer";
                    content.image = NTexture.folder;
                }
                else if (isObject)
                {
                    content.tooltip = "Inspect";
                    content.image = NTexture.inspect;
                }

                if (isObject)
                {

                    if (GUI.Button(rect, content, NGUI.iconButton))
                    {
                        EditorApplication.delayCall += () =>
                        {
                            var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                            asset ??= AssetDatabase.LoadAssetAtPath<ModelImporter>(path);
                            EditorUtility.OpenPropertyEditor(asset);
                            Debug.Log($"Inspecting '{asset.name}'...", asset);
                        };
                    }
                }
                else if (isText && GUI.Button(rect, content, NGUI.iconButton))
                {
                    Debug.Log($"Opening '{path}' in external program...", AssetDatabase.LoadAssetAtPath<Object>(path));
                    UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(path, 0);
                }
                else if (isFolder && GUI.Button(rect, content, NGUI.iconButton))
                {
                    var assetPath = path;

                    //if (Path.Exists(path))
                    if (!path.EndsWith('/'))
                        path += '/';
                    //path = path.Substring(7);
                    path = Path.Combine(SystemPath.Project, path);
                    path = path.Replace("/", "\\");
                    Debug.Log($"Opening '{path}' in file explorer...", AssetDatabase.LoadAssetAtPath<Object>(assetPath));
                    Process.Start("explorer", path);
                }
            }

        }
        private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
        {
            if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
            {
                foreach (var obj in DragAndDrop.objectReferences)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (Event.current.type == EventType.DragPerform)
                    {
                        targetObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid));
                        isDragging = true;
                    }
                }
            }
        }
        private static void OnEditorUpdate()
        {
            if (isDragging && targetObject != null)
            {
                var path = AssetDatabase.GetAssetPath(targetObject);
                foreach (ScriptableObject draggedScriptableObject in DragAndDrop.objectReferences)
                {
                    var instance = ScriptableObject.Instantiate(draggedScriptableObject);
                    AssetDatabase.AddObjectToAsset(instance, path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                isDragging = false;
                targetObject = null;
            }
        }
    }
}