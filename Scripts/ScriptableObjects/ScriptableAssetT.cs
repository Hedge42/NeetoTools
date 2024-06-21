using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Matchwork
{
    [Serializable]
    public class ScriptableAsset<T>
    {
        [Inspect]
        public ScriptableAsset asset;

        public T value
        {
            get => (T)asset?.data;
            set => asset.data = value;
        }
    }
}