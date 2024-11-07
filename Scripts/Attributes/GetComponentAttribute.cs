using System;
using UnityEngine;
using System.Reflection;
using Neeto;


#if UNITY_EDITOR
using UnityEditor;
[CustomPropertyDrawer(typeof(GetComponentAttribute))]
public class AutoGetComponentAttributeDrawer : PropertyDrawer
{
    GetComponentAttribute atr => attribute as GetComponentAttribute;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        using (NGUI.Property(position, label, property))
        {
            if (!property.objectReferenceValue && property.serializedObject.targetObject is Component component)
            {
                var type = atr.type ?? fieldInfo.FieldType;

                property.objectReferenceValue = atr.scope switch
                {
                    ComponentScope.Self => component.GetComponent(type),
                    ComponentScope.Parent => component.GetComponentInParent(type),
                    ComponentScope.Children => component.GetComponentInChildren(type),
                    ComponentScope.Find => GameObject.FindObjectOfType(type),
                    _ => throw new System.NotImplementedException()
                };
            }
            EditorGUI.PropertyField(position, property, label);
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
