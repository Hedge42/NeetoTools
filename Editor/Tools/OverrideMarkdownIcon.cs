using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class OverrideMarkdownIcon
{
    static OverrideMarkdownIcon()
    {
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
    }

    static Texture fg;
    static Texture2D bg;
    static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
    {
        // Get the asset path of the item currently being drawn in the project window
        string assetPath = AssetDatabase.GUIDToAssetPath(guid);


        // Check if the file is an .md file
        if (assetPath.EndsWith(".md"))
        {
            if (!bg)
            {
                bg = new Texture2D(1, 1);
                ColorUtility.TryParseHtmlString("#3d3d3d", out var c);
                bg.SetPixel(0, 0, c);
                bg.Apply();
            }
            if (!fg)
            {
                fg = EditorGUIUtility.IconContent("_Help@2x").image;
            }

            // Adjust the icon rect to exactly overlay the original icon
            Rect iconRect = new Rect(selectionRect.x, selectionRect.y, 16, 16);

            // Draw the original icon first
            GUI.DrawTexture(iconRect, bg);

            // Overlay the custom icon, respecting transparency
            GUI.DrawTexture(iconRect, fg);
        }
    }
}
