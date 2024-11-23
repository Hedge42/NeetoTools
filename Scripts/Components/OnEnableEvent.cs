using System;
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
    public static class OnEnableHelper
    {
        public static void OnEnableAddListener(this GameObject gameObject, Action action)
        {
            gameObject?.GetOrAddComponent<OnEnableEvent>().onEnable.AddListener(action.Invoke);
        }
    }
}