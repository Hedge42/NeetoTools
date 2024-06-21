using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomPropertyDrawer(typeof(PreviewTextureAttribute))]
public class PreviewTextureDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Check if the property is a Texture2D
        if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null && property.objectReferenceValue.GetType() == typeof(Texture2D))
        {
            Texture2D texture = property.objectReferenceValue as Texture2D;

            // Calculate the preview size based on the texture's aspect ratio and the width of the inspector
            float previewSize = Mathf.Min(EditorGUIUtility.currentViewWidth, texture.width);

            float centerX = position.x + (EditorGUIUtility.currentViewWidth - position.x - previewSize) / 2f;

            // Draw the texture preview in the inspector
            EditorGUI.DrawPreviewTexture(new Rect(centerX, position.y, previewSize, texture.height * (previewSize / texture.width)), texture);
        }
        else
        {
            // Draw the default property field
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Check if the property is a Texture2D and return the height of the preview
        if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null && property.objectReferenceValue.GetType() == typeof(Texture2D))
        {
            Texture2D texture = property.objectReferenceValue as Texture2D;

            // Calculate the preview size based on the texture's aspect ratio and the width of the inspector
            float previewSize = Mathf.Min(EditorGUIUtility.currentViewWidth, texture.width);

            return texture.height * (previewSize / texture.width) + EditorGUIUtility.standardVerticalSpacing;
            //return previewSize;
        }
        else
        {
            // Return the default property height
            return base.GetPropertyHeight(property, label);
        }
    }
}
#endif
public class PreviewTextureAttribute : PropertyAttribute
{
    public PreviewTextureAttribute()
    {
    }
}