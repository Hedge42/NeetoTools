using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Matchwork
{
#region EDITOR
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(PhysicsButtonAttribute))]
	public class PhysicsButtonAttributeDrawer : PropertyDrawer
	{
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var Attribute = attribute as PhysicsButtonAttribute;
            var target = property.serializedObject.targetObject;

            EditorGUI.BeginChangeCheck();

            position.xMax -= 24;

            EditorGUI.PropertyField(position, property, label);

            var buttonRect = new Rect(position);
            buttonRect.width = 22f;
            buttonRect.x = position.xMax + 2;

            if (GUI.Button(buttonRect, "p"))
            {
                MEdit.OpenPhysicsSettings();
            }

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
#endregion

	public class PhysicsButtonAttribute : PropertyAttribute
	{
        /// <summary> Describe what this does </summary>
		public PhysicsButtonAttribute()
		{

		}
	}
}