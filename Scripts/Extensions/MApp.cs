using UnityEngine;
using System;
using System.Collections;
using Object = UnityEngine.Object;
using UnityEditor;
using Cysharp.Threading.Tasks;

public static class MApp
{
    public static event Action onQuit;
    public static bool isQuitting { get; private set; }

    [RuntimeInitializeOnLoadMethod]
    static void Setup()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += _ =>
        {
            if (_ == PlayModeStateChange.ExitingPlayMode)
            {
                OnQuit();
            }
        };
#endif
        Application.quitting += OnQuit;
    }
    static void OnQuit()
    {
        Debug.Log("quitting...");
        isQuitting = true;
        onQuit?.Invoke();
        Application.quitting -= OnQuit;
        onQuit = null;
    }

    public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public static void OnAfterSerialization(Action callback)
    {
        bool flag = false;
#if UNITY_EDITOR
        AfterSerializationAsync(callback)
            .Forget();
        flag = true;
#endif
        if (!flag)
            callback();
    }
#if UNITY_EDITOR
    public static bool IsSerializing()
    {
        return EditorApplication.isPlayingOrWillChangePlaymode && EditorApplication.isUpdating;
    }
    public static async UniTask AfterSerializationAsync(Action cb)
    {
        await UniTask.WaitWhile(IsSerializing);
        cb();
    }
#endif
}
