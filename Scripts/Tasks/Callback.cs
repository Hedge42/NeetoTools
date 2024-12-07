using System;
using System.Collections;
using System.Collections.Generic;

namespace Neeto
{
    /// <summary>
    /// useful because you can subscribe delegates with or without an argument...
    /// </summary>
    public struct Callback<T>
    {
        public static implicit operator Action<T>(Callback<T> callback) => callback.Invoke;
        event Action<T> EventT;
        event Action Event;

        public void Invoke(T arg) { EventT?.Invoke(arg); Event?.Invoke(); }
        public void Clear() { EventT = null; Event = null; }
        public void AddListener(Action action) => Event += action;
        public void AddListener(Action<T> action) => EventT += action;
        public void RemoveListener(Action action) => Event -= action;
        public void RemoveListener(Action<T> action) => EventT -= action;
    }
}
