using System;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Neeto
{
    [Serializable]
    public abstract class ObjectReferenceBase
    {
        [field: SerializeField] protected UnityEngine.Object _Object;

        [CustomPropertyDrawer(typeof(ObjectReferenceBase), true)]
        class ObjectReferenceDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                using (NGUI.Property(position, property, false))
                {
                    var referenceType = fieldInfo.FieldType.GenericTypeArguments[0];
                    property = property.FindPropertyRelative(nameof(_Object));


                    // support type changes...
                    if (property.objectReferenceValue && !referenceType.IsAssignableFrom(property.objectReferenceValue.GetType()))
                        property.objectReferenceValue = null;

                    EditorGUI.BeginChangeCheck();
                    var reference = EditorGUI.ObjectField(position, label, property.objectReferenceValue, typeof(Object), true);
                    EditorGUI.LabelField(position.Offset(xMax: -25), referenceType.Name, EditorStyles.label.With(alignment: TextAnchor.MiddleRight));
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (reference == null)
                        {
                            property.objectReferenceValue = null;
                            return;
                        }

                        // support drag-n-drop
                        if (!referenceType.IsAssignableFrom(reference.GetType()))
                        {
                            if (reference is GameObject gameObject)
                                reference = gameObject.GetComponent(referenceType);

                            else if (reference is Component component)
                                reference = component.GetComponent(referenceType);
                        }
                        // validate...
                        if (reference && referenceType.IsAssignableFrom(reference.GetType()))
                        {
                            property.objectReferenceValue = reference;
                        }
                    }
                }
            }
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return NGUI.LineHeight;
            }
        }
    }

    [Serializable]
    public class ObjectReference<T> : ObjectReferenceBase
    {
        public static implicit operator T(ObjectReference<T> reference) => reference.Object;
        public T Object => (T)(object)Object;
    }
}
