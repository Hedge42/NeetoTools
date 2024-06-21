/*
 * something I've wanted to make for a while but still not as clean as I'd like
 */


namespace Matchwork
{
    #region EDITOR
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;

    //[CustomEditor(typeof(Transform))]
    public class ExtendedTransformEditor : Editor
    {
        ExtendedTransform worldTransform;

        Editor editor;

        private void OnEnable()
        {
            //worldTransform = new ExtendedTransform(target as Transform);
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("World Transform", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            worldTransform.ReadWorldValues(target as Transform);

            worldTransform.position = EditorGUILayout.Vector3Field("World Position", worldTransform.position);
            worldTransform.rotation = EditorGUILayout.Vector3Field("World Rotation", worldTransform.rotation);
            worldTransform.scale = EditorGUILayout.Vector3Field("World Rotation", worldTransform.scale);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target as Transform, "Edit Transform");
                worldTransform.ApplyWorldValues(target as Transform);
            }
        }
    }


#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ExtendedTransform))]
    public class ExtendedTransformDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
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
    }
#endif


    [System.Serializable]
    public struct ExtendedTransform
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public void ReadWorldValues(Transform t)
        {
            position = t.position;
            rotation = t.eulerAngles;
            scale = t.lossyScale;
        }
        public void ApplyWorldValues(Transform t)
        {
            t.position = position;// new Property<Vector3>(() => t.position, value => t.position = value);
            t.eulerAngles = rotation;// new Property<Vector3>(() => t.eulerAngles, value => t.eulerAngles = value);
            t.localScale = new Vector3(scale.x / t.lossyScale.x, scale.y / t.lossyScale.y, scale.z / t.lossyScale.z);
        }
    }
}


#endif
#endregion
