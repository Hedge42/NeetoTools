using UnityEngine;
using System;


namespace Neeto
{
    public struct Lazy<T>
    {
        struct FailState { }

        public static implicit operator T(Lazy<T> _) => _.GetValue();

        object _value;
        public T value => GetValue();
        public T GetValue() => (T)(_value ??= factory());

        Func<T> factory;
        public Lazy(Func<T> factory)
        {
            this.factory = factory;
            _value = null;
        }

        
    }
}