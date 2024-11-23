using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
[CustomPropertyDrawer(typeof(DisabledAttribute))]
public class DisabledPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attribute = this.attribute as DisabledAttribute;
        //var target = property.serializedObject.targetObject;


        // todo include condition
        //var condition = true;
        //if (attribute.condition != null)
        //{
        //    var field = fieldInfo.DeclaringType.GetField(nameof(condition));
        //    if (field != null)
        //    {
        //        //field.GetValue()
        //    }
        //    //fieldInfo.DeclaringType.GetProperty(nameof(condition));
        //}

        EditorGUI.BeginDisabledGroup(true);
        EditorGUI.PropertyField(position, property, label);
        EditorGUI.EndDisabledGroup();
    }
}
#endif

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class DisabledAttribute : PropertyAttribute
{
    // todo include condition
    //public string condition;
    //public DisabledAttribute(string condition = null)
    //    => this.condition = condition;
}