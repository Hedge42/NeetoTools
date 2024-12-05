using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
#endif

namespace Neeto
{
    [CreateAssetMenu(menuName = Menu.Main + nameof(ScriptableLoadingTask))]
    public class ScriptableLoadingTask : ScriptableObject
    {
        public LoadingTask task;

        [ContextMenu(nameof(Load))]
        public void Load()
        {
            task.Load();
        }
    }
}