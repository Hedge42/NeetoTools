#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

[InitializeOnLoad]
public static class MToolbar
{
    public static event Action onRightMostToolbarGUI;

    static MToolbar()
    {
        ToolbarExtender.LeftToolbarGUI.Add(OnLeftToolbarGUI);
        ToolbarExtender.RightToolbarGUI.Add(TimeScale);
        ToolbarExtender.RightToolbarGUI.Add(GUILayout.FlexibleSpace);
        ToolbarExtender.RightToolbarGUI.Add(RightMostGUI);

        EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
    }
    public static bool ButtonLayout(GUIContent content)
    {
        GUILayout.Space(2);
        return GUILayout.Button(content, EditorStyles.toolbarButton, GUILayout.Width(ToolbarExtender.buttonWidth));
    }
    public static bool IconPopupLayout(Texture2D texture, string tooltip = null)
    {
        var content = new GUIContent(texture, tooltip);

        GUILayout.Space(2);
        return GUILayout.Button(content, EditorStyles.toolbarPopup, GUILayout.Width(ToolbarExtender.buttonWidth * 1.24f));
    }
    public static bool DropdownSearchbar(GUIContent content)
    {
        GUILayout.Space(2);
        return GUILayout.Button(content, EditorStyles.toolbarSearchField, GUILayout.Width(ToolbarExtender.dropdownWidth));
    }
    static void RightMostGUI()
    {
        onRightMostToolbarGUI?.Invoke();
    }

    private static void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
    {
        if (obj == PlayModeStateChange.ExitingPlayMode)
            GetGameView().Maximize(false);
    }

    static void TimeScale()
    {
        //;Resource.

        //GUILayout.FlexibleSpace();
        if (GUILayout.Button(new GUIContent(MTexture.time, "Time scale"), EditorStyles.toolbarButton, GUILayout.Width(20f)))
            Time.timeScale = 1f;
        EditorGUI.BeginDisabledGroup(true);
        GUILayout.Box(Time.timeScale.ToString("f2"), EditorStyles.toolbarButton, GUILayout.Width(35f));
        EditorGUI.EndDisabledGroup();
    }

    static void OnLeftToolbarGUI()
    {
        GUILayout.FlexibleSpace();

        EditorGUI.BeginDisabledGroup(Application.isPlaying);
        if (GUILayout.Button(new GUIContent(MTexture.expand, "Play maximized"), EditorStyles.toolbarButton, GUILayout.Width(ToolbarExtender.buttonWidth)))
            PlayFullScreen();
        EditorGUI.EndDisabledGroup();
    }

    static void PlayFullScreen()
    {
        var gameView = GetGameView();
        gameView?.Maximize(true);
        EditorApplication.EnterPlaymode();
        EditorApplication.quitting += () => Debug.Log("ye");
        MApp.onQuit += () => GetGameView()?.Maximize(false);
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

#endif
