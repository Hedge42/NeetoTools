using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Neeto
{
    public class TextureCreator : EditorWindow
    {
        public Texture2D inputTexture;
        public Texture2D outputTexture;
        public DefaultAsset outputFolder;
        public Color multiplierColor = Color.green;

        Vector2 scrollPos;
        Editor editor;

        [MenuItem(Menu.Tools + nameof(TextureCreator))]
        static void Init()
        {
            TextureCreator window = (TextureCreator)GetWindow(typeof(TextureCreator));
            window.titleContent = new GUIContent("Texture Creator");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        void OnGUI()
        {
            Editor.CreateCachedEditor(this, null, ref editor);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            editor.OnInspectorGUI();

            GUILayout.BeginHorizontal();

            inputTexture = (Texture2D)EditorGUILayout.ObjectField(nameof(inputTexture), inputTexture, typeof(Texture2D), false);

            GUILayout.Label("->");

            EditorGUILayout.ObjectField(nameof(outputTexture), outputTexture, typeof(Texture2D), false);

            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (GUILayout.Button("Apply Color Multiplier", GUILayout.Height(40)))
            {
                if (inputTexture == null)
                {
                    Debug.LogError("Please assign an input texture.");
                }
                else
                {
                    ApplyColorMultiplier();
                }
            }
            if (GUILayout.Button("Save Texture", GUILayout.Height(40)))
            {
                SaveTexture();
            }

            EditorGUILayout.EndScrollView();
        }

        void ApplyColorMultiplier()
        {
            outputTexture = Texture2D.Instantiate(inputTexture);

            //outputTexture = new Texture2D(inputTexture.width, inputTexture.height, TextureFormat.ARGB32, true, true);
            //outputTexture.alphaIsTransparency = inputTexture.alphaIsTransparency;

            Color[] pixels = inputTexture.GetPixels();

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = (pixels[i] * multiplierColor).With(a: pixels[i].a);
            }

            outputTexture.SetPixels(pixels);
            outputTexture.Apply();
        }

        void SaveTexture()
        {
            if (outputFolder == null)
            {
                Debug.LogError("Please assign an output folder.");
                return;
            }

            string path = AssetDatabase.GetAssetPath(outputFolder);
            if (!Directory.Exists(path))
            {
                Debug.LogError("Invalid output folder.");
                return;
            }

            byte[] bytes = outputTexture.EncodeToPNG();
            string outputPath = Path.Combine(path, inputTexture.name + "_Modified.png");

            File.WriteAllBytes(outputPath, bytes);
            AssetDatabase.Refresh();

            Debug.Log("Texture saved to: " + outputPath);
        }
    }

}
