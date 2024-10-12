using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Neeto
{
    [CreateAssetMenu(menuName = MENU.Neeto + nameof(ScriptableLoadingTask))]
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