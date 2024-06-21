using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = MPath.Main+ nameof(ReadmeAsset), order = int.MinValue)]
public class ReadmeAsset : ScriptableObject
{
    public string value;
    public int fontSize;
    public bool edit;

    private void Reset()
    {
        value = "<b>Try</b> <color=green>me</color>";
        fontSize = 12;
        edit = true;
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(ReadmeAsset))]
public class ReadmeAssetEditor : Editor
{
    private GUIStyle style;

    private void OnEnable()
    {
        Undo.RegisterCreatedObjectUndo(target, "readme");
        (target as ReadmeAsset).edit = false;
    }

    private void Validate()
    {
        if (style == null)
        {
            style = new GUIStyle(EditorStyles.textArea);
            //var buttonStyle = new GUIStyle(EditorStyles.miniButton);
            style.wordWrap = true;
            style.richText = true;
            style.normal.textColor = Color.white;
            style.stretchHeight = true;
            //style.onFocused = buttonStyle.onFocused;
            //style.tex
        }


    }
    public override void OnInspectorGUI()
    {
        var _target = target as ReadmeAsset;

        Validate();

        style.fontSize = _target.fontSize;

        Rect rect;

        EditorGUI.BeginChangeCheck();

        _target.edit = GUILayout.Toggle(_target.edit, "Edit");
        if (_target.edit)
        {
            _target.fontSize = EditorGUILayout.IntSlider("Font Size", _target.fontSize, 4, 128);
            _target.value = EditorGUILayout.TextArea(_target.value);
        }

        // because selectableLabel is dumb
        // https://answers.unity.com/questions/279233/height-and-width-of-labelbox-depending-on-amount-o.html
        rect = GUILayoutUtility.GetRect(new GUIContent(_target.value), style);
        EditorGUI.SelectableLabel(rect, _target.value, style);

        if (EditorGUI.EndChangeCheck())
        {
            //Undo.RecordObject(target, $"{target.GetType()}");
            Undo.RegisterCompleteObjectUndo(target, "readme");
            EditorUtility.SetDirty(target);
        }

        serializedObject.ApplyModifiedProperties();
    }
}


#endif
