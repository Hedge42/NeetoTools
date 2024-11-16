using Neeto;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Opens using <see cref="EditorUtility.SaveFilePanel(string, string, string, string)"/>
/// </summary>
public class ExportAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ExportAttribute))]
public class ExportDrawer : PropertyButtonDrawerBase
{
    public override GUIContent content => EditorGUIUtility.IconContent("SceneLoadIn").With(tooltip: "Export Asset");
    public override void OnClick(SerializedProperty property)
    {
        var obj = property.objectReferenceValue;
        // open save file dialogue
        var path = EditorUtility.SaveFilePanel("Export Asset", Application.dataPath, obj.name, GetExtension(obj));

        path = NString.SystemToAssetPath(path);

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

