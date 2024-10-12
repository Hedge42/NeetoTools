using UnityEngine;
using UnityEditor;
using System.IO;
using Neeto;

public class ScriptReplacer : EditorWindow
{
    [Note("Replace the type of many ScriptableObjects or MonoBehaviours.\nEXPERIMENTAL, use at your own risk...")]
    public MonoScript script;
    Editor editor;
    bool init;

    [MenuItem(MENU.Open + nameof(ScriptReplacer))]
    public static void ShowWindow()
    {
        GetWindow<ScriptReplacer>(nameof(ScriptReplacer));
    }

    void OnGUI()
    {
        Editor.CreateCachedEditor(this, null, ref editor);

        editor.OnInspectorGUI();

        EditorGUI.BeginDisabledGroup(!script || Selection.objects.Length == 0);

        if (GUILayout.Button($"Execute ({Selection.objects.Length})"))
        {
            //Undo.RecordObjects(Selection.objects, nameof(ScriptReplacer));

            foreach (var obj in Selection.objects)
            {
                var s = new SerializedObject(obj);
                var p = s.FindProperty("m_Script");

                p.objectReferenceValue = script;
                s.ApplyModifiedProperties();
            }
        }

        EditorGUI.EndDisabledGroup();


        //if (!init && GUILayoutUtility.GetLastRect().yMax > 3)
        //{
        //    init = true;
        //    SetHeight(this, GetWindowHeight());
        //}
    }

    public static float GetWindowHeight()
    {
        return GUILayoutUtility.GetLastRect().yMax + EditorGUIUtility.standardVerticalSpacing;
    }
    public static void SetHeight(EditorWindow win, float height)
    {
        win.maxSize = win.maxSize.With(y: height);
        win.minSize = win.minSize.With(y: height);
    }
}
