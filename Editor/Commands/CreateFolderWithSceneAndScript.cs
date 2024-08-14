using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.IO;

public class CreateFolderWithSceneAndScript : EditorWindow
{
    private string folderName = "NewFolder";
    private string sceneName = "NewScene";
    private string scriptName = "NewScript";

    [MenuItem(MenuPath.Run + "Create Folder with Scene and Script")]
    private static void ShowWindow()
    {
        GetWindow<CreateFolderWithSceneAndScript>("Create Folder with Scene and Script");
    }

    private void OnGUI()
    {
        GUILayout.Label("Create a new folder with a scene and script", EditorStyles.boldLabel);

        folderName = EditorGUILayout.TextField("Folder Name", folderName);
        sceneName = EditorGUILayout.TextField("Scene Name", sceneName);
        scriptName = EditorGUILayout.TextField("Script Name", scriptName);

        if (GUILayout.Button("Create"))
        {
            CreateFolderWithContent();
        }
    }

    private void CreateFolderWithContent()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path))
        {
            path = "Assets";
        }
        else if (!AssetDatabase.IsValidFolder(path))
        {
            path = Path.GetDirectoryName(path);
        }

        string folderPath = Path.Combine(path, folderName);
        AssetDatabase.CreateFolder(path, folderName);

        // Create Scene
        string scenePath = Path.Combine(folderPath, sceneName + ".unity");
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(newScene, scenePath);
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // Create Script
        string scriptPath = Path.Combine(folderPath, scriptName + ".cs");
        string scriptContent = $"using UnityEngine;\n\npublic class {scriptName} : MonoBehaviour\n{{\n    void Start()\n    {{\n        \n    }}\n\n    void Update()\n    {{\n        \n    }}\n}}";
        File.WriteAllText(scriptPath, scriptContent);
        AssetDatabase.ImportAsset(scriptPath);

        // Attach script to GameObject
        var go = new GameObject(scriptName);
        var scriptType = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath).GetClass();
        go.AddComponent(scriptType);

        // Save Scene with GameObject
        EditorSceneManager.SaveScene(newScene);
    }
}
