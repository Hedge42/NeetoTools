using Neeto;
using System;
using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
public class TextureGenerator : EditorWindow
{
    [SerializeReference, Polymorphic]
    public ITextureGenerator generator = new Texture1x1();

    static string lastPath = "";
    Editor editor;

    [MenuItem(MenuPath.Open + nameof(TextureGenerator), priority = MenuOrder.Bottom)]
    static void Open()
    {
        var window = GetWindow<TextureGenerator>("Gradient Texture Generator");
        window.editor = Editor.CreateEditor(window, null);
        window.Show();
    }

    void OnGUI()
    {
        editor.OnInspectorGUI();

        if (GUILayout.Button("Export"))
        {
            generator.Export();
        }
    }

    public interface ITextureGenerator
    {
        public void Export();
    }

    [Serializable]
    public class Texture1x1 : ITextureGenerator
    {
        public string filename = "Texture_1x1";
        public Color color = Color.green;
        public void Export()
        {
            var tex = color.AsTexturePixel();
            ExportAndDestroy(filename, tex);
        }
    }
    [Serializable]
    public class Gradient1x1s : ITextureGenerator
    {
        public string filename = "Gradient1x1_";
        public Gradient gradient;
        [Min(2)] public int count = 5;

        public void Export()
        {
            Texture2D[] textures = new Texture2D[count];
            for (int i = 0; i < count; i++)
            {
                var t = (float)i / (float)(count - 1);
                textures[i] = gradient.Evaluate(t).AsTexturePixel();
            }
            ExportAndDestroy(filename, textures);
        }
    }
    [Serializable]
    public class GradientLUT : ITextureGenerator
    {
        public Gradient gradient = new Gradient();
        public int resolution = 128;
        public Axis axis;

        public enum Axis
        {
            X,
            Y
        }
        public async void Export()
        {
            Vector2Int size = axis == Axis.X ? new Vector2Int(resolution, 1) : new Vector2Int(1, resolution);

            Texture2D tex = new Texture2D(size.x, size.y);
            for (int r = 0; r < resolution; r++)
            {
                Color color = gradient.Evaluate((float)r / (float)resolution);

                if (axis == Axis.X)
                    tex.SetPixel(r, 0, color);

                else
                    tex.SetPixel(0, r, color);
            }

            tex.alphaIsTransparency = true;
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            string path = EditorUtility.SaveFilePanel("Save Texture", "Assets/Gylfie/UI/Gradients", "Gradient", "png");
            if (path.Length != 0)
            {
                await File.WriteAllBytesAsync(path, bytes);
                AssetDatabase.Refresh();
            }

            DestroyImmediate(tex);
        }
    }
    [Serializable]
    public class Materialized : ITextureGenerator
    {
        public Material material;
        [Min(1f)]
        public int width = 256;
        [Min(1f)]
        public int height = 256;

        [PreviewTexture]
        public Texture2D texture;

        public void Export()
        {
            // Create a new RenderTexture to render the material to
            RenderTexture renderTexture = new RenderTexture(width, height, 24);
            Graphics.Blit(null, renderTexture, material);

            // Now, copy the rendered image to a Texture2D
            var texture = new Texture2D(width, height);
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();

            // Don't forget to clean up!
            RenderTexture.active = null;
            if (renderTexture != null)
            {
                RenderTexture.DestroyImmediate(renderTexture);
            }

            ExportAndDestroy(texture.name, texture);
        }
    }




    public static void ExportAndDestroy(string filename, Texture2D texture)
    {
        if (!filename.EndsWith(".png")) filename += ".png";
        string path = EditorUtility.SaveFolderPanel("Select Folder to Save Textures", lastPath, "");
        if (string.IsNullOrEmpty(path)) return;

        lastPath = path;

        byte[] bytes = texture.EncodeToPNG();
        filename = GetUniqueFilename(path, filename);
        System.IO.File.WriteAllBytes(System.IO.Path.Combine(path, filename), bytes);
        DestroyImmediate(texture);
    }
    public static void ExportAndDestroy(string filename, Texture2D[] textures)
    {
        string path = EditorUtility.SaveFolderPanel("Select Folder to Save Textures", lastPath, "");
        if (string.IsNullOrEmpty(path)) return;

        lastPath = path;

        for (int i = 0; i < textures.Length; i++)
        {
            var texture = textures[i];
            byte[] bytes = texture.EncodeToPNG();
            var _filename = GetUniqueFilename(path, filename + ".png");
            var _path = System.IO.Path.Combine(path, _filename);
            _path = _path.Replace('\\', '/');
            System.IO.File.WriteAllBytes(_path, bytes);
            DestroyImmediate(texture);

            var f = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
            _path = _path.Replace(f, "");
            Debug.Log(_path);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Undo.RegisterCreatedObjectUndo(AssetDatabase.LoadAssetAtPath<Texture2D>(_path), "Create Texture");
        }

    }
    public static string GetUniqueFilename(string path, string filename)
    {
        string baseName = System.IO.Path.GetFileNameWithoutExtension(filename);
        string extension = System.IO.Path.GetExtension(filename);
        int counter = 0;

        while (System.IO.File.Exists(System.IO.Path.Combine(path, filename)))
        {
            counter++;
            filename = $"{baseName}_{counter}{extension}";
        }

        return filename;
    }
}
#endif