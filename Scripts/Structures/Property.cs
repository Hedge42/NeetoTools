using System;

#if UNITY_EDITOR
#endif

namespace Neeto
{
    public struct Property<T>
    {
        public static implicit operator T(Property<T> prop) => prop.Value;
        public static Property<T> operator +(Property<T> prop, Callback<T> action)
        {
            prop._set += action;
            return prop;
        }
        Func<T> _get;
        Action<T> _set;
        public T Value
        {
            get => _get();
            set => _set(value);
        }
        public Property(Func<T> get, Action<T> set)
        {
            _get = get; _set = set;
        }
    }
}