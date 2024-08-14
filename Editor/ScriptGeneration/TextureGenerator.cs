using UnityEngine;
using System.IO;
using System;
using Random = UnityEngine.Random;
using Neeto;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(TextureGenerator), true)]
public class TextureGeneratorEditor : MonoBehaviourEditorBase
{
    private Editor cachedEditor;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var target = this.target as TextureGenerator;

        // ??????? why no show?
        Editor.CreateCachedEditor(target.material, typeof(MaterialEditor), ref cachedEditor);
        cachedEditor.OnInspectorGUI();
    }
}
#endif

[CreateAssetMenu(menuName = "Neeto/Texture Generator")]
public class TextureGenerator : ScriptableObject
{
    public Material material;
    public int width = 256;
    public int height = 256;

    [PreviewTexture]
    public Texture2D texture;

    [Button]
    public void GenerateTexture()
    {
        // Create a new RenderTexture to render the material to
        RenderTexture renderTexture = new RenderTexture(width, height, 24);
        Graphics.Blit(null, renderTexture, material);

        // Now, copy the rendered image to a Texture2D
        texture = new Texture2D(width, height);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();

        // Don't forget to clean up!
        RenderTexture.active = null;
        if (renderTexture != null)
        {
            RenderTexture.DestroyImmediate(renderTexture);
        }
    }

#if UNITY_EDITOR
    [Button]
    public void Export()
    {
        byte[] bytes = texture.EncodeToPNG();
        string path = AssetDatabase.GetAssetPath(this);
        path = Path.GetDirectoryName(path) + "/" + name + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
        File.WriteAllBytes(path, bytes);

        // Refresh the asset database so the new PNG file shows up in the editor
        AssetDatabase.Refresh();
    }
#endif
}