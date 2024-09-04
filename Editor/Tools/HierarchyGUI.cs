using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

using Settings = NeetoSettings;

namespace Neeto
{
    [InitializeOnLoad]
    public static class HierarchyGUI
    {
        static bool enabled
        {
            get => Settings.instance.overrideHierarchyWindow;
            set => Settings.instance.overrideHierarchyWindow = value;
        }
        static HierarchyGUI()
        {
            EditorApplication.hierarchyWindowItemOnGUI += (instanceID, selectionRect) =>
            {
                OnHierarchyGUI(instanceID, selectionRect);
            };
        }
        public static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            var width = 12f;
            selectionRect.x = -3 + selectionRect.xMax - width;
            selectionRect.width = selectionRect.height;

            if (selectionRect.y == 0)
            {
                if (!enabled && GUI.Button(selectionRect, new GUIContent("\\"), NGUI.iconButton))
                {
                    enabled = true;
                    Event.current.Use();
                }
                else if (GUI.Button(selectionRect, new GUIContent("//"), NGUI.iconButton))
                {
                    enabled = false;
                    Event.current.Use();
                }

                return;
            }

            if (!enabled)
                return;

            // returned if first item or not enabled...

            if (GUI.Button(selectionRect, new GUIContent(NTexture.inspect, "Inspect"), NGUI.iconButton))
            {
                var obj = EditorUtility.InstanceIDToObject(instanceID);
                EditorUtility.OpenPropertyEditor(obj);
                if (obj is SceneAsset)
                    Debug.Log("aha!");
            }

            selectionRect.x -= selectionRect.width;

            var instance = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (!instance) return;

            var isPrefab = PrefabUtility.GetPrefabAssetType(instance) != PrefabAssetType.NotAPrefab;



            if (isPrefab)
            {
                var root = PrefabUtility.GetNearestPrefabInstanceRoot(instance);

                if (root == instance)
                {
                    // ping main prefab in Assets/
                    if (GUI.Button(selectionRect, new GUIContent("*", "Ping this prefab's asset in the project folder"), NGUI.iconButton))
                    {
                        // if the instance is a prefab
                        var obj = EditorUtility.InstanceIDToObject(instanceID);
                        var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
                        var asset = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(Object));
                        EditorGUIUtility.PingObject(asset);
                    }
                }
                else
                {
                    // ping prefab instance in hierarchy
                    if (GUI.Button(selectionRect, new GUIContent("^", "Ping this prefab's root GameObject"), NGUI.iconButton))
                    {
                        // if the instance is a prefab
                        EditorGUIUtility.PingObject(root);
                    }
                }
            }
        }
    }
}