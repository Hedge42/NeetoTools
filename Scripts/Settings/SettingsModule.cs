using UnityEngine;
using System;
using SolidUtilities.UnityEngineInternals;
using System.Reflection;
using System.CodeDom;

namespace Neeto
{
    [Serializable]
    public abstract class SettingsModule
    {
        public abstract void SetDefaults();
        public abstract void Apply();

        public void Save() => PlayerPrefs.SetString(GetType().Name, JsonUtility.ToJson(this));
        public static void Load<T>(ref T value) where T : SettingsModule, new()
        {
            if (Preference<T>.TryRead(out var result))
            {
                value = result;
            }
            else
            {
                value = new();
                value.SetDefaults();
            }
            value.Apply();
        }
    }
}