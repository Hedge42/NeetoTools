using Rhinox.Lightspeed.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Toolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Neeto
{
    class SerializedMemberDrawer : PropertyDrawer
    {
        [QuickAction] static void Open() => EditorWindow.GetWindow<Tester>();
        class Tester : EditorWindow
        {
            public SerializedProperty<bool> BoolProp;
            public SerializedEvent<bool> BoolEvent;
            public SerializedEvent VoidEvent;
            Editor editor;

            void OnGUI()
            {
                Editor.CreateCachedEditor(this, null, ref editor);
                editor.OnInspectorGUI();

                if (GUILayout.Button("Test Property..."))
                {
                    if (BoolProp.GetMember() != null)
                    {
                        Debug.Log(BoolProp.Value);
                    }
                }
                if (GUILayout.Button("Subscribe to events..."))
                {
                    if (BoolEvent.GetMember() != null)
                    {
                        BoolEvent.AddListener(_ => { Debug.Log($"result: {_}"); });
                    }
                    if (VoidEvent.GetMember() != null)
                    {
                        VoidEvent.AddListener(() => { Debug.Log($"result: yay!"); });
                    }
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var obj = property.GetProperValue(fieldInfo) as SerializedMember;
            if (obj.NeedsTarget())
            {
                return NGUI.FullLineHeight * 2;
            }
            else
            {
                return NGUI.FullLineHeight;
            }
        }
        public virtual string GetDisplayString(SerializedMember member)
        {
            if (member != null && member.GetMember() is MemberInfo info)
            {
                return info.DeclaringType.FullName + "." + info.Name;
            }
            else
            {
                return "(none)";
            }
        }
        public virtual string GetDropdownString(MemberInfo info)
        {
            return info.ModuleName() + "/" + info.DeclaringType.FullName + "." + info.Name;
        }
    }

    [CustomPropertyDrawer(typeof(SerializedAction))]
    class SerializedActionDrawer : SerializedMemberDrawer
    {
        static Dictionary<Type, MemberInfo[]> cache = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (NGUI.Property(position, property, label))
            {
                var value = property.GetProperValue(fieldInfo) as SerializedMember;
                position = position.With(h: NGUI.LineHeight);

                var display = GetDisplayString(value);

                if (EditorGUI.DropdownButton(EditorGUI.PrefixLabel(position, label), new GUIContent(display), FocusType.Passive))
                {
                    var options = FindMembers().Cast<MethodInfo>()
                        .Select(info => (info, GetDropdownString(info)))
                        .ToArray();

                    DropdownHelper.Show(info =>
                    {
                        Undo.RecordObject(property.serializedObject.targetObject, "Set Member");
                        value.target = null;
                        if (info == null)
                        {
                            value.DeclaringType = value.MemberName = "";
                        }
                        else
                        {
                            value.DeclaringType = info.DeclaringType.AssemblyQualifiedName;
                            value.MemberName = info.Name;
                        }
                        EditorUtility.SetDirty(property.serializedObject.targetObject);
                    }, true, true, display, options);
                }
                else if (value.NeedsTarget())
                {
                    EditorGUI.indentLevel++;
                    Undo.RecordObject(property.serializedObject.targetObject, "Set Member");
                    value.target = EditorGUI.ObjectField(position.Move(y: NGUI.FullLineHeight), "target", value.target, value.GetMember().DeclaringType, true);
                    EditorGUI.indentLevel--;
                }
            }
        }

        public MemberInfo[] FindMembers()
        {
            if (cache.ContainsKey(fieldInfo.FieldType))
                return cache[fieldInfo.FieldType].ToArray();

            return cache[fieldInfo.FieldType] = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.FullName.ToLower().Contains("editor"))
                .SelectMany(assembly => assembly.GetTypes())
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Where(info => info.ReturnType == typeof(void)
                    && info.GetParameters().Length == 0
                    && (typeof(UnityEngine.Object).IsAssignableFrom(info.ReflectedType) || info.IsStatic))
                .ToArray();
        }
    }

    [CustomPropertyDrawer(typeof(SerializedAction<>))]
    class SerializedActionTDrawer : SerializedMemberDrawer
    {
        static Dictionary<Type, MemberInfo[]> cache = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (NGUI.Property(position, property, label))
            {
                var value = property.GetProperValue(fieldInfo) as SerializedMember;
                position = position.With(h: NGUI.LineHeight);

                var display = GetDisplayString(value);

                if (EditorGUI.DropdownButton(EditorGUI.PrefixLabel(position, label), new GUIContent(display), FocusType.Passive))
                {
                    var parameterType = fieldInfo.FieldType.GetGenericArguments()[0];

                    var options = FindMembers(parameterType).Cast<MethodInfo>()
                        .Select(info => (info, GetDropdownString(info)))
                        .ToArray();

                    DropdownHelper.Show(info =>
                    {
                        Undo.RecordObject(property.serializedObject.targetObject, "Set Member");
                        value.target = null;
                        if (info == null)
                        {
                            value.DeclaringType = value.MemberName = "";
                        }
                        else
                        {
                            value.DeclaringType = info.DeclaringType.AssemblyQualifiedName;
                            value.MemberName = info.Name;
                        }
                        EditorUtility.SetDirty(property.serializedObject.targetObject);
                    }, true, true, display, options);
                }
                else if (value.NeedsTarget())
                {
                    EditorGUI.indentLevel++;
                    Undo.RecordObject(property.serializedObject.targetObject, "Set Member");
                    value.target = EditorGUI.ObjectField(position.Move(y: NGUI.FullLineHeight), "target", value.target, value.GetMember().DeclaringType, true);
                    EditorGUI.indentLevel--;
                }
            }
        }

        public MemberInfo[] FindMembers(Type parameterType)
        {
            if (cache.ContainsKey(fieldInfo.FieldType))
                return cache[fieldInfo.FieldType].ToArray();

            return cache[fieldInfo.FieldType] = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.FullName.ToLower().Contains("editor"))
                .SelectMany(assembly => assembly.GetTypes())
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Where(info => info.ReturnType == typeof(void)
                    && info.GetParameters().Length == 1
                    && info.GetParameters()[0].ParameterType == parameterType
                    && (typeof(UnityEngine.Object).IsAssignableFrom(info.ReflectedType) || info.IsStatic))
                .ToArray();
        }
    }

    [CustomPropertyDrawer(typeof(SerializedProperty<>))]
    class SerializedPropertyDrawer : SerializedMemberDrawer
    {
        static Dictionary<Type, MemberInfo[]> cache = new();
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (NGUI.Property(position, property, label))
            {
                var value = property.GetProperValue(fieldInfo) as SerializedMember;
                position = position.With(h: NGUI.LineHeight);

                var display = GetDisplayString(value);

                if (EditorGUI.DropdownButton(EditorGUI.PrefixLabel(position, label), new GUIContent(display), FocusType.Passive))
                {
                    var propertyType = fieldInfo.FieldType.GetGenericArguments()[0];

                    var options = FindMembers().Cast<PropertyInfo>()
                        .Where(info => propertyType.IsAssignableFrom(info.PropertyType))
                        .Select(info => (info, GetDropdownString(info)))
                        .ToArray();

                    DropdownHelper.Show(info =>
                    {
                        Undo.RecordObject(property.serializedObject.targetObject, "Set Member");
                        value.target = null;
                        if (info == null)
                        {
                            value.DeclaringType = value.MemberName = "";
                        }
                        else
                        {
                            value.DeclaringType = info.DeclaringType.AssemblyQualifiedName;
                            value.MemberName = info.Name;
                        }
                        EditorUtility.SetDirty(property.serializedObject.targetObject);
                    }, true, true, display, options);
                }
                else if (value.NeedsTarget())
                {
                    EditorGUI.indentLevel++;
                    Undo.RecordObject(property.serializedObject.targetObject, "Set Member");
                    value.target = EditorGUI.ObjectField(position.Move(y: NGUI.FullLineHeight), "target", value.target, value.GetMember().DeclaringType, true);
                    EditorGUI.indentLevel--;
                }
            }
        }


        public MemberInfo[] FindMembers()
        {
            if (cache.ContainsKey(fieldInfo.FieldType))
                return cache[fieldInfo.FieldType].ToArray();

            return cache[fieldInfo.FieldType] = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.FullName.ToLower().Contains("editor")) // no editor assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)) // public only
                .Where(info => typeof(UnityEngine.Object).IsAssignableFrom(info.ReflectedType) || info.IsStatic()) // either static or target is Unity Object
                .ToArray();
        }
    }

    [CustomPropertyDrawer(typeof(SerializedEvent))]
    class SerializedEventDrawer : SerializedMemberDrawer
    {
        static Dictionary<Type, MemberInfo[]> cache = new();
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (NGUI.Property(position, property, label))
            {
                var value = property.GetProperValue(fieldInfo) as SerializedMember;
                position = position.With(h: NGUI.LineHeight);

                var display = GetDisplayString(value);

                if (EditorGUI.DropdownButton(EditorGUI.PrefixLabel(position, label), new GUIContent(display), FocusType.Passive))
                {
                    var options = FindMembers().Cast<EventInfo>()
                        .Select(info => (info, GetDropdownString(info)))
                        .ToArray();

                    DropdownHelper.Show(info =>
                    {
                        Undo.RecordObject(property.serializedObject.targetObject, "Set Member");
                        value.target = null;
                        if (info == null)
                        {
                            value.DeclaringType = value.MemberName = "";
                        }
                        else
                        {
                            value.DeclaringType = info.DeclaringType.AssemblyQualifiedName;
                            value.MemberName = info.Name;
                        }
                        EditorUtility.SetDirty(property.serializedObject.targetObject);
                    }, true, true, display, options);
                }
                else if (value.NeedsTarget())
                {
                    EditorGUI.indentLevel++;
                    Undo.RecordObject(property.serializedObject.targetObject, "Set Member");
                    property.FindPropertyRelative(nameof(SerializedMember.target)).objectReferenceValue
                        = EditorGUI.ObjectField(position.Move(y: NGUI.FullLineHeight), "target", value.target, value.GetMember().DeclaringType, true);
                    EditorGUI.indentLevel--;
                }
            }
        }
        /*
         * find void EventInfos
         */
        public MemberInfo[] FindMembers()
        {
            if (cache.ContainsKey(fieldInfo.FieldType))
                return cache[fieldInfo.FieldType].ToArray();

            return cache[fieldInfo.FieldType] = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.FullName.ToLower().Contains("editor")) // no editor assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .SelectMany(type => type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)) // public only
                .Where(info => info.EventHandlerType.Equals(typeof(Action)) && (typeof(UnityEngine.Object).IsAssignableFrom(info.ReflectedType) || info.AddMethod.IsStatic)) // either static or target is Unity Object
                .ToArray();
        }
    }

    [CustomPropertyDrawer(typeof(SerializedEvent<>))]
    class SerializedEventTDrawer : SerializedMemberDrawer
    {
        static Dictionary<Type, MemberInfo[]> cache = new();
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (NGUI.Property(position, property, label))
            {
                var value = property.GetProperValue(fieldInfo) as SerializedMember;
                position = position.With(h: NGUI.LineHeight);

                var display = GetDisplayString(value);

                if (EditorGUI.DropdownButton(EditorGUI.PrefixLabel(position, label), new GUIContent(display), FocusType.Passive))
                {
                    var propertyType = fieldInfo.FieldType.GetGenericArguments()[0];

                    var options = FindMembers().Cast<EventInfo>()
                        .Select(info => (info, GetDropdownString(info)))
                        .ToArray();

                    DropdownHelper.Show(info =>
                    {
                        Undo.RecordObject(property.serializedObject.targetObject, "Set Member");
                        value.target = null;
                        if (info == null)
                        {
                            value.DeclaringType = value.MemberName = "";
                        }
                        else
                        {
                            value.DeclaringType = info.DeclaringType.AssemblyQualifiedName;
                            value.MemberName = info.Name;
                        }
                        EditorUtility.SetDirty(property.serializedObject.targetObject);
                    }, true, true, display, options);
                }
                else if (value.NeedsTarget())
                {
                    EditorGUI.indentLevel++;
                    Undo.RecordObject(property.serializedObject.targetObject, "Set Member");
                    value.target = EditorGUI.ObjectField(position.Move(y: NGUI.FullLineHeight), "target", value.target, value.GetMember().DeclaringType, true);
                    EditorGUI.indentLevel--;
                }
            }
        }


        Type eventType;
        bool MatchesField(EventInfo info)
        {
            /*
             SerializedEvent<float>
                requires
             event Action<float>
             */
            return info.EventHandlerType.Equals(eventType ??= typeof(Action<>).MakeGenericType(fieldInfo.FieldType.GetGenericArguments()[0]));
        }

        public MemberInfo[] FindMembers()
        {
            if (cache.ContainsKey(fieldInfo.FieldType))
                return cache[fieldInfo.FieldType].ToArray();

            return cache[fieldInfo.FieldType] = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.FullName.ToLower().Contains("editor")) // no editor assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .SelectMany(type => type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)) // public only
                .Where(info => MatchesField(info)
                    && (typeof(UnityEngine.Object).IsAssignableFrom(info.ReflectedType) || info.AddMethod.IsStatic)) // either static or target is Unity Object
                .ToArray();
        }

    }
}
