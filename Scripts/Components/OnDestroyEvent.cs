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

    }

}