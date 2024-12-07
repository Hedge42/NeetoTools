using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(MaxAttribute))]
public class MaxAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();

        var att = attribute as MaxAttribute;

        var type = fieldInfo.FieldType;

        if (type.Equals(typeof(float)))
        {
            var value = EditorGUI.FloatField(position, label, property.floatValue);
            property.floatValue = Mathf.Min(value, att.max);
        }
        else if (type.Equals(typeof(int)))
        {
            var value = EditorGUI.IntField(position, label, property.intValue);
            property.intValue = Mathf.Min(value, (int)att.max);
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

public class MaxAttribute : PropertyAttribute
{
    public float max;
    public MaxAttribute(float max)
    {
        this.max = max;
    }
    public MaxAttribute(int max)
    {
        this.max = max;
    }
}
