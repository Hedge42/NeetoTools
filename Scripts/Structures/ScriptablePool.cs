using System;
using System.Collections.Generic;
using UnityEngine;

namespace Neeto
{
    [CreateAssetMenu(menuName = Neeto.Menu.Main + nameof(ScriptablePool))]
    public class ScriptablePool : ScriptableObject
    {
        [SerializeReference, Polymorphic]
        public IPool pool = new BasicPool();
    }

    public interface IPool
    {
        GameObject Pop();
        void Push(GameObject instance);
    }

    [Serializable]
    [Script(typeof(ScriptablePool))]
    public class BasicPool : IPool
    {
        [Exposed] public GameObject prefab;

        public bool autoEnable;
        public bool autoReturn = true;
        public float returnAfter = 2f;

        [Min(1)] public int poolSize = 10;

        List<GameObject> pool = new();

        public virtual GameObject Pop()
        {
            if (!pool.TryPop(out var instance))
            {
                instance = GameObject.Instantiate(prefab);
                instance.OnDestroy(() => pool.Remove(instance));
            }

            instance.SetActive(autoEnable);

            if (autoReturn)
                Delay.Void(returnAfter, Token.scene, () => Push(instance));

            return instance;
        }
        public virtual void Push(GameObject obj)
        {
            obj.SetActive(false);
            pool.Add(obj);
        }
    }
}