using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Neeto
{
    public class GameObjectSync : MonoBehaviour
    {
        [Note("Activate the GameObjects in OnEnable, Deactivate them in OnDisable")]
        [ReorderableList(ListStyle.Lined)]
        public GameObject[] GameObjects;

        void OnEnable()
        {
            foreach (var gameObject in GameObjects)
            {
                gameObject.SetActive(true);
            }
        }
        void OnDisable()
        {
            foreach (var gameObject in GameObjects)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
