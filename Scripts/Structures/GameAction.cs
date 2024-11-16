using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Neeto
{

    [System.Serializable]
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
        public virtual void OnAfterDeserialize()
        {
            //referenceData = this;
            IsValid();
        }
        public virtual void OnBeforeSerialize()
        {
            //referenceData = this;
        }
        #endregion

        public string signature;
        public abstract bool IsValid();


        public static IEnumerable<MethodInfo> GetMethods(FieldInfo info)
        {
            var generics = info.FieldType.GetGenericArguments().ToList();

            var isFunc = typeof(GameFuncBase).IsAssignableFrom(info.FieldType);
            var flags = isFunc ? FLAGS_F : FLAGS_M;

            //var methods = Module.ALL.GetTypes()

            var methods = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => asm.GetTypes())
                .Where(type => type.IsPublic && !type.IsEnum && !type.IsGenericType && !type.ContainsGenericParameters)
                .SelectMany(type => type.GetMethods(flags))
                .Where(m => !m.ContainsGenericParameters
                            && !m.GetParameterTypes().Any(p => p.ContainsGenericParameters)); // non-generic parameters

            var fieldTypes = info.FieldType.GetGenericArguments();
            methods = methods.Where(m =>
                m.GetParameters().All(p => // all parameters are supported
                    Argument.EnumOf(p.ParameterType) != Argument.ArgType.Null
                        || fieldTypes.Any(f => p.ParameterType.IsAssignableFrom(f)))); // support as dynamic

            if (isFunc)
            {
                var returnType = info.FieldType.GetGenericArguments()[0];
                methods = methods.Where(m => returnType.IsAssignableFrom(m.ReturnType));
                generics.RemoveAt(0);
            }

            var matching = methods.Where(m => GetMethodArgumentTypes(m).Intersect(generics).Count() == generics.Count);
            //Debug.Log($"matching:{matching.Count()}");

            return matching;
        }
        public static IEnumerable<PropertyInfo> GetProperties(FieldInfo info)
        {
            var generics = info.FieldType.GetGenericArguments().ToList();

            var isProp = typeof(GamePropBase).IsAssignableFrom(info.FieldType);
            var propType = info.FieldType.GetGenericArguments()[0];

            var types = ReflectionHelper.GetRuntimeTypes()
                .Where(t => t.IsPublic && !t.IsEnum);
            var props = types.SelectMany(t => t.GetProperties(FLAGS_P))
                .Where(p => p.PropertyType.Equals(propType));

            return props;
        }
        public static IEnumerable<EventInfo> GetEvents(FieldInfo info)
        {
            var generics = info.FieldType.GetGenericArguments().ToList();

            var types = ReflectionHelper.GetRuntimeTypes()
                .Where(t => t.IsPublic && !t.IsEnum);

            var attribute = info.GetCustomAttribute<PolymorphicAttribute>();

            if (attribute.include != null)
            {
                types = attribute.include;
            }
            else if (attribute.exclude != null)
            {
                types = types.Where(t => !attribute.exclude.Contains(t));
            }


            var events = types.SelectMany(t => t.GetEvents(FLAGS_EVENTS));
            var values = events.Select(e => (e, e.GetAddMethod())); // (EventInfo, MethodInfo)

            values = values.Where(m =>
                m.Item2.GetParameters().All(p => // all parameters are supported
                    Argument.EnumOf(p.ParameterType) != Argument.ArgType.Null));


            // ensure all parameters are matched, but in no specific order
            var matching = values.Where(m => GetMethodArgumentTypes(m.Item2)
                .Intersect(generics).Count() == generics.Count);

            return values.Select(_ => _.Item1);
        }

        public static List<Type> GetMethodArgumentTypes(MethodInfo method)
        {
            /*
            Get full dynamic arguments sequence
            if this is an instance method, make the caller a potential argument
             */

            var list = ReflectionHelper.GetParameterTypes(method).ToList();
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

    #region EDITOR
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Argument))]
    public class ArgumentDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            try
            {
                property = GetSelectedProperty(property);

                return EditorGUI.GetPropertyHeight(property) + EditorGUIUtility.standardVerticalSpacing;
            }
            catch
            {
                return NGUI.FullLineHeight;
            }
        }

        public static bool IsDynamic(SerializedProperty property, int i)
        {
            var dyns = property.FindPropertyRelative(nameof(GameAction.dynamics));
            foreach (SerializedProperty dy in dyns)
            {
                if (dy.intValue == i)
                {
                    return true;
                }
            }
            return false;
        }

        public static Argument.ArgType UpdateArgType(SerializedProperty argumentProperty, Type paramType)
        {
            var enumProperty = argumentProperty.FindPropertyRelative(nameof(Argument.argType));

            var value = Argument.EnumOf(paramType);
            enumProperty.enumValueIndex = (int)value;

            if (value == Argument.ArgType.Reference)
            {
                argumentProperty.FindPropertyRelative(nameof(Argument.referenceType))
                    .stringValue = $"{paramType.AssemblyQualifiedName}";
            }

            return value;
        }

        public static Rect PropertyGUI(Rect position, SerializedProperty selected, FieldInfo info, Type type, bool isDynamic = false, string label = null)
        {
            if (selected == null)
                return position;//.NextLine();

            //var content = label != null ? new GUIContent(label) : new GUIContent(type.GetType().Name);
            var argType = Argument.EnumOf(type);
            var content = new GUIContent(label);
            var rect = position;
            var height = EditorGUI.GetPropertyHeight(selected);

            //if (typeof(GameFuncBase).IsAssignableFrom(info.FieldType))

            if (isDynamic || argType != Argument.ArgType.Reference)
                position = EditorGUI.PrefixLabel(position.With(h: NGUI.LineHeight), content);

            if (isDynamic)
            {
                EditorGUI.LabelField(position, "(dynamic)");
                return position.NextLine();
            }


            if (argType == Argument.ArgType.Null || selected == null)// || !IsSupported(type))
            {
                EditorGUI.LabelField(position, $"not supported");
                return position.NextLine();
            }

            if (type.IsSubclassOf(typeof(Object)))
            {
                var obj = selected.objectReferenceValue;
                if (obj != null && !type.IsAssignableFrom(obj.GetType()))
                {
                    obj = null;
                }

                selected.objectReferenceValue = (Object)EditorGUI.ObjectField(position, obj, type, true);
            }
            else if (type.IsEnum)
            {
                var names = Enum.GetNames(type);
                selected.intValue = EditorGUI.Popup(position, selected.intValue, names);
            }
            else if (type.Equals(typeof(LayerMask)))
            {
                selected.intValue = EditorGUI.MaskField(position, selected.intValue, LayerHelper.GetLayerNames());
            }
            else if (argType == Argument.ArgType.Reference)
            {
                //MEdit.IndentBoxGUI(position.With(h: EditorGUI.GetPropertyHeight(selected)));
                var referenceTypeName = selected.Parent().FindPropertyRelative(nameof(Argument.referenceType)).stringValue;

                var rt = Type.GetType(referenceTypeName);
                PolymorphicDrawer.DrawGUI(position, selected, content, rt);
                //MEdit.EndShadow();
            }
            else if (type.IsValueType || type == typeof(string))
            {
                EditorGUI.PropertyField(position, selected, GUIContent.none);
            }

            //return position.NextLine().With(h: EditorGUI.GetPropertyHeight(selected));
            return rect.NextLine(height);
        }

        static object FieldGUI(Rect position, object value)
        {
            var type = value.GetType();
            switch (type.Name)
            {
                case nameof(Boolean):
                    return EditorGUI.Toggle(position, (bool)value);

                case nameof(String):
                    return EditorGUI.TextField(position, (string)value);

                case nameof(Single):
                    return EditorGUI.FloatField(position, (float)value);

                case nameof(Double):
                    return EditorGUI.DoubleField(position, (double)value);

                case nameof(Int32):
                    return EditorGUI.IntField(position, (int)value);

                case nameof(Vector2):
                    return EditorGUI.Vector2Field(position, "", (Vector2)value);

                case nameof(Vector3):
                    return EditorGUI.Vector3Field(position, "", (Vector3)value);

                case nameof(Vector4):
                    return EditorGUI.Vector4Field(position, "", (Vector4)value);

                case nameof(Color):
                    return EditorGUI.ColorField(position, (Color)value);

                default:
                    break;
            }

            if (type.IsSubclassOf(typeof(Object)))
                return EditorGUI.ObjectField(position, (Object)value, type, true);
            else if (type.IsEnum)
            {
                var options = Enum.GetNames(type);
                var values = Enum.GetValues(type);
                var index = (int)value;

                return EditorGUI.Popup(position, index, options);
            }
            else if (type.IsSerializable || type.IsInterface)
            {

            }

            return null;
        }
        public static SerializedProperty GetSelectedProperty(SerializedProperty property)
        {
            var enumProperty = property.FindPropertyRelative(nameof(Argument.argType));
            var enumValue = (Argument.ArgType)enumProperty.enumValueIndex;
            enumProperty.enumValueIndex = (int)enumValue;
            return property.FindPropertyRelative("arg" + Enum.GetName(typeof(Argument.ArgType), enumValue));
        }
    }
#endif
    #endregion

    [Serializable]
    public abstract class GameMethod : GameCallback, ISerializationCallbackReceiver
    {
        public Argument[] arguments;
        public int[] dynamics;
        private object[] _serializedArgs;
        public object[] serializedArgs => _serializedArgs ??= arguments.Select(arg => arg.value).ToArray();


        // derived
        private MethodInfo _info;
        public MethodInfo info => _info ??= ReflectionHelper.ToMethod(signature);


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
        public PropertyInfo info => _info ??= ReflectionHelper.ToProperty(signature);

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
    [BindingFlags(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.SetProperty | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy)]
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
            var type = ReflectionHelper.GetType(typeName);
            var paramsFull = typeAndMethod[1].TrimEnd(')');

            // Split would create an empty string where it wasn't needed...
            if (!string.IsNullOrWhiteSpace(paramsFull))
            {
                var paramTypes = paramsFull.Split(',').Select(n => ReflectionHelper.GetType(n)).ToArray();
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
                signature = ReflectionHelper.ToSignature(method)
            };
        }

#if UNITY_EDITOR
        [QuickAction] static void Test() => EditorWindow.GetWindow<TestWindow>();
#endif
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(GameMethod), true)]
    public class GameActionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (NGUI.Property(position, label, property))
            {
                var dropdownRect = InvokeButtonGUI(property, position.With(h: NGUI.LineHeight));
                if (HandleDropdownGUI(dropdownRect, property, label))
                {
                    position.y += NGUI.FullLineHeight;
                    HandleArgumentsGUI(position.With(h: NGUI.LineHeight), property);
                }
            }
        }

        object target;
        private Rect InvokeButtonGUI(SerializedProperty property, Rect position)
        {
            target ??= ReflectionHelper.FindReflectionTarget(property, fieldInfo);

            var buttonPosition = position.With(xMin: position.xMax - NGUI.ButtonWidth);
            if (target is GameAction action)
            {
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

            return position.With(xMax: buttonPosition.xMin);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var args = property.FindPropertyRelative(nameof(GameMethod.arguments));
            var count = args.arraySize;
            var height = NGUI.FullLineHeight;

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
            var result = ReflectionHelper.ToMethod(sig.stringValue, out var method);

            if (!result)
            {
                if (sig != null && !sig.stringValue.IsEmpty())
                {
                    ReflectionHelper.ToMethod(sig.stringValue);
                    Debug.LogError($"Invalid method. Does this code still exist? {sig.stringValue}", property.serializedObject.targetObject);
                }
            }

            return method;
        }
        public bool HandleDropdownGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var method = GetMethod(property);
            var content = new GUIContent(GameActionHelper.GetLabelName(method));

            var lbRect = position.With(xMax: position.xMin + EditorGUIUtility.fieldWidth);
            var ddRect = position.With(xMin: lbRect.xMax);

            var isExpandable = method != null && Arguments(property).arraySize > 0;

            if (isExpandable)
                property.isExpanded = EditorGUI.Foldout(lbRect.With(h: NGUI.LineHeight), property.isExpanded, label, true);
            else
                EditorGUI.PrefixLabel(lbRect, label);

            if (EditorGUI.DropdownButton(ddRect.With(h: NGUI.LineHeight), content, FocusType.Passive))
            {
                GameActionHelper.MethodDropdown(fieldInfo, GetMethod(property), _ => Switch(_, property, fieldInfo));
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
                sig.stringValue = ReflectionHelper.ToSignature(_method);// GetSignature(_);


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

                        var assignableTypes = ReflectionHelper.GetAssignableReferenceTypes(methodTypes[a]);
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


    [CustomPropertyDrawer(typeof(GamePropBase), true)]
    public class GamePropDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (NGUI.Property(position, property, label))
            {

                position = position.With(h: NGUI.LineHeight);
                var buttonPosition = position.With(xMin: position.xMax - NGUI.ButtonWidth);
                var dropdownRect = position.With(xMax: buttonPosition.xMin);
                var info = GetProperty(property);



                EditorGUI.BeginDisabledGroup(info == null);
                if (GUI.Button(buttonPosition, "?"))
                {

                    var target = ReflectionHelper.FindReflectionTarget(property, fieldInfo) as GamePropBase;
                    Debug.Log((target.propertyInfo as PropertyInfo).GetValue(target.target));
                }
                EditorGUI.EndDisabledGroup();

                if (HandleDropdownGUI(dropdownRect, property, label))
                {
                    position.y += NGUI.FullLineHeight;

                    HandleTargetGUI(position, property);
                    //HandleArgumentsGUI(position.With(h: MEdit.LineHeight), property);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var targetProperty = property.FindPropertyRelative(nameof(GamePropBase.referenceTarget));
            var height = NGUI.FullLineHeight;

            if (property.isExpanded)
            {
                return height + EditorGUI.GetPropertyHeight(targetProperty);
            }

            return height;
        }

        public static PropertyInfo GetProperty(SerializedProperty property)
        {
            var sig = property.FindPropertyRelative(nameof(GameAction.signature));
            var result = ReflectionHelper.ToProperty(sig.stringValue, out var Property);

            if (!result)
            {
                if (sig != null && !sig.stringValue.IsEmpty())
                {
                    ReflectionHelper.ToProperty(sig.stringValue);
                    Debug.LogError($"Invalid Property. Does this code still exist? {sig.stringValue}", property.serializedObject.targetObject);
                }
            }

            return Property;
        }
        public bool HandleDropdownGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var info = GetProperty(property);
            var content = new GUIContent(GameActionHelper.GetLabelName(info));

            position = position.With(h: NGUI.LineHeight);
            var labelRect = position.With(xMax: position.xMin + EditorGUIUtility.fieldWidth);
            var dropdownRect = position.With(xMax: labelRect.xMin);

            var isExpandable = info != null && !info.GetMethod.IsStatic;// && Arguments(property).arraySize > 0;

            if (isExpandable)
                property.isExpanded = EditorGUI.Foldout(labelRect.With(h: NGUI.LineHeight), property.isExpanded, label, true);
            else
                EditorGUI.PrefixLabel(labelRect, label);

            if (EditorGUI.DropdownButton(dropdownRect.With(h: NGUI.LineHeight), content, FocusType.Passive))
            {
                GameActionHelper.PropertyDropdown(fieldInfo, GetProperty(property), _ => Switch(_, property, fieldInfo));
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
                targetProperty.objectReferenceValue = EditorGUI.ObjectField(position.With(h: NGUI.LineHeight), "target", targetProperty.objectReferenceValue, info.DeclaringType, true);
            }
            else
            {
                var targetProperty = property.FindPropertyRelative(nameof(GamePropBase.referenceTarget));
                PolymorphicDrawer.DrawGUI(position, targetProperty, new GUIContent("target"), info.DeclaringType);
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
            if (info != null && info.GetMethod != null)
            {
                sig.stringValue = ReflectionHelper.ToSignature(info);// GetSignature(_);

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

        public static string GetDisplayPath(MethodInfo info)
        {
            if (info == null)
                return NONE;

            StringBuilder option = new StringBuilder();

            // Append type name
            var tt = info.DeclaringType;
            option.Append($"{info.ModuleName()}/{ReflectionHelper.GetDeclaringString(info.DeclaringType)}.");
            option.Append(info.Name).Append(' ');
            if (info.IsStatic)
                option.Append('*');

            // Append parameter types
            var paramTypes = info.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}");
            option.Append('(').Append(string.Join(",", paramTypes)).Append(')');

            if (info.ReturnType != null)
            {
                option.Append($" => {info.ReturnType.Name}");
            }

            return option.ToString();
        }
        public static string GetDisplayPath(EventInfo info)
        {
            if (info == null)
                return NONE;

            StringBuilder option = new StringBuilder();

            // Append type name
            var tt = info.DeclaringType;
            option.Append($"{info.ModuleName()}/{ReflectionHelper.GetDeclaringString(info.DeclaringType)}.");
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
            option.Append($"{propertyInfo.ModuleName()}/{ReflectionHelper.GetDeclaringString(propertyInfo.DeclaringType)}.{propertyInfo.Name} ");
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
            var properties = ReflectionHelper.RuntimeAssemblies.GetProperties(returnType, flags)
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

            DropdownHelper.Show(onSelect, true, true, GetDisplayPath(selected), properties);
        }
        public static void EventDropdown(FieldInfo field, EventInfo selected, Action<EventInfo> onSelect)
        {
            var flags = BindingFlags.Default;

            if (field.FieldType.TryGetAttribute<BindingFlagsAttribute>(out var f))
                flags |= f.flags;

            if (field.TryGetAttribute<BindingFlagsAttribute>(out var f_attr))
                flags |= f_attr.flags;


            var events = ReflectionHelper.GetRuntimeTypes().GetEvents(flags);

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

            DropdownHelper.Show(onSelect, true, true, GameActionHelper.GetDisplayPath(selected), items);
        }
        public static void MethodDropdown(FieldInfo field, MethodInfo selected, Action<MethodInfo> onSelect)
        {
            var flags = GameAction.FLAGS_M;

            if (field.FieldType.TryGetAttribute<BindingFlagsAttribute>(out var f))
                flags &= f.flags;

            if (field.TryGetAttribute<BindingFlagsAttribute>(out var f_attr))
                flags |= f_attr.flags;


            var methods = ReflectionHelper.GetRuntimeTypes().GetMethods(flags)
                .Where(m => !m.ContainsGenericParameters && (m.IsStatic || m.ReflectedType.IsSerializable) && !m.DeclaringType.ContainsGenericParameters
                && !m.GetParameters().Any(p => p.ParameterType.IsGenericType || p.IsOut || p.IsRetval || p.IsLcid || p.IsIn || !p.ParameterType.IsSerializable || p.ParameterType.IsByRef || p.ParameterType.IsArray || typeof(Delegate).IsAssignableFrom(p.ParameterType)));

            if (methods.Count() == 0)
            {
                Debug.LogError("No methods found!");
                return;
            }

            var gs = field.FieldType.GetGenericArguments().ToList();

            if (typeof(GameAction).Equals(field.FieldType))
            {
                methods = methods.Where(m => m.ReturnType.Equals(typeof(void)));
            }

            // GameFunc requires first generic argument to be the return type
            else if (typeof(GameFuncBase).IsAssignableFrom(field.FieldType))
            {
                var returnType = gs[0];
                methods = methods.Where(m => returnType.IsAssignableFrom(m.ReturnType));
                gs.RemoveAt(0);
            }
            if (gs.Count > 0)
            {
                methods = methods.Where(m => m.ContainsTargetableParameter(gs.ToArray()));
            }
            var items = methods.Select(method => (method, GetDisplayPath(method)))
                .Where(_ => !_.Item2.Contains("&"))
                .ToArray();

            DropdownHelper.Show(onSelect, true, true, GetDisplayPath(selected), items);
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