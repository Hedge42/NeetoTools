using UnityEngine;
using System;
using System.Collections;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Neeto
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(NavMeshAreaMask))]
    public class NavMeshAreaMaskDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var names = GameObjectUtility.GetNavMeshAreaNames();
            property = property.FindPropertyRelative("value");
            property.intValue = EditorGUI.MaskField(position, label, property.intValue, names);
            EditorGUI.EndProperty();
        }
    }
#endif
    [Serializable]
	public struct NavMeshAreaMask
    {
        public static implicit operator int(NavMeshAreaMask mask) => mask.value;
        public static implicit operator NavMeshAreaMask(int value) => new NavMeshAreaMask { value = value };

        public int value;
    }
}