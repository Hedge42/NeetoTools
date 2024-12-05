using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityDropdown.Editor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Neeto
{
    [InitializeOnLoad]
    static class CustomProjectContext
    {
        static CustomProjectContext()
        {
            // Hook into the event that detects right-click on project window items
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
        }

        static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
        {
            // hold control to toggle
            if (NeetoSettings.instance.experimentalEditorFeatures == Event.current.control)
                return;

            // was the rect right-clicked?
            if (Event.current.type == EventType.ContextClick && selectionRect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                ShowDropdown(guid);
            }
        }

        static void ShowDropdown(string guid)
        {
            var items = new List<DropdownItem<(int priority, Action action)>>();

            // Add regular MenuItem attributes (filtered by Assets/)
            foreach (var method in TypeCache.GetMethodsWithAttribute<MenuItem>())
            {
                foreach (var attribute in method.GetCustomAttributes<MenuItem>(false))
                {
                    // Check if the MenuItemAttribute exists and starts with "Assets/"
                    if (attribute == null || !attribute.menuItem.ToLower().StartsWith("assets/"))
                        continue;

                    // Skip validation methods
                    if (attribute.validate)
                        continue;

                    if (NeetoSettings.instance.contextBlacklist.Contains(attribute.menuItem))
                        continue;

                    // Run the validation check
                    if (!PassesValidation(attribute))
                        continue;

                    // Create a dropdown item for each matching menu method
                    items.Add(new DropdownItem<(int priority, Action action)>((attribute.priority, () => InvokeMenuMethod(method, guid)), attribute.menuItem.Substring(7)));
                }
            }

            // Add CreateAssetMenu items
            AddCreateAssetMenuItems(items);

            // Add custom items like "Show in Explorer", "Delete", etc.
            AddCustomItems(items, AssetDatabase.GUIDToAssetPath(guid));

            // Sort items by priority
            items = items.OrderBy(item => item.Value.priority).ToList();

            var menu = new DropdownMenu<(int priority, Action action)>(items, _ => _.action.Invoke());
            menu.ExpandAllFolders();
            menu.ShowAsContext();
        }

        // Invoke menu methods, handling methods that expect parameters like MenuCommand
        static void InvokeMenuMethod(MethodInfo method, string guid)
        {
            var parameters = method.GetParameters();
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(assetPath);

            if (parameters.Length == 0)
            {
                // No parameters expected
                method.Invoke(null, null);
            }
            else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(MenuCommand))
            {
                // MenuCommand parameter expected
                var menuCommand = new MenuCommand(asset);
                method.Invoke(null, new object[] { menuCommand });
            }
            else
            {
                Debug.LogWarning($"Unsupported parameters in method: {method.Name}");
            }
        }

        static void AddCreateAssetMenuItems(List<DropdownItem<(int priority, Action action)>> items)
        {
            // Find all types with CreateAssetMenuAttribute
            foreach (var type in TypeCache.GetTypesWithAttribute<CreateAssetMenuAttribute>())
            {
                var createAssetMenuAttr = type.GetCustomAttribute<CreateAssetMenuAttribute>();
                if (createAssetMenuAttr != null)
                {
                    string menuName = createAssetMenuAttr.menuName ?? type.Name;
                    int priority = createAssetMenuAttr.order;

                    // Create the asset creation action
                    Action createAssetAction = () =>
                    {
                        var asset = ScriptableObject.CreateInstance(type);
                        var assetPath = Selection.activeObject ? AssetDatabase.GetAssetPath(Selection.activeObject) : $"Assets/";
                        var systeMenu = Path.Combine(Path.GetDirectoryName(Application.dataPath), assetPath);
                        if (!Directory.Exists(systeMenu)) // is file?
                            assetPath = Path.GetDirectoryName(assetPath);
                        assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(assetPath, $"{type.Name}.asset"));
                        AssetDatabase.CreateAsset(asset, assetPath);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        EditorUtility.FocusProjectWindow();
                        Selection.activeObject = asset;
                    };

                    // Add the item to the context menu
                    items.Add(new DropdownItem<(int priority, Action action)>((priority, createAssetAction), $"Create/{menuName}"));
                }
            }
        }

        static bool PassesValidation(MenuItem attribute)
        {
            var map = GetValidationMap();

            if (map.ContainsKey(attribute.menuItem))
                return map[attribute.menuItem].Invoke();
            else
                return true;
        }

        static Dictionary<string, Func<bool>> validationMap;
        static Dictionary<string, Func<bool>> GetValidationMap()
        {
            if (validationMap != null)
                return validationMap;

            validationMap = new();
            foreach (var method in TypeCache.GetMethodsWithAttribute<MenuItem>())
            {
                if (!method.ReturnType.Equals(typeof(bool)))
                    continue;

                foreach (var attribute in method.GetCustomAttributes<MenuItem>(false))
                {
                    // Only collect methods marked with 'validate = true'
                    if (attribute != null && attribute.validate)
                    {
                        var c = method.GetParameters().Count();
                        validationMap.Add(attribute.menuItem, () => (bool)method.Invoke(null, new object[c]));
                    }
                }
            }

            return validationMap;
        }

        // Manually add custom items like "Show in Explorer", "Delete", etc.
        static void AddCustomItems(List<DropdownItem<(int priority, Action action)>> items, string assetPath)
        {
            // Show in Explorer
            items.Add(new DropdownItem<(int priority, Action action)>((10, () => EditorUtility.RevealInFinder(assetPath)), "Show in Explorer"));

            // Delete
            items.Add(new DropdownItem<(int priority, Action action)>((20, () => AssetDatabase.DeleteAsset(assetPath)), "Delete"));

            // Rename
            items.Add(new DropdownItem<(int priority, Action action)>((20, () => RenameAsset(assetPath)), "Rename"));

            // Copy Path
            items.Add(new DropdownItem<(int priority, Action action)>((19, () => EditorGUIUtility.systemCopyBuffer = assetPath), "Copy Path"));

            // Export Package
            items.Add(new DropdownItem<(int priority, Action action)>((50, () => ExportPackage(assetPath)), "Export Package..."));

            // Create Script
            items.Add(new DropdownItem<(int priority, Action action)>((-10000, () => CreateNewScript(assetPath)), "Create/C# Script"));

            // Create Folder
            items.Add(new DropdownItem<(int priority, Action action)>((-10000, () => CreateFolder(assetPath)), "Create/New Folder"));

            // Select Dependencies
            items.Add(new DropdownItem<(int priority, Action action)>((40, () => Selection.objects = EditorUtility.CollectDependencies(new UnityEngine.Object[] { AssetDatabase.LoadMainAssetAtPath(assetPath) })), "Select Dependencies"));

            // Refresh
            items.Add(new DropdownItem<(int priority, Action action)>((70, () => AssetDatabase.Refresh()), "Refresh"));

            // Reimport
            items.Add(new DropdownItem<(int priority, Action action)>((70, () => AssetDatabase.ImportAsset(assetPath)), "Reimport"));

            // Reimport All
            items.Add(new DropdownItem<(int priority, Action action)>((70, () => AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate)), "Reimport All"));
        }

        // Helper method to rename an asset
        static void RenameAsset(string assetPath)
        {
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(assetPath);
            EditorWindow.focusedWindow.SendEvent(new Event { keyCode = KeyCode.F2, type = EventType.KeyDown });
        }

        // Helper method to export a package
        static void ExportPackage(string assetPath)
        {
            AssetDatabase.ExportPackage(assetPath, $"{Path.GetFileName(assetPath)}.unitypackage", ExportPackageOptions.Default);
        }

        // Helper method to trigger Unity's built-in "Create C# Script" functionality
        static void CreateNewScript(string assetPath)
        {
            // Set the target folder in the project window
            string folderPath = Directory.Exists(assetPath) ? assetPath : Path.GetDirectoryName(assetPath);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(folderPath);

            // Execute Unity's built-in menu item for creating a C# script
            EditorApplication.ExecuteMenuItem("Assets/Create/C# Script");
        }

        // Helper method to create a new folder
        static void CreateFolder(string assetPath)
        {
            // Determine the folder path to create the new folder
            string folderPath = Directory.Exists(assetPath) ? assetPath : Path.GetDirectoryName(assetPath);
            string newFolderPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folderPath, "New Folder"));

            // Create the folder in the specified directory
            AssetDatabase.CreateFolder(folderPath, "New Folder");
            AssetDatabase.Refresh(); // Refresh the asset database to reflect changes
        }

    }
}
