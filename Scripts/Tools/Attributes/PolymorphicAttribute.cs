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
        DrawGUI(position, property, label, GetReferenceType(property), true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property);
    }

    /// Gets real type of managed reference
    public static Type GetReferenceType(SerializedProperty property)
    {
        var strings = property.managedReferenceFieldTypename.Split(char.Parse(" "));
        var type = Type.GetType($"{strings[1]}, {strings[0]}");

        if (type == null)
            Debug.LogError($"Can not get field type of managed reference : {property.managedReferenceFieldTypename}");

        return type;
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
            ShowMenu(property, returnType);

        ReferenceHelper.ContextMenu(property, linePosition);
        EditorGUI.PropertyField(position, property, label, true);
        if (shadow)
            NGUI.EndShadow();
        EditorGUI.EndProperty();
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

            PolymorphicDrawer.CopyMatchingFields(targetValue, sourceValue, serializedObject.targetObject);
            serializedObject.Update();
            property.managedReferenceValue = targetValue;
            serializedObject.ApplyModifiedProperties();
        };
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

    public static string GetDisplayPath(Type t)
    {
        return $"{t.ModuleName()}/{t.GetDeclaringString()}{t.GetInheritingString()}";
    }
}
#endif