using UnityEngine;
using UnityEditor;
using Rhinox.Lightspeed.Reflection;
using System.Reflection;
using Toolbox.Editor;

namespace Neeto
{
    [CustomPropertyDrawer(typeof(GetLabelAttribute))]
    public class GetLabelAttributeDrawer : PropertyDrawer
    {
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var name = (attribute as GetLabelAttribute).property;

            // array element requires method to pass index into
            if (property.IsArrayElement(out int i))
            {
                var target = property.FindMethodTarget(name, out var method);
                if (method.TryGetValue(target, out string result, i))
                {
                    label.text = $"[{i}] {result}";
                }
            }
            else if (ReflectionUtility.TryGetMember(fieldInfo.ReflectedType, name, out var member, flags))
            {
                var target = property.GetDeclaringObject();
                if (member.TryGetValue(target, out string result) && !string.IsNullOrEmpty(result))
                {
                    label.text = result;
                }
            }
            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }
    }
}
