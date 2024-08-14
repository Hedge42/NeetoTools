//#if UNITY_EDITOR

using System.Diagnostics;
using System.Text;
using UnityEditor;
using UnityEngine;
// https://gist.github.com/edwardrowe/fdec706fe53bfff0671e063f263ada63
public static class ProcessExtensions
{
    // todo project setting
    public const string bash = @"C:\Program Files\Git\git-bash.exe";

    // windows terminal...
    // "C:\Program Files\Git\bin\sh.exe" --login


    [MenuItem(MenuPath.Run + nameof(Run))]
    public static void OpenBash()
    {
        var process = new Process();
        var exitCode = process.Run(bash, "", Application.dataPath.Replace("/Assets", ""),
            out var output, out var errors);

        // fail
        if (exitCode != 0)
        {
            UnityEngine.Debug.Log("Errors: " + errors);
        }
    }

    /// <summary>
    /// Runs the specified process and waits for it to exit. Its output and errors are
    /// returned as well as the exit code from the process.
    /// See: https://stackoverflow.com/questions/4291912/process-start-how-to-get-the-output
    /// Note that if any deadlocks occur, read the above thread (cubrman's response).
    /// </summary>
    /// <remarks>
    /// This should be run from a using block and disposed after use. It won't 
    /// work properly to keep it around.
    /// </remarks>
    public static int Run(this Process process, string application,
        string arguments, string workingDirectory, out string output,
        out string errors)
    {
        process.StartInfo = new ProcessStartInfo
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            FileName = application,
            Arguments = arguments,
            WorkingDirectory = workingDirectory
        };

        // Use the following event to read both output and errors output.
        var outputBuilder = new StringBuilder();
        var errorsBuilder = new StringBuilder();
        process.OutputDataReceived += (_, args) => outputBuilder.AppendLine(args.Data);
        process.ErrorDataReceived += (_, args) => errorsBuilder.AppendLine(args.Data);

        // Start the process and wait for it to exit.
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        output = outputBuilder.ToString().TrimEnd();
        errors = errorsBuilder.ToString().TrimEnd();
        return process.ExitCode;
    }
}

//#endif