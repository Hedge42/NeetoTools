using UnityEngine;
using System;


namespace Neeto
{
    public struct Lazy<T> where T : class
    {
        public static implicit operator T(Lazy<T> _) => _.value ??= _.factory();

        public T value { get; private set; }
        Func<T> factory;
        public Lazy(Func<T> factory)
        {
            this.factory = factory;
            value = null;
        }
    }
}