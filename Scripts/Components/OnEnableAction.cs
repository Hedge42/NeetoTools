using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnEnableAction : MonoBehaviour
{
    public UnityEvent onEnable;

    private void OnEnable()
    {
        onEnable?.Invoke();
    }
}
public static partial class ObjectExtensions
{
    public static void OnEnableAddListener(this GameObject gameObject, Action action)
    {
        gameObject?.GetOrAddComponent<OnEnableAction>().onEnable.AddListener(action.Invoke);
    }
}
