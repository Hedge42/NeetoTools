using System;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Neeto
{
    [Serializable]
    public abstract class UIElement
    {
        public string selector;
        public VisualElement element { get; protected set; }
        public void Q(VisualElement root)
        {
            if (element == null)
            {
                element = root.Q(selector);
                if (element != null)
                    OnSetElement();
            }
        }
        protected virtual void OnSetElement() { }
    }

    [Serializable]
    public abstract class UIElement<T> : UIElement where T : VisualElement
    {
        public T Element => element as T;
    }
    [Serializable]
    public class ButtonEventElement : UIElement<Button>
    {
        public UnityEvent clicked;
        protected override void OnSetElement() => Element.clicked += clicked.Invoke;
    }
    [Serializable]
    public class ButtonActionElement : UIElement<Button>
    {
        public GameAction action;
        protected override void OnSetElement() => Element.clicked += action.Invoke;
    }
    [Serializable]
    public class TextElement : UIElement<Label>
    {
        public string text;
        protected override void OnSetElement() => Element.text = text;
    }
    [Serializable] public class SliderElement : UIElement<Slider> { }
    [Serializable] public class ProgressBarElement : UIElement<ProgressBar> { }
}
