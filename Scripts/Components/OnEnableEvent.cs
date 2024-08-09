using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Neeto
{
    public class OnEnableEvent : MonoBehaviour
    {
        public UnityEvent onEnable;

        private void OnEnable()
        {
            onEnable?.Invoke();
        }
    }
    public static partial class NObject
    {
        public static void OnEnableAddListener(this GameObject gameObject, Action action)
        {
            gameObject?.GetOrAddComponent<OnEnableEvent>().onEnable.AddListener(action.Invoke);
        }
    }
}