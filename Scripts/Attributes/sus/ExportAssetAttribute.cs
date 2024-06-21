using Matchwork;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Opens using <see cref="EditorUtility.SaveFilePanel(string, string, string, string)"/>
/// </summary>
public class ExportAssetAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ExportAssetAttribute))]
public class ExportAssetDrawer : ObjectButtonDrawer
{
    public override GUIContent content => new GUIContent(MTexture.save, "Export Asset");
    public override void OnClick(SerializedProperty property)
    {
        var obj = property.objectReferenceValue;
        // open save file dialogue
        var path = EditorUtility.SaveFilePanel("Export Asset", Application.dataPath, obj.name, GetExtension(obj));

        path = StringExtensions.SystemToAssetPath(path);

        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(obj, path);
            Undo.RegisterCreatedObjectUndo(obj, "Export Asset");

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(obj);
        }
    }
    static string GetExtension(Object obj)
    {
        return obj switch
        {
            ScriptableObject => "asset",
            Mesh => "mesh",
            GameObject => "prefab",

            _ => "idk"
        };
    }
}

#endif

