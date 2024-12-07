using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class UserSettingAttribute : PropertyAttribute
{
    public string Key { get; }

    public UserSettingAttribute(string key)
    {
        Key = key;
    }

    public void Save(object value)
    {
        string json = JsonUtility.ToJson(new Wrapper { Value = value });
        PlayerPrefs.SetString(Key, json);
        PlayerPrefs.Save();
    }

    public object Load(Type type)
    {
        if (!PlayerPrefs.HasKey(Key)) return GetDefaultValue(type);

        string json = PlayerPrefs.GetString(Key);
        return JsonUtility.FromJson<Wrapper>(json)?.Value ?? GetDefaultValue(type);
    }

    private object GetDefaultValue(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    [Serializable]
    private class Wrapper
    {
        public object Value;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(UserSettingAttribute))]
    public class UserSettingDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            UserSettingAttribute settingAttribute = (UserSettingAttribute)attribute;

            // Load the value from PlayerPrefs and update the serialized property
            object loadedValue = settingAttribute.Load(GetFieldType(property));
            SetPropertyValue(property, loadedValue);

            // Draw the property using the default property drawer to ensure compatibility
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property, label, true);

            if (EditorGUI.EndChangeCheck())
            {
                // Save the updated value back to PlayerPrefs
                SavePropertyValue(settingAttribute, property);
            }
        }
        private Type GetFieldType(SerializedProperty property)
        {
            // Use reflection to get the field type from the serialized property
            var targetObject = property.serializedObject.targetObject;
            var fieldInfo = targetObject.GetType().GetField(property.propertyPath);
            return fieldInfo?.FieldType;
        }

        private void SetPropertyValue(SerializedProperty property, object value)
        {
            if (value == null) return;

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.intValue = Convert.ToInt32(value);
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = Convert.ToSingle(value);
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = value.ToString();
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = Convert.ToBoolean(value);
                    break;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = (Vector3)value;
                    break;
                    // Add more cases if needed to support additional types
            }
        }

        private void SavePropertyValue(UserSettingAttribute settingAttribute, SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    settingAttribute.Save(property.intValue);
                    break;
                case SerializedPropertyType.Float:
                    settingAttribute.Save(property.floatValue);
                    break;
                case SerializedPropertyType.String:
                    settingAttribute.Save(property.stringValue);
                    break;
                case SerializedPropertyType.Boolean:
                    settingAttribute.Save(property.boolValue);
                    break;
                case SerializedPropertyType.Vector3:
                    settingAttribute.Save(property.vector3Value);
                    break;
                    // Add more cases if needed to support additional types
            }
        }
    }
#endif
}
