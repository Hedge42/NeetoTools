using UnityEngine;

namespace Neeto
{
    public class LoadingScreenSetting : SingletonObject<LoadingScreenSetting>
    {
        [field: SerializeField] public SceneReference scene { get; protected set; }
    }
}
