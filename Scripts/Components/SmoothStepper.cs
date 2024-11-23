using UnityEngine;
using UnityEngine.Events;

namespace Neeto
{
    public class SmoothStepper : MonoBehaviour
    {
        public float upSpeed = 1f;
        public float downSpeed = 1f;

        public float targetValue { get; set; }
        public float value { get; private set; }

        public UnityEvent<float> output;

        void OnEnable()
        {
            value = targetValue;
        }
        void Update()
        {
            var maxDelta = (targetValue > value ? upSpeed : downSpeed) * Time.deltaTime;
            value = Mathf.MoveTowards(value, targetValue, maxDelta);

            output?.Invoke(value);
        }
    }
}
