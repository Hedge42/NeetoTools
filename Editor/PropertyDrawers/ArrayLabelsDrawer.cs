using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Buffers;

namespace Matchwork
{
    //[CustomPropertyDrawer(typeof(ArrayLabelsAttribute))]
    public class ArrayLabelsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DrawGUI(position, property, label);
        }
        public static void DrawGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.isArray)
            {
                //EditorGUI.BeginProperty(position, label, property);
                var newLabel = $"{property.displayName} [{property.arraySize}]";
                EditorGUI.PropertyField(position, property, new GUIContent(newLabel), true);
                //EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }
        public static GUIContent GetManagedReferenceLabel(SerializedProperty property)
        {
            try
            {
                var label = new GUIContent(property.displayName);

                var text = FindLabel(property);
                if (text != null)
                    label.text = text;

                return label;
            }
            catch (Exception e)
            {
                e.LogWarning($"Failed to get label from property {property.propertyPath}");
                return new GUIContent(property.displayName);
            }
        }

        private static string FindLabel(SerializedProperty property)
        {
            /*
             try to find by interface
             */
            if (property.managedReferenceValue is ILabelOverride o)
            {
                if (!string.IsNullOrEmpty(o.label))
                    return o.label;
            }

            /*
             give default label
             */
            var isElement = property.GetArrayElementIndex(out var elementIndex);
            if (isElement && property.GetArrayProperty(out var arrayProp))
            {
                return $"{arrayProp.displayName} [{elementIndex}]";
            }

            else
            {
                return null;
            }
        }
    }
}