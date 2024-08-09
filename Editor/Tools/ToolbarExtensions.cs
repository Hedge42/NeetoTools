using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;
using Debug = UnityEngine.Debug;

using Settings = NeetoSettings;

namespace Neeto
{
    [InitializeOnLoad]
    public static class ToolbarExtensions
    {
        static bool enabled
        {
            get => Settings.instance.overrideToolbarWindow;
            set => Settings.instance.overrideToolbarWindow = value;
        }
        static ToolbarExtensions()
        {
            // draw maximize window button
            ToolbarExtender.LeftToolbarGUI.Add(() =>
            {
                if (!enabled)
                    return;

                GUILayout.FlexibleSpace();

                EditorGUI.BeginDisabledGroup(Application.isPlaying);
                if (GUILayout.Button(new GUIContent(NTexture.expand, "Play maximized"), EditorStyles.toolbarButton, GUILayout.Width(ToolbarExtender.buttonWidth)))
                {
                    // play full screen
                    var gameView = GetGameView();
                    gameView?.Maximize(true);
                    EditorApplication.EnterPlaymode();
                    EditorApplication.quitting += () => Debug.Log("ye");
                    Program.onQuit += () => GetGameView()?.Maximize(false);
                }
                EditorGUI.EndDisabledGroup();

            });

            // un-maximize on exit play mode
            EditorApplication.playModeStateChanged += playState =>
            {
                if (playState == PlayModeStateChange.ExitingPlayMode)
                {
                    GetGameView().Maximize(false);
                }
            };

            // draw timescale control
            ToolbarExtender.RightToolbarGUI.Add(() =>
            {
                if (!enabled)
                    return;

                if (GUILayout.Button(new GUIContent(NTexture.time, "Time scale"), EditorStyles.toolbarButton, GUILayout.Width(20f)))
                    Time.timeScale = 1f;
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Box(Time.timeScale.ToString("f2"), EditorStyles.toolbarButton, GUILayout.Width(35f));
                EditorGUI.EndDisabledGroup();

                GUILayout.FlexibleSpace();

            });
        }
        static EditorWindow GetGameView()
        {
            var windows = (UnityEditor.EditorWindow[])Resources.FindObjectsOfTypeAll(typeof(UnityEditor.EditorWindow));
            foreach (var window in windows)
            {
                if (window != null && window.GetType().FullName == "UnityEditor.GameView")
                {
                    return window;
                }
            }
            return null;
        }
        public static void Maximize(this EditorWindow window, bool value)
        {
            window.maximized = value;
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
    }
}