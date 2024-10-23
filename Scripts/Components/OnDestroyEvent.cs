using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Neeto
{

    public class OnDestroyEvent : MonoBehaviour
    {
        public UnityEvent onDestroy;

        private void OnDestroy()
        {
            onDestroy?.Invoke();
        }
    }
    public static class OnDestroyHelper
    {
        public static void OnDestroyAddListener(this GameObject gameObject, Action action)
        {
            gameObject?.GetOrAddComponent<OnDestroyEvent>().onDestroy.AddListener(action.Invoke);
        }
    }

}