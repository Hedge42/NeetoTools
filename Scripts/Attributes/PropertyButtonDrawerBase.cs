#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Neeto
{

    /// <summary>
    /// Inherit from this to easily draw a button at the end of a line
    /// <br/><see cref="InspectAttributeDrawer"/>
    /// <br/><see cref="CreateInstanceAttributeDrawer"/>
    /// </summary>
    public abstract class PropertyButtonDrawerBase : PropertyDrawer
    {
        public abstract GUIContent content { get; }
        public abstract void OnClick(SerializedProperty property);
        public virtual bool IsEnabled(SerializedProperty property) => property.objectReferenceValue;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.width -= 20;
            var buttonPos = position.With(w: 18f);
            buttonPos.x += position.width + 1;

            EditorGUI.BeginDisabledGroup(!IsEnabled(property));
            if (GUI.Button(buttonPos, content, EditorStyles.iconButton))
            {
                OnClick(property);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndProperty();
        }
    }

}


#endif
