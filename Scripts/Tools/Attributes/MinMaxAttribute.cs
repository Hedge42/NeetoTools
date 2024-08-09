using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(MinMaxAttribute))]
public class MinMaxDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();

        var attribute = base.attribute as MinMaxAttribute;

        if (!string.IsNullOrEmpty(attribute.label))
            label.text = attribute.label;
        position = EditorGUI.PrefixLabel(position, label);


        // draw vector2
        var value = GetValue(property);
        var fieldWidth = 80f;
        var sliderWidth = position.width - 2 * fieldWidth;

        position.width = fieldWidth;
        value.x = EditorGUI.FloatField(position, value.x);
        position.x += position.width;
        position.width = sliderWidth;
        EditorGUI.MinMaxSlider(position, ref value.x, ref value.y, attribute.min, attribute.max);
        position.x += position.width;
        position.width = fieldWidth;
        value.y = EditorGUI.FloatField(position, value.y);

        if (EditorGUI.EndChangeCheck())
        {
            SetValue(property, value);
            //property.vector2Value = value;
            property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label);
    }

    public Vector2 GetValue(SerializedProperty property)
    {

        switch (property.propertyType)
        {
            case SerializedPropertyType.Vector2:
                return property.vector2Value;
            case SerializedPropertyType.Vector2Int:
                return property.vector2IntValue;
            case SerializedPropertyType.Float:
            case SerializedPropertyType.Integer:

                if (!GetProperties(property, out var minProp, out var maxProp))
                    return Vector2.zero;

                var minValue = minProp.propertyType == SerializedPropertyType.Float
                    ? minProp.floatValue
                    : minProp.intValue;

                var maxValue = maxProp.propertyType == SerializedPropertyType.Float
                    ? maxProp.floatValue
                    : maxProp.intValue;

                return new Vector2(minValue, maxValue);
            default:
                return Vector2.zero;
        }
    }
    bool GetProperties(SerializedProperty property, out SerializedProperty minProp, out SerializedProperty maxProp)
    {
        var attribute = this.attribute as MinMaxAttribute;

        var path = property.propertyPath.Substring(0, 1 + property.propertyPath.LastIndexOf('.'));
        minProp = property.serializedObject.FindProperty(path + attribute.minName);
        maxProp = property.serializedObject.FindProperty(path + attribute.maxName);

        if (minProp != null && maxProp != null)
            return true;

        if (minProp == null)
        {
            Debug.LogError($"could not find property {path + attribute.minName}", property.serializedObject.targetObject);
        }
        if (maxProp == null)
        {
            Debug.LogError($"could not find property {path + attribute.maxName}", property.serializedObject.targetObject);
        }
        return false;
    }
    public void SetValue(SerializedProperty property, Vector2 value)
    {

        switch (property.propertyType)
        {
            case SerializedPropertyType.Vector2:
                property.vector2Value = value;
                break;
            case SerializedPropertyType.Vector2Int:
                property.vector2IntValue = new Vector2Int((int)value.x, (int)value.y);
                break;
            case SerializedPropertyType.Float:
            case SerializedPropertyType.Integer:

                if (!GetProperties(property, out var minProp, out var maxProp))
                    break;

                switch (property.propertyType)
                {
                    case SerializedPropertyType.Float:
                        minProp.floatValue = value.x;
                        maxProp.floatValue = value.y;
                        break;
                    case SerializedPropertyType.Integer:
                        minProp.intValue = (int)value.x;
                        maxProp.intValue = (int)value.y;
                        break;
                }

                break;
            default:
                break;
        }
    }
}
#endif

public class MinMaxAttribute : PropertyAttribute
{
    public float min;
    public float max;
    public string minName;
    public string maxName;
    public string label;

    public MinMaxAttribute(float min, float max, string minName = null, string maxName = null)
    {
        this.min = min;
        this.max = max;
        this.minName = minName;
        this.maxName = maxName;
    }

    public MinMaxAttribute(string label, float min, float max, string minName = null, string maxName = null)
    {
        this.label = label;
        this.min = min;
        this.max = max;
        this.minName = minName;
        this.maxName = maxName;
    }
}
