using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEditor;
using Toolbox.Editor;

namespace Neeto
{
    [CustomPropertyDrawer(typeof(GamePropBase), true)]
    public class GamePropDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (NGUI.Property(position, property, label))
            {
                position = position.With(h: NGUI.LineHeight);
                var buttonPosition = position.With(xMin: position.xMax - NGUI.ButtonWidth);
                var dropdownRect = position.With(xMax: buttonPosition.xMin);
                var info = GetProperty(property);

                if (HandleDropdownGUI(position.With(h: NGUI.LineHeight), property, label))
                {
                    position.y += NGUI.FullLineHeight;
                    HandleTargetGUI(position, property);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var targetProperty = property.FindPropertyRelative(nameof(GamePropBase.referenceTarget));
            var height = NGUI.FullLineHeight;

            if (property.isExpanded)
            {
                return height + EditorGUI.GetPropertyHeight(targetProperty);
            }

            return height;
        }

        public static PropertyInfo GetProperty(SerializedProperty property)
        {
            var sig = property.FindPropertyRelative(nameof(GameAction.signature));
            var result = NGUI.ToProperty(sig.stringValue, out var Property);

            if (!result)
            {
                if (sig != null && !sig.stringValue.IsEmpty())
                {
                    NGUI.ToProperty(sig.stringValue);
                    Debug.LogError($"Invalid Property. Does this code still exist? {sig.stringValue}", property.serializedObject.targetObject);
                }
            }

            return Property;
        }
        public bool HandleDropdownGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var info = GetProperty(property);
            var content = new GUIContent(GameActionHelper.GetLabelName(info));

            position = position.With(h: NGUI.LineHeight);
            var labelRect = position.With(xMax: position.xMin + EditorGUIUtility.fieldWidth);
            var dropdownRect = position.With(xMax: labelRect.xMin);

            var isExpandable = info != null && !info.GetMethod.IsStatic;// && Arguments(property).arraySize > 0;

            if (isExpandable)
                property.isExpanded = EditorGUI.Foldout(labelRect.With(h: NGUI.LineHeight), property.isExpanded, label, true);
            else
                EditorGUI.PrefixLabel(labelRect, label);

            if (EditorGUI.DropdownButton(dropdownRect.With(h: NGUI.LineHeight), content, FocusType.Passive))
            {
                GameActionHelper.PropertyDropdown(fieldInfo, GetProperty(property), _ => Switch(_, property, fieldInfo));
            }
            return isExpandable && property.isExpanded;
        }
        public void HandleTargetGUI(Rect position, SerializedProperty property)
        {
            EditorGUI.indentLevel++;
            //var targetProperty = property.FindPropertyRelative(nameof(GamePropBase.referenceTarget));
            var info = GetProperty(property);

            if (typeof(Object).IsAssignableFrom(info.DeclaringType))
            {
                var targetProperty = property.FindPropertyRelative(nameof(GamePropBase.objectTarget));
                targetProperty.objectReferenceValue = EditorGUI.ObjectField(position.With(h: NGUI.LineHeight), "target", targetProperty.objectReferenceValue, info.DeclaringType, true);
            }
            else
            {
                var targetProperty = property.FindPropertyRelative(nameof(GamePropBase.referenceTarget));
                PolymorphicDrawer.DrawGUI(position, targetProperty, new GUIContent("target"), info.DeclaringType);
            }
            EditorGUI.indentLevel--;
        }

        public static SerializedProperty Signature(SerializedProperty property)
        {
            return property.FindPropertyRelative(nameof(GameCallback.signature));
        }


        private static void Switch(PropertyInfo info, SerializedProperty property, FieldInfo field)
        {
            var sig = Signature(property);
            var targetProperty = property.FindPropertyRelative(nameof(GamePropBase.referenceTarget));
            var objectProperty = property.FindPropertyRelative(nameof(GamePropBase.objectTarget));
            var isReferenceProperty = property.FindPropertyRelative(nameof(GamePropBase.isReferenceTarget));
            targetProperty.managedReferenceValue = null;
            objectProperty.objectReferenceValue = null;
            isReferenceProperty.boolValue = false;

            var infoProp = property.FindPropertyRelative(nameof(GamePropBase.propertyInfo));
            //Debug.Log(infoProp.managedReferenceValue);
            infoProp.managedReferenceValue = info;

            // TODO try-catch?
            if (info != null && info.GetMethod != null)
            {
                sig.stringValue = NGUI.ToSignature(info);// GetSignature(_);

                if (!info.GetMethod.IsStatic)
                {
                    if (isReferenceProperty.boolValue = !typeof(Object).IsAssignableFrom(info.DeclaringType) && info.DeclaringType.IsClass && !info.DeclaringType.IsAbstract)
                    {
                        targetProperty.managedReferenceValue = Activator.CreateInstance(info.DeclaringType);
                        objectProperty.objectReferenceValue = null;
                        isReferenceProperty.boolValue = true;
                    }
                }
            }
            else
            {
                sig.stringValue = "";
                targetProperty.managedReferenceValue = null;
            }

            //property.serializedObject.Update();
            property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }
    }
}