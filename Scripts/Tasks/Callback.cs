using System;

namespace Neeto
{
    public struct Callback
    {
        public delegate void CallbackHandler();

        event Action Event;
        public void Invoke() => Event?.Invoke();
        public static Callback operator ~(Callback callback)
        {
            callback.Event = null;
            return callback;
        }
        public static Callback operator +(Callback callback, Action action)
        {
            callback.Event += action;
            return callback;
        }
        public static Callback operator -(Callback callback, Action action)
        {
            callback.Event -= action;
            return callback;
        }

        public static implicit operator Action(Callback callback) => callback.Invoke;
        public static implicit operator Callback(Action action) => new Callback() + action;
        public static Callback Create(params Action[] actions)
        {
            var cb = new Callback();
            foreach (var del in actions)
                cb += del;
            return cb;
        }
    }

    /// <summary>
    /// useful because you can subscribe delegates with or without an argument...
    /// </summary>
    public struct Callback<T>
    {
        event Action<T> EventT;
        event Action Event;

        public void Invoke(T arg)
        {
            EventT?.Invoke(arg);
            Event?.Invoke();
        }
        public void Clear()
        {
            EventT = null;
            Event = null;
        }
        public Callback<T> With(params Callback<T>[] callbacks)
        {
            foreach (var cb in callbacks)
                this += cb;
            return this;
        }
        public Callback<T> Without(params Callback<T>[] callbacks)
        {
            foreach (var _ in callbacks)
                this -= _;
            return this;
        }

        public static Callback<T> operator ~(Callback<T> callback)
        {
            callback.EventT = null;
            callback.Event = null;
            return callback;
        }
        public static Callback<T> operator +(Callback<T> callback, Action action)
        {
            callback.Event += action;
            return callback;
        }
        public static Callback<T> operator -(Callback<T> callback, Action action)
        {
            callback.Event -= action;
            return callback;
        }
        public static Callback<T> operator +(Callback<T> callback, Action<T> action)
        {
            callback.EventT += action;
            return callback;
        }
        public static Callback<T> operator -(Callback<T> callback, Action<T> action)
        {
            callback.EventT -= action;
            return callback;
        }
        public static Callback<T> operator +(Callback<T> cb, Callback<T>[] callbacks)
        {
            foreach (var _ in callbacks)
            {
                cb += _;
            }
            return cb;
        }
        public static Callback<T> operator -(Callback<T> cb, Callback<T>[] callbacks)
        {
            foreach (var _ in callbacks)
            {
                cb += _;
            }
            return cb;
        }

        public static implicit operator Action<T>(Callback<T> callback) => callback.Invoke;
        public static implicit operator Callback<T>(Action action) => new Callback<T>() + action;
        public static implicit operator Callback<T>(Action<T> action) => new Callback<T>() + action;

        public static Callback<T> Create(params Callback<T>[] dels) => new Callback<T>().With(dels);
    }
}
