using Rhinox.Lightspeed.Reflection;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Neeto
{
    [Serializable]
    public abstract class SerializedMember : ISerializationCallbackReceiver
    {
        public UnityEngine.Object owner;
        public UnityEngine.Object target;
        [SerializeField] public string DeclaringType;
        [SerializeField] public string MemberName;

        public abstract MemberInfo GetMember();
        public virtual bool NeedsTarget() => GetMember() is MemberInfo info && !info.IsStatic();

        public void OnAfterDeserialize()
        {
            if (!DeclaringType.Equals(""))
            {
                var type = Type.GetType(DeclaringType);
                if (type == null)
                {
                    Debug.LogError($"Type not found. Did the code change? '{DeclaringType}'", owner);
                    return;
                }

                var member = GetMember();
                if (member == null)
                {
                    Debug.LogError($"Member not found. Did the code change? '{MemberName}'", owner);
                    return;
                }
            }
        }
        public void OnBeforeSerialize() { }
    }

    /// <summary>
    /// Allows selecting a method (Action) in the inspector and invoking it.
    /// </summary>
    [Serializable]
    public class SerializedAction : SerializedMember
    {
        private Action _methodDelegate;
        private MethodInfo _methodInfo;

        public override MemberInfo GetMember()
        {
            try
            {
                return Type.GetType(DeclaringType)?.GetMethod(MemberName);
            }
            catch
            {
                return null;
            }
        }

        public void Invoke()
        {
            if (_methodDelegate == null)
            {
                _methodDelegate = InitializeDelegate();
                if (_methodDelegate == null) return;
            }

            _methodDelegate.Invoke();
        }

        private Action InitializeDelegate()
        {
            try
            {
                _methodInfo ??= GetMember() as MethodInfo;
                if (_methodInfo == null)
                {
                    Debug.LogError($"Failed to get MethodInfo from '{DeclaringType}.{MemberName}'");
                    return null;
                }

                var targetInstance = Expression.Constant(target);
                var methodCall = Expression.Call(targetInstance, _methodInfo);
                var methodExpression = Expression.Lambda<Action>(methodCall);
                return methodExpression.Compile();
            }
            catch
            {
                Debug.LogError($"Something went wrong in '{DeclaringType}.{MemberName}'");
                return null;
            }
        }
    }

    /// <summary>
    /// Allows selecting a method (Action<T>) in the inspector and invoking it with a parameter.
    /// </summary>
    [Serializable]
    public class SerializedAction<T> : SerializedMember
    {
        private Action<T> _methodDelegate;
        private MethodInfo _methodInfo;

        public override bool NeedsTarget()
        {
            return GetMember() is MethodInfo info && !info.IsStatic;
        }

        public void Invoke(T arg)
        {
            (_methodDelegate ??= InitializeDelegate())?.Invoke(arg);
        }

        public override MemberInfo GetMember()
        {
            try
            {
                return Type.GetType(DeclaringType)?.GetMethod(MemberName);
            }
            catch
            {
                return null;
            }
        }

        private Action<T> InitializeDelegate()
        {
            try
            {
                _methodInfo ??= GetMember() as MethodInfo;
                if (_methodInfo == null)
                {
                    Debug.LogError($"Failed to get MethodInfo from '{DeclaringType}.{MemberName}'");
                    return null;
                }

                var targetInstance = Expression.Constant(target);
                var parameter = Expression.Parameter(typeof(T), "arg");
                var methodCall = Expression.Call(targetInstance, _methodInfo, parameter);
                var methodExpression = Expression.Lambda<Action<T>>(methodCall, parameter);
                return methodExpression.Compile();
            }
            catch
            {
                Debug.LogError($"Something went wrong in '{DeclaringType}.{MemberName}'");
                return null;
            }
        }
    }

    /// <summary>
    /// Like a UnityEvent, but useful for getting values.
    /// Properties requires accessible getter and setter.
    /// </summary>
    [Serializable]
    public class SerializedProperty<T> : SerializedMember
    {
        public T Value
        {
            get => (_get ??= InitializeGetter())();
            set => (_set ??= InitializeSetter())(value);
        }

        Func<T> _get;
        Action<T> _set;
        PropertyInfo _info;

        public override MemberInfo GetMember()
        {
            try
            {
                return Type.GetType(DeclaringType).GetProperty(MemberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to get PropertyInfo from '{DeclaringType}.{MemberName}'");
                return null;
            }
        }

        Func<T> InitializeGetter()
        {
            try
            {
                _info ??= GetMember() as PropertyInfo;
                var targetInstance = Expression.Constant(target);
                var propertyExpression = Expression.Property(targetInstance, _info);
                var getExpression = Expression.Lambda<Func<T>>(propertyExpression);
                return getExpression.Compile();
            }
            catch
            {
                Debug.LogError($"Something went wrong in '{DeclaringType}.{MemberName}'");
                return () => default;
            }
        }
        Action<T> InitializeSetter()
        {
            Action<T> Default = _ => { };
            try
            {
                _info ??= GetMember() as PropertyInfo;
                if (_info == null)
                {
                    Debug.LogError($"Failed to get PropertyInfo from '{DeclaringType}.{MemberName}'");
                    return Default;
                }

                if (!_info.CanWrite)
                {
                    Debug.LogWarning($"Property '{_info.NameOr("NULL")}' does not have a setter.");
                    return Default;
                }

                var targetInstance = Expression.Constant(target);
                var valueParameter = Expression.Parameter(typeof(T), "value");
                var propertyExpression = Expression.Property(targetInstance, _info);
                var assignExpression = Expression.Assign(propertyExpression, valueParameter);
                var setExpression = Expression.Lambda<Action<T>>(assignExpression, valueParameter);
                return setExpression.Compile();
            }
            catch
            {
                Debug.LogError($"Something went wrong in '{DeclaringType}.{MemberName}'");
                return Default;
            }
        }
    }

    /// <summary>
    /// Allows selecting event Action in the inspector
    /// </summary>
    [Serializable]
    public class SerializedEvent : SerializedMember
    {
        private Action<Action> _addHandler;
        private Action<Action> _removeHandler;
        private EventInfo _info;

        public override bool NeedsTarget()
        {
            return GetMember() is EventInfo info && !info.AddMethod.IsStatic;
        }

        public void AddListener(Action handler)
        {
            (_addHandler ??= InitializeAddHandler())?.Invoke(handler);
        }
        public void RemoveListener(Action handler)
        {
            (_removeHandler ??= InitializeRemoveHandler())?.Invoke(handler);
        }

        public override MemberInfo GetMember()
        {
            try
            {
                return Type.GetType(DeclaringType)?.GetEvent(MemberName);
            }
            catch
            {
                return null;
            }
        }

        private Action<Action> InitializeAddHandler()
        {
            try
            {
                _info ??= GetMember() as EventInfo;
                if (_info == null)
                {
                    Debug.LogError($"Failed to get EventInfo from '{DeclaringType}.{MemberName}'");
                    return null;
                }

                var targetInstance = Expression.Constant(target);
                var handlerParameter = Expression.Parameter(typeof(Action), "handler");
                var addMethod = _info.GetAddMethod();
                var addCall = Expression.Call(targetInstance, addMethod, handlerParameter);
                var addExpression = Expression.Lambda<Action<Action>>(addCall, handlerParameter);
                return addExpression.Compile();
            }
            catch
            {
                Debug.LogError($"Something went wrong in '{DeclaringType}.{MemberName}'");
                return null;
            }
        }

        private Action<Action> InitializeRemoveHandler()
        {
            try
            {
                _info ??= GetMember() as EventInfo;
                if (_info == null)
                {
                    Debug.LogError($"Failed to get EventInfo from '{DeclaringType}.{MemberName}'");
                    return null;
                }

                var targetInstance = Expression.Constant(target);
                var handlerParameter = Expression.Parameter(typeof(Action), "handler");
                var removeMethod = _info.GetRemoveMethod();
                var removeCall = Expression.Call(targetInstance, removeMethod, handlerParameter);
                var removeExpression = Expression.Lambda<Action<Action>>(removeCall, handlerParameter);
                return removeExpression.Compile();
            }
            catch
            {
                Debug.LogError($"Something went wrong in '{DeclaringType}.{MemberName}'");
                return null;
            }
        }
    }

    /// <summary>
    /// Allows selecting event Action<T> in the inspector
    /// </summary>
    [Serializable]
    public class SerializedEvent<T> : SerializedMember
    {
        private Action<Action<T>> _addHandler;
        private Action<Action<T>> _removeHandler;
        private EventInfo _info;

        public override bool NeedsTarget()
        {
            return GetMember() is EventInfo info && !info.AddMethod.IsStatic;
        }

        public void AddListener(Action<T> handler)
        {
            (_addHandler ??= InitializeAddHandler())?.Invoke(handler);
        }

        public void RemoveListener(Action<T> handler)
        {
            (_removeHandler ??= InitializeRemoveHandler())?.Invoke(handler);
        }

        public override MemberInfo GetMember()
        {
            try
            {
                return Type.GetType(DeclaringType)?.GetEvent(MemberName);
            }
            catch
            {
                return null;
            }
        }

        private Action<Action<T>> InitializeAddHandler()
        {
            try
            {
                _info ??= GetMember() as EventInfo;
                if (_info == null)
                {
                    Debug.LogError($"Failed to get EventInfo from '{DeclaringType}.{MemberName}'");
                    return null;
                }

                var targetInstance = Expression.Constant(target);
                var handlerParameter = Expression.Parameter(typeof(Action<T>), "handler");
                var addMethod = _info.GetAddMethod();
                var addCall = Expression.Call(targetInstance, addMethod, handlerParameter);
                var addExpression = Expression.Lambda<Action<Action<T>>>(addCall, handlerParameter);
                return addExpression.Compile();
            }
            catch
            {
                Debug.LogError($"Something went wrong in '{DeclaringType}.{MemberName}'");
                return null;
            }
        }

        private Action<Action<T>> InitializeRemoveHandler()
        {
            try
            {
                _info ??= GetMember() as EventInfo;
                if (_info == null)
                {
                    Debug.LogError($"Failed to get EventInfo from '{DeclaringType}.{MemberName}'");
                    return null;
                }

                var targetInstance = Expression.Constant(target);
                var handlerParameter = Expression.Parameter(typeof(Action<T>), "handler");
                var removeMethod = _info.GetRemoveMethod();
                var removeCall = Expression.Call(targetInstance, removeMethod, handlerParameter);
                var removeExpression = Expression.Lambda<Action<Action<T>>>(removeCall, handlerParameter);
                return removeExpression.Compile();
            }
            catch
            {
                Debug.LogError($"Something went wrong in '{DeclaringType}.{MemberName}'");
                return null;
            }
        }
    }
}
