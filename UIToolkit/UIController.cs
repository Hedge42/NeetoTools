using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using UnityEngine;
using UnityEngine.UIElements;

namespace Neeto
{
    [ExecuteAlways]
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(UIDocument))]
    public class UIController : MonoBehaviour
    {
        [GetComponent]
        public UIDocument document;
        public VisualElement root => document.rootVisualElement;

        [SerializeReference, Polymorphic]
        public UIElement[] elements;

        bool started;

        bool HasRoot
        {
            get
            {
                if (root == null)
                    Debug.LogWarning($"rootVisualElement not found on '{name}'", this);

                return root != null;
            }
        }
        //#if UNITY_EDITOR
        //        void OnValidate()
        //        {
        //            UnityEditor.EditorApplication.delayCall += () =>
        //            {
        //                SetVisibility(enabled);
        //            };
        //        }
        //#endif
        async void Awake()
        {
            await UniTask.Yield();

            SetVisibility(enabled);
        }
        void OnEnable()
        {
            if (started)
                Show();
        }
        void OnDisable()
        {
            Hide();
        }
        void Start()
        {
            started = true;
            Show();
            
            if (Application.isPlaying)
            {
                foreach (var e in elements)
                {
                    e.Q(root);
                }
            }
        }

        [QuickAction]
        [ContextMenu(nameof(Show))]
        public void Show()
        {
            if (!HasRoot) return;

            root.style.display = DisplayStyle.Flex;
            enabled = true;
        }
        [QuickAction]
        [ContextMenu(nameof(Hide))]
        public void Hide()
        {
            if (!HasRoot) return;

            root.style.display = DisplayStyle.None;
            enabled = false;
        }
        public void ToggleVisibility()
        {
            if (!enabled)
                Show();
            else
                Hide();
        }
        public void SetVisibility(bool value)
        {
            if (value)
                Show();
            else
                Hide();
        }
    }
}
