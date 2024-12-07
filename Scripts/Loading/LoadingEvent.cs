using UnityEngine;

namespace Neeto
{
    public class LoadingEvent : MonoBehaviour
    {
        public bool loadOnEnable;
        public LoadingTask task;

        private void OnEnable()
        {
            if (loadOnEnable)
                Load();
        }

        public void Load()
        {
            if (!enabled || task == null)
            {
                Debug.LogError($"'{name}' failed to load", this);
                return;
            }

            task.Load();
        }
    }
}