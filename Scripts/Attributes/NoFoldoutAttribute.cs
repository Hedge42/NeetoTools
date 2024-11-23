using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Neeto
{
    public class NoFoldoutAttribute : PropertyAttribute { }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(NoFoldoutAttribute))]
    public class NoFoldoutDrawer : PropertyDrawer
    {
        //float height;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            /* Me: I will handle potential error cases appropriately for proper design and stability
             * Also me: If a tree falls and no one is around to hear it, does it make a noise? ☠
             */
            try
            {
                property.isExpanded = true;

                var p = property.GetEndProperty();
                property.NextVisible(true);

                do
                {
                    EditorGUI.BeginChangeCheck();
                    position.height = property.GetHeight();
                    EditorGUI.PropertyField(position, property, true);
                    position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                    property.NextVisible(false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(property.serializedObject.targetObject);
                    }
                }
                while (!property.propertyPath.Equals(p.propertyPath));
            }
            catch { /*get fucked lmao*/ }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isArray)
            {
                float height = 0;
                var arraySize = property.FindPropertyRelative("Array.size");
                for (int i = 0; i < arraySize.intValue; i++)
                {
                    height += EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(i)) + EditorGUIUtility.standardVerticalSpacing;
                }
                // Add extra space for size field
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                return height;
            }

            return property.GetHeight() - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing;
        }
    }
#endif
}

