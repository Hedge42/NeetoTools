using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Neeto
{
    [Serializable]
    public struct Resource<T> where T : Object
    {
        public string resourcePath;
        public T asset;

        public Resource(string resourcePath)
        {
            this.resourcePath = resourcePath;
            this.asset = null;
        }

        public T Load()
        {
            return asset ??= Resources.Load(resourcePath) as T;
        }
    }
}
