using UnityEngine;
using UnityEngine.Events;

namespace Neeto
{
    public class OnDestroyEvent : MonoBehaviour
    {
        public UnityEvent onDestroy = new(); // without new() AddComponent does not automatically create it


        private void OnDestroy()
        {
            onDestroy?.Invoke();
        }
    }
}