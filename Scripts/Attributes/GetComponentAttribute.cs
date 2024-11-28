using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Neeto
{
    /// <summary>
    /// Find the first component in the given scope
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class GetComponentAttribute : PropertyAttribute
    {
        public ComponentScope scope;

        public GetComponentAttribute(ComponentScope scope = ComponentScope.Self)
        {
            this.scope = scope;
        }
    }
    [Flags]
    public enum ComponentScope
    {
        Self = 1,
        Parent = 2,
        Children = 4,
        Scene = 8
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(GetComponentAttribute))]
    class GetComponentAttributeDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (NGUI.Property(position, property, false))
            {
                if (!property.objectReferenceValue && property.serializedObject.targetObject is Component component)
                {
                    var attribute = base.attribute as GetComponentAttribute;
                    var type = fieldInfo.FieldType;
                    var obj = property.objectReferenceValue;

                    if (!obj && attribute.scope.HasFlag(ComponentScope.Self))
                        obj = component.GetComponent(type);

                    if (!obj && attribute.scope.HasFlag(ComponentScope.Parent))
                        obj = component.GetComponentInParent(type);

                    if (!obj && attribute.scope.HasFlag(ComponentScope.Children))
                        obj = component.GetComponentInChildren(type);

                    if (!obj && attribute.scope.HasFlag(ComponentScope.Scene))
                        obj = GameObject.FindObjectOfType(type);

                    property.objectReferenceValue = obj;
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
}