using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Neeto
{
    using System;
    using System.Reflection;
#if UNITY_EDITOR
    using UnityEditor;
    [CustomPropertyDrawer(typeof(SyncedPropertyAttribute))]
    public class SyncedPropertyAttributeDrawer : PropertyDrawer
    {
        public const BindingFlags FLAGS =
        BindingFlags.Public
        | BindingFlags.NonPublic
        | BindingFlags.Static
        | BindingFlags.Instance
        | BindingFlags.GetProperty
        | BindingFlags.GetField
        | BindingFlags.FlattenHierarchy
        //| BindingFlags.DeclaredOnly
            ;

        const MemberTypes TYPES = MemberTypes.Property | MemberTypes.Field;



        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var _attribute = attribute as SyncedPropertyAttribute;
            var propName = _attribute.propertyInfoName;
            var target = property.serializedObject.targetObject;
            //var path = property.propertyPath.Substring(0, property.propertyPath.IndexOf('.'));

            var path = property.propertyPath.Replace($"{fieldInfo.Name}", propName);

            var info = GetPropertyInfo(property.serializedObject, path, out var obj);

            //var info = property.serializedObject.targetObject.GetType().GetProperty(path, FLAGS);
            //var propertyInfo = property.serializedObject.targetObject.GetType().GetProperty(_attribute.propertyInfoName, FLAGS);

            // validate propertyInfo
            bool validProperty = info != null && info.PropertyType.Equals(fieldInfo.FieldType);

            //bool setProperty = propertyInfo != null && propertyInfo.PropertyType.IsAssignableFrom(fieldInfo.FieldType);

            //fieldInfo.decl

            // assign field value from synced property
            if (validProperty)
            {
                fieldInfo.SetValue(obj, info.GetValue(obj));
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            }

            EditorGUI.BeginChangeCheck();

            // draw property normally
            EditorGUI.PropertyField(position, property, label);

            // display error
            if (!validProperty)
            {
                var fieldRect = GUILayoutUtility.GetRect(position.width, 30);
                EditorGUI.HelpBox(fieldRect, "Invalid property: types must match", MessageType.Error);
            }

            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObject(target, "Update property");
                if (validProperty && info.SetMethod != null)
                    info.SetValue(obj, fieldInfo.GetValue(obj));

                // update fieldInfo before applying to propertyInfo
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();

                //property.ApplyAndMarkDirty();

                // route result through property


                //EditorUtility.SetDirty(target);
            }
        }

        public static PropertyInfo GetPropertyInfo(SerializedObject serializedObject, string propertyPath, out object target)
        {
            //Debug.Log("Searching for property " + propertyPath);
            var pathParts = propertyPath.Split('.');
            object currentObject = serializedObject.targetObject;
            PropertyInfo propertyInfo = null;

            for (int i = 0; i < pathParts.Length; i++)
            {
                string part = pathParts[i];
                if (currentObject == null) break;

                var currentType = currentObject.GetType();
                propertyInfo = currentType.GetProperty(part, FLAGS);

                if (propertyInfo != null)
                {
                    //Debug.Log("Found property " + propertyInfo.Name);

                    if (i < pathParts.Length - 1)
                        currentObject = propertyInfo.GetValue(currentObject, null);
                }
                else
                {
                    // If the property isn't found on the current object, it might be a field or a property of a field.
                    var fieldInfo = currentType.GetField(part, FLAGS);
                    if (fieldInfo != null)
                    {
                        //Debug.Log("Found field " + fieldInfo.Name);
                        currentObject = fieldInfo.GetValue(currentObject);
                    }
                    else
                    {
                        // Handle array and list elements which have a propertyPath segment like Array.data[0]
                        if (part.StartsWith("Array.data["))
                        {
                            var index = int.Parse(part.Substring("Array.data[".Length, part.Length - "Array.data[".Length - 1));
                            if (currentObject is System.Collections.IList list && index < list.Count)
                            {
                                currentObject = list[index];
                            }
                            else
                            {
                                currentObject = null;
                            }
                        }
                    }
                }
            }

            target = currentObject;

            return propertyInfo;
        }
    }
#endif

    /// <summary>
    /// Route an inspector's field through a property
    /// </summary>
    public class SyncedPropertyAttribute : PropertyAttribute
    {
        public string propertyInfoName;

        /// <summary>
        /// Synced property's Type must match field, and must contain getter and setter
        /// </summary>
        /// <param name="propertyInfoName">tip: use nameof(property)</param>
        public SyncedPropertyAttribute(string propertyInfoName)
        {
            this.propertyInfoName = propertyInfoName;
            this.order = -1;
        }
    }
}