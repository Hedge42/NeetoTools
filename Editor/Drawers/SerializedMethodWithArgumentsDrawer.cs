using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Toolbox.Editor;
using UnityDropdown.Editor;

namespace Neeto.Exp
{
    [CustomPropertyDrawer(typeof(SerializedMethodWithArguments))]
    public class SerializedMethodWithArgumentsDrawer : PropertyDrawer
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
                    typeof(SerializedMethodWithArguments.TestWindow)
                    //.SelectMany(type => type
                        .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.InvokeMethod)
                    .ToArray();

                var value = property.GetProperValue(fieldInfo) as SerializedMethodWithArguments;
                position = position.With(h: NGUI.LineHeight);

                if (EditorGUI.DropdownButton(EditorGUI.PrefixLabel(position, new GUIContent(label)), new GUIContent(value.methodName), FocusType.Passive))
                {
                    new DropdownMenu<MethodInfo>(methods.Select(m => new DropdownItem<MethodInfo>(m, NGUI.GetFullDropdownContent(m))).ToList(),
                        selectedMethod =>
                        {
                            value.methodName = selectedMethod.Name;
                            value.declaringType = selectedMethod.DeclaringType.AssemblyQualifiedName;
                            value.arguments = selectedMethod.GetParameters()
                                .Select(parameter =>
                                {
                                    var argument = new SerializedArgument
                                    {
                                        ArgumentType = SerializedArgument.GetArgumentType(parameter.ParameterType),
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

                var argsProp = property.FindPropertyRelative(nameof(SerializedMethodWithArguments.arguments));

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
            var array = property.FindPropertyRelative(nameof(SerializedMethodWithArguments.arguments));
            foreach (SerializedProperty p in array)
            {
                height += EditorGUI.GetPropertyHeight(p) + EditorGUIUtility.standardVerticalSpacing;
            }
            return height;
        }
    }
}
