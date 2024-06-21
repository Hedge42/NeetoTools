using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityDropdown;
using UnityDropdown.Editor;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Matchwork
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(TypeSelector<>))]
    public class TypeSelectorDrawer : PropertyDrawer
    {
        private Dictionary<Type, List<Type>> _cachedDerivedTypes = new Dictionary<Type, List<Type>>();

        public static List<Type> GetDerivedTypes(Type baseType, Type[] exclude, bool onlyParameterless)
        {
            var cache = new Dictionary<Type, List<Type>>();

            if (!cache.ContainsKey(baseType))
            {
                var query = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(subType => baseType.IsAssignableFrom(subType) && !subType.IsAbstract);

                if (onlyParameterless)
                {
                    query = query.Where(p => p.GetConstructor(Type.EmptyTypes) != null);
                }

                if (exclude != null && exclude.Length > 0)
                {
                    // Exclude types that are the same or derived from any type in the 'exclude' array.
                    query = query.Where(p => !exclude.Any(e => e.IsAssignableFrom(p)));
                }

                cache[baseType] = query.ToList();
            }

            var result = cache[baseType];
            return cache[baseType];
        }

        public List<Type> GetDerivedTypes(Type baseType)
        {
            Type[] exclude = { };
            bool onlyParameterless = true;

            ExcludeTypesAttribute filter = fieldInfo.GetCustomAttribute<ExcludeTypesAttribute>();
            if (filter != null)
            {
                exclude = filter.exclude;
            }
            if (fieldInfo.GetCustomAttribute<ParameterlessAttribute>() != null)
            {
                onlyParameterless = true;
            }

            if (!_cachedDerivedTypes.ContainsKey(baseType))
            {
                var query = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(subType => baseType.IsAssignableFrom(subType) && !subType.IsAbstract);

                if (onlyParameterless)
                {
                    query = query.Where(p => p.GetConstructor(Type.EmptyTypes) != null);
                }

                if (exclude != null && exclude.Length > 0)
                {
                    // Exclude types that are the same or derived from any type in the 'exclude' array.
                    query = query.Where(p => !exclude.Any(e => e.IsAssignableFrom(p)));
                }

                _cachedDerivedTypes[baseType] = query.ToList();
            }

            return _cachedDerivedTypes[baseType];
        }

        DropdownMenu<Type> menu;

        DropdownMenu<Type> GetMenu(SerializedProperty selectionProperty)
        {
            var fieldType = fieldInfo.FieldType;
            if (fieldType.HasElementType)
                fieldType = fieldType.GetElementType();
            var baseType = fieldType.GetGenericArguments()[0];
            List<Type> types = GetDerivedTypes(baseType);

            var items = types.Select(t => new DropdownItem<Type>(t, t.Name)).ToList();
            foreach (var i in items)
            {
                i.IsSelected = selectionProperty.stringValue.Equals(i.SearchName);
            }
            void Changed(Type type)
            {
                selectionProperty.stringValue = type.Name.ToString();
                selectionProperty.ApplyAndMarkDirty();
            }

            menu = new DropdownMenu<Type>(items, Changed,
                sortItems: true, showNoneElement: true);

            return menu;
        }
        void SetType(Type t)
        {
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var selectionProperty = property.FindPropertyRelative("typeName");

            //EditorGUI.BeginProperty(position, label, property);
            if (EditorGUI.DropdownButton(position, label, FocusType.Passive))
            {
                GetMenu(selectionProperty).ShowAsContext();
            }
            //EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }
#endif



}