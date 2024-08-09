using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

// https://www.youtube.com/watch?v=mCKeSNdO_S0&t=636s

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
public class ExposedAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ExposedAttribute))]
public class ExposedAttributeDrawer : PropertyDrawer
{
    private Editor editor = null;
    private bool changed;
    float height;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(position, property, label, true);
        changed = EditorGUI.EndChangeCheck();

        // foldout
        if (property.objectReferenceValue != null)
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, GUIContent.none, true);
        else return;

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            Editor.CreateCachedEditor(property.objectReferenceValue, null, ref editor);

            EditorGUI.BeginChangeCheck();
            if (property.objectReferenceValue)
                editor.OnInspectorGUI();
            var _changed = EditorGUI.EndChangeCheck();
            changed = _changed || changed;
            EditorGUI.indentLevel--;
        }

        if (changed)
        {
            property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
public class ExposeMaterialAttribute : PropertyAttribute { }

[CustomPropertyDrawer(typeof(ExposeMaterialAttribute))]
public class ExposeMaterialAttributeDrawer : PropertyDrawer
{
    private Editor editor = null;
    private bool changed;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(position, property, label, true);
        changed = EditorGUI.EndChangeCheck();

        // foldout
        if (property.objectReferenceValue != null)
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, GUIContent.none, true);
        else return;

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            if (!editor)
                //Editor.CreateEditor()
                MaterialEditor.DrawFoldoutInspector(property.objectReferenceValue as Material, ref editor);
            //editor.ser
            EditorGUI.BeginChangeCheck();
            editor.OnInspectorGUI();
            changed = changed || EditorGUI.EndChangeCheck();
            EditorGUI.indentLevel--;
        }

        if (changed)
        {
            property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }
    }
}

#endif
