using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System;


#if UNITY_EDITOR
using UnityEditor;
[CustomPropertyDrawer(typeof(LabelAttribute))]
public class LabelAttributeDrawer : PropertyDrawer
{
    LabelAttribute _atr;
    LabelAttribute atr => _atr == null ? _atr = attribute as LabelAttribute : _atr;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // indexed label override, only works for array elements
        if (atr.index >= 0)
        {
            try
            {
                var i = GetLastIntegerBetweenBrackets(property.propertyPath);
                if (i == (int)atr.index)
                {
                    label = new GUIContent(atr.text);
                }
            }
            catch
            {
                Debug.LogError($"Error getting index from property {property.propertyPath}", property.serializedObject.targetObject);
            }
        }
        else
        {
            // usual, non-array case
            label = new GUIContent(atr.text);
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

    public static int GetLastIntegerBetweenBrackets(string input)
    {
        Match match = Regex.Match(input, @".*\[(\d+)\][^\[]*$");

        if (match.Success)
        {
            return int.Parse(match.Groups[1].Value);
        }
        else
        {
            throw new ArgumentException("The input string does not contain a valid integer between the last set of square brackets.");
        }
    }
}
#endif


public class LabelAttribute : PropertyAttribute
{
    public int index { get; private set; }
    public string text { get; private set; }
    public LabelAttribute(string _text, int _index = -1)
    {
        this.text = _text;
        this.index = _index;
    }
}

