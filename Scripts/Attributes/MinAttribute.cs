using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(MinAttribute))]
public class MinAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();

        var att = attribute as MinAttribute;

        var type = fieldInfo.FieldType;

        if (type.Equals(typeof(float)))
        {
            var value = EditorGUI.FloatField(position, label, property.floatValue);
            property.floatValue = Mathf.Max(value, att.min);
        }
        else if (type.Equals(typeof(int)))
        {
            var value = EditorGUI.IntField(position, label, property.intValue);
            property.intValue = Mathf.Max(value, (int)att.min);
        }


        if (EditorGUI.EndChangeCheck())
        {
            property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label);
    }
}
#endif

public class MinAttribute : PropertyAttribute
{
    public float min;
    public MinAttribute(float min)
    {
        this.min = min;
    }
    public MinAttribute(int min)
    {
        this.min = min;
    }
}
