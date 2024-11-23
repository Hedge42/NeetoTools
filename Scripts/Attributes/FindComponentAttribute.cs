using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Neeto
{
    /// <summary>
    /// Find a component in the hierarchy <br/>
    /// search strings are normalized (to lower) <br/>
    /// *  matches 0 or more characters <br/>
    /// ?  matches a single character <br/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FindComponentAttribute : PropertyAttribute
    {
        public SearchScope scope;
        public string search;

        public FindComponentAttribute(string search, SearchScope scope = SearchScope.Children)
        {
            this.scope = scope;
            this.search = search;
        }
    }

    [Flags]
    public enum SearchScope
    {
        Children = 1,
        Parent = 2,
        Scene = 4,
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(FindComponentAttribute), true)]
    class FindComponentAttributeDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, Component> cache = new Dictionary<string, Component>();
        private static bool hierarchyChanged = true;

        static FindComponentAttributeDrawer()
        {
            EditorApplication.hierarchyChanged += () => hierarchyChanged = true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (NGUI.Property(position, property, false))
            {
                if (!property.objectReferenceValue && property.serializedObject.targetObject is Component component)
                {
                    var attribute = (FindComponentAttribute)this.attribute;
                    var root = component.transform;
                    var search = attribute.search;
                    var scope = attribute.scope;
                    var type = fieldInfo.FieldType;

                    string cacheKey = $"{component.GetInstanceID()}_{search}_{scope}_{type.FullName}";

                    if (!cache.TryGetValue(cacheKey, out var cachedComponent) || hierarchyChanged)
                    {
                        cachedComponent = FindComponent(root, search, scope, type);

                        if (cachedComponent != null)
                        {
                            cache[cacheKey] = cachedComponent;
                        }

                        hierarchyChanged = false; // Only reset after completing the search
                    }

                    property.objectReferenceValue = cachedComponent;
                }

                EditorGUI.PropertyField(position, property, label);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }

        private static Component FindComponent(Transform root, string searchString, SearchScope scope, Type type)
        {
            Component result = null;
            if (scope.HasFlag(SearchScope.Children))
                result ??= SearchInChildren(root, searchString, type);
            if (scope.HasFlag(SearchScope.Parent))
                result ??= SearchInParent(root, searchString, type);
            if (scope.HasFlag(SearchScope.Scene))
                result ??= SearchInScene(root, searchString, type);
            return result;
        }

        private static Component SearchInChildren(Transform root, string searchString, Type type)
        {
            var regex = WildcardToRegex(searchString);
            foreach (Transform child in root.GetComponentsInChildren<Transform>())
            {
                if (regex.IsMatch(child.name))
                {
                    var component = child.GetComponent(type);
                    if (component != null)
                    {
                        return component;
                    }
                }
            }
            return null;
        }

        private static Component SearchInParent(Transform root, string searchString, Type type)
        {
            var regex = WildcardToRegex(searchString);
            var parent = root.parent;
            while (parent != null)
            {
                if (regex.IsMatch(parent.name))
                {
                    var component = parent.GetComponent(type);
                    if (component != null)
                        return component;
                }
                parent = parent.parent;
            }
            return null;
        }

        private static Component SearchInScene(Transform root, string searchString, Type type)
        {
            var regex = WildcardToRegex(searchString);
            var sceneObjects = root.gameObject.scene.GetRootGameObjects();
            foreach (var obj in sceneObjects)
            {
                var component = obj.GetComponentsInChildren<Transform>()
                    .FirstOrDefault(child => regex.IsMatch(child.name) && child.GetComponent(type) != null)?
                    .GetComponent(type);
                if (component != null)
                    return component;
            }
            return null;
        }

        private static Regex WildcardToRegex(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return new Regex(".*", RegexOptions.IgnoreCase); // Match everything if the pattern is empty

            // Escape special characters and replace wildcards
            pattern = Regex.Escape(pattern)
                          .Replace(@"\*", ".*") // '*' -> Match zero or more characters
                          .Replace(@"\?", "."); // '?' -> Match exactly one character

            return new Regex($"^{pattern}$", RegexOptions.IgnoreCase); // Anchors ensure full-string match
        }
    }
#endif
}
