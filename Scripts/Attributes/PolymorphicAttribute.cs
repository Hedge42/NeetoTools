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
    public BindingFlags flags { get; }

    const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    public PolymorphicAttribute(Type[] include = null, Type[] exclude = null, BindingFlags flags = FLAGS)
    {
        this.exclude = exclude;
        this.include = include;
        this.flags = flags;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(PolymorphicAttribute))]
public class PolymorphicDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        DrawGUI(position, property, label, property.GetManagedReferenceFieldType());
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return property.GetHeight();
    }
    public static void DrawGUI(Rect position, SerializedProperty property, GUIContent label, Type returnType)
    {
        using (NGUI.Property(position, property, label))
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                Debug.LogError($"Property '{property.propertyPath}' is not marked as [SerializeReference]");
                return;
            }
            var line = position.With(h: NGUI.LineHeight);
            if (EditorGUI.DropdownButton(EditorGUI.PrefixLabel(line, label), new GUIContent(property.managedReferenceValue.TypeNameOrNull()), FocusType.Passive))
                ShowMenu(property, returnType);

            NGUI.ContextMenu(property, line);
            EditorGUI.PropertyField(position, property, GUIContent.none, true);
            //EditorGUI.indentLevel = 0;
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