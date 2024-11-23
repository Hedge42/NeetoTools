#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
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

        
        
        

        private void OnGUI()
        {
            // header
            //style ??= new GUIStyle() { alignment = TextAnchor.MiddleLeft };
            Editor.CreateCachedEditor(this, null, ref editor);
            editor.OnInspectorGUI();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(NGUI.settings.With(text: "Open Build Settings"), EditorStyles.miniButton, GUILayout.MinWidth(20f)))
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
                if (GUILayout.Button(NGUI.sceneOut, GUILayout.Width(25f)))
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
                }

                EditorGUI.BeginDisabledGroup(true);

                var asset = Factory.Cache(scene.path, () => AssetDatabase.LoadAssetAtPath(scene.path, typeof(Object)));

                EditorGUILayout.ObjectField(asset, typeof(SceneAsset), false);

                EditorGUI.EndDisabledGroup();

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();


        }
        static bool LockButtonLayout(bool isLocked)
        {
            var content = isLocked ? NGUI.hidden : NGUI.visible;

            if (GUILayout.Button(content, EditorStyles.iconButton, GUILayout.Width(20)))
                isLocked = !isLocked;
            return isLocked;
        }

        [UnityEditor.MenuItem(MENU.Open + nameof(Neeto.QuickScene), priority = MENU.Top)]
        public static void Open()
        {
            var window = GetWindow<QuickScene>();
            window.titleContent = new GUIContent("Quick-Load");
            window.Show();
        }
    }
}

#endif