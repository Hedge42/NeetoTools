using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
#endif

namespace Neeto
{
    [CreateAssetMenu(menuName = Menu.Neeto + nameof(ScriptableLoadingTask))]
    public class ScriptableLoadingTask : ScriptableObject
    {
        public LoadingTask task;

        [Button]
        public void Load()
        {
            task.Load();
        }
    }
}