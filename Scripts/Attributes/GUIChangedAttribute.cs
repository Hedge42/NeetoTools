using System.Reflection;
using Debug = UnityEngine.Debug;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Neeto
{
    public class GUIChangedAttribute : PropertyAttribute
    {
        public string callback;
        public BindingFlags flags;
        public bool markDirty;
        public GUIChangedAttribute(string callback,
            bool markDirty = false,
            BindingFlags flags =
                //BindingFlags.Static |
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.InvokeMethod
            )
        {
            this.markDirty = markDirty;
            this.callback = callback;
            this.flags = flags;
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(GUIChangedAttribute))]
        public class GUIChangedDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(position, property, label);
                if (EditorGUI.EndChangeCheck())
                {
                    var attribute = this.attribute as GUIChangedAttribute;
                    var method = property.FindMethod(attribute.callback, out var target);

                    if (method == null)
                    {
                        Debug.LogError($"[GUIChanged] '{attribute.callback}' not found", property.serializedObject.targetObject);
                        return;
                    }

                    if (attribute.markDirty)
                        Undo.RecordObject(property.serializedObject.targetObject, "OnGUIChanged");

                    method.Invoke(target, new object[0]);

                    if (attribute.markDirty)
                    {
                        property.serializedObject.Update();
                        EditorUtility.SetDirty(property.serializedObject.targetObject);
                    }
                }
            }


        }
#endif
    }
}