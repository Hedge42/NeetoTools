using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;


namespace Neeto
{
    using Settings = NeetoSettings;
    [InitializeOnLoad]
    [DefaultExecutionOrder(-10)]
    public static class ToolbarGUI
    {
        static bool enabled
        {
            get => NeetoSettings.instance.experimentalEditorFeatures;
            set => NeetoSettings.instance.experimentalEditorFeatures = value;
        }
        static ToolbarGUI()
        {
            // draw timescale control
            ToolbarExtender.RightToolbarGUI.Add(() =>
            {
                if (!enabled)
                    return;

                var content = EditorGUIUtility.IconContent("d_UnityEditor.AnimationWindow@2x");
                content.tooltip = "Reset Timescale";

                if (GUILayout.Button(content, EditorStyles.toolbarButton, GUILayout.Width(20f)))
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