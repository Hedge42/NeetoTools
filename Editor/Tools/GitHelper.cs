using UnityEngine;
using Neeto;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityToolbarExtender;
using UnityDropdown.Editor;
using UnityEditor;

namespace Neeto
{
    static class GitHelper
    {
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

        static string ProjectPath => Path.GetDirectoryName(Application.dataPath);
        static string ObjectPath => Path.Combine(ProjectPath, AssetDatabase.GetAssetPath(Selection.activeObject));
        static bool isRunning;

        [MenuItem("Tools/GitBash", priority = -9000)]
        static void OpenBash() => OpenBash(null);
        static void OpenBash(string dir)
        {
            dir ??= ProjectPath;
            Process.Start(new ProcessStartInfo
            {
                FileName = @"C:\Program Files\Git\git-bash.exe",
                WorkingDirectory = dir,
                UseShellExecute = false
            });
        }

        [MenuItem("Assets/git bash here", priority = 20, validate = false)]
        static void BashHere()
        {
            OpenBash(ObjectPath);
        }
        [MenuItem("Assets/git bash here", priority = 20, validate = true)]
        static bool _BashHere()
        {
            return Directory.Exists(ObjectPath);
        }

        static void ForceToolbarRefresh()
        {
            EditorApplication.delayCall += () =>
            {
                // Use reflection to get the internal Toolbar type
                var toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
                var repaintMethod = toolbarType.GetMethod("RepaintImmediately", BindingFlags.Public | BindingFlags.Instance);
                var toolbar = Resources.FindObjectsOfTypeAll(toolbarType)[0];
                repaintMethod.Invoke(toolbar, null);
            };
        }

        static void QuickSync()
        {
            Run("git fetch origin\n" +
                "git add .\n" +
                "git commit -am \"updates\"\n" +
                "git push origin HEAD -f\n" +
                "git rebase -S -Xtheirs --continue origin/main\n" +
                "git checkout origin/HEAD -- $(find . -type d -name \"$(git rev-parse --abbrev-ref HEAD)\" -exec find {} -type f \\;)\n" +
                "git add .\n" +
                "git commit -am \"rebase main\"\n" +
                "git push origin HEAD -f");
        }

        static async void Run(string cmd)
        {
            var cmds = cmd.Split('\n');

            foreach (var _ in cmds)
            {
                var result = await RunAsync(cmd);
                Debug.Log(cmd + "\n" + result.output.WithHTML(size: 10, color: Color.white.WithV(.7f)));
            }
        }
        static async Task<ProcessResult> RunAsync(string command, string workingDirectory = null, uint timeout = 10000) // 10 secs
        {
            if (isRunning)
            {
                throw new Exception("A command is currently running. Please wait...");
            }

            isRunning = true;
            ForceToolbarRefresh();
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

                isRunning = false;
                ForceToolbarRefresh();
                return new ProcessResult(process.ExitCode, output.Trim(), error.Trim());
            }
            catch (Exception ex)
            {
                Debug.LogError("WTF WTF");
                isRunning = false;
                return default;
            }
        }
        static string GetShellExecutable()
        {
#if UNITY_EDITOR_WIN
            return "cmd.exe";
#else
        return "/bin/bash";
#endif
        }
        static string GetShellArguments(string command)
        {
#if UNITY_EDITOR_WIN
            return $"/c {command}";
#else
        return $"-c \"{command}\"";
#endif
        }
    }
}