using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Neeto
{
	[CustomPropertyDrawer(typeof(Reference<>))]
	public class ScriptableAssetTDrawer : PropertyDrawer
	{
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            property = property.FindPropertyRelative(nameof(Reference<object>.value));

            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.ObjectField(position.WithRightButton(out var buttonPosition), label, property.objectReferenceValue, typeof(ScriptableReference), false);
            EditorGUI.EndDisabledGroup();

            if (EditorGUI.DropdownButton(buttonPosition, GUIContent.none, FocusType.Passive))
            {
                //Event.current.Use();

                var type = fieldInfo.FieldType.GetGenericArguments()[0];
                // .ScriptableAsset(type, property);
            }

            EditorGUI.EndProperty();
        }
    }
}