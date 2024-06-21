using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using System.Linq.Expressions;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Matchwork;

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

        var methods = Module.ALL.GetTypes()
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

        var types = Module.ALL.GetTypes()
            .Where(t => t.IsPublic && !t.IsEnum);
        var props = types.SelectMany(t => t.GetProperties(FLAGS_P))
            .Where(p => p.PropertyType.Equals(propType));

        return props;
    }
    public static IEnumerable<EventInfo> GetEvents(FieldInfo info)
    {
        var generics = info.FieldType.GetGenericArguments().ToList();

        var types = Module.ALL.GetTypes()
            .Where(t => t.IsPublic && !t.IsEnum);

        var filter = info.GetCustomAttribute<TypeFilterAttribute>();

        types = TypeFilterAttribute.Evaluate(info, types);


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

        var list = MReflect.GetParameterTypes(method).ToList();
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

[Serializable]
public abstract class GameMethod : GameCallback, ISerializationCallbackReceiver
{
    public Argument[] arguments;
    public int[] dynamics;
    private object[] _serializedArgs;
    public object[] serializedArgs => _serializedArgs ??= arguments.Select(arg => arg.value).ToArray();


    // derived
    private MethodInfo _info;
    public MethodInfo info => _info ??= MReflect.ToMethod(signature);


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
    public PropertyInfo info => _info ??= MReflect.ToProperty(signature);

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
        var type = MReflect.GetType(typeName);
        var paramsFull = typeAndMethod[1].TrimEnd(')');

        // Split would create an empty string where it wasn't needed...
        if (!string.IsNullOrWhiteSpace(paramsFull))
        {
            var paramTypes = paramsFull.Split(',').Select(n => MReflect.GetType(n)).ToArray();
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
            signature = MReflect.ToSignature(method)
        };
    }
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


public static class GameActionExtensions
{
    public static void Invoke(this GameAction[] actions)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            Debug.Log("Invoking " + actions[i].signature);
            actions[i].Invoke();
        }
    }
}
