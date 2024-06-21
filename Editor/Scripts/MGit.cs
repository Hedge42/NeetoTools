//#if UNITY_EDITOR

using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using System.Linq;
//using static ak.wwise;
using System.Text;
using Matchwork;
using UnityDropdown.Editor;
using System.Threading;

public class MGit : EditorWindow
{
    private string gitOutput = "";
    private Vector2 scrollPos;
    private int toolbarOption = 0;
    private string[] toolbarTexts = { "Status", "Log", "Branches", "Commit", "Pull" };

    private string fileSelection = "";
    private string commitMessage = "";
    private string selectedBranch = "";
    private string selectedCommit = "";

    static float height = EditorGUIUtility.singleLineHeight - 4;

    private Dictionary<string, List<string>> commitDic;
    private string currentBranch;

    private string _output;

    [MenuItem(MPath.Tools + "Git", priority = -420)]
    public static void ShowWindow()
    {
        GetWindow<MGit>("Git Interface");
    }
    private void OnEnable()
    {
        RefreshAsync().Forget();
    }

    async UniTask RefreshAsync()
    {
        var branchNames = await GetBranchNames();
        foreach (var branch in branchNames)
        {
            await ExecuteCommandAsync($"fetch origin/{branch}");
        }

        //await ExecuteGitCommandAsync("fetch --all origin");
        commitDic = await PopulateDictionaryWithCommitsAsync();
        currentBranch = GetBranchName(await ExecuteCommandAsync("rev-parse --abbrev-ref HEAD"));
    }

    async UniTask<List<string>> GetBranchNames()
    {
        var list = new List<string>();
        //var output = await ExecuteGitCommandAsync("branch -a");
        var output = await ExecuteCommandAsync("branch -a");
        Debug.Log(output);

        string[] branchLines = output.Split('\n');

        foreach (string branchLine in branchLines)
            list.Add(GetBranchName(branchLine));

        return list;
    }
    public async UniTask<Dictionary<string, List<string>>> PopulateDictionaryWithCommitsAsync()
    {
        Dictionary<string, List<string>> branchCommits = new Dictionary<string, List<string>>();

        var branchOutput = await ExecuteCommandAsync("branch -a");
        string[] branchLines = branchOutput.Split('\n');

        foreach (string branchLine in branchLines)
        {
            string branchName = GetBranchName(branchLine);

            if (!string.IsNullOrEmpty(branchName))
            {
                var logOutput = await ExecuteCommandAsync($"log {branchName} --oneline {branchName}");
                string[] commitLines = logOutput.Split('\n');

                List<string> commits = new List<string>();

                foreach (string commitLine in commitLines)
                {
                    string commitMessage = GetCommitMessage(commitLine);

                    if (!string.IsNullOrEmpty(commitMessage))
                    {
                        commits.Add(commitMessage);
                    }
                }

                branchCommits.Add(branchName, commits);
            }
        }

        return branchCommits;
    }
    private string GetBranchName(string branchLine)
    {
        string branchName = branchLine.TrimStart('*').TrimEnd('\n').Trim();
        //Debug.Log($"\"{branchName}\"");
        return branchName;
    }
    private string GetCommitMessage(string commitLine)
    {
        string commitMessage = commitLine.Trim();

        if (!string.IsNullOrEmpty(commitMessage))
        {
            int spaceIndex = commitMessage.IndexOf(' ');

            if (spaceIndex != -1)
            {
                commitMessage = commitMessage.Substring(spaceIndex + 1);
            }
        }

        return commitMessage;
    }

    private void OnGUI()
    {
        GUILayout.Label("This is EXPERIMENTAL and should probably not be used (yet)");

        toolbarOption = GUILayout.Toolbar(toolbarOption, toolbarTexts);

        //scrollPos = this.position.width;

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
        {
            switch (toolbarOption)
            {
                case 0: // Status
                    ExecuteGitCommand("status");
                    EditorGUILayout.LabelField(gitOutput, EditorStyles.wordWrappedLabel);
                    break;
                case 1: // Log
                    DrawBranchSelector();
                    ExecuteGitCommand($"log {selectedBranch} --pretty=format:\"%ad|%an|%h|%s\" --date=short");
                    DisplayCommitsInColumns();
                    break;
                case 2: // Branches
                    ExecuteGitCommand("branch");
                    //PopulateBranchList();
                    EditorGUILayout.LabelField(gitOutput, EditorStyles.wordWrappedLabel);
                    break;
                case 3: // Commit
                    fileSelection = EditorGUILayout.TextField("File selection", fileSelection);
                    commitMessage = EditorGUILayout.TextField("Commit message", commitMessage);

                    if (GUILayout.Button("Add Except Selection"))
                    {
                        ExecuteGitCommand($"add --all :^{fileSelection}");
                    }
                    if (GUILayout.Button("Add Only Selection"))
                    {
                        ExecuteGitCommand($"add {fileSelection}");
                    }
                    if (GUILayout.Button("Remove Selection"))
                    {
                        ExecuteGitCommand($"reset {fileSelection}");
                    }
                    if (GUILayout.Button("Clear Additions"))
                    {
                        ExecuteGitCommand("reset");
                    }
                    if (GUILayout.Button("diff"))
                    {
                        if (!string.IsNullOrEmpty(fileSelection))
                            _output = ExecuteGitCommand($"diff -r --summary {selectedBranch} -- {fileSelection}");
                        else
                            _output = ExecuteGitCommand($"diff -r --summary {selectedBranch}");
                    }
                    if (GUILayout.Button("diff Except"))
                    {
                        if (!string.IsNullOrEmpty(fileSelection))
                            _output = ExecuteGitCommand($"diff -r --summary {selectedBranch} -- :^{fileSelection}");
                        //else
                        //_output = ExecuteGitCommand($"diff -r --summary {selectedBranch}");
                    }

                    if (GUILayout.Button("Commit"))
                    {
                        ExecuteGitCommand($"commit -m \"{commitMessage}\"");
                        Refresh();
                    }


                    if (GUILayout.Button("Push Current Branch"))
                    {
                        ExecuteGitCommand("push");
                    }

                    DisplayStaging();

                    break;
                case 4: // Pull
                    fileSelection = EditorGUILayout.TextField("Selection", fileSelection);

                    DrawBranchSelector();

                    var commitHashes = commitDic[selectedBranch];
                    int commitIndex = 0; //Initialization
                    commitIndex = EditorGUILayout.Popup("Commit:", commitIndex != -1 ? commitIndex : 0, commitHashes.ToArray(), GUILayout.ExpandWidth(true));
                    selectedCommit = commitHashes[commitIndex];

                    if (GUILayout.Button("Checkout Only Selection"))
                    {
                        ExecuteGitCommand($"checkout {selectedBranch} -- {fileSelection}");
                    }
                    if (GUILayout.Button("Checkout Except Selection"))
                    {
                        ExecuteGitCommand($"checkout {selectedBranch} -- :^{fileSelection}");
                    }

                    if (GUILayout.Button("diff"))
                    {
                        if (!string.IsNullOrEmpty(fileSelection))
                            //_output = ExecuteGitCommand($"diff --name-only --cached {selectedBranch}~1 -- .{preserveFiles}");
                            _output = ExecuteGitCommand($"diff -R --summary {selectedBranch} -- {fileSelection}");
                        //_output = ExecuteGitCommand($"diff --name-only HEAD..{GetCommitHash(selectedCommit, selectedBranch)}");
                        else
                            _output = ExecuteGitCommand($"diff -R --summary {selectedBranch}");
                    }

                    if (GUILayout.Button("Pull Except Selection", GUILayout.ExpandWidth(true)))
                    {
                        string tempBranch = $"temp_{System.Guid.NewGuid()}";
                        string switchCommitMessage = $"Switching for pulling from branch: {selectedBranch}, commit: {selectedCommit}";

                        if (!string.IsNullOrEmpty(fileSelection))
                        {
                            ExecuteGitCommand($"add . && git commit -m \"{switchCommitMessage}\" && git checkout -b {tempBranch} && git pull {selectedBranch} {selectedCommit} --strategy-option theirs && git checkout {tempBranch} -- {fileSelection} && git branch -d {tempBranch}");
                        }
                        else
                        {
                            ExecuteGitCommand($"add . && git commit -m \"{switchCommitMessage}\" && git checkout -b {tempBranch} && git pull {selectedBranch} {selectedCommit} --strategy-option theirs && git branch -d {tempBranch}");
                        }

                        Refresh();
                    }
                    else if (GUILayout.Button("Pull Only Selection"))
                    {

                    }

                    DisplayOutput();
                    break;
            }
        }
        EditorGUILayout.EndScrollView();
    }
    void DisplayOutput()
    {
        var lines = _output.Split('\n');
        foreach (var line in lines)
        {
            GUILayout.Label(line);
        }
    }
    void DisplayStaging()
    {
        var output = ExecuteGitCommand("diff --name-only --cached");
        var lines = output.Split('\n');

        foreach (var line in lines)
        {
            GUILayout.Label(line);
        }
    }
    void DrawBranchSelector()
    {
        var branchNames = commitDic.Keys.ToList();
        selectedBranch = branchNames.Contains(selectedBranch) ? selectedBranch : currentBranch;
        int branchIndex = branchNames.IndexOf(selectedBranch);
        branchIndex = branchIndex < 0 ? branchNames.IndexOf(currentBranch) : branchIndex;
        branchIndex = EditorGUILayout.Popup("Branch", branchIndex, branchNames.ToArray(), GUILayout.ExpandWidth(true));
        selectedBranch = branchNames[branchIndex];
    }
    private void DisplayCommitsInColumns()
    {
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.SelectableLabel("Date", EditorStyles.label, GUILayout.Width(80), GUILayout.Height(height));
            EditorGUILayout.SelectableLabel("Author", EditorStyles.label, GUILayout.Width(100), GUILayout.Height(height));
            EditorGUILayout.SelectableLabel("Hash", EditorStyles.label, GUILayout.Width(60), GUILayout.Height(height));
            EditorGUILayout.SelectableLabel("Message", EditorStyles.label, GUILayout.Height(height));
        }
        EditorGUILayout.EndHorizontal();

        string[] commits = gitOutput.Split(new[] { "\n" }, System.StringSplitOptions.None);
        foreach (string commit in commits)
        {
            string[] elements = commit.Split('|');
            if (elements.Length == 4)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.SelectableLabel(elements[0].Trim(), EditorStyles.label, GUILayout.Width(80), GUILayout.Height(height));
                    EditorGUILayout.SelectableLabel(elements[1].Trim(), EditorStyles.label, GUILayout.Width(100), GUILayout.Height(height));
                    EditorGUILayout.SelectableLabel(elements[2].Trim(), EditorStyles.label, GUILayout.Width(60), GUILayout.Height(height));
                    EditorGUILayout.SelectableLabel(elements[3].Trim(), EditorStyles.label, GUILayout.Height(height));
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
    private void Refresh()
    {
        switch (toolbarOption)
        {
            case 0: // Status
                ExecuteGitCommand("status");
                break;
            case 1: // Log
                ExecuteGitCommand("log --pretty=format:\"%ad|%an|%h|%s\" --date=short");
                //PopulateCommitHashes();
                break;
            case 2: // Branches
                ExecuteGitCommand("branch");
                //PopulateBranchList();
                break;
            case 3: // Commit
                ExecuteGitCommand("status");
                break;
            case 4: // Pull
                ExecuteGitCommand("status");
                break;
        }
    }
    private string ExecuteGitCommand(string gitArguments)
    {
        string gitCommand = "git";

        Process gitProcess = new Process();
        gitProcess.StartInfo.FileName = gitCommand;
        gitProcess.StartInfo.Arguments = gitArguments;
        gitProcess.StartInfo.RedirectStandardOutput = true;
        gitProcess.StartInfo.RedirectStandardError = true;  // Added line
        gitProcess.StartInfo.UseShellExecute = false;
        gitProcess.StartInfo.CreateNoWindow = true;
        gitProcess.StartInfo.WorkingDirectory = Application.dataPath.Replace("/Assets", "");

        gitProcess.Start();

        string gitOutput = gitProcess.StandardOutput.ReadToEnd();
        string gitError = gitProcess.StandardError.ReadToEnd();  // Added line

        gitProcess.WaitForExit();
        gitProcess.Close();

        if (!string.IsNullOrEmpty(gitError))  // Added condition
        {
            // Handle or log the error message from Git
            // You can display, throw an exception, or handle it as needed
            // For example:
            Debug.LogError("Git error: " + gitError);
        }

        return this.gitOutput = gitOutput;
    }

    /// <summary>
    /// Runs git.exe with the specified arguments and returns the output.
    /// </summary>
    public static string Run(string arguments)
    {
        Debug.Log("Running git args: " + arguments);

        using (var process = new System.Diagnostics.Process())
        {
            var exitCode = process.Run(@"git", arguments, Application.dataPath.Replace("/Assets", ""),
                out var output, out var errors);

            if (exitCode != 0)
            {
                Debug.Log("Errors: " + errors);
            }

            output = $"(git) exited with ({exitCode})\n{output}";

            Debug.Log(output);

            return output;
        }
    }

    public static void GitLog()
    {
        Debug.Log(RunCommand("git log", 5f));
    }
    public static void GitFetch()
    {
        RunCommand("git fetch origin", 20f);
        Debug.Log($"fetch completed");
    }
    public static void GitCommit()
    {
        var message = $"{DateTime.Now:MM_HH_mm_ss}";
        Debug.Log(RunCommand($"git add . && git commit -am \"{message}\"", 6f));
    }


    public static async UniTask<string> ExecuteCommandAsync(params string[] args)
    {
        var output = new StringBuilder();
        foreach (var arg in args)
        {
            string gitCommand = "git";

            Process gitProcess = new Process();
            gitProcess.StartInfo.FileName = gitCommand;
            gitProcess.StartInfo.Arguments = arg;
            gitProcess.StartInfo.RedirectStandardOutput = true;
            gitProcess.StartInfo.UseShellExecute = false;
            gitProcess.StartInfo.CreateNoWindow = true;
            gitProcess.StartInfo.WorkingDirectory = Application.dataPath.Replace("/Assets", "");

            gitProcess.Start();

            await UniTask.WaitUntil(() => gitProcess.HasExited).Timeout(TimeSpan.FromSeconds(5f));

            output.AppendLine(gitProcess.StandardOutput.ReadToEnd());

            gitProcess.WaitForExit();
            gitProcess.Close();
        }
        return output.ToString();
    }

    [MenuItem("Assets/Git/Commit Selected", false, 200)]
    public static void CommitSelected()
    {
        var selectedAssetPaths = GetSelectedAssetPaths();
        if (selectedAssetPaths.Length == 0) return;

        selectedAssetPaths = selectedAssetPaths.Select(path => "\"" + path + "\"").ToArray();

        var paths = string.Join(" ", selectedAssetPaths);
        var commitMessage = $"update {selectedAssetPaths.Length} assets: {paths}";
        Run($"commit -m \"{commitMessage}\"");
    }
    [MenuItem("Assets/Git/Add Selected", false, 200)]
    public static void AddSelected()
    {
        var selectedAssetPaths = GetSelectedAssetPaths();
        if (selectedAssetPaths.Length == 0) return;

        selectedAssetPaths = selectedAssetPaths.Select(path => "\"" + path + "\"").ToArray();

        var paths = string.Join(" ", selectedAssetPaths);
        Run($"add {paths}");
    }

    [MenuItem("Assets/Git/Checkout Selected", false, 200)]
    public static void CheckoutSelected()
    {
        var selectedAssetPaths = GetSelectedAssetPaths();
        if (selectedAssetPaths.Length == 0) return;

        var paths = selectedAssetPaths.Select(path => "\"" + path + "\"").ToArray();
        var objs = selectedAssetPaths.Select(path => AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object))).ToArray();

        Undo.RecordObjects(objs, "Checkout files");

        Run($"checkout -- {string.Join(" ", paths)}");
    }
    [MenuItem("Assets/Git/Revert Selected to Previous Commit", false, 200)]
    public static void RevertSelectedToPreviousCommit()
    {
        var selectedAssetPaths = GetSelectedAssetPaths();
        if (selectedAssetPaths.Length == 0) return;

        foreach (var path in selectedAssetPaths)
        {
            var relativePath = path.Substring(Application.dataPath.Length + 1).Replace("\\", "/");
            var logCommand = $"log -n 2 --pretty=format:%H -- \"{relativePath}\"";
            ExecuteCommand(logCommand);
            var commitHashes = output.Split('\n');

            if (commitHashes.Length < 2)
            {
                UnityEngine.Debug.LogWarning($"No previous commit found for {relativePath}");
                continue;
            }

            var previousCommitHash = commitHashes[1].Trim();
            var showCommand = $"show {previousCommitHash}:\"{relativePath}\"";
            ExecuteCommand(showCommand);
            var fileContent = output;

            File.WriteAllText(path, fileContent);
            UnityEngine.Debug.Log($"Reverted '{relativePath}' to its previous state in commit '{previousCommitHash}'");
        }

        AssetDatabase.Refresh();
    }

    public static string[] GetSelectedAssetPaths()
    {
        var selectedObjects = Selection.objects;
        var paths = new string[selectedObjects.Length];
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            paths[i] = AssetDatabase.GetAssetPath(selectedObjects[i]);
        }
        return paths;
    }

    static string output;
    public static void ExecuteCommand(string command)
    {
        Debug.Log($"(git) {command}");

        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = command,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = Application.dataPath
        };

        using (var process = Process.Start(startInfo))
        {
            if (process == null) return;

            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(output))
                Debug.Log($"Git Output: {output}");
            if (!string.IsNullOrEmpty(error))
                Debug.LogError($"Git Error: {error}");
        }
    }

    public static string RunCommand(string command, float timeout = -1)
    {
        Debug.Log($"running '{command}'...");

        var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = processInfo })
        {
            process.Start();

            if (timeout > 0f)
                process.WaitForExit((int)(timeout * 1000f));
            else
                process.WaitForExit();

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            Debug.Log($"exited with ({process.ExitCode}) '{command}'");

            if (!string.IsNullOrWhiteSpace(error))
            {
                return $"Error: {error}";
            }

            return output;
        }
    }
    public static async UniTask<string> RunCommandAsync(string command, float timeout = -1f)
    {

        var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = processInfo })
        {
            var cts = new CancellationTokenSource();
            try
            {
                var exit = UniTask.WaitUntil(() => process.HasExited, cancellationToken: cts.Token);
                if (timeout > 0f)
                {
                    bool timedOut = false;
                    cts.Token.Register(() => timedOut = true);
                    var time = UniTask.Delay(TimeSpan.FromSeconds(timeout), DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, cts.Token);
                    await UniTask.WhenAny(exit, time);
                    timedOut.Log($"Command timed out ({command})");
                }
                else
                {
                    await exit;
                }
            }
            finally
            {
                cts.Kill();
            }

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(error))
            {
                return $"Error: {error}";
            }

            return output;
        }
    }
}

// https://gist.github.com/edwardrowe/fdec706fe53bfff0671e063f263ada63
public static class ProcessExtensions
{
    /* Properties ============================================================================================================= */

    /* Methods ================================================================================================================ */

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