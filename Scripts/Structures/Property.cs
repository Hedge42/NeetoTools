using System;

#if UNITY_EDITOR
#endif

namespace Neeto
{
    public struct Property<T>
    {
        public static implicit operator T(Property<T> prop) => prop.Value;
        //public static explicit operator T(Property<T> prop) => prop.Value;

        Func<T> _get;
        Action<T> _set;

        public T Value
        {
            get => _get();
            set => _set(value);
        }

        public Property(Func<T> getter, Action<T> setter)
        {
            _get = getter;
            _set = setter;
        }
    }
}