using UnityEngine;
using System.IO;
using UnityEditor;

public partial struct SystemPath
{
    public static string Builds => Path.Combine(Application.dataPath, "../Builds");
    public static string Assets => Path.GetDirectoryName(Application.dataPath) + '\\';
    public static string Persistent => Application.persistentDataPath;
    public static string Project => Application.dataPath.Substring(0, Application.dataPath.Length - 6);
    public static string Manifest => Project + "Packages/manifest.json";

#if UNITY_EDITOR
    public static void Execute(string path)
    {
        EditorUtility.OpenWithDefaultApp(path);
    }
    public static void OpenWithExplorer(string path)
    {
        EditorUtility.OpenWithDefaultApp(Path.GetDirectoryName(path));
    }
#endif
}
