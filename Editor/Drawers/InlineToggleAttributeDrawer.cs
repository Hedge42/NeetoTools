#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Neeto
{
    [CustomPropertyDrawer(typeof(InlineToggleAttribute))] 
    public class InlineToggleAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (NGUI.Property(position, property, false))
            {
                var attribute = base.attribute as InlineToggleAttribute;
                var toggleProperty = property.FindSiblingProperty(attribute.fieldName);
                var fieldRect = EditorGUI.PrefixLabel(position.Offset(xMin: 17), label).Offset(xMin: -30);

                EditorGUI.PropertyField(fieldRect, property, GUIContent.none);
                if (!(toggleProperty.boolValue = EditorGUI.ToggleLeft(position, GUIContent.none, toggleProperty.boolValue)))
                    GUI.Box(position, NGUI.shadow);

                EditorGUI.LabelField(position.With(w: 30), new GUIContent("", toggleProperty.name + "?"));
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.GetHeight();
        }
    }
}
