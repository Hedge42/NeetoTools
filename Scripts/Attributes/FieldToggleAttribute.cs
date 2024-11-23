#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Neeto
{
    public class FieldToggleAttribute : PropertyAttribute
    {
        public string fieldName { get; }
        public bool isSibling { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName">name of the boolean field that the toggle represents</param>
        /// <param name="isSibling">false: the property is a child</param>
        public FieldToggleAttribute(string fieldName, bool isSibling = true)
        {
            this.isSibling = isSibling;
            this.fieldName = fieldName;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(FieldToggleAttribute))] 
    public class FieldToggleAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (NGUI.Property(position, property, false))
            {
                var Attribute = attribute as FieldToggleAttribute;
                var toggle = Attribute.isSibling
                    ? property.FindSiblingProperty(Attribute.fieldName)
                    : property.FindPropertyRelative(Attribute.fieldName);
                var fieldRect = EditorGUI.PrefixLabel(position.Move(xMin: 17), label).Move(xMin: -30);

                EditorGUI.PropertyField(fieldRect, property, GUIContent.none);
                if (!(toggle.boolValue = EditorGUI.ToggleLeft(position, GUIContent.none, toggle.boolValue)))
                    GUI.Box(position, NGUI.shadow);

                EditorGUI.LabelField(position.With(w: 30), new GUIContent("", toggle.name + "?"));
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.GetHeight();
        }
    }
#endif
}
