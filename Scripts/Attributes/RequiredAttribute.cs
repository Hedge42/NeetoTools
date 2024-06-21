using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Matchwork
{
    #region EDITOR
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredAttributeDrawer : PropertyDrawer
    {
        public const float HEIGHT = 25f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (typeof(Object).IsAssignableFrom(fieldInfo.FieldType))
            {
                if (property.objectReferenceValue == null)
                {
                    var rect = GUILayoutUtility.GetRect(position.width, HEIGHT + EditorGUIUtility.standardVerticalSpacing);
                    EditorGUI.HelpBox(rect, "Object reference cannot be null", MessageType.Error);
                    position.y += rect.height;
                }
            }
            else
            {
                Debug.LogError("Attribute can only be drawn over an object reference");
            }

            EditorGUI.BeginChangeCheck();

            EditorGUI.PropertyField(position, property, label);

            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
    }
#endif
    #endregion

    /// <summary>
    /// Draws an error message if the object reference is null
    /// </summary>
    public class RequiredAttribute : PropertyAttribute
    {
    }
}