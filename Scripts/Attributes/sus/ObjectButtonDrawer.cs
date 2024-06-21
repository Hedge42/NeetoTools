#if UNITY_EDITOR

using Matchwork;
using UnityEditor;
using UnityEngine;

public abstract class ObjectButtonDrawer : PropertyDrawer
{
    public virtual GUIStyle style => Styles.iconButton;
    public abstract GUIContent content { get; }
    public abstract void OnClick(SerializedProperty property);
    public virtual bool IsEnabled(SerializedProperty property) => property.objectReferenceValue;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        position.width -= 20;
        var buttonPos = position.WithWidth(18f);
        buttonPos.x += position.width + 1;

        EditorGUI.BeginDisabledGroup(!IsEnabled(property));
        if (GUI.Button(buttonPos, content, style))
        {
            OnClick(property);
        }
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginProperty(position, label, property);
        //property.objectReferenceValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, fieldInfo.FieldType, true);
        EditorGUI.PropertyField(position, property, label);
        EditorGUI.EndProperty();
    }


}



#endif

