using UnityEngine;
using System;
using Matchwork;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
[CustomPropertyDrawer(typeof(DisabledAttribute))]
public class DisabledPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attribute = this.attribute as DisabledAttribute;
        var target = property.serializedObject.targetObject;

        var conditional = fieldInfo.GetCustomAttribute<ConditionalAttribute>();
        var condition = conditional == null || conditional.GetConditional(target);

        EditorGUI.BeginDisabledGroup(condition);
        EditorGUI.PropertyField(position, property, label);
        EditorGUI.EndDisabledGroup();
    }
}
#endif

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class DisabledAttribute : PropertyAttribute
{
}