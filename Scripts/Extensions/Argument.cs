using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Linq;
using static Argument;
using UnityEditor;
using Matchwork;

[System.Serializable]
public class Argument
{
    [SerializeReference]
    public object data;

    public enum ArgType
    {
        Null,
        Boolean,
        String,
        Float,
        Double,
        Int,
        Vector2,
        Vector3,
        Vector4,
        Color,
        ObjectReference,
        Enum,
        Generic,
        Reference,
        Dynamic,
    }
    public Argument(Type type)
    {
        this.argType = EnumOf(type);
    }


    [HideInInspector] public bool argBoolean;
    [HideInInspector] public string argString;
    [HideInInspector] public float argFloat;
    [HideInInspector] public double argDouble;
    [HideInInspector] public int argInt;
    [HideInInspector] public Vector2 argVector2;
    [HideInInspector] public Vector3 argVector3;
    [HideInInspector] public Vector4 argVector4;
    [HideInInspector] public Color argColor;
    [HideInInspector] public Object argObjectReference;
    [HideInInspector] public int argEnum;
    [HideInInspector] public string argGeneric;
    [SerializeReference, Polymorphic] public object argReference;
    [HideInInspector] public string referenceType;
    [HideInInspector] public bool isDynamic;

    public ArgType argType;


    public object value
    {
        get
        {
            switch (argType)
            {
                case ArgType.Boolean:
                    return argBoolean;
                case ArgType.String:
                    return argString;
                case ArgType.Float:
                    return argFloat;
                case ArgType.Double:
                    return argDouble;
                case ArgType.Int:
                    return argInt;
                case ArgType.Vector2:
                    return argVector2;
                case ArgType.Vector3:
                    return argVector3;
                case ArgType.Vector4:
                    return argVector4;
                case ArgType.Color:
                    return argColor;
                case ArgType.ObjectReference:
                    return argObjectReference;
                case ArgType.Enum:
                    return argEnum;
                case ArgType.Reference:
                    return argReference;
                //case ArgType.Generic:
                //return argGeneric;
                default:
                    return null;
            }
        }
    }

    public static string GetFieldName(Type type)
    {
        switch (type.Name)
        {
            case nameof(Boolean):
                return nameof(argBoolean);
            case nameof(String):
                return nameof(argString);
            case nameof(Single):
                return nameof(argFloat);
            case nameof(Double):
                return nameof(argDouble);
            case nameof(LayerMask):
            case nameof(Int32):
                return nameof(argInt);
            case nameof(Vector2):
                return nameof(argVector2);
            case nameof(Vector3):
                return nameof(argVector3);
            case nameof(Vector4):
                return nameof(argVector4);
            case nameof(Color):
                return nameof(argColor);
            default:
                break;
        }

        if (typeof(Object).IsAssignableFrom(type))
            return nameof(argObjectReference);
        else if (type.IsEnum)
            return nameof(argEnum);
        else if (type.IsSerializable || type.IsInterface || type.IsAbstract)
            return nameof(argReference);
        else return "";
    }

    public static ArgType EnumOf(Type argType)
    {
        switch (argType.Name)
        {
            case nameof(Boolean):
                return ArgType.Boolean;
            case nameof(String):
                return ArgType.String;
            case nameof(Single):
                return ArgType.Float;
            case nameof(Double):
                return ArgType.Double;
            case nameof(LayerMask):
            case nameof(Int32):
                return ArgType.Int;
            case nameof(Vector2):
                return ArgType.Vector2;
            case nameof(Vector3):
                return ArgType.Vector3;
            case nameof(Vector4):
                return ArgType.Vector4;
            case nameof(Color):
                return ArgType.Color;
            default:
                break;
        }

        if (typeof(Object).IsAssignableFrom(argType))
            return ArgType.ObjectReference;
        else if (argType.IsEnum)
            return ArgType.Enum;
        else if (argType.IsSerializable || argType.IsInterface)
            return ArgType.Reference;

        return ArgType.Null;
    }
}

[System.Serializable]
public class ArgRef
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ArgRef))]
    public class ArgRefDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return MEdit.fullLineHeight;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var type = Module.ALL.GetType(property.FindPropertyRelative(nameof(ArgRef.type)).stringValue);
            var prop = property.FindPropertyRelative(ArgRef.GetFieldName(type));

            EditorGUI.PropertyField(position, property, label);

            EditorGUI.EndProperty();
        }
    }
#endif

    public string type;

    [HideInInspector] public bool argBoolean;
    [HideInInspector] public string argString;
    [HideInInspector] public float argFloat;
    [HideInInspector] public double argDouble;
    [HideInInspector] public int argInt;
    [HideInInspector] public Vector2 argVector2;
    [HideInInspector] public Vector3 argVector3;
    [HideInInspector] public Vector4 argVector4;
    [HideInInspector] public Color argColor;
    [HideInInspector] public Object argObjectReference;
    [HideInInspector] public int argEnum;
    [HideInInspector] public string argGeneric;
    [HideInInspector, SerializeReference] public object argReference;

    public static ArgRef Create(Type type)
    {
        if (typeof(bool).Equals(type))
        {
            return new ArgRef() { type = typeof(bool).FullName };
        }
        else if (typeof(string).Equals(type))
        {
            return new ArgRef() { type = typeof(string).FullName };
        }
        else if (typeof(float).Equals(type))
        {
            return new ArgRef() { type = typeof(float).FullName };
        }
        else if (typeof(double).Equals(type))
        {
            return new ArgRef() { type = typeof(double).FullName };
        }
        else if (typeof(int).Equals(type))
        {
            return new ArgRef() { type = typeof(int).FullName };
        }
        else if (typeof(Vector2).Equals(type))
        {
            return new ArgRef() { type = typeof(Vector2).FullName };
        }
        else if (typeof(Vector3).Equals(type))
        {
            return new ArgRef() { type = typeof(Vector3).FullName };
        }
        else if (typeof(Vector4).Equals(type))
        {
            return new ArgRef() { type = typeof(Vector4).FullName };
        }
        else if (typeof(Color).Equals(type))
        {
            return new ArgRef() { type = typeof(Color).FullName };
        }
        else if (typeof(Object).IsAssignableFrom(type))
        {
            return new ArgRef() { type = type.FullName };
        }
        return null;
    }

    public static string GetFieldName(Type type)
    {
        if (typeof(bool).Equals(type))
        {
            return nameof(argBoolean);
        }
        else if (typeof(string).Equals(type))
        {
            return nameof(argString);
        }
        else if (typeof(float).Equals(type))
        {
            return nameof(argFloat);
        }
        else if (typeof(double).Equals(type))
        {
            return nameof(argDouble);
        }
        else if (typeof(int).Equals(type))
        {
            return nameof(argInt);
        }
        else if (typeof(Vector2).Equals(type))
        {
            return nameof(argVector2);
        }
        else if (typeof(Vector3).Equals(type))
        {
            return nameof(argVector3);
        }
        else if (typeof(Vector4).Equals(type))
        {
            return nameof(argVector4);
        }
        else if (typeof(Color).Equals(type))
        {
            return nameof(argColor);
        }
        else if (typeof(Object).IsAssignableFrom(type))
        {
            return nameof(argObjectReference);
        }
        return null;
    }
}