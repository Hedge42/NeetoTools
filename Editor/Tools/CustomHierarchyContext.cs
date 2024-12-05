using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityDropdown.Editor;
using System.Collections.Generic;
using System.Linq;

namespace Neeto
{
    [InitializeOnLoad]
    static class CustomHierarchyContext
    {
        static CustomHierarchyContext()
        {
            // Hook into the event that detects right-click on game objects in the hierarchy window
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemGUI;
        }

        static void OnHierarchyWindowItemGUI(int instanceID, Rect selectionRect)
        {
            // hold control to toggle
            if (NeetoSettings.instance.experimentalEditorFeatures == Event.current.control)
                return;

            // was this object right-clicked?
            if (Event.current.type == EventType.ContextClick && selectionRect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                ShowDropdown((GameObject)EditorUtility.InstanceIDToObject(instanceID));
            }
        }

        static void ShowDropdown(GameObject selectedObject)
        {
            var items = new List<DropdownItem<(int priority, Action action)>>();

            // Add MenuItem attributes dynamically (from TypeCache)
            foreach (var method in TypeCache.GetMethodsWithAttribute<MenuItem>())
            {
                foreach (var attribute in method.GetCustomAttributes<MenuItem>(false))
                {
                    // Check if the MenuItemAttribute exists
                    if (attribute == null)
                        continue;

                    // Only include items that start with "GameObject/"
                    if (!attribute.menuItem.ToLower().StartsWith("gameobject/"))
                        continue;

                    // Skip validation methods
                    if (attribute.validate)
                        continue;

                    // Run the validation check
                    if (!PassesValidation(attribute))
                        continue;

                    // Create a dropdown item for each matching menu method
                    items.Add(new DropdownItem<(int priority, Action action)>((attribute.priority, () => InvokeMenuMethod(method, selectedObject)), attribute.menuItem.Substring(11)));
                }
            }

            // Add custom items (e.g., "Set Active", "Duplicate", etc.)
            AddCustomItems(items, selectedObject);

            // Sort items by priority
            items = items.OrderBy(item => item.Value.priority).ToList();

            var menu = new DropdownMenu<(int priority, Action action)>(items, _ => _.action.Invoke());
            menu.ExpandAllFolders();
            menu.ShowAsContext();
        }

        // Invoke menu methods, handling methods that expect parameters like MenuCommand
        static void InvokeMenuMethod(MethodInfo method, GameObject selectedObject)
        {
            var parameters = method.GetParameters();
            if (parameters.Length == 0)
            {
                // No parameters expected
                method.Invoke(null, null);
            }
            else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(MenuCommand))
            {
                // MenuCommand parameter expected
                var menuCommand = new MenuCommand(selectedObject);
                method.Invoke(null, new object[] { menuCommand });
            }
            else
            {
                Debug.LogWarning($"Unsupported parameters in method: {method.Name}");
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

        // Manually add custom items for GameObject context actions
        static void AddCustomItems(List<DropdownItem<(int priority, Action action)>> items, GameObject selectedObject)
        {
            if (selectedObject != null)
            {
                // Add GameObject specific actions
                // Set Active
                items.Add(new DropdownItem<(int priority, Action action)>((10, () => selectedObject.SetActive(!selectedObject.activeSelf)), "Set Active/Toggle"));

                // Duplicate
                items.Add(new DropdownItem<(int priority, Action action)>((20, () => DuplicateObject(selectedObject)), "Duplicate"));

                // Copy Name
                items.Add(new DropdownItem<(int priority, Action action)>((30, () => EditorGUIUtility.systemCopyBuffer = selectedObject.name), "Copy Name"));

                // Copy Path in Hierarchy
                items.Add(new DropdownItem<(int priority, Action action)>((40, () => EditorGUIUtility.systemCopyBuffer = GetGameObjectPath(selectedObject)), "Copy Path in Hierarchy"));

                // Delete
                items.Add(new DropdownItem<(int priority, Action action)>((50, () => UnityEngine.Object.DestroyImmediate(selectedObject)), "Delete"));
            }
        }

        // Helper method to duplicate a GameObject
        static void DuplicateObject(GameObject original)
        {
            UnityEngine.Object.Instantiate(original, original.transform.parent);
        }

        // Helper method to get the path of a GameObject in the hierarchy
        static string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = obj.name + "/" + path;
            }
            return path;
        }
    }
}
