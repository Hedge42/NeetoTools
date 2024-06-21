//using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace mtk
{
    [CustomPropertyDrawer(typeof(DropdownAttribute), true)]
    public class DropdownAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var buttonPos = position.With(xMin: position.xMax - 20f);
            position = position.Add(width: -20f);
            EditorGUI.PropertyField(position, property, label);
            if (EditorGUI.DropdownButton(buttonPos, new GUIContent("↓"), FocusType.Passive))
            {
                var factory = (attribute as DropdownAttribute).factory;
                var method = fieldInfo.DeclaringType.GetMethod(factory);
                var listType = method.ReturnType;
                if (listType.IsArray && listType.GetElementType().Equals(fieldInfo.FieldType))
                {
                }
            }
            //var position = EditorGUIUtility.labelWidth
        }
    }
}
