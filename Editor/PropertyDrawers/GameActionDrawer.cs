using System;
using System.Linq;
using UnityEngine;
using System.Reflection;
using UnityDropdown.Editor;
using System.Collections.Generic;
using Matchwork;
using System.Text;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(GameMethod), true)]
public class GameActionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        MEdit.IndentBoxGUI(position);

        var dropdownRect = InvokeButtonGUI(property, position.With(height: MEdit.lineHeight));

        if (HandleDropdownGUI(dropdownRect, property, label))
        {
            position.y += MEdit.fullLineHeight;
            HandleArgumentsGUI(position.With(height: MEdit.lineHeight), property);
        }

        MEdit.EndShadow();

        EditorGUI.EndProperty();
    }

    object target;
    private Rect InvokeButtonGUI(SerializedProperty property, Rect position)
    {
        target ??= MReflect.FindReflectionTarget(property, fieldInfo);

        if (target is GameAction action)
        {
            position = position.WithRightButton(out var buttonPosition);
            if (GUI.Button(buttonPosition, "?"))
            {
                var method = GetMethod(property);
                Debug.Log($"{property.propertyPath}.{target.TypeNameOrNull()}.{method.NameOrNull()}", property.serializedObject.targetObject);
                if (method != null)
                {
                    action.Invoke();
                }
            }
        }

        return position;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var args = property.FindPropertyRelative(nameof(GameMethod.arguments));
        var count = args.arraySize;
        var height = MEdit.fullLineHeight;

        if (property.isExpanded && count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                var prop = ArgumentDrawer.GetSelectedProperty(args.GetArrayElementAtIndex(i));

                if (prop != null)
                    height += EditorGUI.GetPropertyHeight(prop);
            }
        }

        return height;
    }

    public static MethodInfo GetMethod(SerializedProperty property)
    {
        var sig = property.FindPropertyRelative(nameof(GameAction.signature));
        var result = MReflect.ToMethod(sig.stringValue, out var method);

        if (!result)
        {
            if (sig != null && !sig.stringValue.IsEmpty())
            {
                MReflect.ToMethod(sig.stringValue);
                Debug.LogError($"Invalid method. Does this code still exist? {sig.stringValue}", property.serializedObject.targetObject);
            }
        }

        return method;
    }
    public bool HandleDropdownGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var method = GetMethod(property);
        var content = new GUIContent(GetLabelName(method));

        position.ToLabelAndField(out var lbRect, out var ddRect);

        var isExpandable = method != null && Arguments(property).arraySize > 0;

        if (isExpandable)
            property.isExpanded = EditorGUI.Foldout(lbRect.With(height: MEdit.lineHeight), property.isExpanded, label, true);
        else
            EditorGUI.PrefixLabel(lbRect, label);

        if (EditorGUI.DropdownButton(ddRect.With(height: MEdit.lineHeight), content, FocusType.Passive))
        {
            MDropdown.MethodDropdown(fieldInfo, GetMethod(property), _ => Switch(_, property, fieldInfo));
        }
        return isExpandable && property.isExpanded;
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
            sig.stringValue = MReflect.ToSignature(_method);// GetSignature(_);


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

                    var assignableTypes = MReflect.GetAssignableReferenceTypes(methodTypes[a]);
                    if (assignableTypes.Count() == 1)
                        referenceProperty.managedReferenceValue = Activator.CreateInstance(assignableTypes.ElementAt(0));
                    else
                        referenceProperty.managedReferenceValue = null;

                    typeProperty.stringValue = methodTypes[a].FullName;
                }

                //foreach (var paramType in types)
                //{
                //    if (sequence[a].Equals(paramType))
                //    {
                //        types.Remove(paramType);
                //        arg.FindPropertyRelative(nameof(Argument.argType))
                //            .enumValueIndex = (int)Argument.ArgType.Dynamic;
                //        break;
                //    }

                //}
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

    public static string GetDisplayPath(MethodInfo methodInfo)
    {
        StringBuilder option = new StringBuilder();

        // Append type name
        var tt = methodInfo.DeclaringType;
        option.Append($"{methodInfo.ModuleName()}/{MReflect.GetDeclaringString(methodInfo.DeclaringType)}.");
        option.Append(methodInfo.Name).Append(' ');
        if (methodInfo.IsStatic)
            option.Append('*');

        // Append parameter types
        var paramTypes = methodInfo.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}");
        option.Append('(').Append(string.Join(",", paramTypes)).Append(')');

        if (methodInfo.ReturnType != null)
        {
            option.Append($" => {methodInfo.ReturnType.Name}");
        }

        return option.ToString();
    }
    public static string GetSearchName(MethodInfo info)
    {
        return $"{info.DeclaringType.Name}.{info.Name}";
    }
    public static string GetLabelName(MethodInfo methodInfo)
    {
        if (methodInfo == null)
        {
            return "...";
        }

        StringBuilder option = new StringBuilder();
        var tt = methodInfo.DeclaringType;
        option.Append(methodInfo.ReflectedType.Name).Append('.');
        option.Append(methodInfo.Name).Append(' ');
        if (methodInfo.IsStatic)
            option.Append('*');

        // Append parameter types
        var paramTypes = methodInfo.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}");
        option.Append('(').Append(string.Join(",", paramTypes)).Append(')');

        if (methodInfo.ReturnType != null)
        {
            option.Append($" => {methodInfo.ReturnType.Name}");
        }

        return option.ToString();
    }
}


[CustomPropertyDrawer(typeof(GamePropBase), true)]
public class GamePropDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        MEdit.IndentBoxGUI(position);

        var dropdownRect = position.With(height: MEdit.lineHeight).WithRightButton(out var buttonPosition);
        var info = GetProperty(property);

        

        EditorGUI.BeginDisabledGroup(info == null);
        if (GUI.Button(buttonPosition, "?"))
        {

            var target = MReflect.FindReflectionTarget(property, fieldInfo) as GamePropBase;
            Debug.Log((target.propertyInfo as PropertyInfo).GetValue(target.target));
        }
        EditorGUI.EndDisabledGroup();

        if (HandleDropdownGUI(dropdownRect, property, label))
        {
            position.y += MEdit.fullLineHeight;

            HandleTargetGUI(position, property);
            //HandleArgumentsGUI(position.With(h: MEdit.lineHeight), property);
        }


        MEdit.EndShadow();

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var targetProperty = property.FindPropertyRelative(nameof(GamePropBase.referenceTarget));
        var height = MEdit.fullLineHeight;

        if (property.isExpanded)
        {
            return height + EditorGUI.GetPropertyHeight(targetProperty);
        }

        return height;
    }

    public static PropertyInfo GetProperty(SerializedProperty property)
    {
        var sig = property.FindPropertyRelative(nameof(GameAction.signature));
        var result = MReflect.ToProperty(sig.stringValue, out var Property);

        if (!result)
        {
            if (sig != null && !sig.stringValue.IsEmpty())
            {
                MReflect.ToProperty(sig.stringValue);
                Debug.LogError($"Invalid Property. Does this code still exist? {sig.stringValue}", property.serializedObject.targetObject);
            }
        }

        return Property;
    }
    public bool HandleDropdownGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var info = GetProperty(property);
        var content = new GUIContent(MDropdown.GetDisplayPath(info));

        position.ToLabelAndField(out var lbRect, out var ddRect);

        var isExpandable = info != null && !info.GetMethod.IsStatic;// && Arguments(property).arraySize > 0;

        if (isExpandable)
            property.isExpanded = EditorGUI.Foldout(lbRect.With(height: MEdit.lineHeight), property.isExpanded, label, true);
        else
            EditorGUI.PrefixLabel(lbRect, label);

        if (EditorGUI.DropdownButton(ddRect.With(height: MEdit.lineHeight), content, FocusType.Passive))
        {
            MDropdown.PropertyDropdown(fieldInfo, GetProperty(property), _ => Switch(_, property, fieldInfo));
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
            targetProperty.objectReferenceValue = EditorGUI.ObjectField(position.With(height: MEdit.lineHeight), "target", targetProperty.objectReferenceValue, info.DeclaringType, true);
        }
        else
        {
            var targetProperty = property.FindPropertyRelative(nameof(GamePropBase.referenceTarget));
            PolymorphicDrawer.DrawGUI(position, targetProperty, new GUIContent("target"), info.DeclaringType, true);
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
        if (info != null)
        {
            sig.stringValue = MReflect.ToSignature(info);// GetSignature(_);

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

#endif