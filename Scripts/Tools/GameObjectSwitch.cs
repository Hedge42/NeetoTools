using UnityEngine;

namespace Neeto
{
    public class GameObjectSwitch : MonoBehaviour
    {
        public bool suppressWarning;
        public GameObject Default;


        [Note("call Switch(gameObject) and all other objects will be disabled")]
        [ReorderableList(ListStyle.Lined)]
        public GameObject[] GameObjects;

        public void Switch(GameObject GameObject)
        {
            foreach (var gameObject in GameObjects)
            {
                gameObject.SetActive(gameObject == GameObject);
            }

            if (!GameObject.activeSelf)
            {
                GameObject.SetActive(true);
                Debug.LogWarning($"GameObject '{GameObject.name}' not a part of the list can cannot be disabled by '{name}'. Enable suppressWarning if this was intentional.", GameObject);
            }
        }
    }
}
