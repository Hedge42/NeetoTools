using UnityEngine;
using UnityEngine.Events;

namespace Neeto
{
    public class EventMonitor : MonoBehaviour
    {
        public SerializedEvent Event;
        public UnityEvent output;

        void OnEnable()
        {
            Event.AddListener(output.Invoke);
        }
        void OnDisable()
        {
            Event.RemoveListener(output.Invoke);
        }
    }
}
