using UnityEngine;
using Object = UnityEngine.Object;
using Toolbox;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Neeto
{

    /// <summary>
    /// Open properties window using <see cref="EditorUtility.OpenPropertyEditor(Object)"/>
    /// </summary>
    public class InspectAttribute : PropertyAttribute { }

    public class InlineInspectAttribute : PropertyAttribute { }

    #region EDITOR
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InspectAttribute))]
    public class InspectAttributeDrawer : PropertyButtonDrawerBase
    {
        public override GUIContent content => EditorGUIUtility.IconContent("d_SearchQueryAsset Icon").With(tooltip: "Inspect");
        public override void OnClick(SerializedProperty property) => EditorUtility.OpenPropertyEditor(property.objectReferenceValue);
    }

    [CustomPropertyDrawer(typeof(InlineInspectAttribute))]
    public class InlineInspectAttributeDrawer : PropertyDrawer
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
                    var last = GUILayoutUtility.GetLastRect().y;
                    EditorGUI.indentLevel++;
                    Editor.CreateCachedEditor(property.objectReferenceValue, null, ref editor);
                    editor.OnInspectorGUI();
                    EditorGUI.indentLevel--;
                    Debug.Log(GUILayoutUtility.GetLastRect().y - last);
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Editor.CreateCachedEditor(property.objectReferenceValue, null, ref editor);
            var height = NGUI.FullLineHeight;
            if (property.objectReferenceValue)
            {
                height += NGUI.GetRecursiveHeight(editor.serializedObject);
            }
            Debug.Log(height);
            return height;
        }
    }
#endif
    #endregion
}