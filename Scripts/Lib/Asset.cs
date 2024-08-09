using Object = UnityEngine.Object;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Neeto
{
    public struct Asset<T> : IAsset<T> where T : Object
    {
        public string primaryKey { get; }
        public string assetPath { get; }

        private T _cached;
        private int _referenceCount;
        private AsyncOperationHandle<T> _handle;

        public Asset(string primaryKey, string assetPath = "")
        {
            this.primaryKey = primaryKey;
            this.assetPath = assetPath;
            _cached = default;
            _referenceCount = 0;
            _handle = default;
        }

        private void Retain()
        {
            _referenceCount++;
        }

        public void Release()
        {
            if (_referenceCount > 0)
            {
                _referenceCount--;
                if (_referenceCount == 0)
                {
                    Addressables.Release(_handle);
                    _cached = default;
                    _handle = default;
                }
            }
        }

        public T Load()
        {
            if (_cached != null)
            {
                Retain();
                return _cached;
            }

            _handle = Addressables.LoadAssetAsync<T>(primaryKey);
            _cached = _handle.WaitForCompletion();
            Retain();
            return _cached;
        }

        public async Task<T> LoadAsync()
        {
            if (_cached != null)
            {
                Retain();
                return _cached;
            }

            _handle = Addressables.LoadAssetAsync<T>(primaryKey);
            _cached = await _handle.Task;
            Retain();
            return _cached;
        }
    }

    public interface IAsset
    {
        public string primaryKey { get; }
    }
    public interface IAsset<T> : IAsset // : IAsset<T> where T : Object
    {
        public T Load();
    }
    public struct AddressableSceneAsset : IAsset<SceneInstance>
    {
        public string primaryKey { get; }
        public string assetPath { get; }

        public AddressableSceneAsset(string _primaryKey, string _assetPath = "")
        {
            primaryKey = _primaryKey;
            assetPath = _assetPath;
        }

        public SceneInstance Load()
        {
            return Addressables.LoadSceneAsync(assetPath, UnityEngine.SceneManagement.LoadSceneMode.Additive).WaitForCompletion();
        }
    }
}