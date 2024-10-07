using UnityEngine;
using Neeto;
using System.IO;
using System;
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
    static string ProjectPath => Path.GetDirectoryName(Application.dataPath);
    static string SelectionPath => Path.Combine(ProjectPath, AssetDatabase.GetAssetPath(Selection.activeObject));


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

    [TextArea(2, 50)]
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

            var output = result.output.WithHTML(size: 9, color: Color.white.WithV(.7f));

            //if (result != null)
            if (result.exitCode == 0)
                Debug.Log($"{"✔  ".WithHTML(color: Color.green)}{cmd}\n{output}", this);
            else
            {
                var error = result.error.WithHTML(size: 9, color: Color.white.WithV(.7f));
                Debug.LogError($"{"✘  ".WithHTML(color: Color.red)}{cmd}\n{output}\n{error}", this);
                return;
            }
        }
    }

    public static void OpenBash(string dir)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = @"C:\Program Files\Git\git-bash.exe",
            WorkingDirectory = dir,
            UseShellExecute = false
        });
    }
    static async Task<ProcessResult> RunBash(string dir, string command)
    {
        //command = command.Substring(4, command.Length - 4);
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = @"C:\Program Files\Git\bin\bash",
            Arguments = $"git status",
            WorkingDirectory = dir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
        });

        Debug.Log($"Running `{command}`...");

        //process.Start();

        //var output = await process.StandardOutput.ReadToEndAsync();
        //var error = await process.StandardError.ReadToEndAsync();
        //var output = await process.StandardOutput.ReadToEndAsync();

        //process.WaitForExit();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        process.WaitForExit();

        //Debug.Log($"exited with code {process.ExitCode}");
        //Debug.Log(output);
        //Debug.Log(error);
        //process.Close();
        //Debug.Log(error);

        return new ProcessResult(process.ExitCode, output, error);
    }

    [QuickAction]
    public static async void GitStatus()
    {
        var result = await RunBash(ProjectPath, "git status");

        Debug.Log(result.output);
        //RunBash(ProjectPath, "git status");
    }

    [QuickAction]
    public static void OpenProjectInBash()
    {
        OpenBash(Path.GetDirectoryName(Application.dataPath));
    }

    [MenuItem("Assets/git/bash here", priority = 20, validate = false)]
    public static void BashHere()
    {
        OpenBash(SelectionPath);
    }
    [MenuItem("Assets/git/bash here", priority = 20, validate = true)]
    public static bool _BashHere()
    {
        return Directory.Exists(SelectionPath);
    }

    struct ProcessResult
    {
        public int exitCode;
        public string output;
        public string error;
        public ProcessResult(int exitCode, string output, string error)
        {
            this.exitCode = exitCode;
            this.output = output;
            this.error = error;
        }
    }
    static async Task<ProcessResult> RunAsync(string command, string workingDirectory = null, uint timeout = 10000) // 10 secs
    {
        workingDirectory = workingDirectory ?? ProjectPath;

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

            process.WaitForExit((int)timeout);

            return new ProcessResult(process.ExitCode, output.Trim(), error.Trim());
        }
        catch (Exception ex)
        {
            return default;
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
