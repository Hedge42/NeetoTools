using UnityEngine;
using System;
using System.Collections;
using Object = UnityEngine.Object;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Matchwork
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ScriptAttribute : PropertyAttribute
    {
        public string assetPath;
        public ScriptAttribute(string assetPath)
        {
            this.assetPath = assetPath;
        }
    }

#if UNITY_EDITOR
    //[CustomPropertyDrawer(typeof(ScriptAttribute))]
    public class ScriptDrawer // : PropertyDrawer
    {
        public static TextAsset FindScript(SerializedProperty property)
        {
            TextAsset script = null;
            object value = null;
            switch (property.propertyType)
            {
                case SerializedPropertyType.ManagedReference:
                    value = property.managedReferenceValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    value = property.objectReferenceValue;
                    break;
                case SerializedPropertyType.ExposedReference:
                    value = property.exposedReferenceValue;
                    break;
                default:
                    break;
            }
            if (value != null)
            {
                var type = value.GetType();
                var attribute = type.GetCustomAttribute<ScriptAttribute>();
                if (attribute != null)
                {
                    script = AssetDatabase.LoadAssetAtPath<TextAsset>(attribute.assetPath);
                }
            }

            return script;
        }
        public static Rect Draw(Rect position, SerializedProperty property)
        {
            var script = FindScript(property);
            if (script != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.ObjectField(position, "Script", script, typeof(TextAsset), false);
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.EndDisabledGroup();
            }
            return position;
        }
        public static float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var script = FindScript(property);
            var height = EditorGUI.GetPropertyHeight(property, label, true);
            if (script != null && property.isExpanded)
                height += MEdit.fullLineHeight;
            return height;
        }
        public static float GetExpandablePropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUI.GetPropertyHeight(property, label, true);
            if (property.isExpanded)
                height += MEdit.fullLineHeight;
            return height;
        }
    }
#endif
}