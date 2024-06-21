using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;


[CustomPropertyDrawer(typeof(BoolPopupAttribute))]
public class BoolPopupAttributeDrawer : PropertyDrawer
{
    BoolPopupAttribute _atr;
    BoolPopupAttribute atr => _atr == null ? _atr = attribute as BoolPopupAttribute : _atr;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();

        var result = EditorGUI.Popup(position, label.ToString(), GetInt(property), atr.options);

        property.boolValue = result == 0 ? false : true;

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

    private int GetInt(SerializedProperty prop)
    {

        return prop.boolValue ? 1 : 0;
    }
}
#endif

public class BoolPopupAttribute : PropertyAttribute
{
    public string[] options;


    public BoolPopupAttribute(string _falseText, string _trueText)
    {
        options = new string[]
        {
            _falseText,
            _trueText
        };
    }


}
