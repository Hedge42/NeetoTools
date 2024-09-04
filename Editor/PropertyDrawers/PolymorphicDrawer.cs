using System;
using UnityEngine;
using UnityDropdown.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Neeto
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(PolymorphicAttribute))]
    public partial class PolymorphicDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DrawGUI(position, property, label, GetManagedReferenceFieldType(property), true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }

        /// Gets real type of managed reference
        public static Type GetManagedReferenceFieldType(SerializedProperty property)
        {
            var realPropertyType = GetRealTypeFromTypename(property.managedReferenceFieldTypename);
            if (realPropertyType != null)
                return realPropertyType;

            Debug.LogError($"Can not get field type of managed reference : {property.managedReferenceFieldTypename}");
            return null;
        }
        /// Gets real type of managed reference's field typeName
        public static Type GetRealTypeFromTypename(string stringType)
        {
            var names = GetSplitNamesFromTypename(stringType);
            var realType = Type.GetType($"{names.ClassName}, {names.AssemblyName}");
            return realType;
        }
        /// Get assembly and class names from typeName
        public static (string AssemblyName, string ClassName) GetSplitNamesFromTypename(string typename)
        {
            if (string.IsNullOrEmpty(typename))
                return ("", "");

            var typeSplitString = typename.Split(char.Parse(" "));
            var typeClassName = typeSplitString[1];
            var typeAssemblyName = typeSplitString[0];
            return (typeAssemblyName, typeClassName);
        }

        public static void DrawGUI(Rect position, SerializedProperty property, GUIContent label, Type returnType, bool shadow = false)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                Debug.LogError($"Property '{property.propertyPath}' is not marked as [SerializeReference]");
            }

            EditorGUI.BeginProperty(position, label, property);

            //MEdit.BeginShadow(position.WithHeight(EditorGUI.GetPropertyHeight(property)));
            if (shadow)
                NGUI.IndentBoxGUI(position);

            var linePosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            //GUI.Box(linePosition, GUIContent.none); // draw below the label
            if (EditorGUI.DropdownButton(position: position.Add(xMin: EditorGUIUtility.labelWidth).With(height: NGUI.lineHeight),
                                         content: new GUIContent(property.managedReferenceValue.TypeNameOrNull()), FocusType.Passive))
                ActivatorMenu(property, returnType).ShowAsContext();

            ManagedReferenceCopyPaste.ContextMenu(property, linePosition);
            EditorGUI.PropertyField(position, property, label, true);
            if (shadow)
                NGUI.EndShadow();
            EditorGUI.EndProperty();
        }
        public static DropdownMenu<Type> ActivatorMenu(SerializedProperty property, Type valueType)
        {
            var serializedObject = property.serializedObject;
            var propertyPath = property.propertyPath;
            var types = NReflect.GetAssignableReferenceTypes(valueType);

            //Debug.Log($"Showing ({types.Count()}) potential classes for ({valueType.NameOrNull()})");

            DropdownItem<Type> Create(Type type)
            {

                var label = $"{type.ModuleName()}/{type.GetDeclaringString()}{type.GetInheritingString()}";
                var item = new DropdownItem<Type>(type, label);
                item.IsSelected = type.Equals(serializedObject.FindProperty(propertyPath)?.managedReferenceValue?.GetType());
                return item;
            }

            var items = types.Select(Create).ToList();
            var menu = new DropdownMenu<Type>(items, OnItemSelected, sortItems: true, showNoneElement: true);

            void OnItemSelected(Type type)
            {
                var property = serializedObject.FindProperty(propertyPath);
                var sourceValue = property.managedReferenceValue;
                var targetValue = (object)null;

                if (type != null && !typeof(Object).IsAssignableFrom(type))
                    targetValue = Activator.CreateInstance(type);

                PolymorphicDrawer.CopyMatchingFields(targetValue, sourceValue, serializedObject.targetObject);
                serializedObject.Update();
                property.managedReferenceValue = targetValue;
                serializedObject.ApplyModifiedProperties();
            };

            return menu;
        }

        
        public static void CopyMatchingFields(object destTarget, object sourceTarget, object reference)
        {
            if (sourceTarget == null || destTarget == null)
                return;

            var targetType = destTarget.GetType();
            var sourceType = sourceTarget.GetType();

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.FlattenHierarchy;

            var sourceFields = sourceType.GetFields(flags);
            foreach (var sourceField in sourceFields)
            {
                try
                {
                    var targetField = targetType.GetField(sourceField.Name);
                    targetField.SetValue(destTarget, sourceField.GetValue(sourceTarget));
                }
                catch // (Exception e)
                {
                    // target does not contain source field
                    //Debug.LogWarning($"Could not copy property {sourceField.Name}", (UnityEngine.Object)reference);
                }
            }
        }
    }

#endif
}