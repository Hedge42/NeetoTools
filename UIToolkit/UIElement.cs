using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using System.Linq;

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
                else
                    Debug.LogError($"VisualElement '{selector}' not found...");
            }
        }
        protected virtual void OnSetElement() { }
        protected virtual void OnVisible() { }
        protected virtual void OnHidden() { }
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
    [Serializable]
    public class SliderElement : UIElement<Slider>
    {
        public UnityEvent<float> changed;
        protected override void OnSetElement()
        {
            Element.RegisterValueChangedCallback(_ => changed?.Invoke(_.newValue));
        }
    }
#if WWISE
    [Serializable]
    public class RtpcSlider : UIElement<Slider>
    {
        public float Value
        {
            get => PlayerPrefs.GetFloat(selector, .5f);
            set
            {
                PlayerPrefs.SetFloat(selector, value);
                rtpc.SetValue(null, value);
            }
        }

        public AK.Wwise.RTPC rtpc;
        public float min = 0f;
        public float max = 1f;
        protected override void OnSetElement()
        {
            Element.lowValue = min;
            Element.highValue = max;
            Element.value = Value;
            rtpc.SetValue(null, Value);
            Element.RegisterValueChangedCallback(value => Value = value.newValue);
        }
    }
#endif
    [Serializable]
    public class ResolutionDropdown : UIElement<DropdownField>
    {
        public string Value
        {
            get => PlayerPrefs.GetString(selector, "1920x1080");
            set
            {
                PlayerPrefs.SetString(selector, value);
                GraphicsSettings.ParseResolution(value).ApplyResolution();
            }
        }

        protected override void OnSetElement()
        {
            Element.choices = GraphicsSettings.GetResolutionLabels().ToList();
            Element.index = GraphicsSettings.GetIndex(Screen.currentResolution);
            Element.RegisterValueChangedCallback(i => Value = i.newValue);
        }
    }
    [Serializable]
    public class FullscreenDropdown : UIElement<DropdownField>
    {
        public string Value
        {
            get => PlayerPrefs.GetString(selector, Enum.GetName(typeof(FullScreenMode), FullScreenMode.FullScreenWindow));
            set
            {
                PlayerPrefs.SetString(selector, value);
                var res = Screen.currentResolution;
                Screen.SetResolution(res.width, res.height, Enum.Parse<FullScreenMode>(value));
            }
        }

        protected override void OnSetElement()
        {
            Element.choices = Enum.GetNames(typeof(FullScreenMode)).ToList();
            Element.index = Element.choices.IndexOf(Enum.GetName(typeof(FullScreenMode), Screen.fullScreenMode));
            Element.RegisterValueChangedCallback(i => Value = i.newValue);
        }
    }
    [Serializable]
    public class ToggleSetting : UIElement<Toggle>
    {
        public bool Value
        {
            get => PlayerPrefs.GetInt(selector, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(selector, value ? 1 : 0);
                changed?.Invoke(value);
            }
        }

        public UnityEvent<bool> changed;
        protected override void OnSetElement()
        {
            Element.SetValueWithoutNotify(Value);
            changed?.Invoke(Value);
            Element.RegisterValueChangedCallback(_ => Value = _.newValue);
        }
    }
    [Serializable] public class ProgressBarElement : UIElement<ProgressBar> { }
}
