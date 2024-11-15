using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Neeto;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PolymorphicAttribute : PropertyAttribute
{
    public Type[] include { get; }
    public Type[] exclude { get; }
    public Type Default { get; }
    public BindingFlags flags { get; }

    const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    public PolymorphicAttribute(Type Default = null, Type[] include = null, Type[] exclude = null, BindingFlags flags = FLAGS)
    {
        this.exclude = exclude;
        this.include = include;
        this.flags = flags;
        this.Default = Default;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(PolymorphicAttribute))]
public class PolymorphicDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //var i = EditorGUI.indentLevel;
        //EditorGUI.indentLevel = 0;
        using (NGUI.Property(position, property))
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                Debug.LogError($"Property '{property.propertyPath}' is not marked as [SerializeReference]");
                return;
            }
            if (attribute is PolymorphicAttribute Attribute && property.managedReferenceValue == null && Attribute.Default != null)
            {
                property.managedReferenceValue = Activator.CreateInstance(Attribute.Default);
            }

            DrawGUI(position, property, label, property.GetManagedReferenceFieldType());
        }
        //EditorGUI.indentLevel = i;
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return property.GetHeight();
    }
    public static void DrawGUI(Rect position, SerializedProperty property, GUIContent label, Type returnType)
    {
        if (property.propertyType != SerializedPropertyType.ManagedReference)
        {
            Debug.LogError($"Property '{property.propertyPath}' is not marked as [SerializeReference]");
            return;
        }

        var line = position.With(h: NGUI.LineHeight);
        var dropdownRect = EditorGUI.PrefixLabel(line, label);
        if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(property.managedReferenceValue.TypeNameOrNull()), FocusType.Passive))
            ShowMenu(property, returnType);

        NGUI.ContextMenu(property, dropdownRect);

        if (property.HasChildProperties())
        {
            position = line.With(xMax: dropdownRect.xMin);
            if (property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, GUIContent.none, true))
            {
                dropdownRect.y += NGUI.FullLineHeight;
                EditorGUI.indentLevel++;
                property.DrawProperties(line.Move(y: NGUI.FullLineHeight));
                EditorGUI.indentLevel--;
            }
        }
    }
    public static void ShowMenu(SerializedProperty property, Type valueType)
    {
        var serializedObject = property.serializedObject;
        var propertyPath = property.propertyPath;
        var types = ReflectionHelper.GetAssignableReferenceTypes(valueType)
            .Select(t => (t, GetDisplayPath(t))).ToArray();
        var selected = property.managedReferenceValue != null ?
            GetDisplayPath(property.managedReferenceValue.GetType()) : "none";

        DropdownHelper.Show(OnItemSelected, true, true, selected, types);

        void OnItemSelected(Type type)
        {
            var property = serializedObject.FindProperty(propertyPath);
            var sourceValue = property.managedReferenceValue;
            var targetValue = (object)null;

            if (type != null && !typeof(UnityEngine.Object).IsAssignableFrom(type))
                targetValue = Activator.CreateInstance(type);

            NGUI.CopyMatchingFields(targetValue, sourceValue);
            serializedObject.Update();
            property.managedReferenceValue = targetValue;
            serializedObject.ApplyModifiedProperties();
        };
    }
    public static string GetDisplayPath(Type t)
    {
        return $"{t.ModuleName()}/{t.GetDeclaringString()}{t.GetInheritingString()}";
    }
}
#endif