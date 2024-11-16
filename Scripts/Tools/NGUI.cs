using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using SolidUtilities.UnityEditorInternals;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Neeto
{
    public static class NGUI
    {
        #region RUNTIME
        static Texture2D _shadow, _highlight;
        public static Texture2D shadow => _shadow ??= Color.black.With(a: .1f).AsTexturePixel();
        public static Texture2D highlight => _highlight ??= Color.white.With(a: .06f).AsTexturePixel();

        public static Rect LerpRect(Rect rectA, Rect rectB, float t)
        {
            var result = new Rect();
            result.position = Vector2.Lerp(rectA.position, rectB.position, t);
            result.size = Vector2.Lerp(rectA.size, rectB.size, t);
            return result;
        }
        public static Vector2 SetTextAndUpdateWidth(this TextMeshProUGUI tmp, string text, float margins = 0f)
        {
            tmp.text = text;
            var size = tmp.rectTransform.sizeDelta;
            size.x = Mathf.Max(tmp.preferredWidth + margins * 2, size.y);
            return tmp.rectTransform.sizeDelta = size;
        }
        public static void SetTextAndUpdateHeight(this TextMeshProUGUI tmp, string text)
        {
            tmp.text = text;
            var size = tmp.rectTransform.sizeDelta;
            size.y = tmp.preferredHeight;
            tmp.rectTransform.sizeDelta = size;
        }
        #endregion RUNTIME
        #region EDITOR
#if UNITY_EDITOR

        public class PropertyScope : IDisposable
        {
            public PropertyScope(Rect position, SerializedProperty property, GUIContent label, bool box = true)
            {
                EditorGUI.BeginProperty(position, label, property);
                if (box)
                    IndentBoxGUI(position);
            }
            void IDisposable.Dispose()
            {
                EditorGUI.EndProperty();
            }
        }
        public static PropertyScope Property(Rect position, SerializedProperty property, GUIContent label, bool box = true)
        {
            return new PropertyScope(position, property, label, box);
        }
        public static PropertyScope Property(Rect position, SerializedProperty property, bool box = true)
        {
            return new PropertyScope(position, property, GUIContent.none, box);
        }
        public static PropertyScope Property(Rect position, GUIContent label, SerializedProperty property, bool box = true)
        {
            return new PropertyScope(position, property, label, box);
        }
        public static void IndentBoxGUI(Rect position)
        {
            var texture = EditorGUI.indentLevel % 2 == 0 ? NGUI.shadow : NGUI.highlight;
            if (texture != null)
                GUI.DrawTexture(EditorGUI.IndentedRect(position), texture);
        }
        public class DisabledScope : IDisposable
        {
            public DisabledScope(bool disabled = true) => EditorGUI.BeginDisabledGroup(disabled);
            public void Dispose() => EditorGUI.EndDisabledGroup();
        }
        public static DisabledScope Disabled(bool disabled = true)
        {
            return new DisabledScope(disabled);
        }
        public static T LoadAssetFromGUID<T>(string guid) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
        }
        public static T LoadFirstAsset<T>(string search) where T : UnityEngine.Object
        {
            var guids = AssetDatabase.FindAssets(search);
            if (guids.Length == 0)
                return default;
            return LoadAssetFromGUID<T>(guids[0]);
        }

        public static void CopyMatchingFields(object target, object source)
        {
            if (source == null || target == null)
                return;

            var targetType = target.GetType();
            var sourceType = source.GetType();

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.FlattenHierarchy;

            var sourceFields = sourceType.GetFields(flags);
            foreach (var sourceField in sourceFields)
            {
                try
                {
                    var targetField = targetType.GetField(sourceField.Name);
                    targetField.SetValue(target, sourceField.GetValue(source));
                }
                catch // (Exception e)
                {
                    // target does not contain source field
                    //Debug.LogWarning($"Could not copy property {sourceField.Name}", (UnityEngine.Object)reference);
                }
            }
        }
        public static object buffer;
        public static void ContextMenu(SerializedProperty property, Rect rect)
        {
            Event evt = Event.current;

            if (evt.type == EventType.ContextClick && rect.Contains(evt.mousePosition))
            {
                GenericMenu menu = new GenericMenu();
                if (property.propertyType == SerializedPropertyType.ManagedReference)
                {
                    menu.AddItem(new GUIContent("Copy managedReferenceValue"), false, () => CopyManagedReferenceValue(property));
                    if (buffer != null && GetManagedReferenceFieldType(property).IsAssignableFrom(buffer.GetType()))
                        //if (buffer != null && Type.GetType(property.managedReferenceFullTypename).IsAssignableFrom(buffer.GetType()))
                        menu.AddItem(new GUIContent("Paste managedReferenceValue"), false, () => PasteManagedReferenceValue(property));
                }
                menu.ShowAsContext();
                evt.Use(); // Mark the event as used
            }
        }
        public static void CopyManagedReferenceValue(SerializedProperty property)
        {
            buffer = (object)property.managedReferenceValue;
        }
        public static void PasteManagedReferenceValue(SerializedProperty property)
        {
            if (buffer != null)
            {
                // undo support
                property.serializedObject.Update();
                Undo.RecordObject(property.serializedObject.targetObject, "Paste property");

                // creates copy
                var json = JsonUtility.ToJson(buffer);
                var value = JsonUtility.FromJson(json, buffer.GetType());

                // set value
                property.managedReferenceValue = value; // Adjust this for different property types
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary> Get the real field type from a SerializeReference field </summary>
        public static Type GetManagedReferenceFieldType(this SerializedProperty property)
        {
            var strings = property.managedReferenceFieldTypename.Split(char.Parse(" "));
            var type = Type.GetType($"{strings[1]}, {strings[0]}");

            if (type == null)
                Debug.LogError($"Can not get field type of managed reference : {property.managedReferenceFieldTypename}");

            return type;
        }

        public static float IndentWidth => 17f;
        public static float FullLineHeight => LineHeight + VerticalSpacing;
        public static float LineHeight => EditorGUIUtility.singleLineHeight;
        public static float LabelWidth => EditorGUIUtility.labelWidth;
        public static float ButtonWidth => 22f;
        public static float VerticalSpacing => EditorGUIUtility.standardVerticalSpacing;
        public static string CopyBuffer => EditorGUIUtility.systemCopyBuffer;
        public static Vector2 screenSize => new Vector2(Screen.width, Screen.height);


        public static GUIContent locked => EditorGUIUtility.IconContent("Locked@2x");
        public static GUIContent unlocked => EditorGUIUtility.IconContent("Unlocked@2x");
        public static GUIContent settings => EditorGUIUtility.IconContent("d_SettingsIcon@2x");
        public static GUIContent select => EditorGUIUtility.IconContent("d_scenepicking_pickable_hover@2x");
        public static GUIContent refresh => EditorGUIUtility.IconContent("d_Refresh@2x");
        public static GUIContent star => EditorGUIUtility.IconContent("d_Favorite On Icon");
        public static GUIContent sceneOut => EditorGUIUtility.IconContent("SceneLoadOut");
        public static GUIContent sceneIn => EditorGUIUtility.IconContent("SceneLoadIn");
        public static GUIContent hidden => EditorGUIUtility.IconContent("scenevis_hidden@2x");
        public static GUIContent visible => EditorGUIUtility.IconContent("d_animationvisibilitytoggleon@2x");

        public static void MinMaxWithFieldsLayout(string label, ref float min, ref float max, float minLimit, float maxLimit)
        {
            var rect = GUILayoutUtility.GetLastRect();

            //rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
            rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight);

            var lineWidth = rect.width;
            //var fieldWidth = EditorGUIUtility.fieldWidth;
            //var labelWidth = lineWidth - fieldWidth;

            var end = rect.xMax;

            var fieldWidth = 80;

            EditorGUI.LabelField(rect, label);
            //rect.x += (EditorGUIUtility.fieldWidth - (EditorGUIUtility.labelWidth - rect.x)); // ???
            rect.x = EditorGUIUtility.labelWidth - 10;

            rect.width = fieldWidth;

            min = Mathf.Clamp(EditorGUI.FloatField(rect, min), minLimit, max);
            rect.x += rect.width - 25;
            rect.xMax = end - fieldWidth + 25;
            //rect.width = remaining;

            EditorGUI.MinMaxSlider(rect, ref min, ref max, minLimit, maxLimit);
            rect.xMin = end - fieldWidth;
            rect.width = fieldWidth;

            max = Mathf.Clamp(EditorGUI.FloatField(rect, max), min, maxLimit);
        }
        public static void DrawScriptField(ScriptableObject obj)
        {
            var script = MonoScript.FromScriptableObject(obj);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(script, typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();
        }
        public static void DrawScriptField(SerializedObject obj)
        {
            var target = obj.targetObject;
            var type = obj.targetObject.GetType();

            MonoScript script;
            if (target is ScriptableObject so)
                script = MonoScript.FromScriptableObject(so);
            else if (target is MonoBehaviour mb)
                script = MonoScript.FromMonoBehaviour(mb);
            else
            {
                return;
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(script, typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();
        }
        public static void DrawPropertiesLayout(this SerializedObject serializedObject, params string[] props)
        {
            EditorGUI.BeginChangeCheck();
            foreach (var propName in props)
            {
                var serializedProperty = serializedObject.FindProperty(propName);
                EditorGUILayout.PropertyField(serializedProperty);
            }
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(serializedObject.targetObject);
            }
        }
        public static void DrawProperties(this SerializedObject serializedObject, Rect rect, params string[] props)
        {
            EditorGUI.BeginChangeCheck();
            foreach (var propName in props)
            {
                var property = serializedObject.FindProperty(propName);
                rect.height = EditorGUI.GetPropertyHeight(property);
                EditorGUI.PropertyField(rect, property);
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
            }
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(serializedObject.targetObject);
            }
        }

        public static Dictionary<Type, Type> FindPropertyDrawerTypes()
        {
            var result = new Dictionary<Type, Type>();
            var typeField = typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var drawerType in TypeCache.GetTypesDerivedFrom<PropertyDrawer>())
            {
                var attributes = drawerType.GetCustomAttributes(typeof(CustomPropertyDrawer), true)
                                           .Cast<CustomPropertyDrawer>();

                foreach (var attr in attributes)
                {
                    var targetType = (Type)typeField.GetValue(attr);
                    if (!result.ContainsKey(targetType))
                        result.Add(targetType, drawerType);
                }
            }

            return result;
        }

        public static float GetHeight(this SerializedProperty property) => EditorGUI.GetPropertyHeight(property);

        public static bool TryFindFirstAsset(string search, out Object result)
        {
            var guids = AssetDatabase.FindAssets(search);
            if (guids.Length == 0)
            {
                result = null;
                return false;
            }

            var guid = guids[0];
            var path = AssetDatabase.GUIDToAssetPath(guid);
            return result = AssetDatabase.LoadAssetAtPath<Object>(path);
        }
        public static List<T> FindAssetsOfType<T>() where T : Object
        {
            return FindAssetsOfType(typeof(T)).ConvertAll(o => (T)o);
        }
        public static List<Object> FindAssetsOfType(Type type)
        {
            if (!typeof(Object).IsAssignableFrom(type))
            {
                Debug.LogError($"Type {type.Name} does not derive from UnityEngine.Object");
                return new List<Object>();
            }

            List<Object> assets = new List<Object>();

            string[] guids = AssetDatabase.FindAssets($"t:{type.Name}");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Object asset = AssetDatabase.LoadAssetAtPath(assetPath, type);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            return assets;
        }
        public static IEnumerable<T> FindAssets<T>() where T : Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(g => AssetDatabase.GUIDToAssetPath(g))
                .Select(path => AssetDatabase.LoadAssetAtPath<T>(path));
        }
        public static void Dirty(this Object obj)
        {
#if UNITY_EDITOR
            // EditorUtility.CopySerializedManagedFieldsOnly
            Undo.RecordObject(obj, obj.name);
            EditorUtility.SetDirty(obj);
#endif
        }
        public static bool FindAsset<T>(string search, out T result) where T : Object
        {
            var guids = AssetDatabase.FindAssets(search);
            if (guids.Length == 0)
            {
                result = null;
                return false;
            }

            var guid = guids[0];
            var path = AssetDatabase.GUIDToAssetPath(guid);
            return result = AssetDatabase.LoadAssetAtPath<T>(path);
        }

        public static void CreateAssetDialogue<T>(string name = null) where T : ScriptableObject
        {
            // Prompt the user to choose a save location and file name
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Asset",
                name ?? $"New{typeof(T).Name}",
                "asset",
                "Please enter a file name to save the asset to"
            );

            // Check if the user canceled the save file dialogue
            if (string.IsNullOrEmpty(path))
                return;

            // Create the asset and save it to the chosen path
            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            // Focus the Project window and highlight the newly created asset
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
        public static T CreateAsset<T>() where T : ScriptableObject
        {
            // try to find the selected asset's folder
            string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!AssetDatabase.IsValidFolder(selectedPath))
                selectedPath = System.IO.Path.GetDirectoryName(selectedPath);

            // default to Assets/
            if (!AssetDatabase.IsValidFolder(selectedPath))
                selectedPath = Application.dataPath;

            // instantiate into AssetDatabase
            T instance = ScriptableObject.CreateInstance<T>();
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{selectedPath}/{typeof(T).Name}.asset");
            AssetDatabase.CreateAsset(instance, assetPath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = instance;
            EditorGUIUtility.PingObject(instance);
            return instance;
        }
        public static T CreateAsset<T>(T from, string path = null) where T : ScriptableObject
        {
            // Get the selected object in the Project window
            var selectedObject = Selection.activeObject;

            var x = ScriptableObject.Instantiate(from);

            // Get the path of the selected object
            path ??= Application.dataPath;

            if (selectedObject)
            {
                // If the selected object is not a folder, select its parent folder
                if (!AssetDatabase.IsValidFolder(path))
                {
                    path = System.IO.Path.GetDirectoryName(path);
                }
            }
            else
            {
                var script = MonoScript.FromScriptableObject(x);
                path = AssetDatabase.GetAssetPath(script);
                path = Path.GetDirectoryName(path);
                path = path.Replace('\\', '/') + "/";
            }

            // Base asset folder if no path selected
            if (!AssetDatabase.IsValidFolder(path))
            {
                path = Application.dataPath;
                path = path.Substring(path.Length - "Assets".Length);
            }

            // Create a unique asset file name within the selected folder
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/{typeof(T).Name}.asset");

            // Create the asset in the Project window
            AssetDatabase.CreateAsset(x, assetPath);

            // Save the asset to disk
            AssetDatabase.SaveAssets();

            // Select the newly created asset
            Selection.activeObject = x;
            EditorGUIUtility.PingObject(x);

            return x;
        }
        public static T CreateUniqueAsset<T>(T obj) where T : Object
        {
            var oldPath = AssetDatabase.GetAssetPath(obj);
            var newPath = AssetDatabase.GenerateUniqueAssetPath(oldPath);

            File.Copy(oldPath, newPath);
            AssetDatabase.Refresh();
            T clone = AssetDatabase.LoadAssetAtPath<T>(newPath);
            clone.name = Path.GetFileNameWithoutExtension(newPath);
            Undo.RegisterCreatedObjectUndo(clone, "Create asset");
            Undo.undoRedoPerformed += () =>
            {
                if (!AssetDatabase.Contains(clone))
                {
                    //Editor.DestroyImmediate(clone);
                    File.Delete(newPath);
                    File.Delete(newPath + ".meta");
                    AssetDatabase.Refresh();
                }
            };
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(clone);
            AssetDatabase.Refresh();
            return clone;
        }

        public static void ApplyAndMarkDirty(this SerializedProperty property)
        {
            //Undo.RecordObject(property.serializedObject.targetObject, "Apply properties");
            property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }
        public static bool GetArrayElementIndex(this SerializedProperty property, out int result)
        {
            result = -1;
            if (property.Parent() is SerializedProperty parent && parent.isArray)
            {
                for (int i = 0; i < parent.arraySize; i++)
                {
                    if (parent.GetArrayElementAtIndex(i).propertyPath.Equals(property.propertyPath))
                    {
                        result = i;
                        return true;
                    }
                }
            }
            return false;
        }
        public static SerializedProperty Parent(this SerializedProperty property)
        {
            // https://gist.github.com/monry/9de7009689cbc5050c652bcaaaa11daa
            var propertyPaths = property.propertyPath.Split('.');
            if (propertyPaths.Length <= 1)
            {
                return default;
            }

            var parentSerializedProperty = property.serializedObject.FindProperty(propertyPaths.First());
            for (var index = 1; index < propertyPaths.Length - 1; index++)
            {
                if (propertyPaths[index] == "Array" && propertyPaths.Length > index + 1 && Regex.IsMatch(propertyPaths[index + 1], "^data\\[\\d+\\]$"))
                {
                    var match = Regex.Match(propertyPaths[index + 1], "^data\\[(\\d+)\\]$");
                    var arrayIndex = int.Parse(match.Groups[1].Value);
                    parentSerializedProperty = parentSerializedProperty.GetArrayElementAtIndex(arrayIndex);
                    index++;
                }
                else
                {
                    parentSerializedProperty = parentSerializedProperty.FindPropertyRelative(propertyPaths[index]);
                }
            }

            return parentSerializedProperty;
        }
        public static SerializedProperty FindSiblingProperty(this SerializedProperty property, string siblingPropertyName)
        {
            if (property == null)
                throw new System.ArgumentNullException(nameof(property));

            if (string.IsNullOrEmpty(siblingPropertyName))
                throw new System.ArgumentException("Sibling property name cannot be null or empty.", nameof(siblingPropertyName));

            // Get the property path of the current property
            string propertyPath = property.propertyPath;

            // Find the last separator in the property path
            int lastSeparator = propertyPath.LastIndexOf('.');

            // Determine the parent path
            string parentPath = lastSeparator >= 0 ? propertyPath.Substring(0, lastSeparator) : "";

            // Construct the sibling's property path
            string siblingPropertyPath = string.IsNullOrEmpty(parentPath) ? siblingPropertyName : $"{parentPath}.{siblingPropertyName}";

            // Find and return the sibling property
            return property.serializedObject.FindProperty(siblingPropertyPath);
        }
        public static void DrawProperties(this SerializedProperty property, Rect position)
        {
            var currentProperty = property.Copy();
            var endProperty = property.GetEndProperty();

            bool enterChildren = true;

            while (currentProperty.NextVisible(enterChildren))
            {
                if (SerializedProperty.EqualContents(currentProperty, endProperty))
                    break;

                position = position.With(h: currentProperty.GetHeight());
                EditorGUI.PropertyField(position, currentProperty, true);
                position.y += position.height + NGUI.VerticalSpacing;
                enterChildren = false;
            }
        }
        public static bool HasChildProperties(this SerializedProperty property)
        {
            //Debug.Log(property.propertyPath + " " + property.Copy().CountRemaining());
            property = property.Copy();
            return property.CountRemaining() >= 1;
        }
        public static bool IsArrayElement(this SerializedProperty property)
        {
            return property.propertyPath.EndsWith(']');
        }
        public static bool IsArrayElement(this SerializedProperty property, out int index)
        {
            index = -1;
            var result = property.IsArrayElement();

            if (result)
            {
                var start = property.propertyPath.LastIndexOf('[') + 1;
                var str = property.propertyPath.Substring(start, (property.propertyPath.Length - 1) - start);
                index = int.Parse(str);
            }
            return result;
        }

        public static Rect NextLine(this Rect rect, float? height = null)
        {
            return rect.With(y: rect.yMax + NGUI.VerticalSpacing, h: height.HasValue ? height.Value : NGUI.LineHeight);
        }
        public static Rect GetRect()
        {
            //return GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, FullLineHeight).With(h: LineHeight);
            return GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, LineHeight);  // does the vertical spacing need to be reserved
        }
        public static Rect With(this Rect rect, Vector2? pos = null, Vector2? size = null, float? x = null, float? y = null, float? w = null, float? h = null, float? xMin = null, float? xMax = null, float? yMin = null, float? yMax = null, Vector2? min = null, Vector2? max = null)
        {
            if (pos.HasValue)
            {
                rect.position = pos.Value;
            }
            if (size.HasValue)
            {
                rect.size = size.Value;
            }
            if (x.HasValue)
            {
                rect.x = x.Value;
            }
            if (y.HasValue)
            {
                rect.y = y.Value;
            }
            if (w.HasValue)
            {
                rect.width = w.Value;
            }
            if (h.HasValue)
            {
                rect.height = h.Value;
            }
            if (xMin.HasValue)
            {
                rect.xMin = xMin.Value;
            }
            if (xMax.HasValue)
            {
                rect.xMax = xMax.Value;
            }
            if (yMin.HasValue)
            {
                rect.yMin = yMin.Value;
            }
            if (yMax.HasValue)
            {
                rect.yMax = yMax.Value;
            }
            if (min.HasValue)
            {
                rect.min = min.Value;
            }
            if (max.HasValue)
            {
                rect.max = max.Value;
            }

            return rect;
        }
        public static RectInt With(this RectInt rect, Vector2Int? pos = null, Vector2Int? size = null, int? x = null, int? y = null, int? w = null, int? h = null, int? xMin = null, int? xMax = null, int? yMin = null, int? yMax = null, Vector2Int? min = null, Vector2Int? max = null)
        {
            if (pos.HasValue)
            {
                rect.position = pos.Value;
            }
            if (size.HasValue)
            {
                rect.size = size.Value;
            }
            if (x.HasValue)
            {
                rect.x = x.Value;
            }
            if (y.HasValue)
            {
                rect.y = y.Value;
            }
            if (w.HasValue)
            {
                rect.width = w.Value;
            }
            if (h.HasValue)
            {
                rect.height = h.Value;
            }
            if (xMin.HasValue)
            {
                rect.xMin = xMin.Value;
            }
            if (xMax.HasValue)
            {
                rect.xMax = xMax.Value;
            }
            if (yMin.HasValue)
            {
                rect.yMin = yMin.Value;
            }
            if (yMax.HasValue)
            {
                rect.yMax = yMax.Value;
            }
            if (min.HasValue)
            {
                rect.min = min.Value;
            }
            if (max.HasValue)
            {
                rect.max = max.Value;
            }

            return rect;
        }
        public static Rect Move(this Rect rect, Vector2? position = null, Vector2? size = null, float? x = null, float? y = null, float? w = null, float? h = null, float? xMin = null, float? xMax = null, float? yMin = null, float? yMax = null, Vector2? min = null, Vector2? max = null)
        {
            if (y is float Y)
            {

                var p = rect.position;
                p.y += Y;
                rect.position = p;
            }
            if (x is float X)
            {

                var p = rect.position;
                p.x += X;
                rect.position = p;
            }

            if (position.HasValue)
            {
                rect.position += position.Value;
            }
            if (size.HasValue)
            {
                rect.size += size.Value;
            }
            if (w.HasValue)
            {
                rect.width += w.Value;
            }
            if (h.HasValue)
            {
                rect.height += h.Value;
            }
            if (xMin.HasValue)
            {
                rect.xMin += xMin.Value;
            }
            if (xMax.HasValue)
            {
                rect.xMax += xMax.Value;
            }
            if (yMin.HasValue)
            {
                rect.yMin += yMin.Value;
            }
            if (yMax.HasValue)
            {
                rect.yMax += yMax.Value;
            }
            if (min.HasValue)
            {
                rect.min += min.Value;
            }
            if (max.HasValue)
            {
                rect.max += max.Value;
            }

            return rect;
        }
        public static RectInt Move(this RectInt rect, Vector2Int? position = null, Vector2Int? size = null, int? x = null, int? y = null, int? width = null, int? height = null, int? xMin = null, int? xMax = null, int? yMin = null, int? yMax = null, Vector2Int? min = null, Vector2Int? max = null)
        {
            if (y.HasValue)
            {

                var p = rect.position;
                p.y += (int)y;
                rect.position = p;
            }
            if (x.HasValue)
            {

                var p = rect.position;
                p.x += (int)x;
                rect.position = p;
            }

            if (position.HasValue)
            {
                rect.position += position.Value;
            }
            if (size.HasValue)
            {
                rect.size += size.Value;
            }
            if (width.HasValue)
            {
                rect.width += width.Value;
            }
            if (height.HasValue)
            {
                rect.height += height.Value;
            }
            if (xMin.HasValue)
            {
                rect.xMin += xMin.Value;
            }
            if (xMax.HasValue)
            {
                rect.xMax += xMax.Value;
            }
            if (yMin.HasValue)
            {
                rect.yMin += yMin.Value;
            }
            if (yMax.HasValue)
            {
                rect.yMax += yMax.Value;
            }
            if (min.HasValue)
            {
                rect.min += min.Value;
            }
            if (max.HasValue)
            {
                rect.max += max.Value;
            }

            return rect;
        }
        public static GUIStyle With(this GUIStyle style, int? fontSize = null, TextAnchor? alignment = null, float? fixedHeight = null, float? fixedWidth = null, RectOffset padding = null, RectOffset margins = null, Font font = null, bool? richText = null, bool? wordWrap = null, GUIStyleState normal = null, GUIStyleState hover = null, GUIStyleState active = null)
        {
            return new GUIStyle(style)
            {
                fontSize = fontSize ?? style.fontSize,
                alignment = alignment ?? style.alignment,
                fixedHeight = fixedHeight ?? style.fixedHeight,
                fixedWidth = fixedWidth ?? style.fixedWidth,
                margin = margins ?? style.margin,
                padding = padding ?? style.padding,
                font = font ?? style.font,
                richText = richText ?? style.richText,
                wordWrap = wordWrap ?? style.wordWrap,
                normal = normal ?? style.normal,
                hover = hover ?? style.hover,
                active = active ?? style.active,
            };
        }
        public static GUIStyleState With(this GUIStyleState src, Texture2D background = null, Texture2D[] scaledBackground = null, Color? textColor = null)
        {
            return new GUIStyleState()
            {
                background = background ?? src.background,
                scaledBackgrounds = scaledBackground ?? src.scaledBackgrounds,
                textColor = textColor ?? src.textColor,
            };
        }
        public static GUIContent With(this GUIContent content, string text = null, Texture image = null, string tooltip = null)
        {
            content = new GUIContent(content);
            if (text != null)
                content.text = text;
            if (image != null)
                content.image = image;
            if (tooltip != null)
                content.tooltip = tooltip;
            return content;
        }



        public static void StartDrag(Object[] objects)
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = objects;
            DragAndDrop.StartDrag("Dragging Prefab");
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        }
        public static bool AcceptDrag(Rect dropArea)
        {
            Event e = Event.current;
            GUI.Box(dropArea, "");

            switch (e.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:

                    if (!dropArea.Contains(e.mousePosition))
                        break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (e.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        return true;
                    }
                    e.Use();
                    break;
            }
            return false;
        }
        public static int MouseDown(Rect rect)
        {
            var e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                if (MouseInRect(rect))
                    return e.button;
            }
            return -1;
        }
        public static bool MouseInRect(Rect rect)
        {
            var mouseLocal = Event.current.mousePosition - rect.position;
            return mouseLocal.x > 0
                && mouseLocal.x < rect.width
                && mouseLocal.y > 0
                && mouseLocal.y < rect.height;
        }
        public static bool MouseUp(this Event Event)
        {
            return Event.type == EventType.MouseUp;// == EventType.MouseUp;// && Event.button == button;
        }
        public static IEnumerable<T> LoadAssets<T>() where T : Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)));
        }


        public static bool GetArrayProperty(this SerializedProperty property, out SerializedProperty arrayProperty)
        {
            var i = property.propertyPath.LastIndexOf(".Array");
            var newPath = property.propertyPath.Substring(0, i);
            arrayProperty = property.serializedObject.FindProperty(newPath);

            return arrayProperty != null;
        }
        public static bool IsArrayOrElement(this SerializedProperty property)
        {
            return property.isArray || IsElement(property);
        }
        public static bool IsElement(this SerializedProperty property)
        {
            return property.propertyPath.EndsWith(']');
        }
#endif
        #endregion

    }
}