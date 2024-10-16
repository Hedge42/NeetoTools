using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using UnityEditor.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Neeto
{
    public static class NScene
    {
        public static bool IsSceneAvailable(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (sceneName.Equals(SceneManager.GetSceneAt(i).name))
                {
                    return true;
                }
            }
            return false;
        }

        #region editor
#if UNITY_EDITOR
        public static SceneAsset[] buildSceneAssets { get; private set; }
        public static IEnumerable<string> buildScenePaths => EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path);

        [InitializeOnLoadMethod]
        static void Init()
        {
            buildSceneAssets = GetBuildSceneAssets();

            EditorSceneManager.sceneClosing += (_, _) =>
            {
                var scene = EditorSceneManager.GetActiveScene();
                var view = SceneView.lastActiveSceneView;
                if (view)
                {
                    var orientation = new Point(view.pivot, view.rotation);
                    var json = JsonUtility.ToJson(orientation);
                    EditorPrefs.SetString(scene.path, json);
                    Debug.Log($"Saved sceneView '{scene.name}': {orientation}");
                }
            };
            EditorSceneManager.activeSceneChangedInEditMode += (_, _) =>
            {
                var scene = EditorSceneManager.GetActiveScene();
                if (EditorPrefs.HasKey(scene.path))
                {
                    var json = EditorPrefs.GetString(scene.path);
                    var orientation = JsonUtility.FromJson<Point>(json);
                    var view = SceneView.lastActiveSceneView;
                    if (view)
                    {
                        view.rotation = orientation.rotation;
                        view.pivot = orientation.position;
                        SceneView.lastActiveSceneView.Repaint();
                        Debug.Log($"Loaded sceneView '{scene.name}': {orientation}");
                    }
                }
            };
        }
        public static string[] GetBuildSceneNames()
        {
            var sceneNames = new string[SceneManager.sceneCountInBuildSettings];
            for (int i = 0; i < sceneNames.Length; i++)
            {
                sceneNames[i] = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
            }
            return sceneNames;

        }
        public static SceneAsset[] GetBuildSceneAssets()
        {
            return EditorBuildSettings.scenes
                .Select(scene => AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path))
                .ToArray();
        }
        public static IEnumerable<string> GetEnabledBuildScenePaths()
        {
            return EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();
        }
        public static IEnumerable<string> GetBuildScenePaths()
        {
            return buildSceneAssets
                .Select(sceneAsset => AssetDatabase.GetAssetPath(sceneAsset));
        }
        public static bool IsSceneInBuildSettings(this SceneAsset asset)
        {
            if (!asset)
                return false;

            string scenePath = AssetDatabase.GetAssetPath(asset);
            return EditorBuildSettings.scenes.Any(scene => scene.path == scenePath);
        }
        public static void DragPrefab()
        {
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.DragUpdated:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    Event.current.Use();
                    break;
                case EventType.DragPerform:
                    DragAndDrop.AcceptDrag();

                    foreach (var dragged_object in DragAndDrop.objectReferences)
                    {
                        if (dragged_object is GameObject)
                        {
                            GameObject go = GameObject.Instantiate(dragged_object as GameObject, GetSceneDropPosition(), Quaternion.identity);
                            Undo.RegisterCreatedObjectUndo(go, "Drag Drop Prefab");
                        }
                    }
                    Event.current.Use();
                    break;
            }
        }
        public static void DragHierarchyToScene()
        {
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.DragUpdated:

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    Event.current.Use();
                    break;
                case EventType.DragPerform:
                    DragAndDrop.AcceptDrag();

                    foreach (var dragged_object in DragAndDrop.objectReferences)
                    {
                        if (dragged_object is GameObject)
                        {
                            GameObject go = GameObject.Instantiate(dragged_object as GameObject, GetSceneDropPosition(), Quaternion.identity);
                            Undo.RegisterCreatedObjectUndo(go, "Drag Drop Prefab");
                        }
                    }
                    Event.current.Use();
                    break;
            }
        }
        public static Ray mouseViewRay => HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        public static LayerMask clickLayers => NeetoSettings.instance.snappingLayers;
        public static bool MouseCast(out RaycastHit hit)
        {
            return Physics.Raycast(mouseViewRay, out hit, 100f, (int)clickLayers, QueryTriggerInteraction.Ignore);
        }
        public static Vector3 GetSceneDropPosition()
        {
            MouseCast(out var hit);
            return hit.point;
        }
        public static void DrawTransformHandles(Transform target)
        {
            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.KeyDown)
            {
                switch (currentEvent.keyCode)
                {
                    case KeyCode.W:
                        Tools.current = Tool.Move;
                        break;
                    case KeyCode.E:
                        Tools.current = Tool.Rotate;
                        break;
                    case KeyCode.R:
                        Tools.current = Tool.Scale;
                        break;
                }
            }

            switch (Tools.current)
            {
                case Tool.Move:
                    EditorGUI.BeginChangeCheck();
                    Vector3 newPosition = Handles.PositionHandle(target.position, target.rotation);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Move");
                        target.position = newPosition;
                    }
                    break;
                case Tool.Rotate:
                    EditorGUI.BeginChangeCheck();
                    Quaternion newRotation = Handles.RotationHandle(target.rotation, target.position);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Rotate");
                        target.rotation = newRotation;
                    }
                    break;
                case Tool.Scale:
                    EditorGUI.BeginChangeCheck();
                    Vector3 newScale = Handles.ScaleHandle(target.localScale, target.position, target.rotation, HandleUtility.GetHandleSize(target.position));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Scale");
                        target.localScale = newScale;
                    }
                    break;
            }
        }
        public static bool TransformGUI(Transform transform, out Vector3 position, out Quaternion rotation)
        {
            EditorGUI.BeginChangeCheck();
            position = Handles.PositionHandle(transform.position, transform.rotation);
            rotation = Handles.RotationHandle(transform.rotation, transform.position);
            return EditorGUI.EndChangeCheck();
        }
        public static bool NavCast(out Vector3 position)
        {
            var result = Physics.Raycast(mouseViewRay, out var hit, 100, NeetoSettings.instance.snappingLayers, QueryTriggerInteraction.Ignore);
            position = hit.point;

            if (result)
            {
                NavMesh.SamplePosition(hit.point, out var navHit, 5, NavMesh.AllAreas);
            }

            return false;
        }
#endif
        #endregion
    }
}