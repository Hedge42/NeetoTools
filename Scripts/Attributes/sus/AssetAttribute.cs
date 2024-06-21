using System;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
[CustomPropertyDrawer(typeof(AssetAttribute))]
public class AssetAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var component = property.serializedObject.targetObject as Component;

        EditorGUI.BeginChangeCheck();
        if (component && property.objectReferenceValue == null)
        {
            var attribute = this.attribute as AssetAttribute;
            var type = attribute.type ?? fieldInfo.FieldType;
            var query = $"t:{type} {attribute.filter}";
            var assets = AssetDatabase.FindAssets(query);

            if (assets.Length == 0)
            {
                Debug.LogError($"No asset found matching query '{query}'");
                return;
            }
            else
            {
                if (assets.Length > 1)
                    Debug.LogWarning($"{assets.Length} assets found matching query '{query}.' Only taking first.");

                var path = AssetDatabase.GUIDToAssetPath(assets[0]);
                property.objectReferenceValue = AssetDatabase.LoadAssetAtPath(path, type);
            }

            property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }

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
}

#endif

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class AssetAttribute : PropertyAttribute
{
    public string filter;
    public Type type;
    public AssetAttribute(string filter = null, Type type = null)
    {
        this.filter = filter;
        this.type = type;
    }
}
