using UnityEngine;

#if UNITY_EDITOR
#endif


namespace Neeto
{
    using System;

#if UNITY_EDITOR
    using UnityEditor;
    [CustomEditor(typeof(MultiScene))]
    public class MultiSceneEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var _target = target as MultiScene;

            if (GUILayout.Button("Load"))
            {
                _target.LoadScenes();
            }
        }
    }
#endif


    [ExecuteInEditMode, DefaultExecutionOrder(-10)]
    public class MultiScene : MonoBehaviour
    {
        public bool loadOnEnable = true;
        public bool loadAtRuntime = true;

        [SerializeReference, Polymorphic]
        public ILoadAsync[] load;

        bool hasAppContext => (loadOnEnable && !Application.isPlaying) || (loadAtRuntime && Application.isPlaying);

        void Start()
        {
            if (hasAppContext)
                LoadScenes();
        }
        public async void LoadScenes()
        {
            Debug.Log($"Loading scenes...", this);
            foreach (var task in load)
            {
                await task.LoadAsync();
            }
        }
    }
}