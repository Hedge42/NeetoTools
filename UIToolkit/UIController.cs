using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Neeto
{
    [DefaultExecutionOrder(69)]
    [RequireComponent(typeof(UIDocument))]
    public class UIController : MonoBehaviour
    {
        [GetComponent] public UIDocument document;
        [OnValueChanged(nameof(DisplayChanged))]
        public DisplayStyle display = DisplayStyle.Flex;
        public VisualElement root => document.rootVisualElement;

        [SerializeReference, Polymorphic, ReorderableList(ListStyle.Lined)]
        public UIElement[] elements;

        public UnityEvent onVisible;
        public UnityEvent onHidden;

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
        bool isVisible
        {
            get => root.style.display == DisplayStyle.Flex;
            set => root.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }
        void Start()
        {
            foreach (var e in elements)
                e.Q(root);
        }
        void Update()
        {
            if (root.style.display != display) // was changed externally?
            {
                if (isVisible)
                    Show(); // invoke events and update field
                else
                    Hide(); 
            }
        }

        public void DisplayChanged() => UniTask.Post(() => SetDisplay(display));
        public void SetDisplay(DisplayStyle display)
        {
            if (root == null)
                return;

            bool invoke = display != root.style.display;

            root.style.display = this.display = display;

            if (invoke)
            {
                if (isVisible)
                    onVisible?.Invoke();
                else
                    onHidden?.Invoke();
            }
        }


        [QuickAction]
        [ContextMenu(nameof(Show))]
        public void Show()
        {
            SetDisplay(DisplayStyle.Flex);
        }
        [QuickAction]
        [ContextMenu(nameof(Hide))]
        public void Hide()
        {
            SetDisplay(DisplayStyle.None);
        }
        public void ToggleVisibility()
        {
            if (!isVisible)
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
