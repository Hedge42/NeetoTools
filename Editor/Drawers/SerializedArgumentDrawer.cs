using Toolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Neeto.Exp
{
    [CustomPropertyDrawer(typeof(SerializedArgument))]
    public class SerializedArgumentDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (NGUI.Property(position, property))
            {
                var value = property.GetProperValue(fieldInfo) as SerializedArgument;
                switch (value.ArgumentType)
                {
                    case ArgumentType.UnityObject:
                        EditorGUI.ObjectField(position, property.FindPropertyRelative(nameof(SerializedArgument.UnityObject)), value.Type);
                        break;
                    case ArgumentType.ReferenceObject:
                        EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(SerializedArgument.ReferenceObject)), GUIContent.none);
                        break;
                    case ArgumentType.ValueObject:
                        EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(SerializedArgument.ValueObject)), GUIContent.none);
                        break;
                    default:
                        EditorGUI.LabelField(position, "Unsupported Type");
                        break;
                };
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var argumentType = (ArgumentType)property.FindPropertyRelative(nameof(SerializedArgument.ArgumentType)).enumValueIndex;
            return argumentType switch
            {
                ArgumentType.UnityObject => EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(SerializedArgument.UnityObject))),
                ArgumentType.ReferenceObject => EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(SerializedArgument.ReferenceObject))),
                ArgumentType.ValueObject => EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(SerializedArgument.ValueObject))),
                _ => EditorGUIUtility.singleLineHeight
            };
        }
    }
}
