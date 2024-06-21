using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnDestroyAction : MonoBehaviour
{
    public UnityEvent onDestroy;

    private void OnDestroy()
    {
        onDestroy?.Invoke();
    }
}
public static partial class ObjectExtensions
{
    public static void OnDestroyAddListener(this GameObject gameObject, Action action)
    {
        gameObject?.GetOrAddComponent<OnDestroyAction>().onDestroy.AddListener(action.Invoke);
    }
}
