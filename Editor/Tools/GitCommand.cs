using UnityEngine;
using Neeto;
using System.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Tools/Git Command")]
public class GitCommand : ScriptableObject
{
#if UNITY_EDITOR
    [CustomEditor(typeof(GitCommand))]
    class GitCommandEditor : Editor
    {
        bool locked = true;
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(locked);
            base.OnInspectorGUI();
            EditorGUI.EndDisabledGroup();

            locked = GUI.Toggle(new Rect(0, 25, 16, 16), locked, EditorGUIUtility.IconContent(locked ? "Locked" : "Unlocked"), EditorStyles.iconButton);

            GUILayout.Space(10);
            if (GUILayout.Button(nameof(GitCommand.Run)))
                (target as GitCommand).Run();
        }
    }

    [TextArea(2, 10)]
    public string commands;

    [Tooltip("Defaults to project root")]
    public string assetPath;

    [QuickAction]
    public async void Run()
    {
        var dir = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
        if (!string.IsNullOrWhiteSpace(assetPath))
            dir += assetPath;

        foreach (var cmd in commands.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(cmd))
                continue;


            var result = await RunAsync(cmd, dir);

            if (result != null)
                Debug.Log($"<color=green>✔  </color>{cmd}\n\n<size=10>{result}</size>", this);
            else
                Debug.LogError($"<color=red>✘  </color>{cmd}", this);
        }
    }

    public static async Task<string> RunAsync(string command, string workingDirectory = null)
    {
        workingDirectory = workingDirectory ?? Application.dataPath + "/../";

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = GetShellExecutable(),
                    Arguments = GetShellArguments(command),
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
                throw new Exception($"Git command error: {error}");

            return output.Trim();
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    private static string GetShellExecutable()
    {
#if UNITY_EDITOR_WIN
        return "cmd.exe";
#else
        return "/bin/bash";
#endif
    }
    private static string GetShellArguments(string command)
    {
#if UNITY_EDITOR_WIN
        return $"/c {command}";
#else
        return $"-c \"{command}\"";
#endif
    }
#endif
}
