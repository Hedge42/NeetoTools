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
    /// <summary>
    /// Only here for the QuickAction test
    /// </summary>
    class SerializedMemberDrawer
    {
        [QuickAction] static void Open() => EditorWindow.GetWindow<Tester>();
        class Tester : EditorWindow
        {
            public SerializedProperty<bool> BoolProp;
            public SerializedEvent<bool> BoolEvent;
            public SerializedEvent VoidEvent;
            public SerializedAction MyAction;
            public SerializedAction<int> MyActionWithParam;
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
                if (GUILayout.Button("Invoke Actions..."))
                {
                    MyAction.Invoke();
                    MyActionWithParam.Invoke(42);
                }
            }
        }
    }

    public abstract class SerializedMemberDrawer<TMemberInfo> : PropertyDrawer where TMemberInfo : MemberInfo
    {
        static Dictionary<Type, MemberInfo[]> cache = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (NGUI.Property(position, property))
            {
                var value = property.GetProperValue(fieldInfo) as SerializedMember;
                value.owner = property.serializedObject.targetObject;

                position = position.With(h: NGUI.LineHeight);

                var display = GetDisplayString(value);

                if (EditorGUI.DropdownButton(EditorGUI.PrefixLabel(position, label), new GUIContent(display), FocusType.Passive))
                {
                    var options = FindMembers().Cast<TMemberInfo>()
                        .Select(info => (info, GetDropdowNGUI(info)))
                        .ToArray();

                    NGUI.ShowDropdown(info =>
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
                    var targetType = value.GetMember().DeclaringType;
                    value.target = EditorGUI.ObjectField(position.Move(y: NGUI.FullLineHeight), "target", value.target, targetType, true);

                    // helper to try get component if it is null
                    if (!value.target)
                    {
                        var sourceType = property.serializedObject.targetObject.GetType();
                        if (typeof(Component).IsAssignableFrom(targetType) && typeof(Component).IsAssignableFrom(sourceType))
                            value.target = (property.serializedObject.targetObject as Component).GetComponent(targetType);
                    }


                    EditorGUI.indentLevel--;
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

        public virtual string GetDropdowNGUI(MemberInfo info)
        {
            return info.GetModuleName() + "/" + info.DeclaringType.FullName + "." + info.Name;
        }



        public MemberInfo[] FindMembers()
        {
            if (cache.ContainsKey(fieldInfo.FieldType))
                return cache[fieldInfo.FieldType].ToArray();

            return cache[fieldInfo.FieldType] = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.FullName.ToLower().Contains("editor"))
                .SelectMany(assembly => assembly.GetTypes())
                .SelectMany(type => GetMembers(type))
                .Where(info => IsValidMember(info))
                .ToArray();
        }

        protected abstract IEnumerable<TMemberInfo> GetMembers(Type type);
        protected abstract bool IsValidMember(TMemberInfo memberInfo);
    }

    [CustomPropertyDrawer(typeof(SerializedAction))]
    class SerializedActionDrawer : SerializedMemberDrawer<MethodInfo>
    {
        protected override IEnumerable<MethodInfo> GetMembers(Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
        }

        protected override bool IsValidMember(MethodInfo info)
        {
            return info.ReturnType == typeof(void)
                && info.GetParameters().Length == 0
                && (typeof(UnityEngine.Object).IsAssignableFrom(info.ReflectedType) || info.IsStatic);
        }
    }

    [CustomPropertyDrawer(typeof(SerializedAction<>))]
    class SerializedActionTDrawer : SerializedMemberDrawer<MethodInfo>
    {
        protected override IEnumerable<MethodInfo> GetMembers(Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
        }

        protected override bool IsValidMember(MethodInfo info)
        {
            var parameterType = fieldInfo.FieldType.GetGenericArguments()[0];

            return info.ReturnType == typeof(void)
                && info.GetParameters().Length == 1
                && info.GetParameters()[0].ParameterType == parameterType
                && (typeof(UnityEngine.Object).IsAssignableFrom(info.ReflectedType) || info.IsStatic);
        }
    }

    [CustomPropertyDrawer(typeof(SerializedProperty<>))]
    class SerializedPropertyDrawer : SerializedMemberDrawer<PropertyInfo>
    {
        protected override IEnumerable<PropertyInfo> GetMembers(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
        }

        protected override bool IsValidMember(PropertyInfo info)
        {
            var propertyType = fieldInfo.FieldType.GetGenericArguments()[0];

            return propertyType.IsAssignableFrom(info.PropertyType)
                && (typeof(UnityEngine.Object).IsAssignableFrom(info.ReflectedType) || info.IsStatic());
        }
    }

    [CustomPropertyDrawer(typeof(SerializedEvent))]
    class SerializedEventDrawer : SerializedMemberDrawer<EventInfo>
    {
        protected override IEnumerable<EventInfo> GetMembers(Type type)
        {
            return type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
        }

        protected override bool IsValidMember(EventInfo info)
        {
            return info.EventHandlerType == typeof(Action)
                && (typeof(UnityEngine.Object).IsAssignableFrom(info.ReflectedType) || info.AddMethod.IsStatic);
        }
    }

    [CustomPropertyDrawer(typeof(SerializedEvent<>))]
    class SerializedEventTDrawer : SerializedMemberDrawer<EventInfo>
    {
        protected override IEnumerable<EventInfo> GetMembers(Type type)
        {
            return type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
        }

        protected override bool IsValidMember(EventInfo info)
        {
            var parameterType = fieldInfo.FieldType.GetGenericArguments()[0];
            var eventType = typeof(Action<>).MakeGenericType(parameterType);

            return info.EventHandlerType == eventType
                && (typeof(UnityEngine.Object).IsAssignableFrom(info.ReflectedType) || info.AddMethod.IsStatic);
        }
    }
}
