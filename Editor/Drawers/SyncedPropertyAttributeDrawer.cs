using UnityEngine;
using Toolbox.Editor;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;


namespace Neeto
{

    [CustomPropertyDrawer(typeof(SyncedPropertyAttribute))]
    public class SyncedPropertyAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (NGUI.Property(position, property, false))
            {
                var attribute = base.attribute as SyncedPropertyAttribute;
                var fieldTarget = property.GetDeclaringObject();
                var propertyInfo = fieldTarget.GetType().GetProperty(attribute.propertyInfoName);

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
                else if (propertyInfo.GetMethod == null)
                {
                    EditorGUI.HelpBox(position, $"property '{attribute.propertyInfoName}' must at least have an accessible getter", MessageType.Error);
                    return;
                }

                var hasSetter = propertyInfo.SetMethod != null;
                var propertyTarget = propertyInfo.IsStatic() ? null : fieldTarget;

                EditorGUI.BeginChangeCheck();
                EditorGUI.BeginDisabledGroup(!hasSetter);

                EditorGUI.PropertyField(position, property, label);
                var fieldValue = property.GetProperValue(fieldInfo);
                var propertyValue = propertyInfo.GetValue(propertyTarget);

                EditorGUI.EndDisabledGroup();
                if (EditorGUI.EndChangeCheck() || propertyValue != fieldValue)
                {
                    if (hasSetter)
                        propertyInfo.SetValue(propertyTarget, fieldValue); // set via propertyInfo instead of field
                    else
                        property.SetProperValue(fieldInfo, propertyValue); // read-only
                }
            }
        }
    }
}