using UnityEngine;
using Neeto;
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

        float t;

        bool rootXZ;
        bool rootY = true;

        static PreviewInInspectorDrawer()
        {
            // Listen for changes in selection
            EditorApplication.update += OnEditorUpdate;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (NGUI.Property(position, property))
            {
                position.height = NGUI.LineHeight;

                property.isExpanded = NGUI.IsExpanded(property, position, label);

                EditorGUI.PropertyField(position, property, label);

                if (property.propertyType != SerializedPropertyType.ObjectReference)
                {
                    EditorGUI.LabelField(position, label.text, "Use with AnimationClip");
                    return;
                }



                // Get the attribute and find the AnimationClip by field name
                PreviewAnimationAttribute previewAttr = (PreviewAnimationAttribute)attribute;

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

                // Check if the slider value is being adjusted
                if (!property.isExpanded)
                {
                    return;
                }
                position.y += NGUI.FullLineHeight;

                position = EditorGUI.PrefixLabel(position, new("Preview"));

                EditorGUI.BeginChangeCheck();
                EditorGUI.indentLevel++;
                rootY = EditorGUI.ToggleLeft(position.With(xMin: position.xMax -= 50), new GUIContent("Y"), rootY);
                rootXZ = EditorGUI.ToggleLeft(position.With(xMin: position.xMax -= 50), new GUIContent("XZ"), rootXZ);

                AnimationClip clip = property.objectReferenceValue as AnimationClip;
                if (clip)
                    t = EditorGUI.Slider(position, t, 0f, 1f);
                else
                    EditorGUI.HelpBox(position, "Use with AnimationClip", MessageType.Error);
                position.xMax -= 20;




                EditorGUI.indentLevel--;
                if (EditorGUI.EndChangeCheck() && clip && !Application.isPlaying)
                {
                    // Only preview the animation while adjusting the slider
                    GameObject target = Selection.activeGameObject;
                    Animator animator = target?.GetComponent<Animator>();

                    if (animator)
                    {
                        //var root = animator.applyRootMotion;
                        // animator.applyRootMotion = false;

                        var hips = animator.GetBoneTransform(HumanBodyBones.Hips);


                        var hipsPosition = hips.localPosition;

                        //clip.

                        clip.SampleAnimation(target, t);

                        if (!rootXZ) // reset XZ
                            hips.localPosition = hipsPosition.With(y: hips.localPosition.y);
                        if (!rootY) // reset y
                            hips.localPosition = hips.localPosition.With(y: hipsPosition.y);


                        // Sample the animation at the specified time only if Animator and AnimationClip are available
                        //float time = t * clip.length;
                        //AnimationMode.StartAnimationMode();
                        //AnimationMode.SampleAnimationClip(target, clip, time);
                    }
                    else
                    {
                        Debug.LogWarning("No Animator found on selected object.");
                    }
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = NGUI.LineHeight;

            if (property.isExpanded)
            {
                height += NGUI.FullLineHeight;
            }

            return height;
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
    }

#endif
    #endregion
}
