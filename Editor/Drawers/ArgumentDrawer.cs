﻿using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEditor;
using Toolbox.Editor;

namespace Neeto
{
    [CustomPropertyDrawer(typeof(Argument))]
    public class ArgumentDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            try
            {
                property = GetSelectedProperty(property);

                return EditorGUI.GetPropertyHeight(property) + EditorGUIUtility.standardVerticalSpacing;
            }
            catch
            {
                return NGUI.FullLineHeight;
            }
        }

        public static bool IsDynamic(SerializedProperty property, int i)
        {
            var dyns = property.FindPropertyRelative(nameof(GameAction.dynamics));
            foreach (SerializedProperty dy in dyns)
            {
                if (dy.intValue == i)
                {
                    return true;
                }
            }
            return false;
        }

        public static Argument.ArgType UpdateArgType(SerializedProperty argumentProperty, Type paramType)
        {
            var enumProperty = argumentProperty.FindPropertyRelative(nameof(Argument.argType));

            var value = Argument.EnumOf(paramType);
            enumProperty.enumValueIndex = (int)value;

            if (value == Argument.ArgType.Reference)
            {
                argumentProperty.FindPropertyRelative(nameof(Argument.referenceType))
                    .stringValue = $"{paramType.AssemblyQualifiedName}";
            }

            return value;
        }

        public static Rect PropertyGUI(Rect position, SerializedProperty selected, FieldInfo info, Type type, bool isDynamic = false, string label = null)
        {
            if (selected == null)
                return position;//.NextLine();

            //var content = label != null ? new GUIContent(label) : new GUIContent(type.GetType().Name);
            var argType = Argument.EnumOf(type);
            var content = new GUIContent(label);
            var rect = position;
            var height = EditorGUI.GetPropertyHeight(selected);

            //if (typeof(GameFuncBase).IsAssignableFrom(info.FieldType))

            if (isDynamic || argType != Argument.ArgType.Reference)
                position = EditorGUI.PrefixLabel(position.With(h: NGUI.LineHeight), content);

            if (isDynamic)
            {
                EditorGUI.LabelField(position, "(dynamic)");
                return position.NextLine();
            }


            if (argType == Argument.ArgType.Null || selected == null)// || !IsSupported(type))
            {
                EditorGUI.LabelField(position, $"not supported");
                return position.NextLine();
            }

            if (type.IsSubclassOf(typeof(Object)))
            {
                var obj = selected.objectReferenceValue;
                if (obj != null && !type.IsAssignableFrom(obj.GetType()))
                {
                    obj = null;
                }

                selected.objectReferenceValue = (Object)EditorGUI.ObjectField(position, obj, type, true);
            }
            else if (type.IsEnum)
            {
                var names = Enum.GetNames(type);
                selected.intValue = EditorGUI.Popup(position, selected.intValue, names);
            }
            else if (type.Equals(typeof(LayerMask)))
            {
                selected.intValue = EditorGUI.MaskField(position, selected.intValue, NGUI.GetLayerNames());
            }
            else if (argType == Argument.ArgType.Reference)
            {
                //MEdit.IndentBoxGUI(position.With(h: EditorGUI.GetPropertyHeight(selected)));
                var referenceTypeName = selected.Parent().FindPropertyRelative(nameof(Argument.referenceType)).stringValue;

                var rt = Type.GetType(referenceTypeName);
                PolymorphicDrawer.DrawGUI(position, selected, content, rt);
                //MEdit.EndShadow();
            }
            else if (type.IsValueType || type == typeof(string))
            {
                EditorGUI.PropertyField(position, selected, GUIContent.none);
            }

            //return position.NextLine().With(h: EditorGUI.GetPropertyHeight(selected));
            return rect.NextLine(height);
        }

        static object FieldGUI(Rect position, object value)
        {
            var type = value.GetType();
            switch (type.Name)
            {
                case nameof(Boolean):
                    return EditorGUI.Toggle(position, (bool)value);

                case nameof(String):
                    return EditorGUI.TextField(position, (string)value);

                case nameof(Single):
                    return EditorGUI.FloatField(position, (float)value);

                case nameof(Double):
                    return EditorGUI.DoubleField(position, (double)value);

                case nameof(Int32):
                    return EditorGUI.IntField(position, (int)value);

                case nameof(Vector2):
                    return EditorGUI.Vector2Field(position, "", (Vector2)value);

                case nameof(Vector3):
                    return EditorGUI.Vector3Field(position, "", (Vector3)value);

                case nameof(Vector4):
                    return EditorGUI.Vector4Field(position, "", (Vector4)value);

                case nameof(Color):
                    return EditorGUI.ColorField(position, (Color)value);

                default:
                    break;
            }

            if (type.IsSubclassOf(typeof(Object)))
                return EditorGUI.ObjectField(position, (Object)value, type, true);
            else if (type.IsEnum)
            {
                var options = Enum.GetNames(type);
                var values = Enum.GetValues(type);
                var index = (int)value;

                return EditorGUI.Popup(position, index, options);
            }
            else if (type.IsSerializable || type.IsInterface)
            {

            }

            return null;
        }
        public static SerializedProperty GetSelectedProperty(SerializedProperty property)
        {
            var enumProperty = property.FindPropertyRelative(nameof(Argument.argType));
            var enumValue = (Argument.ArgType)enumProperty.enumValueIndex;
            enumProperty.enumValueIndex = (int)enumValue;
            return property.FindPropertyRelative("arg" + Enum.GetName(typeof(Argument.ArgType), enumValue));
        }
    }
}