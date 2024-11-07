using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Toolbox.Editor;
using UnityEditor;
using UnityEngine;
using UnityDropdown;
using System.Linq;
using Rhinox.Lightspeed.Reflection;
using UnityDropdown.Editor;
using NUnit.Framework.Constraints;

namespace Neeto.Exp
{
    public class ArgumentTester : EditorWindow
    {
        public MethodWithArguments method;
        public MethodWithArguments method2;

        Editor editor;
        [QuickAction]
        static void Test()
        {
            GetWindow<ArgumentTester>();
        }

        void OnGUI()
        {
            Editor.CreateCachedEditor(this, null, ref editor);
            editor.OnInspectorGUI();
        }

        public static void TestOne(Vector3 vector, MonoBehaviour mb) { }
        public static void TestTwo(MethodWithArguments mwa, int i) { }
    }
    [Serializable]
    public class MethodWithArguments
    {
        public string methodName;
        public string declaringType;
        public Argument[] arguments;

        public MethodInfo GetMethod() => Type.GetType(declaringType).GetMethod(methodName);
    }
    [CustomPropertyDrawer(typeof(MethodWithArguments))]
    public class MethodDrawer : PropertyDrawer
    {
        static MethodInfo[] methods;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (NGUI.Property(position, GUIContent.none, property))
            {
                // todo 
                // better method filter
                methods ??= //AppDomain.CurrentDomain.GetAssemblies()
                    //.SelectMany(asm => asm.GetTypesSafe())
                    typeof(ArgumentTester)
                    //.SelectMany(type => type
                        .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.InvokeMethod)
                    .ToArray();

                var value = property.GetProperValue(fieldInfo) as MethodWithArguments;
                position = position.With(h: NGUI.LineHeight);

                if (EditorGUI.DropdownButton(EditorGUI.PrefixLabel(position, new GUIContent(label)), new GUIContent(value.methodName), FocusType.Passive))
                {
                    new DropdownMenu<MethodInfo>(methods.Select(m => new DropdownItem<MethodInfo>(m, ReflectionHelper.FullDropdownContent(m))).ToList(),
                        selectedMethod =>
                        {
                            value.methodName = selectedMethod.Name;
                            value.declaringType = selectedMethod.DeclaringType.AssemblyQualifiedName;
                            value.arguments = selectedMethod.GetParameters()
                                .Select(parameter =>
                                {
                                    var argument = new Argument
                                    {
                                        ArgumentType = Argument.GetArgumentType(parameter.ParameterType),
                                        ObjectType = parameter.ParameterType.AssemblyQualifiedName
                                    };

                                    switch (argument.ArgumentType)
                                    {
                                        case ArgumentType.UnityObject:
                                            argument.UnityObject = null; // Initially null, user can assign later
                                            break;
                                        //case ArgumentType.ReferenceObject:
                                            //argument.ReferenceObject = Activator.CreateInstance(parameter.ParameterType);
                                            //break;
                                        case ArgumentType.ValueObject:
                                            argument.ValueObject = new Value(Activator.CreateInstance(parameter.ParameterType)); // Handle primitive types
                                            break;
                                    }

                                    return argument;

                                }).ToArray();

                        }).ShowAsContext();
                }

                EditorGUI.indentLevel++;

                var argsProp = property.FindPropertyRelative(nameof(MethodWithArguments.arguments));

                foreach (SerializedProperty p in argsProp)
                {
                    position = NGUI.NextLine(position).With(h: EditorGUI.GetPropertyHeight(p));
                    EditorGUI.PropertyField(position, p);
                }

                EditorGUI.indentLevel--;
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = NGUI.FullLineHeight;
            var array = property.FindPropertyRelative(nameof(MethodWithArguments.arguments));
            foreach (SerializedProperty p in array)
            {
                height += EditorGUI.GetPropertyHeight(p) + EditorGUIUtility.standardVerticalSpacing;
            }
            return height;
        }
    }
    [CustomPropertyDrawer(typeof(Argument))]
    public class ArgumentDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (NGUI.Property(position, GUIContent.none, property))
            {
                var value = property.GetProperValue(fieldInfo) as Argument;
                switch (value.ArgumentType)
                {
                    case ArgumentType.UnityObject:
                        EditorGUI.ObjectField(position, property.FindPropertyRelative(nameof(Argument.UnityObject)), value.Type);
                        break;
                    case ArgumentType.ReferenceObject:
                        EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(Argument.ReferenceObject)), GUIContent.none);
                        break;
                    case ArgumentType.ValueObject:
                        EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(Argument.ValueObject)), GUIContent.none);
                        break;
                    default:
                        EditorGUI.LabelField(position, "Unsupported Type");
                        break;
                };
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var argumentType = (ArgumentType)property.FindPropertyRelative(nameof(Argument.ArgumentType)).enumValueIndex;
            return argumentType switch
            {
                ArgumentType.UnityObject => EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(Argument.UnityObject))),
                ArgumentType.ReferenceObject => EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(Argument.ReferenceObject))),
                ArgumentType.ValueObject => EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(Argument.ValueObject))),
                _ => EditorGUIUtility.singleLineHeight
            };
        }
    }
    [Serializable]
    public class Argument
    {
        public UnityEngine.Object UnityObject;
        [SerializeReference] public object ReferenceObject;
        public Value ValueObject;

        public ArgumentType ArgumentType;
        public string ObjectType;
        public Type Type => Type.GetType(ObjectType);
        public object Value => ArgumentType switch
        {
            ArgumentType.UnityObject => UnityObject,
            ArgumentType.ReferenceObject => ReferenceObject,
            ArgumentType.ValueObject => ValueObject,
            _ => null
        };
        public static ArgumentType GetArgumentType(Type type)
        {
            if (type.IsSubclassOf(typeof(UnityEngine.Object)))
                return ArgumentType.UnityObject;
            else
                return ArgumentType.ValueObject;
            // TODO
        }
    }
    public enum ArgumentType
    {
        NotSupported,
        UnityObject,
        ReferenceObject,
        ValueObject
    }
}
