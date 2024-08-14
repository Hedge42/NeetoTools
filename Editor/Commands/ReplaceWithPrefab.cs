using UnityEngine;
using System;
using System.Collections;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using System.Linq;
using Neeto;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public class ReplaceWithPrefab : EditorWindow
{

    // Opens this editor window
    [MenuItem(MenuPath.GameObject + "Replace With Prefab", validate = false)]
    public static void Open(MenuCommand command)
    {
        if (command.context == Selection.activeGameObject)
        {
            var window = GetWindow<ReplaceWithPrefab>();//.Show();
                                                        //window.position = window.position.With(height: 40);
            var size = new Vector2(300, 42);
            window.minSize = size;
            window.maxSize = size;
            window.Show();
        }
    }
    // Opens this editor window
    [MenuItem(MenuPath.GameObject + "Replace With Prefab", validate = true)]
    public static bool ValidateOpen()
    {
        return Selection.gameObjects.Length > 0;
    }

    public GameObject prefab;

    /// <summary> Draws this window GUI on screen. </summary>
    private void OnGUI()
    {
        prefab = EditorGUILayout.ObjectField(prefab, typeof(GameObject), false) as GameObject;

        var label = "Replace";
        if (GUILayout.Button(label))
        {
            foreach (var gameObject in Selection.gameObjects)
            {
                var newGameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                Undo.RegisterCreatedObjectUndo(newGameObject, label);
                gameObject.transform.CopyTo(newGameObject.transform);
            }

            var list = Selection.gameObjects.ToList();
            Undo.RecordObjects(Selection.gameObjects, label);
            while (list.Count > 0)
            {
                Undo.DestroyObjectImmediate(list[0]);
                list.RemoveAt(0);
            }
        }
    }
}
#endif
