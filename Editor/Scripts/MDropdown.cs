using UnityEngine;
using System;
using System.Linq;

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using UnityEditor;
using UnityDropdown.Editor;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Reflection;
using System.Text;
using NUnit.Framework.Internal.Filters;

namespace Matchwork
{
    public static class MDropdown
    {
        public static void ScriptableAsset<T>(SerializedProperty property)
        {
            var assets = MEdit.LoadAssets<ScriptableAsset>().Where(asset => asset.data is T);
            var items = assets.Select(asset => new DropdownItem<ScriptableAsset>(asset, asset.name, isSelected: ReferenceEquals(asset, property.objectReferenceValue)));

            var menu = new DropdownMenu<ScriptableAsset>(items.ToList(), _ =>
            {
                Undo.RecordObject(property.serializedObject.targetObject, "Change asset");
                property.objectReferenceValue = _;
                property.ApplyAndMarkDirty();
            }, sortItems: true, showNoneElement: true);

            menu.ShowAsContext();
        }
        public static void ScriptableAsset(Type type, SerializedProperty property)
        {
            var assets = MEdit.LoadAssets<ScriptableAsset>().Where(asset => type.IsAssignableFrom(asset.data?.GetType()));
            var items = assets.Select(asset => new DropdownItem<ScriptableAsset>(asset, asset.name, isSelected: ReferenceEquals(asset, property.objectReferenceValue)));

            var menu = new DropdownMenu<ScriptableAsset>(items.ToList(), _ =>
            {
                Undo.RecordObject(property.serializedObject.targetObject, "Change asset");
                property.objectReferenceValue = _;
                property.ApplyAndMarkDirty();
            }, sortItems: true, showNoneElement: true);

            menu.ShowAsContext();
        }


        public static DropdownMenu<MethodInfo> MethodDropdown(FieldInfo field, MethodInfo selected, Action<MethodInfo> onSelect)
        {
            var flags = GameAction.FLAGS_M;

            if (field.FieldType.TryGetAttribute<BindingFlagsAttribute>(out var f))
                flags &= f.flags;

            if (field.TryGetAttribute<BindingFlagsAttribute>(out var f_attr))
                flags |= f_attr.flags;


            var mods = Module.ALL;
            if (field.TryGetAttribute<ModuleAttribute>(out var m_attr) && m_attr != null)
                mods = (Module)m_attr.module;

            var methods = MReflect.GetMethods(mods, flags);

            if (methods.Count() == 0)
            {
                Debug.LogError("No methods found!");
                return new DropdownMenu<MethodInfo>(new List<DropdownItem<MethodInfo>>(), null);
            }

            var gs = field.FieldType.GetGenericArguments().ToList();

            // GameFunc requires first generic argument to be the return type
            if (typeof(GameFuncBase).IsAssignableFrom(field.FieldType))
            {
                var returnType = gs[0];
                methods = methods.Where(m => returnType.IsAssignableFrom(m.ReturnType));
                gs.RemoveAt(0);
            }
            if (gs.Count > 0)
            {
                methods = methods.Where(m => m.ContainsTargetableParameter(gs.ToArray()));
            }

            //.Where(m => m.GetParameterTypesWithTarget());
            var items = methods.Select(method =>
                    new DropdownItem<MethodInfo>(method,
                                                path: GetDisplayPath(method),
                                                isSelected: method.Equals(selected)
                )).ToList();

            items.Prepend(new DropdownItem<MethodInfo>(null, "Clear"));

            var menu = new DropdownMenu<MethodInfo>(items, onSelect, sortItems: true, showNoneElement: true);
            menu.ShowAsContext();
            return menu;
        }
        public static DropdownMenu<PropertyInfo> PropertyDropdown(FieldInfo field, PropertyInfo selected, Action<PropertyInfo> onSelect)
        {
            var flags = GameAction.FLAGS_P;

            if (field.FieldType.TryGetAttribute<BindingFlagsAttribute>(out var f))
                flags &= f.flags;

            if (field.TryGetAttribute<BindingFlagsAttribute>(out var f_attr))
                flags |= f_attr.flags;


            var mods = Module.ALL;
            if (field.TryGetAttribute<ModuleAttribute>(out var m_attr) && m_attr != null)
                mods = (Module)m_attr.module;

            var properties = MReflect.GetProperties(mods, flags);

            if (properties.Count() == 0)
            {
                Debug.LogError("No methods found!");
                return new DropdownMenu<PropertyInfo>(new List<DropdownItem<PropertyInfo>>(), null);
            }

            var returnType = field.FieldType.GetGenericArguments()[0];
            properties = properties.Where(p => p.PropertyType.Equals(returnType));

            //.Where(m => m.GetParameterTypesWithTarget());
            var items = properties.Select(property =>
                    new DropdownItem<PropertyInfo>(property,
                                                path: GetDisplayPath(property),
                                                isSelected: property.Equals(selected)
                )).ToList();

            items.Prepend(new DropdownItem<PropertyInfo>(null, "Clear"));

            var menu = new DropdownMenu<PropertyInfo>(items, onSelect, sortItems: true, showNoneElement: true);
            menu.ShowAsContext();
            return menu;
        }
        public static DropdownMenu<EventInfo> EventDropdown(FieldInfo field, EventInfo selected, Action<EventInfo> onSelect)
        {
            var flags = BindingFlags.Default;

            if (field.FieldType.TryGetAttribute<BindingFlagsAttribute>(out var f))
                flags |= f.flags;

            if (field.TryGetAttribute<BindingFlagsAttribute>(out var f_attr))
                flags |= f_attr.flags;


            var mods = Module.ALL;
            if (field.TryGetAttribute<ModuleAttribute>(out var m_attr) && m_attr != null)
                mods = (Module)m_attr.module;

            var events = MReflect.GetEvents(mods, flags);

            if (events.Count() == 0)
            {
                Debug.LogError("No methods found!");
                return new DropdownMenu<EventInfo>(new List<DropdownItem<EventInfo>>(), null);
            }

            var generics = field.FieldType.GetGenericArguments();
            var eventType = generics.Length == 0 ? typeof(void) : generics[0];

            events = events.Where(p => p.EventHandlerType.Equals(eventType));

            foreach (var _ in events)
            {
                Debug.Log($"{_.Name} {_.EventHandlerType}");
            }

            //.Where(m => m.GetParameterTypesWithTarget());
            var items = events.Select(_ =>
                    new DropdownItem<EventInfo>(_,
                                                path: GetDisplayPath(_),
                                                isSelected: _.Equals(selected)
                )).ToList();

            items.Prepend(new DropdownItem<EventInfo>(null, "Clear"));

            var menu = new DropdownMenu<EventInfo>(items, onSelect, sortItems: true, showNoneElement: true);
            menu.ShowAsContext();
            return menu;
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
        public static string GetDisplayPath(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                return "...";

            StringBuilder option = new StringBuilder();

            // Append type name
            option.Append($"{propertyInfo.ModuleName()}/{MReflect.GetDeclaringString(propertyInfo.DeclaringType)}.");
            option.Append(propertyInfo.Name).Append(' ');
            if (propertyInfo.GetMethod.IsStatic)
                option.Append('*');

            option.Append("{ get; set; }");
            option.Append($" => {propertyInfo.PropertyType.Name}");

            return option.ToString();
        }
        public static string GetDisplayPath(EventInfo info)
        {
            if (info == null) return "...";

            StringBuilder option = new StringBuilder();

            // Append type name
            var tt = info.DeclaringType;
            option.Append($"{info.ModuleName()}/{MReflect.GetDeclaringString(info.DeclaringType)}.");
            option.Append(info.Name).Append(' ');
            if (info.AddMethod.IsStatic)
                option.Append('*');

            option.Append($"({info.EventHandlerType})");

            return option.ToString();
        }
    }
}