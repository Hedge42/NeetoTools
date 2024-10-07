using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Neeto
{
    public class MainMenu : MonoBehaviour
    {
        public UIDocument doc;

        public string titleText;

        public string title = "Title";
        public string play = "Play";
        public string settings = "Settings";
        public string quit = "Quit";

        public Label Title;
        public Button Play;
        public Button Settings;
        public Button Quit;

        public string bg;
        public VisualElement Bg;

        private void Awake()
        {
            this.Title = doc.rootVisualElement.Q<Label>(title);
            this.Play = doc.rootVisualElement.Q<Button>(play);
            this.Settings = doc.rootVisualElement.Q<Button>(settings);
            this.Quit = doc.rootVisualElement.Q<Button>(quit);
            //this.bg = doc.rootVisualElement.Q

            Title.text = titleText;
            Play.clicked += OnPlay;
            Settings.clicked += OnSettings;
            Quit.clicked += OnQuit;

            
        }

        private void OnEnable()
        {
        }

        public void OnPlay()
        {
            Debug.Log("play");
        }
        public void OnSettings()
        {
            Debug.Log("settings");
        }
        public void OnQuit()
        {
            Debug.Log("quit");
        }
    }

    public class CustomControl : VisualElement
    {
        [Preserve] public new class UxmlFactory : UxmlFactory<CustomControl> { }

        public CustomControl()
        {

        }
    }
}
