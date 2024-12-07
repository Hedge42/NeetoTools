using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Neeto
{
    public class ExposedAttribute : PropertyAttribute { }


#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ExposedAttribute))]
    public class InlineInspectorAttributeDrawer : PropertyDrawer
    {
        Editor editor;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (NGUI.Property(position, property))
            {
                var fieldRect = EditorGUI.PrefixLabel(position.With(h: NGUI.LineHeight), label);
                var labelRect = fieldRect.With(xMin: position.xMin, xMax: fieldRect.xMin);
                var p = property.Copy();

                EditorGUI.PropertyField(position.With(h: NGUI.LineHeight), property, label);

                if (property.objectReferenceValue && NGUI.IsExpanded(property, labelRect))
                {
                    Editor.CreateCachedEditor(property.objectReferenceValue, null, ref editor);

                    EditorGUI.indentLevel++;
                    position.y += NGUI.FullLineHeight;
                    editor.serializedObject.DrawAllProperties(position);
                    EditorGUI.indentLevel--;
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Editor.CreateCachedEditor(property.objectReferenceValue, null, ref editor);
            var height = NGUI.FullLineHeight;
            if (property.objectReferenceValue && property.isExpanded)
            {
                height += editor.serializedObject.GetCumulativeHeight();
            }
            return height;
        }

    }
#endif
}