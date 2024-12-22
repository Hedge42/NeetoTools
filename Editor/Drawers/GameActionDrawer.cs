using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using Rhinox.Lightspeed;
#if UNITY_EDITOR
using UnityEditor;
using Toolbox.Editor;
using Rhinox.Lightspeed.Editor;
using Rhinox.Lightspeed.Reflection;
using UnityDropdown.Editor;
#endif

namespace Neeto
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(GameMethod), true)]
    public class GameActionDrawer : PropertyDrawer
    {
        //GameMethod target;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (NGUI.Property(position, label, property))
            {

                if (HandleDropdownGUI(position.With(h: NGUI.LineHeight), property, label))
                {
                    HandleArgumentsGUI(position.Offset(y: NGUI.FullLineHeight), property);
                }
            }

            var target = property.GetProperValue(fieldInfo) as GameMethod;
            if (target == null || !target.IsValid())
            {
                EditorGUI.DrawRect(position, Color.red.With(a: .2f));
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var argsProp = property.FindPropertyRelative(nameof(GameMethod.arguments));
            var height = NGUI.FullLineHeight;

            for (int i = 0; property.isExpanded && i < argsProp.arraySize; i++)
            {
                var prop = ArgumentDrawer.GetSelectedProperty(argsProp.GetArrayElementAtIndex(i));

                if (prop != null)
                    height += EditorGUI.GetPropertyHeight(prop);
            }

            return height;
        }

        public static MethodInfo GetMethod(SerializedProperty property)
        {
            var sig = property.FindPropertyRelative(nameof(GameAction.signature));
            var result = NGUI.ToMethod(sig.stringValue, out var method);

            if (!result)
            {
                if (sig != null && !sig.stringValue.IsEmpty())
                {
                    NGUI.ToMethod(sig.stringValue);
                    Debug.LogError($"Invalid method. Does this code still exist? {sig.stringValue}", property.serializedObject.targetObject);
                }
            }

            return method;
        }
        public bool HandleDropdownGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var target = property.GetProperValue(fieldInfo) as GameMethod;

            var method = GetMethod(property);
            var content = new GUIContent(target.signature);

            var ddRect = EditorGUI.PrefixLabel(position.With(h: NGUI.LineHeight), label);
            var lbRect = ddRect.With(xMin: position.xMin, xMax: ddRect.xMin);

            var isExpandable = method != null && Arguments(property).arraySize > 0;

            if (target.IsValid() && target.arguments?.Length > 0)
            {
                NGUI.IsExpanded(property, lbRect);
            }

            if (EditorGUI.DropdownButton(ddRect.With(h: NGUI.LineHeight), content, FocusType.Passive))
            {
                MethodDropdown(property, fieldInfo, GetMethod(property), _ => Switch(_, property, fieldInfo));
            }
            return isExpandable && property.isExpanded;
        }
        public static void MethodDropdown(SerializedProperty property, FieldInfo fieldInfo, MethodInfo selected, Action<MethodInfo> onSelect)
        {
            var methods = (IEnumerable<MethodInfo>)default;

            if (fieldInfo.TryGetAttribute<ReflectionFilterAttribute>(out var filter))
            {
                var target = property.GetDeclaringObject();
                var filterMethod = target.GetType().GetMethod(filter.source);
                target = filterMethod.IsStatic ? null : target;
                methods = (IEnumerable<MethodInfo>)target.GetType().GetValue(target);
            }
            else
            {
                methods = NGUI.GetRuntimeTypes().GetMethods(GameAction.FLAGS_M);
            }

            var gs = fieldInfo.FieldType.GetGenericArguments();

            if (typeof(GameAction).Equals(fieldInfo.FieldType))
            {
                methods = methods.Where(m => m.ReturnType.Equals(typeof(void)));
            }
            else if (typeof(GameFuncBase).IsAssignableFrom(fieldInfo.FieldType))
            {
                methods = methods.Where(m => fieldInfo.GetReturnType().IsAssignableFrom(m.ReturnType));
                gs = gs.RemoveAt(0);
            }
            if (gs.Count() > 0)
            {
                methods = methods.Where(m => m.ContainsTargetableParameter(gs.ToArray()));
            }
            if (methods.Count() == 0)
            {
                Debug.LogError("No methods found!");
                return;
            }
            else
            {

                var items = methods.Select(m => new DropdownItem<MethodInfo>(m, GetDisplayString(m))).ToList();
                var menu = new DropdownMenu<MethodInfo>(items, onSelect, sortItems: true);

                foreach (var _ in items)
                {
                    Debug.Log(_.Path);
                }

                menu.ShowAsContext();
            }

        }
        public static string GetDisplayString(MethodInfo info)
        {
            if (info == null)
                return "(none)";

            var text = new StringBuilder();

            // Append type name
            var tt = info.DeclaringType;
            text.Append($"{info.GetModuleName()}/{NGUI.GetDeclaringString(info.DeclaringType)}.");
            text.Append(info.Name).Append(' ');
            if (info.IsStatic)
                text.Append('*');

            // Append parameter types
            var paramTypes = info.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}");
            text.Append('(').Append(string.Join(",", paramTypes)).Append(')');

            if (info.ReturnType != null)
            {
                text.Append($" => {info.ReturnType.Name}");
            }

            return text.ToString();
        }
        public void HandleArgumentsGUI(Rect position, SerializedProperty property)
        {
            var size = Arguments(property).arraySize;
            var argumentTypes = GameAction.GetMethodArgumentTypesAndNames(GetMethod(property));

            if (argumentTypes.Count != size)
            {
                Debug.Log($"wtf ({Signature(property).stringValue}:{argumentTypes.JoinString()})");
            }

            EditorGUI.indentLevel++;
            for (int i = 0; i < size /*&& i < tan.Count*/; i++)
            {
                var label = $"{argumentTypes[i].name}({argumentTypes[i].type.Name})";
                var arg = Arguments(property, i);
                var prop = ArgumentDrawer.GetSelectedProperty(arg);
                var isDynamic = ArgumentDrawer.IsDynamic(property, i);

                position = ArgumentDrawer.PropertyGUI(position, prop, fieldInfo, argumentTypes[i].type, isDynamic, label);
                //position.y += height;
            }
            EditorGUI.indentLevel--;
        }

        public static SerializedProperty Arguments(SerializedProperty property)
        {
            return property.FindPropertyRelative(nameof(GameMethod.arguments));
        }
        public static SerializedProperty Arguments(SerializedProperty property, int i)
        {
            return property.FindPropertyRelative(nameof(GameMethod.arguments))
                .GetArrayElementAtIndex(i);
        }
        public static SerializedProperty Signature(SerializedProperty property)
        {
            return property.FindPropertyRelative(nameof(GameMethod.signature));
        }

        private static void Switch(MethodInfo _method, SerializedProperty property, FieldInfo info)
        {
            var sig = Signature(property);
            var args = Arguments(property);

            // TODO try-catch?
            if (_method != null)
            {
                sig.stringValue = NGUI.ToSignature(_method);// GetSignature(_);


                var methodTypes = GameAction.GetMethodArgumentTypes(_method);
                var dynamicTypes = GetFieldTypeArguments(info).ToList();
                UpdateDynamics(property, methodTypes, dynamicTypes);

                args.arraySize = methodTypes.Count;
                for (int a = 0; a < methodTypes.Count; a++)
                {
                    var arg = args.GetArrayElementAtIndex(a);
                    var argType = ArgumentDrawer.UpdateArgType(arg, methodTypes[a]);

                    if (argType == Argument.ArgType.Reference)
                    {
                        var referenceProperty = arg.FindPropertyRelative(nameof(Argument.argReference));
                        var typeProperty = arg.FindPropertyRelative(nameof(Argument.referenceType));

                        var assignableTypes = NGUI.GetAssignableReferenceTypes(methodTypes[a]);
                        if (assignableTypes.Count() == 1)
                            referenceProperty.managedReferenceValue = Activator.CreateInstance(assignableTypes.ElementAt(0));
                        else
                            referenceProperty.managedReferenceValue = null;

                        typeProperty.stringValue = methodTypes[a].FullName;
                    }
                }
            }
            else
            {
                sig.stringValue = "";
                args.arraySize = 0;
            }

            //property.serializedObject.Update();
            property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }

        private static void UpdateDynamics(SerializedProperty property, List<Type> methodTypes, IEnumerable<Type> argumentTypes)
        {
            methodTypes = new List<Type>(methodTypes);
            var dynamicsProp = property.FindPropertyRelative(nameof(GameMethod.dynamics));
            dynamicsProp.arraySize = argumentTypes.Count();
            int i = 0;
            foreach (SerializedProperty p in dynamicsProp)
            {
                var index = methodTypes.IndexOf(methodTypes.First(t => argumentTypes.ElementAt(i).Equals(t)));
                methodTypes.RemoveAt(index);
                p.intValue = index + i;
                i++;
            }
        }

        public static IEnumerable<Type> GetFieldTypeArguments(FieldInfo fieldInfo)
        {
            /*
             return an array representing the potential argument types
             for GameAction<bool,float>, return [bool,float]
             */
            var argumentTypes = fieldInfo.FieldType.GetGenericArguments().ToList();

            /*
             the return type of a func is not a potential argument
             */
            if (typeof(GameFuncBase).IsAssignableFrom(fieldInfo.FieldType))
                argumentTypes.RemoveAt(0);

            return argumentTypes;
        }

    }
#endif
}