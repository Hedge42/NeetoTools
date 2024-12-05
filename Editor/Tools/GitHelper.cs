using UnityEngine;
using Neeto;
using System.IO;
using System;
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

        [QuickMenu("Open Project in Bash", -6969)]
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
        static void BashHere() => OpenBash(ObjectPath);

        [MenuItem("Assets/git bash here", priority = 20, validate = true)]
        static bool _BashHere() => Directory.Exists(ObjectPath);
    }
}