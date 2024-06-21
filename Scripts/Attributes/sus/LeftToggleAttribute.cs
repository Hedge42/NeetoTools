using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(LeftToggleAttribute))]
public class LeftToggleDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        position.height = EditorGUIUtility.singleLineHeight;
        //position.x = EditorGUIUtility.currentViewWidth;
        position.x += EditorGUIUtility.labelWidth - (EditorGUI.indentLevel + 1) * 16f;
        position.y -= MEdit.fullLineHeight;
        position.width = 18f;
        EditorGUI.BeginProperty(position, GUIContent.none, property);


        property.boolValue = EditorGUI.Toggle(position, GUIContent.none, property.boolValue);
        //EditorGUI.PropertyField(position, property, GUIContent.none);
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return -EditorGUIUtility.standardVerticalSpacing;
    }
}
#endif

public class LeftToggleAttribute : PropertyAttribute
{
}
