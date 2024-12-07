using System;
using UnityEngine;


namespace Neeto
{
    public struct Preference<T>
    {
        static string key => typeof(T).ToString();
        static bool hasJson => PlayerPrefs.HasKey(key);
        static string json
        {
            get => PlayerPrefs.GetString(key);
            set => PlayerPrefs.SetString(key, value);
        }

        public T Read() => JsonUtility.FromJson<T>(json);
        public void Write(T value) => json = JsonUtility.ToJson(value);
        public static bool TryRead(out T result)
        {
            if (hasJson && JsonUtility.FromJson(json, typeof(T)) is T Result)
            {
                result = Result;
                return true;
            }

            result = default;
            return false;
        }
    }

    public struct FloatPreference
    {
        public static implicit operator FloatPreference(string key) => new (key);
        public static implicit operator float(FloatPreference fp) => fp.GetValue();
        readonly string key;
        public FloatPreference(string key) => this.key = key;
        public float GetValue() => PlayerPrefs.GetFloat(key);
        public void SetValue(float value) => PlayerPrefs.SetFloat(key, value);
    }
    public struct BoolPreference
    {
        public static implicit operator BoolPreference(string key) => new(key);
        public static implicit operator bool(BoolPreference fp) => fp.GetValue();
        readonly string key;
        public BoolPreference(string key) => this.key = key;
        public bool GetValue() => PlayerPrefs.GetInt(key) == 1;
        public void SetValue(bool value) => PlayerPrefs.SetInt(key, value ? 1 : 0);
    }
}