using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Matchwork
{
    #region EDITOR
#if UNITY_EDITOR
    //	[CustomEditor(typeof(ScriptableJson))]
    public class ScriptableJsonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
#endif
    #endregion

    [CreateAssetMenu(menuName = MPath.Main + nameof(ScriptableAsset))]
    public class ScriptableAsset : ScriptableObject
    {
        [SerializeReference, Polymorphic]
        public object data;

        [MenuItem(MPath.CreateAsset + nameof(ScriptableAsset))]
        static void Create()
        {
            MEdit.CreateAssetDialogue<ScriptableAsset>();
        }
    }

    public interface IAssetSource
    {
        public object GetValue();
    }
    [Serializable]
    public class JsonSource
    {
        public string path;
        public object value;
    }

    [Serializable]
    public class ValueSource : IAssetSource
    {
        [SerializeReference, Polymorphic]
        public object value;
        public object GetValue()
        {
            return value;
        }
    }
    [Serializable]
    public class AddressableSource : IAssetSource
    {
        public UnityEngine.AddressableAssets.AssetReference asset;

        public object GetValue()
        {
            return asset;
        }
    }
    [Serializable]
    public class ReferenceSource : IAssetSource
    {
        public Object objectReference;
        public object GetValue()
        {
            return objectReference;
        }
    }
}