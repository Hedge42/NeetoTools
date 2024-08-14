#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using Object = UnityEngine.Object;
using UnityEditor.SceneManagement;

namespace Neeto
{
    public class QuickScene : EditorWindow
    {
        static GUIStyle style;
        Editor editor;
        Vector2 scroll;

        const string isLocked_key = nameof(QuickScene) + "." + nameof(isLocked);
        bool isLocked
        {
            get => EditorPrefs.GetBool(isLocked_key);
            set => EditorPrefs.SetBool(isLocked_key, value);
        }

        Texture2D unlocked;
        private void OnGUI()
        {
            // header
            //style ??= new GUIStyle() { alignment = TextAnchor.MiddleLeft };
            Editor.CreateCachedEditor(this, null, ref editor);
            editor.OnInspectorGUI();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Open Build Settings", NTexture.save), EditorStyles.miniButton, GUILayout.Height(20f), GUILayout.MinWidth(20f)))
            {
                EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
                EditorApplication.ExecuteMenuItem("File/Build Settings...");
            }

            isLocked = LockButtonLayout(isLocked);

            EditorGUILayout.EndHorizontal();

            DividerAttributeDrawer.DrawDividerGUILayout();

            // draw scene list
            scroll = GUILayout.BeginScrollView(scroll);
            GUILayout.BeginVertical();

            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (isLocked && !scene.enabled)
                    continue;

                GUILayout.BeginHorizontal();

                // load button
                if (GUILayout.Button("←", GUILayout.Width(25f)))
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
                }

                EditorGUI.BeginDisabledGroup(true);

                var asset = Factory.Cache(scene.path, () => AssetDatabase.LoadAssetAtPath(scene.path, typeof(Object)));

                EditorGUILayout.ObjectField(asset, typeof(SceneAsset), false);

                scene.enabled = EditorGUILayout.Toggle(scene.enabled, GUILayout.Width(20));
                EditorGUI.EndDisabledGroup();

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();


        }
        static bool LockButtonLayout(bool isLocked)
        {
            var content = new GUIContent(isLocked ? NTexture.locked : NTexture.unlocked);

            if (GUILayout.Button(content, NGUI.iconButton, GUILayout.Width(20), GUILayout.Height(NGUI.lineHeight)))
                isLocked = !isLocked;
            return isLocked;
        }

        [MenuItem(MenuPath.Open + nameof(QuickScene), priority = MenuOrder.Min)]
        public static void Open()
        {
            var window = GetWindow<QuickScene>();
            window.titleContent = new GUIContent("Quick-Load");
            window.Show();
        }
    }
}

#endif