using UnityEngine;

namespace Neeto
{
    using System.Reflection;
    using Toolbox.Editor;
    using UnityEditor;
    using Rhinox.Lightspeed;
    using Rhinox.Lightspeed.Editor;
    using Rhinox.Lightspeed.Reflection;

    [CustomPropertyDrawer(typeof(SyncedPropertyAttribute))]
    public class SyncedPropertyAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (NGUI.Property(position, property, false))
            {
                var attribute = base.attribute as SyncedPropertyAttribute;
                var target =  property.FindReflectionTarget(fieldInfo);
                var propertyInfo = target.GetType().GetProperty(attribute.propertyInfoName);

                if (propertyInfo == null)
                {
                    EditorGUI.HelpBox(position, $"property '{attribute.propertyInfoName}' not found", MessageType.Error);
                    return;
                }
                else if (propertyInfo.PropertyType != fieldInfo.FieldType)
                {
                    EditorGUI.HelpBox(position, $"property '{attribute.propertyInfoName}' does not match field type of '{fieldInfo.Name}'", MessageType.Error);
                    return;
                }
                else if (propertyInfo.GetMethod == null || propertyInfo.SetMethod == null)
                {
                    EditorGUI.HelpBox(position, $"property '{attribute.propertyInfoName}' must have get and set accessors", MessageType.Error);
                    return;
                }

                // get value from property
                property.SetProperValue(fieldInfo, propertyInfo.GetValue(target));

                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(position, property, label);
                var changed = EditorGUI.EndChangeCheck();

                // display error
                if (changed)
                {
                    propertyInfo.SetValue(target, fieldInfo.GetValue(target));
                }
            }
        }
    }
}