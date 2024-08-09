using System;
using UnityEngine;
using System.Reflection;


#if UNITY_EDITOR
using UnityEditor;
[CustomPropertyDrawer(typeof(GetComponentAttribute))]
public class AutoGetComponentAttributeDrawer : PropertyDrawer
{
    GetComponentAttribute atr => attribute as GetComponentAttribute;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var component = property.serializedObject.targetObject as Component;

        var type = atr.type ?? fieldInfo.FieldType;

        if (component && property.objectReferenceValue == null)
        {
            bool changed;
            switch (atr.scope)
            {
                case ComponentScope.Self:

                    var c = component.GetComponent(type);
                    if (changed = c)
                        property.objectReferenceValue = c;
                    break;
                case ComponentScope.Parent:
                    var cp = component.GetComponentInParent(type);
                    if (changed = cp)
                        property.objectReferenceValue = cp;
                    break;
                case ComponentScope.Children:
                    var cc = component.GetComponentInChildren(type);
                    if (changed = cc)
                        property.objectReferenceValue = cc;
                    break;
                case ComponentScope.Find:
                    var f = GameObject.FindObjectOfType(type);
                    if (changed = f)
                        property.objectReferenceValue = f;
                    break;
                default:
                    throw new System.NotImplementedException();
            }

            if (changed)
            {
                property.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }
        }

        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(position, property, label);
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

[Flags]
public enum ComponentScope
{
    Self = 1,
    Parent = 2,
    Children = 4,
    Find = 8
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class GetComponentAttribute : PropertyAttribute
{
    public Type type;
    public ComponentScope scope;

    public GetComponentAttribute(ComponentScope scope = ComponentScope.Self)
    {
        this.scope = scope;
    }

    /// <summary>Automates editor referencing</summary>
    public GetComponentAttribute(Type type, ComponentScope scope = ComponentScope.Self)
    {
        this.type = type;
        this.scope = scope;
    }
}
