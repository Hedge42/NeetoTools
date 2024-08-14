using UnityEngine;
using UnityEditor;
using System.IO;

public class ScriptRenamer : EditorWindow
{
    string findText = "";
    string replaceText = "";
    string[] selectedScriptPaths;

    [MenuItem("Tools/Script Renamer")]
    public static void ShowWindow()
    {
        GetWindow<ScriptRenamer>("Script Renamer");
    }

    void OnGUI()
    {
        GUILayout.Label("Find and Replace", EditorStyles.boldLabel);
        findText = EditorGUILayout.TextField("Find", findText);
        replaceText = EditorGUILayout.TextField("Replace", replaceText);

        if (GUILayout.Button("Select Scripts"))
        {
            selectedScriptPaths = SelectScripts();
        }

        if (selectedScriptPaths != null && selectedScriptPaths.Length > 0)
        {
            GUILayout.Label("Selected Scripts:");
            foreach (var path in selectedScriptPaths)
            {
                GUILayout.Label(path);
            }

            if (GUILayout.Button("Rename Scripts"))
            {
                RenameScripts();
            }
        }
    }

    string[] SelectScripts()
    {
        string path = EditorUtility.OpenFolderPanel("Select Folder with Scripts", "", "");
        if (string.IsNullOrEmpty(path))
            return null;

        return Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
    }

    void RenameScripts()
    {
        if (string.IsNullOrEmpty(findText) || string.IsNullOrEmpty(replaceText))
        {
            EditorUtility.DisplayDialog("Error", "Find and Replace fields cannot be empty", "OK");
            return;
        }

        foreach (var scriptPath in selectedScriptPaths)
        {
            string content = File.ReadAllText(scriptPath);
            string newContent = content.Replace(findText, replaceText);

            if (newContent != content)
            {
                File.WriteAllText(scriptPath, newContent);

                string newFilePath = scriptPath.Replace(findText, replaceText);
                if (newFilePath != scriptPath)
                {
                    File.Move(scriptPath, newFilePath);
                    AssetDatabase.Refresh();
                }
            }
        }

        EditorUtility.DisplayDialog("Success", "Scripts renamed successfully", "OK");
    }
}
