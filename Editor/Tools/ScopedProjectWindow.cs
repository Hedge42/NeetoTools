#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class ScopedProjectWindow : EditorWindow
{
    const string MENU_NAME = "Assets/Show in Scope";

    [SerializeField] private string rootFolderPath;
    [SerializeField] private TreeViewState treeViewState;
    private FolderTreeView treeView;
    private SearchField searchField;
    private bool showSearchBar = false; // Default the search bar to off

    // Set the menu order to 66
    [MenuItem(MENU_NAME, false, 19)]
    static void OpenWindow()
    {
        var selectedObject = Selection.activeObject;
        string path = AssetDatabase.GetAssetPath(selectedObject);

        if (AssetDatabase.IsValidFolder(path))
        {
            var window = CreateInstance<ScopedProjectWindow>();
            window.Init(path);
            window.Show();
        }
    }

    [MenuItem(MENU_NAME, true, 19)]
    static bool ValidateOpenWindow()
    {
        var selectedObject = Selection.activeObject;
        if (selectedObject == null) return false;
        string path = AssetDatabase.GetAssetPath(selectedObject);
        return AssetDatabase.IsValidFolder(path);
    }

    public void Init(string rootPath)
    {
        rootFolderPath = rootPath;
        treeViewState = treeViewState ?? new TreeViewState();
        treeView = new FolderTreeView(treeViewState, rootFolderPath);

        searchField = searchField ?? new SearchField();
        searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;

        treeView.Reload();

        string folderName = Path.GetFileName(rootPath);
        // Remove the tooltip by not providing it
        titleContent = new GUIContent(folderName, EditorGUIUtility.IconContent("FolderOpened Icon").image);
    }

    void OnEnable()
    {
        treeViewState = treeViewState ?? new TreeViewState();
        if (treeView == null && !string.IsNullOrEmpty(rootFolderPath))
        {
            treeView = new FolderTreeView(treeViewState, rootFolderPath);
            treeView.Reload();
        }

        searchField = searchField ?? new SearchField();
        if (treeView != null)
            searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;

        EditorApplication.projectChanged += OnProjectChanged;
    }

    void OnDisable()
    {
        EditorApplication.projectChanged -= OnProjectChanged;
    }

    void OnGUI()
    {
        if (treeView != null)
        {
            float toolbarHeight = 20f;
            if (showSearchBar)
            {
                Rect searchBarRect = new Rect(0, 0, position.width, toolbarHeight);
                treeView.searchString = searchField.OnToolbarGUI(searchBarRect, treeView.searchString);
            }

            float treeViewTop = showSearchBar ? toolbarHeight : 0;
            float bottomToolbarHeight = 22f;
            float treeViewHeight = position.height - treeViewTop - bottomToolbarHeight;
            Rect treeViewRect = new Rect(0, treeViewTop, position.width, treeViewHeight);
            treeView.OnGUI(treeViewRect);

            DrawBottomToolbar(bottomToolbarHeight);
        }
        else
        {
            EditorGUILayout.LabelField("No folder selected.");
        }
    }

    void DrawBottomToolbar(float toolbarHeight)
    {
        Rect toolbarRect = new Rect(0, position.height - toolbarHeight, position.width, toolbarHeight);
        EditorGUI.DrawRect(toolbarRect, EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.8f, 0.8f, 0.8f));

        GUILayout.BeginArea(toolbarRect);
        GUILayout.BeginHorizontal();

        float buttonWidth = 23f;

        // Toolbar buttons
        GUIContent searchBarIcon = EditorGUIUtility.IconContent("Search Icon");
        GUI.color = showSearchBar ? Color.white : Color.gray;
        if (GUILayout.Button(new GUIContent(searchBarIcon.image, showSearchBar ? "Hide Search Bar" : "Show Search Bar"), EditorStyles.toolbarButton, GUILayout.Width(buttonWidth)))
        {
            showSearchBar = !showSearchBar;
            Repaint();
        }
        GUI.color = Color.white;

        // Open Root Folder Button
        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("Folder Icon").image, "Open in Explorer"), EditorStyles.toolbarButton, GUILayout.Width(buttonWidth)))
        {
            OpenRootFolderInExplorer();
        }

        // Ping Script Button
        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("cs Script Icon").image, "Ping Script"), EditorStyles.toolbarButton, GUILayout.Width(buttonWidth)))
        {
            PingScript();
        }

        // Display Root Asset Path as selectable label
        GUILayout.Space(5); // Add padding
        GUIStyle pathStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            normal = { textColor = EditorStyles.label.normal.textColor }
        };
        EditorGUILayout.SelectableLabel(rootFolderPath, pathStyle, GUILayout.Height(toolbarHeight));

        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }


    void OpenRootFolderInExplorer()
    {
        string fullPath = Path.GetFullPath(rootFolderPath);

        // For Windows
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            System.Diagnostics.Process.Start("explorer.exe", fullPath);
        }
        // For macOS
        else if (Application.platform == RuntimePlatform.OSXEditor)
        {
            System.Diagnostics.Process.Start("open", fullPath);
        }
        // For Linux or other platforms
        else
        {
            Debug.LogWarning("Opening folders is not implemented for this platform.");
        }
    }


    void PingScript()
    {
        MonoScript script = MonoScript.FromScriptableObject(this);
        if (script != null)
        {
            EditorGUIUtility.PingObject(script);
            Selection.activeObject = script;
        }
    }

    void OnSelectionChange()
    {
        if (treeView != null)
        {
            treeView.OnSelectionChange();
            Repaint();
        }
    }

    void OnProjectChanged()
    {
        if (treeView != null)
        {
            treeView.Reload();
            Repaint();
        }
    }
}

class FolderTreeView : TreeView
{
    private string rootPath;
    private Dictionary<int, string> idToPath = new Dictionary<int, string>();
    private Dictionary<string, int> pathToId = new Dictionary<string, int>();
    private int idCounter = 0;

    public FolderTreeView(TreeViewState state, string rootPath) : base(state)
    {
        this.rootPath = NormalizePath(rootPath);
        showAlternatingRowBackgrounds = true;
    }

    protected override TreeViewItem BuildRoot()
    {
        idToPath.Clear();
        pathToId.Clear();
        idCounter = 0;

        var rootItem = new TreeViewItem { id = GetUniqueID(), depth = -1 };

        if (string.IsNullOrEmpty(searchString))
            AddChildrenRecursive(rootItem, rootPath, 0);
        else
            BuildTreeForSearchResults(rootItem, GetSearchResults());

        rootItem.children = rootItem.children ?? new List<TreeViewItem>();
        SetupDepthsFromParentsAndChildren(rootItem);
        return rootItem;
    }

    private TreeViewItem CreateTreeViewItem(string path, int depth)
    {
        int id = GetUniqueID();
        string displayName = AssetDatabase.IsValidFolder(path) ? Path.GetFileName(path) : Path.GetFileNameWithoutExtension(path);

        var item = new TreeViewItem
        {
            id = id,
            depth = depth,
            displayName = displayName,
            icon = GetIcon(path)
        };

        idToPath[id] = path;
        pathToId[path] = id;
        return item;
    }

    private void AddChildrenRecursive(TreeViewItem parentItem, string parentPath, int depth)
    {
        foreach (string folder in AssetDatabase.GetSubFolders(parentPath))
        {
            var folderItem = CreateTreeViewItem(folder, depth);
            parentItem.AddChild(folderItem);
            AddChildrenRecursive(folderItem, folder, depth + 1);
        }

        foreach (string assetPath in AssetDatabase.FindAssets("", new[] { parentPath }).Select(AssetDatabase.GUIDToAssetPath).Distinct())
        {
            if (AssetDatabase.IsValidFolder(assetPath)) continue;

            string assetParentPath = NormalizePath(Path.GetDirectoryName(assetPath));
            if (!string.Equals(assetParentPath, NormalizePath(parentPath), System.StringComparison.OrdinalIgnoreCase))
                continue;

            var assetItem = CreateTreeViewItem(assetPath, depth);
            parentItem.AddChild(assetItem);
        }
    }

    private List<string> GetSearchResults()
    {
        return AssetDatabase.FindAssets(searchString, new[] { rootPath }).Select(AssetDatabase.GUIDToAssetPath).ToList();
    }

    private void BuildTreeForSearchResults(TreeViewItem rootItem, List<string> searchResults)
    {
        var pathToItemMap = new Dictionary<string, TreeViewItem> { [rootPath] = rootItem };

        foreach (string assetPath in searchResults)
        {
            string normalizedAssetPath = NormalizePath(assetPath);
            if (!normalizedAssetPath.StartsWith(rootPath)) continue;

            string relativePath = normalizedAssetPath.Substring(rootPath.Length).TrimStart('/');
            string[] folders = relativePath.Split('/');

            string currentPath = rootPath;
            TreeViewItem parent = rootItem;

            for (int i = 0; i < folders.Length; i++)
            {
                string folderOrAsset = folders[i];
                currentPath = $"{currentPath}/{folderOrAsset}";

                if (!pathToItemMap.TryGetValue(currentPath, out TreeViewItem item))
                {
                    int depth = i;
                    var newItem = CreateTreeViewItem(currentPath, depth);
                    parent.AddChild(newItem);
                    pathToItemMap[currentPath] = newItem;
                    parent = newItem;
                }
                else
                {
                    parent = item;
                }
            }
        }

        rootItem.children = rootItem.children ?? new List<TreeViewItem>();
    }

    private string NormalizePath(string path) => path.Replace("\\", "/");

    private int GetUniqueID() => idCounter++;

    private Texture2D GetIcon(string assetPath)
    {
        var icon = AssetDatabase.GetCachedIcon(assetPath) as Texture2D;
        return icon ?? EditorGUIUtility.FindTexture("DefaultAsset Icon");
    }

    protected override void RowGUI(RowGUIArgs args)
    {
        base.RowGUI(args);

        if (idToPath.TryGetValue(args.item.id, out string assetPath))
        {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            EditorApplication.projectWindowItemOnGUI?.Invoke(guid, args.rowRect);
        }
    }

    protected override bool CanMultiSelect(TreeViewItem item) => true;

    protected override void SelectionChanged(IList<int> selectedIds)
    {
        Selection.objects = selectedIds.Select(id =>
        {
            idToPath.TryGetValue(id, out string assetPath);
            return AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        }).Where(obj => obj != null).ToArray();
    }

    public void OnSelectionChange()
    {
        SetSelection(Selection.objects.Select(obj =>
        {
            string path = AssetDatabase.GetAssetPath(obj);
            return pathToId.TryGetValue(path, out int id) ? id : -1;
        }).Where(id => id != -1).ToList(), TreeViewSelectionOptions.RevealAndFrame);
    }

    protected override void DoubleClickedItem(int id)
    {
        if (idToPath.TryGetValue(id, out string assetPath))
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<Object>(assetPath));
    }

    protected override void ContextClickedItem(int id)
    {
        if (idToPath.TryGetValue(id, out string assetPath))
        {
            var obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (obj != null)
            {
                EditorUtility.DisplayPopupMenu(new Rect(Event.current.mousePosition, Vector2.zero), "Assets/", new MenuCommand(obj));
                Event.current.Use();
            }
        }
    }

    protected override bool CanStartDrag(CanStartDragArgs args) => true;

    protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
    {
        DragAndDrop.PrepareStartDrag();
        DragAndDrop.objectReferences = args.draggedItemIDs.Select(id =>
        {
            idToPath.TryGetValue(id, out string assetPath);
            return AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        }).Where(obj => obj != null).ToArray();
        DragAndDrop.paths = DragAndDrop.objectReferences.Select(AssetDatabase.GetAssetPath).ToArray();
        DragAndDrop.StartDrag("Dragging Assets");
    }

    protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
    {
        if (args.dragAndDropPosition == DragAndDropPosition.OutsideItems)
            return DragAndDropVisualMode.None;

        if (args.performDrop && idToPath.TryGetValue(args.parentItem.id, out string parentPath))
        {
            string folderPath = AssetDatabase.IsValidFolder(parentPath) ? parentPath : Path.GetDirectoryName(parentPath);

            // Expand the folder after moving
            SetExpanded(args.parentItem.id, true);

            // Since Undo doesn't support asset moves directly, we'll prompt the user
            bool proceed = EditorUtility.DisplayDialog("Move Assets", "Undo is not supported for moving assets via this window. Proceed?", "Yes", "No");
            if (!proceed)
            {
                Event.current.Use(); // Consume the event to prevent further processing
                return DragAndDropVisualMode.Rejected;
            }

            DragAndDrop.AcceptDrag();

            foreach (var obj in DragAndDrop.objectReferences)
            {
                string sourcePath = AssetDatabase.GetAssetPath(obj);
                string destPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folderPath, Path.GetFileName(sourcePath)));

                AssetDatabase.MoveAsset(sourcePath, destPath);
            }

            AssetDatabase.Refresh();
            Reload();
            return DragAndDropVisualMode.Move;
        }

        return DragAndDropVisualMode.Generic;
    }

}
#endif