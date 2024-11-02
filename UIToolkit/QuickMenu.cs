using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace Neeto
{
    public class QuickMenu
    {
        const string uxmlPath = "USS/QuickContext_UXML";
        const string panelSettingsPath = "PanelSettings";
        const string LabelName = "label";
        const string YesButtonName = "yes-button";
        const string NoButtonName = "no-button";

        static QuickMenu instance;
        public UIDocument ui { get; private set; }
        public VisualElement root => ui.rootVisualElement;
        public Label Label { get; private set; }
        public Button yesButton { get; private set; }
        public Button noButton { get; private set; }

        public static QuickMenu GetInstance()
        {
            if (instance != null)
                return instance;

            instance = new QuickMenu();
            var gameObject = new GameObject(nameof(QuickMenu));
            GameObject.DontDestroyOnLoad(gameObject);
            instance.ui = gameObject.AddComponent<UIDocument>();
            instance.ui.panelSettings = Resources.Load<PanelSettings>(panelSettingsPath);
            instance.ui.visualTreeAsset = Resources.Load<VisualTreeAsset>(uxmlPath);
            instance.Label = instance.root.Q<Label>(LabelName);
            instance.yesButton = instance.root.Q<Button>(YesButtonName);
            instance.noButton = instance.root.Q<Button>(NoButtonName);

            return instance;
        }
    }
}
