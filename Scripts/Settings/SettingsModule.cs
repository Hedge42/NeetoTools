using UnityEngine;
using System;


namespace Neeto
{
    [Serializable]
    public abstract class SettingsModule
    {
        string key => GetType().Name;
        public abstract void SetDefaults();
        public abstract void Apply();
        public void Save() => PlayerPrefs.SetString(key, JsonUtility.ToJson(this));
        public static T Load<T>() where T : SettingsModule, new()
        {
            T result = null;
            try
            {
                var json = PlayerPrefs.GetString(typeof(T).Name, "");
                if (!string.IsNullOrEmpty(json))
                {
                    result = JsonUtility.FromJson<T>(json);
                }
            }
            finally
            {
                if (result == null)
                {
                    result = new();
                    result.SetDefaults();
                }
                result.Apply();
            }
            return result;
        }
    }
}