using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Text;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Neeto
{
    using static BindingFlags;

    [Serializable]
    public abstract class GameCallback : ISerializationCallbackReceiver
    {
        #region FLAGS
        public const BindingFlags FLAGS_ALL =
            BindingFlags.Public
            | BindingFlags.Static
            | BindingFlags.Instance
            | BindingFlags.FlattenHierarchy
            | BindingFlags.DeclaredOnly;
        //| BindingFlags.ExactBinding;

        public const BindingFlags FLAGS_M =
            FLAGS_ALL
            | BindingFlags.InvokeMethod
            | BindingFlags.SetProperty;

        public const BindingFlags FLAGS_F =
            FLAGS_ALL
            | BindingFlags.GetProperty
            | BindingFlags.CreateInstance;

        public const BindingFlags FLAGS_EVENTS =
            BindingFlags.Public
            | BindingFlags.Static
            | BindingFlags.FlattenHierarchy
            | BindingFlags.Instance;

        public const BindingFlags FLAGS_P =
            FLAGS_ALL
            | (BindingFlags.SetProperty & BindingFlags.GetProperty);
        #endregion
        #region Serialization
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Neeto.Delay.Editor(() =>
            {
                IsValid();
            });
        }
        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
        #endregion

        public string signature;
        public abstract bool IsValid();

        public static List<Type> GetMethodArgumentTypes(MethodInfo method)
        {
            /*
            Get full dynamic arguments sequence
            if this is an instance method, make the caller a potential argument
             */

            var list = NGUI.GetParameterTypes(method).ToList();
            if (!method.IsStatic)
                list.Insert(0, method.DeclaringType); // add the calling type as a potential argument
                                                      //list.AddRange(MReflect.GetParameterTypes(method));
            return list;
        }
        public static List<(string name, Type type)> GetMethodArgumentTypesAndNames(MethodInfo info)
        {
            var list = GetParameterTypesAndNames(info);
            if (!info.IsStatic)
                list.Insert(0, ("target", info.DeclaringType)); // add the calling type as a potential argument
            return list;
        }
        public static List<(string name, Type type)> GetParameterTypesAndNames(MethodInfo info)
        {
            return info.GetParameters().Select(_ => (_.Name, _.ParameterType)).ToList();
        }
    }

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

    [Serializable]
    public abstract class GameMethod : GameCallback, ISerializationCallbackReceiver
    {
        public Argument[] arguments;
        public int[] dynamics;
        private object[] _serializedArgs;
        public object[] serializedArgs => _serializedArgs ??= arguments.Select(arg => arg.value).ToArray();


        // derived
        private MethodInfo _info;
        public MethodInfo info => _info ??= NGUI.ToMethod(signature);


        private bool valid;
        private bool flag;

        //#if UNITY_EDITOR
        public override bool IsValid()
        {
            if (!(valid = dynamicInvoke != null))
            {
                // only display error with empty signature
                if (!string.IsNullOrWhiteSpace(signature) && !flag)
                {
                    var msg = $"Failed to build delegate '{signature}'";
                    Debug.LogWarning(msg);
                    flag = true;
                }
            }
            return valid;
        }
        //#endif

        private ActionDelegate _dynamicInvoke;
        public ActionDelegate dynamicInvoke
        {
            get
            {
                if (_dynamicInvoke == null)
                {
                    try
                    {
                        _dynamicInvoke = CreateDelegate(info);
                        valid = _dynamicInvoke != null;
                    }
                    catch
                    {
                        valid = false;
                    }
                }

                return _dynamicInvoke;
            }
        }
        public void Log(Exception e)
        {
            if (IsValid())
            {
                Debug.LogError($"Error calling method '{signature}'\n{e.Message}\n{e.StackTrace}");
            }
        }

        public delegate object ActionDelegate(params object[] args);
        public static ActionDelegate CreateDelegate(MethodInfo method)
        {
            var argsParameter = Expression.Parameter(typeof(object[]), "args");

            // Extracting the target from the first parameter of the array
            var instanceCast = method.IsStatic ? null :
                Expression.Convert(
                Expression.ArrayIndex(argsParameter,
                Expression.Constant(0)),
                method.DeclaringType);

            var methodParameters = method.GetParameters();
            var argExpressions = new Expression[methodParameters.Length];

            // If the method is static, start from args[0], otherwise start from args[1]
            int startArgIndex = method.IsStatic ? 0 : 1;

            for (int i = 0; i < methodParameters.Length; i++)
            {
                var index = Expression.Constant(i + startArgIndex);
                var arrayAccess = Expression.ArrayIndex(argsParameter, index);
                var convert = Expression.Convert(arrayAccess, methodParameters[i].ParameterType);

                argExpressions[i] = convert;
            }

            var methodCall = Expression.Call(instanceCast, method, argExpressions);

            // Adjust for methods that return void
            Expression returnExpression;
            if (method.ReturnType == typeof(void))
            {
                // If the method is void, return null after the method call
                var returnDefault = Expression.Constant(null, typeof(object));
                var block = Expression.Block(methodCall, returnDefault);
                returnExpression = block;
            }
            else
            {
                returnExpression = Expression.Convert(methodCall, typeof(object));
            }

            var lambda = Expression.Lambda<ActionDelegate>(
                returnExpression,
                argsParameter);

            return lambda.Compile();
        }



    }

    [Serializable]
    public abstract class GamePropBase : GameCallback
    {
        [SerializeReference]
        public object referenceTarget;
        public Object objectTarget;
        public bool isReferenceTarget;

        [SerializeReference]
        public object propertyInfo;

        public object target => isReferenceTarget ? referenceTarget : objectTarget;

        PropertyInfo _info;
        public PropertyInfo info => _info ??= NGUI.ToProperty(signature);

        public abstract object objectValue { get; }
    }

    [Serializable]
    public class GameProp<T> : GamePropBase, ISerializationCallbackReceiver
    {
        public delegate object GetDelegate(object target);
        public delegate void SetDelegate(object target, object value);



        bool validGetter;
        bool validSetter;

        public override object objectValue => value;
        public T value
        {
            get
            {
                if (getter == null)
                {
                    if (info == null) throw new MissingMemberException($"Cannot find PropertyInfo from '{signature}'");
                    throw new NotSupportedException($"Cannot read property from '{info}'");
                }
                return (T)getter.Invoke(target);
            }
            set
            {
                if (setter == null)

                    setter.Invoke(target, value);
            }
        }

        public GetDelegate CreateGetDelegate(PropertyInfo property)
        {
            var instance = Expression.Parameter(typeof(object), "target");
            var instanceCast = Expression.Convert(instance, property.DeclaringType);

            var propertyAccess = Expression.Property(instanceCast, property);
            var convertPropertyAccess = Expression.Convert(propertyAccess, typeof(object));

            var getterLambda = Expression.Lambda<GetDelegate>(
                convertPropertyAccess, instance);

            return getterLambda.Compile();
        }
        public SetDelegate CreateSetDelegate(PropertyInfo property)
        {
            var target = Expression.Parameter(typeof(object), "target");
            var value = Expression.Parameter(typeof(object), "value");

            var targetCast = Expression.Convert(target, property.DeclaringType);
            var valueCast = Expression.Convert(value, property.PropertyType);

            var propertyAccess = Expression.Property(targetCast, property);
            var assign = Expression.Assign(propertyAccess, valueCast);

            var setterLambda = Expression.Lambda<SetDelegate>(
                assign, target, value);

            return setterLambda.Compile();
        }


        protected GetDelegate _getter;
        public GetDelegate getter
        {
            get
            {
                if (_getter == null)
                {
                    try
                    {
                        _getter = CreateGetDelegate(info);
                        validGetter = _getter != null;
                    }
                    catch
                    {
                        validGetter = false;
                    }
                }

                return _getter;
            }
        }
        protected SetDelegate _setter;
        public SetDelegate setter
        {
            get
            {
                if (_setter == null)
                {
                    try
                    {
                        _setter = CreateSetDelegate(info);
                        validSetter = _setter != null;
                    }
                    catch
                    {
                        validSetter = false;
                    }
                }

                return _setter;
            }
        }

        public override bool IsValid()
        {
            if (info == null)
            {
                return false;
            }

            //if (info.CanRead)

            bool _valid = getter != null && setter != null;
            if (!_valid)
            {
                // only display error with empty signature
                if (!string.IsNullOrWhiteSpace(signature))
                {
                    var msg = $"Failed to build delegate '{signature}'";
                    Debug.LogWarning(msg);
                }
            }
            return validSetter;
        }
    }

    [Serializable]
    public abstract class GameActionBase : GameMethod { }

    [Serializable]

    public abstract class GameFuncBase : GameMethod { }

    [Serializable]
    [BindingFlags(OptionalParamBinding | Public | NonPublic | Instance | DeclaredOnly | Static | SetProperty)]
    public class GameAction : GameActionBase
    {
        public static implicit operator GameAction(string s) => new GameAction() { signature = s };

        public event Action onInvoke;

        public void Invoke()
        {
            if (IsValid())
            {
                try
                {
                    dynamicInvoke.Invoke(serializedArgs);
                    onInvoke?.Invoke();
                }
                catch (Exception e)
                {
                    Log(e);
                }
            }
        }
        public static MethodInfo GetMethod(string signature)
        {
            var typeAndMethod = signature.Split('(');
            var typeAndMethodParts = typeAndMethod[0].Split('.');
            var methodName = typeAndMethodParts.Last();

            var typeParts = typeAndMethodParts.Take(typeAndMethodParts.Length - 1);
            var typeName = string.Join('.', typeParts);
            var type = NGUI.GetType(typeName);
            var paramsFull = typeAndMethod[1].TrimEnd(')');

            // Split would create an empty string where it wasn't needed...
            if (!string.IsNullOrWhiteSpace(paramsFull))
            {
                var paramTypes = paramsFull.Split(',').Select(n => NGUI.GetType(n)).ToArray();
                return type.GetMethod(methodName, paramTypes);
            }
            else
            {
                return type.GetMethod(methodName, new Type[] { });
            }
        }

        public static GameAction Create(MethodInfo method)
        {
            return new GameAction
            {
                signature = NGUI.ToSignature(method)
            };
        }

#if UNITY_EDITOR
        [QuickAction] static void Test() => EditorWindow.GetWindow<TestWindow>();
#endif
    }

    /// <summary> Instance method of type T or 1 dynamic arg </summary>
    [Serializable]
    public class GameAction<T> : GameActionBase
    {
        public Action callback;

        public void Invoke(T obj)
        {
            if (IsValid())
            {
                try
                {
                    var i = dynamics[0];
                    if (i >= 0)
                        serializedArgs[i] = obj;

                    dynamicInvoke.Invoke(serializedArgs);
                    callback?.Invoke();
                }
                catch (Exception e)
                {
                    Log(e);
                }
            }
        }
    }

    /// <summary> 2 dynamic args or instance method of type T or V with 1 arg</summary>
    [Serializable]
    public class GameAction<T, V> : GameActionBase
    {
        public void Invoke(T t, V v)
        {
            if (IsValid())
            {
                try
                {

                    var i1 = dynamics[0];
                    var i2 = dynamics[1];

                    if (i1 >= 0)
                        serializedArgs[i1] = t;
                    if (i2 >= 0)
                        serializedArgs[i2] = v;

                    dynamicInvoke.Invoke(serializedArgs);
                }
                catch (Exception e)
                {
                    Log(e);
                }
            }
        }
    }

    [Serializable]
    [BindingFlags(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy | BindingFlags.CreateInstance)]
    public class GameFunc<R> : GameFuncBase
    {
        public R Invoke()
        {
            if (IsValid())
            {
                try
                {
                    return (R)dynamicInvoke.Invoke(serializedArgs);
                }
                catch (Exception e)
                {
                    Log(e);
                }
            }
            return default;
        }
    }

    [Serializable]
    public class GameFunc<R, T> : GameFuncBase
    {
        public R Invoke(T obj)
        {
            if (IsValid())
            {
                try
                {
                    var i = dynamics[0];
                    if (i >= 0)
                        serializedArgs[i] = obj;

                    return (R)dynamicInvoke.Invoke(serializedArgs);
                }
                catch (Exception e)
                {
                    if (IsValid())
                    {
                        Debug.LogError($"Error calling method\n{e.Message}\n{e.StackTrace}");
                    }
                }
            }
            return default;
        }
    }

    [Serializable]
    public class GameFunc<R, T, U> : GameFuncBase
    {
        public R Invoke(T t, U u)
        {
            if (IsValid())
            {
                try
                {
                    var i1 = dynamics[0];
                    var i2 = dynamics[1];

                    if (i1 >= 0)
                        serializedArgs[i1] = t;
                    if (i2 >= 0)
                        serializedArgs[i2] = u;

                    return (R)dynamicInvoke.Invoke(serializedArgs);
                }
                catch (Exception e)
                {
                    if (IsValid())
                    {
                        Debug.LogError($"Error calling method\n{e.Message}\n{e.StackTrace}");
                    }
                }
            }
            return default;
        }
    }


    public static class GameActionHelper
    {
        public const string NONE = "(none)";
        public static void Invoke(this GameAction[] actions)
        {
            for (int i = 0; i < actions.Length; i++)
            {
                Debug.Log("Invoking " + actions[i].signature);
                actions[i].Invoke();
            }
        }
        
        public static string GetDisplayPath(EventInfo info)
        {
            if (info == null)
                return NONE;

            StringBuilder option = new StringBuilder();

            // Append type name
            var tt = info.DeclaringType;
            option.Append($"{info.GetModuleName()}/{NGUI.GetDeclaringString(info.DeclaringType)}.");
            option.Append(info.Name).Append(' ');
            if (info.AddMethod.IsStatic)
                option.Append('*');

            option.Append($"({info.EventHandlerType})");

            return option.ToString();
        }
        public static bool TryGetDisplayPath(PropertyInfo info, out string path)
        {
            return !(path = GetDisplayPath(info)).Equals(NONE);
        }
        public static string GetDisplayPath(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null || propertyInfo.GetMethod == null)
                return NONE;


            StringBuilder option = new StringBuilder();

            // Append type name
            option.Append($"{propertyInfo.GetModuleName()}/{NGUI.GetDeclaringString(propertyInfo.DeclaringType)}.{propertyInfo.Name} ");
            if (propertyInfo.GetMethod.IsStatic)
                option.Append('*');

            //option.Append("{ get; set; }");
            option.Append($" => {propertyInfo.PropertyType.Name}");

            return option.ToString();
        }

        public static void PropertyDropdown(FieldInfo field, PropertyInfo selected, Action<PropertyInfo> onSelect)
        {
            var flags = BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Instance;
            var returnType = field.FieldType.GetGenericArguments()[0];
            var properties = NGUI.RuntimeAssemblies.GetProperties(returnType, flags)
                // needs get method for other logic & generics not supported
                .Where(prop => prop.GetMethod != null && prop.CanRead && !prop.DeclaringType.ContainsGenericParameters && (prop.GetMethod.IsStatic || prop.DeclaringType.IsSerializable) && returnType.IsAssignableFrom(prop.PropertyType))
                .Select(prop => (prop, GetDisplayPath(prop)))
                .Where((prop, path) => !path.Equals(NONE))
                .ToArray();

            if (properties.Length == 0)
            {
                Debug.LogError("No methods found!");
                return;
            }

            NGUI.ShowDropdown(onSelect, true, true, GetDisplayPath(selected), properties);
        }
        public static void EventDropdown(FieldInfo field, EventInfo selected, Action<EventInfo> onSelect)
        {
            var flags = BindingFlags.Default;

            if (field.FieldType.TryGetAttribute<BindingFlagsAttribute>(out var f))
                flags |= f.flags;

            if (field.TryGetAttribute<BindingFlagsAttribute>(out var f_attr))
                flags |= f_attr.flags;


            var events = NGUI.GetRuntimeTypes().GetEvents(flags);

            if (events.Count() == 0)
            {
                Debug.LogError("No methods found!");
                return;
            }

            var generics = field.FieldType.GetGenericArguments();
            var eventType = generics.Length == 0 ? typeof(void) : generics[0];

            events = events.Where(p => p.EventHandlerType.Equals(eventType));

            foreach (var _ in events)
            {
                Debug.Log($"{_.Name} {_.EventHandlerType}");
            }

            //.Where(m => m.GetParameterTypesWithTarget());
            var items = events.Select(evnt => (evnt, GameActionHelper.GetDisplayPath(evnt))).ToArray();

            NGUI.ShowDropdown(onSelect, true, true, GameActionHelper.GetDisplayPath(selected), items);
        }
        

        public static string GetSearchName(MethodInfo info)
        {
            return $"{info.DeclaringType.Name}.{info.Name}";
        }
        public static string GetLabelName(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                return GameActionHelper.NONE;
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
        public static string GetLabelName(PropertyInfo info)
        {
            if (info == null && info.GetMethod != null)
            {
                return GameActionHelper.NONE;
            }

            StringBuilder option = new StringBuilder();
            var tt = info.DeclaringType;
            option.Append(info.ReflectedType.Name + ".");
            option.Append(info.Name + " ");
            if (info.GetMethod.IsStatic)
                option.Append('*');

            if (info.SetMethod != null && info.SetMethod.IsPublic)
                option.Append(@"{ get; set; }");
            else
                option.Append(@" { get; } ");

            // Append parameter types
            if (info.PropertyType != null)
            {
                option.Append($" => {info.PropertyType.Name}");
            }

            return option.ToString();
        }
    }

#if UNITY_EDITOR
    class TestWindow : EditorWindow
    {


        public GameAction action;
        public GameFunc<bool> func;
        public GameProp<bool> prop;

        Editor editor;

        void OnGUI()
        {
            Editor.CreateCachedEditor(this, null, ref editor);
            editor.OnInspectorGUI();
        }
    }
#endif
}