using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PreviewAnimationAttribute : PropertyAttribute
{
    #region EDITOR
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(PreviewAnimationAttribute))]
    class PreviewInInspectorDrawer : PropertyDrawer
    {
        private static GameObject lastSelectedObject;

        static PreviewInInspectorDrawer()
        {
            // Listen for changes in selection
            EditorApplication.update += OnEditorUpdate;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Float)
            {
                EditorGUI.LabelField(position, label.text, "Use PreviewInInspector with float.");
                return;
            }

            // Get the attribute and find the AnimationClip by field name
            PreviewAnimationAttribute previewAttr = (PreviewAnimationAttribute)attribute;
            SerializedProperty clipProperty = FindSiblingProperty(property, previewAttr.clipName);
            if (clipProperty == null || clipProperty.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.LabelField(position, label.text, "Invalid AnimationClip reference.");
                return;
            }

            // Reset to T-Pose
            Rect buttonPosition = position;
            buttonPosition.xMin = buttonPosition.xMax - 20;
            if (GUI.Button(buttonPosition, new GUIContent("T", "Reset to T-Pose")))
            {
                if (Selection.activeGameObject?.GetComponent<Animator>() is Animator animator)
                {
                    animator.Rebind();
                    animator.Update(0f); // Force immediate reset to T-pose or default pose
                    EditorUtility.SetDirty(Selection.activeGameObject);
                    return;
                }
            }
            position.xMax -= 20;

            AnimationClip clip = clipProperty.objectReferenceValue as AnimationClip;

            // Check if the slider value is being adjusted
            EditorGUI.BeginChangeCheck();
            property.floatValue = EditorGUI.Slider(position, label, property.floatValue, 0f, 1f);


            if (EditorGUI.EndChangeCheck() && clip != null && !Application.isPlaying)
            {
                // Only preview the animation while adjusting the slider
                GameObject selectedObject = Selection.activeGameObject;
                if (selectedObject != null)
                {
                    Animator animator = selectedObject.GetComponent<Animator>();
                    if (animator != null)
                    {
                        // Sample the animation at the specified time only if Animator and AnimationClip are available
                        float time = property.floatValue * clip.length;
                        AnimationMode.StartAnimationMode();
                        AnimationMode.SampleAnimationClip(selectedObject, clip, time);
                    }
                    else
                    {
                        Debug.LogWarning("No Animator found on selected object.");
                    }
                }
                else
                {
                    Debug.LogWarning("No object selected.");
                }
            }
        }

        // This method is called on every update by EditorApplication.update
        private static void OnEditorUpdate()
        {
            // If the selected object is different from the last selected, reset pose on the last one
            if (lastSelectedObject != Selection.activeGameObject)
            {
                if (lastSelectedObject != null)
                {
                    Animator animator = lastSelectedObject.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.Rebind();
                        animator.Update(0f); // Force immediate reset to T-pose or default pose
                    }
                }

                // Update last selected object
                lastSelectedObject = Selection.activeGameObject;
            }
        }

        static SerializedProperty FindSiblingProperty(SerializedProperty property, string siblingPropertyName)
        {
            if (property == null)
                throw new System.ArgumentNullException(nameof(property));

            if (string.IsNullOrEmpty(siblingPropertyName))
                throw new System.ArgumentException("Sibling property name cannot be null or empty.", nameof(siblingPropertyName));

            // Get the property path of the current property
            string propertyPath = property.propertyPath;

            // Find the last separator in the property path
            int lastSeparator = propertyPath.LastIndexOf('.');

            // Determine the parent path
            string parentPath = lastSeparator >= 0 ? propertyPath.Substring(0, lastSeparator) : "";

            // Construct the sibling's property path
            string siblingPropertyPath = string.IsNullOrEmpty(parentPath) ? siblingPropertyName : $"{parentPath}.{siblingPropertyName}";

            // Find and return the sibling property
            return property.serializedObject.FindProperty(siblingPropertyPath);
        }
    }

#endif
    #endregion

    public string clipName { get; }

    public PreviewAnimationAttribute(string clipName)
    {
        this.clipName = clipName;
    }
}
