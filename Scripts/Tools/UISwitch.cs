using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Neeto
{
    [DefaultExecutionOrder(6969)]
    public class UISwitch : MonoBehaviour
    {
        public bool suppressWarning;
        public UIDocument Default;


        [Note("call Switch(ui) and all other objects will be disabled")]
        [ReorderableList(ListStyle.Lined)]
        public UIDocument[] Docs;

        void Start()
        {
            if (Default)
                Switch(Default);
        }

        public void DisableAll()
        {
            foreach (var doc in Docs)
            {
                doc.rootVisualElement.style.display = DisplayStyle.None;
            }
        }
        public void Switch(UIDocument Doc)
        {
            foreach (var doc in Docs)
            {
                doc.rootVisualElement.style.display = Doc == doc ? DisplayStyle.Flex : DisplayStyle.None;
            }
            if (Doc.rootVisualElement.style.display != DisplayStyle.Flex)
            {
                Doc.rootVisualElement.style.display = DisplayStyle.Flex;
                Debug.LogWarning($"UIDocument '{Doc.name}' not a part of the list can cannot be disabled by '{name}'. Enable suppressWarning if this was intentional.", Doc);
            }
        }
    }
}
