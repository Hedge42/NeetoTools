#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using Matchwork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

public static partial class MEdit
{
    public static float fullLineHeight => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
    public static float lineHeight => EditorGUIUtility.singleLineHeight;

    [MenuItem(MPath.Commands + nameof(RaycastMoveSelectedTransform))]
    public static void RaycastMoveSelectedTransform()
    {
        if (!Selection.activeGameObject)
            return;

        //Ray ViewRay()
        //{
        //    var view = SceneView.lastActiveSceneView;
        //    if (view)
        //    {
        //        var camera = view.camera;
        //        var point = view.pivot;

        //        return new Ray(point, point - camera.)
        //    }
        //}


        var transform = Selection.activeGameObject.transform;
        var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        var qti = QueryTriggerInteraction.Ignore;
        var dist = Mathf.Infinity;
        var layers = MLayer.Level | MLayer.Terrain;
        Event.current.Use();

        // NOTE don't use NavMesh.Raycast, it doesn't seem to work
        var result = Physics.Raycast(ray, out var hit, dist, layers, qti);

        // stupid

        var output = $"RaycastMove transform '{transform.name}' {result.SuccessOrFail()}";
        Undo.RecordObject(transform, output);
        Debug.Log(output, transform);
        transform.position = hit.point;


        if (result)
        {
            transform.position = hit.point;
            Debug.Log(output + " (SUCCESS)");
        }
        else Debug.LogError(output + " (FAIL)", transform);
    }

    [MenuItem(MPath.Commands + nameof(SnapToNavMesh))]
    public static void SnapToNavMesh()
    {
        if (!Selection.activeGameObject)
            return;

        var dist = 1f;
        var transforms = Selection.gameObjects.Select(g => g.transform).ToArray();
        var output = transforms.Length == 1
            ? $"Snap ({transforms[0].name}) to NavMesh"
            : $"Snap ({transforms.Length}) objects to NavMesh";
        Undo.RecordObjects(transforms, output);
        Debug.Log(output);
        foreach (var transform in transforms)
        {
            var result = NavMesh.SamplePosition(transform.position, out var hit, dist, NavMesh.AllAreas);

            if (result)
                transform.position = hit.position;
            else
                Debug.LogError($"NavMesh not found within ({dist:F1}) distance of ({transform.name})", transform);
        }
    }

    [MenuItem(MPath.Commands + nameof(SnapToGround))]
    public static void SnapToGround()
    {
        if (!Selection.activeGameObject)
            return;

        var qti = QueryTriggerInteraction.Ignore;
        var layers = MLayer.Level | MLayer.Terrain;
        var transforms = Selection.gameObjects.Select(g => g.transform).ToArray();
        var output = transforms.Length == 1
            ? $"Snap ({transforms[0].name}) to ground"
            : $"Snap ({transforms.Length}) transforms to ground";

        Undo.RecordObjects(transforms, output);
        Debug.Log(output);
        foreach (var transform in transforms)
        {
            var pos = transform.position + Vector3.up * 2f; // start from above
            var dir = Vector3.down;
            var result = Physics.Raycast(pos, dir, out var hit, 10f, layers, qti);

            if (result)
                transform.position = hit.point;
            else
                Debug.LogError($"Physical collider not found underneath ({transform.name})", transform);
        }
    }

    [MenuItem(MPath.Commands + nameof(ApplyNamingConventions))]
    public static void ApplyNamingConventions()
    {
        var objects = Selection.objects;

        var output = $"Rename ({objects.Length}) objects...";
        Undo.RecordObjects(objects, output);

        foreach (var _ in objects)
        {
            _.name = _.name.Replace(' ', '_');
            EditorUtility.SetDirty(_);
        }
    }

    [MenuItem(MPath.Commands + nameof(CreateSubAsset))]
    public static void CreateSubAsset()
    {
        var asset = Selection.activeObject;


        if (Selection.activeObject is ScriptableObject so)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            var sub = ScriptableObject.CreateInstance(so.GetType());
            sub.name = "s_" + asset.name;

            AssetDatabase.AddObjectToAsset(sub, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    [MenuItem(MPath.Commands + nameof(CreateScript))]
    public static void CreateScript()
    {
        //Selection.activeObject
    }

    [MenuItem(MPath.Commands + nameof(OpenPhysicsSettings))]
    public static void OpenPhysicsSettings()
    {
        SettingsService.OpenProjectSettings("Project/Physics");
    }

    [MenuItem(MPath.Commands + nameof(ClearProjectHideFlags))]
    public static void ClearProjectHideFlags()
    {
        var folders = AssetDatabase.GetAllAssetPaths().Where(p => System.IO.Directory.Exists(p));
    }

    [MenuItem(MPath.Commands + nameof(ClearSceneHideFlags))]
    public static void ClearSceneHideFlags()
    {
        var components = GameObject.FindObjectsByType<Component>(FindObjectsSortMode.InstanceID);

        foreach (var c in components)
        {
            if (c.hideFlags != HideFlags.None)
            {
                c.hideFlags = HideFlags.None;
                Debug.Log($"Set hideflags ({c.name})", c);
                Undo.RecordObject(c, "Hide Flags");
                EditorUtility.SetDirty(c);
            }

        }
    }

    // search
    public static Object FindFirstAsset(string search)
    {
        if (!TryFindFirstAsset(search, out var result))
        {
            Debug.LogError($"No asset found matching search filter ({search})");
        }

        return result;
    }
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

    [MenuItem("CONTEXT/" + nameof(ScriptableObject) + "/Search.....")]
    public static void Search(MenuCommand command)
    {
        SetSearchFilter($"t:{command.context.GetType()}");
    }

    [MenuItem(MPath.Commands + nameof(SearchGame))]
    public static void SearchGame()
    {
        SetSearchFilter("Gylfie/ " + GetSearchFilter());
    }
    public static string GetSearchFilter()
    {
        var hierarchy = EditorWindow.GetWindow<SearchableEditorWindow>();//.GetWindow<SearchableEditorWindow>();
        return (string)typeof(SearchableEditorWindow)
            .GetField("m_SearchFilter", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(hierarchy);
    }
    public static void SetSearchFilter(string filter)
    {
        var hierarchy = EditorWindow.GetWindow<SearchableEditorWindow>();//.GetWindow<SearchableEditorWindow>();
        var filterMode = (int)SearchableEditorWindow.SearchMode.All;
        var method = typeof(SearchableEditorWindow).GetMethod("SetSearchFilter", BindingFlags.NonPublic | BindingFlags.Instance);
        var parameters = new object[] { filter, filterMode, false, false }; ;
        method.Invoke(hierarchy, parameters);
    }
    public static Object ToAsset(this object _, string assetPath = null, Func<Object> factory = null)
    {
        // wtf

        if (assetPath != null || _ is string)
            return AssetDatabase.LoadAssetAtPath(assetPath ?? _ as string, typeof(Object));
        if (factory != null)
            return MCache.Get(factory);
        if (_ is Object)
            return _ as Object;

        return default(Object);
    }

    // asset creation
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
    public static IEnumerable<T> FindAssets<T>() where T : Object
    {
        return AssetDatabase.FindAssets($"t:{typeof(T).Name}")
            .Select(g => AssetDatabase.GUIDToAssetPath(g))
            .Select(path => AssetDatabase.LoadAssetAtPath<T>(path));
    }
    public static List<MonoScript> GetCompatibleMonoScripts(this Type type)
    {
        var types = type.GetAssignableSubtypes();
        var scripts = types.Select(t => t.GetMonoScript()).Where(m => m != null).ToList();
        return scripts;
    }
    public static MonoScript GetMonoScript(this Type type)
    {
        // Find all MonoScripts in the project
        string[] guids = AssetDatabase.FindAssets("t:MonoScript");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);


            // Check if the script's class matches the given Type
            if (script.GetClass() == type)
            {
                return script;
            }
        }

        return null;
    }

    // reflection utils
    public class MemberInfoWrapper
    {
        public static implicit operator MemberInfo(MemberInfoWrapper w) => w.MemberInfo;

        public FieldInfo FieldInfo { get; }
        public PropertyInfo PropertyInfo { get; }
        public Type MemberType { get; }
        public MemberInfo MemberInfo { get; }

        public MemberInfoWrapper(FieldInfo fieldInfo)
        {
            FieldInfo = fieldInfo;
            MemberType = fieldInfo.FieldType;
            MemberInfo = FieldInfo;
        }

        public MemberInfoWrapper(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            MemberType = propertyInfo.PropertyType;
            MemberInfo = PropertyInfo;
        }

    }
    public static MemberInfoWrapper GetMemberViaPath(this Type type, string path)
    {
        var parent = type;
        var paths = path.Split('.');
        MemberInfoWrapper memberInfoWrapper = null;

        for (int i = 0; i < paths.Length; i++)
        {
            var memberName = paths[i];

            // Check if the path contains a backing field
            if (memberName.StartsWith("<") && memberName.EndsWith(">k__BackingField"))
            {
                // Extract the property name from the backing field name
                memberName = memberName.Substring(1, memberName.IndexOf(">") - 1);
            }

            var fieldInfo = parent.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            var propertyInfo = parent.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            if (fieldInfo != null)
            {
                memberInfoWrapper = new MemberInfoWrapper(fieldInfo);
                parent = fieldInfo.FieldType;
            }
            else if (propertyInfo != null)
            {
                // If we found a property, try to get its backing field
                var backingFieldName = $"<{propertyInfo.Name}>k__BackingField";
                fieldInfo = parent.GetField(backingFieldName, BindingFlags.NonPublic | BindingFlags.Instance);

                if (fieldInfo != null)
                {
                    memberInfoWrapper = new MemberInfoWrapper(fieldInfo);
                    parent = fieldInfo.FieldType;
                }
                else
                {
                    memberInfoWrapper = new MemberInfoWrapper(propertyInfo);
                    parent = propertyInfo.PropertyType;
                }
            }
            else
            {
                return null;
            }

            if (memberInfoWrapper.MemberType.IsArray)
            {
                parent = memberInfoWrapper.MemberType.GetElementType();
                i += 2;
                continue;
            }

            if (memberInfoWrapper.MemberType.IsGenericType)
            {
                parent = memberInfoWrapper.MemberType.GetGenericArguments()[0];
                i += 2;
                continue;
            }
        }

        return memberInfoWrapper;
    }
    public static List<Type> GetAssignableSubtypes(this Type type)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p) && !p.IsAbstract)
            .ToList();
    }
    public static void HandleContextCopyPaste(Rect position, SerializedProperty parentProperty, SerializedProperty property)
    {
        // Listen for right-click event in the given position rect
        if (Event.current.type == EventType.ContextClick && position.Contains(Event.current.mousePosition))
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Copy"), false, () => CopyValue(property));
            menu.AddItem(new GUIContent("Paste"), false, () => PasteValue(parentProperty, property));

            menu.ShowAsContext();

            Event.current.Use();
        }
    }
    public static void MakeContextMenu(Rect position, SerializedProperty property, params (string text, Action<SerializedProperty> action)[] options)
    {
        // Listen for right-click event in the given position rect
        if (Event.current.type == EventType.ContextClick && position.Contains(Event.current.mousePosition))
        {
            GenericMenu menu = new GenericMenu();

            foreach (var (text, action) in options)
            {
                menu.AddItem(new GUIContent(text), false, () => action(property));
            }

            menu.ShowAsContext();

            Event.current.Use();
        }
    }
    public static void CopyValue(SerializedProperty property)
    {
        EditorGUIUtility.systemCopyBuffer = property.stringValue;
    }
    public static void PasteValue(SerializedProperty parentProperty, SerializedProperty property)
    {
        property.stringValue = EditorGUIUtility.systemCopyBuffer;
        property.serializedObject.ApplyModifiedProperties();
        property.serializedObject.Update();
    }

    // toolbar
    [MenuItem("Help/Display default editor overrides")]
    public static void DisplayDefaultEditorOverrides()
    {
        HashSet<Type> editorTypes = new HashSet<Type>();

        var asss = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var ass in asss)
        {
            var types = ass.DefinedTypes;
            foreach (var type in types)
            {
                var att = type.GetCustomAttributes<CustomEditor>();
                if (att == null || att.Count() == 0)
                    continue;

                foreach (var _att in att)
                {
                    var field = _att.GetType().GetField("m_InspectedType", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Default);
                    var editorType = (Type)field.GetValue(_att);

                    // what other types do people override?
                    if (editorType.Equals(typeof(MonoBehaviour)) || editorType.Equals(typeof(ScriptableObject)))
                    {
                        editorTypes.Add(type);
                    }
                }
            }
        }

        var output = $"Found {editorTypes.Count} editor scripts:\n"
            + string.Join('\n', editorTypes.Select(t => t.FullName));

        Debug.Log(output);
    }

    // gui utils
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
    public static void ApplyAndMarkDirty(this SerializedProperty property)
    {
        property.serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(property.serializedObject.targetObject);
    }
    public static SerializedProperty DrawScriptField(SerializedObject obj)
    {
        var prop = obj.GetIterator();
        prop.NextVisible(true);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField(prop);
        EditorGUI.EndDisabledGroup();
        prop.NextVisible(false);
        return prop;
    }
    public static void DrawScriptField(ScriptableObject obj)
    {
        var script = MonoScript.FromScriptableObject(obj);
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
    public static Rect NextLine(this Rect rect, float? height = null)
    {
        rect.height = height ?? EditorGUIUtility.singleLineHeight;
        rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
        return rect.With(height: height ?? MEdit.lineHeight);
    }
    public static float FullLineHeight()
    {
        return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
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
    public static bool GetArrayElementIndex(this SerializedProperty property, out int index)
    {
        Match match = Regex.Match(property.propertyPath, @"\[(\d+)\]$");
        index = match.Success ? int.Parse(match.Groups[1].Value) : -1;
        return match.Success;
    }
    public static T ObjectField<T>(T obj, bool disabled = false) where T : Object
    {
        EditorGUI.BeginDisabledGroup(disabled);
        var result = (T)EditorGUILayout.ObjectField(obj, typeof(T), true);
        EditorGUI.EndDisabledGroup();
        return result;
    }
    public static void HorizontalGUILayout(Action OnGUI, Rect rect)
    {
        GUILayout.BeginArea(rect);
        GUILayout.BeginHorizontal();

        OnGUI?.Invoke();

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
    public static void VerticalGUILayout(Action OnGUI, Rect rect)
    {
        GUILayout.BeginArea(rect);
        GUILayout.BeginVertical();

        OnGUI?.Invoke();

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    public static Vector2 VerticalScrollGUILayout(Vector2 scrollPosition, Rect rect, Action OnGUI)
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        GUILayout.BeginArea(rect);
        GUILayout.BeginVertical();

        OnGUI?.Invoke();

        GUILayout.EndVertical();
        GUILayout.EndArea();
        EditorGUILayout.EndScrollView();
        return scrollPosition;
    }
    public static Vector2 VerticalScrollGUI(Vector2 scrollPosition, Rect area, float viewHeight, Action<Rect> onGUI)
    {

        var viewRect = new Rect(area);
        viewRect.width = viewHeight > area.height ? area.width - 16f : area.width;
        viewRect.height = viewHeight;
        viewRect.position = Vector2.zero;

        scrollPosition = GUI.BeginScrollView(area, scrollPosition, viewRect);
        onGUI.Invoke(viewRect);
        GUI.EndScrollView();

        return scrollPosition;
    }
    public static Rect CenterRect(Vector2 size)
    {
        // Calculate the position to place the window in the middle of the screen
        var center = new Vector2(
            Screen.currentResolution.width / 2 - size.x / 2,
            Screen.currentResolution.height / 2 - size.y / 2
        );

        return new Rect(center, size);
    }
    public static void SplitVertical(Rect area, float ratio, out Rect left, out Rect right)
    {
        left = new Rect(area);
        right = new Rect(area);

        left.width *= ratio;
        right.width -= left.width;
        right.x += left.width;
    }
    public static void GetLeftSidebar(Rect area, float barWidth, out Rect left, out Rect right)
    {
        left = new Rect(area);
        right = new Rect(area);

        left.xMax = right.xMin = area.position.x + barWidth;
    }
    public static void GetRightSidebar(Rect area, float barWidth, out Rect left, out Rect right)
    {
        left = new Rect(area);
        right = new Rect(area);

        left.xMax = right.xMin = area.position.x + area.size.x - barWidth;
    }
    public static void GetBottomBar(Rect area, float barHeight, out Rect top, out Rect bottom)
    {
        top = new Rect(area);
        bottom = new Rect(area);

        top.yMax = bottom.yMin = area.yMax - barHeight;
    }
    public static void GetTopBar(Rect area, float barHeight, out Rect top, out Rect bottom)
    {
        top = new Rect(area);
        bottom = new Rect(area);

        top.yMax = bottom.yMin = area.position.y + barHeight;
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

    public static IEnumerable<T> LoadAssets<T>() where T : Object
    {
        return AssetDatabase.FindAssets($"t:{typeof(T).Name}")
            .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)));
    }

    public static Type FindPropertyDrawerType(object target)
    {
        /* untested
         */

        if (target == null)
            return null;

        var targetType = target.GetType();
        foreach (var drawerType in TypeCache.GetTypesWithAttribute<CustomPropertyDrawer>())
        {
            var attribute = drawerType.GetCustomAttribute<CustomPropertyDrawer>();
            var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField;
            var attributeType = typeof(CustomPropertyDrawer).GetField("m_Type", flags).GetValue(attribute);

            if (targetType.Equals(attributeType))
                return drawerType;
        }

        return null;
    }
    public static void IndentBoxGUI(Rect position)
    {
        var texture = EditorGUI.indentLevel % 2 == 0 ? MTexture.shadow : MTexture.highlight;
        if (texture != null)
            GUI.DrawTexture(EditorGUI.IndentedRect(position), texture);
    }
    public static void EndShadow()
    {
    }

    // scene gui utils
    public static void MoveSceneViewCamera(Vector3 pivot)
    {
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            // Set the camera position
            sceneView.pivot = pivot; // Change this to your desired position

            // If you also want to set the rotation:
            // sceneView.rotation = Quaternion.Euler(0, 0, 0); // Change this to your desired rotation

            // Repaint the scene view to reflect changes
            sceneView.Repaint();
        }
    }
    public static bool MouseUp(this Event Event)
    {
        return Event.type == EventType.MouseUp;// == EventType.MouseUp;// && Event.button == button;
    }
    #region MMM stuff

    [MenuItem(MPath.Main + nameof(CreateReadMe))]
    public static void CreateReadMe()
    {
        CreateAsset<ReadmeAsset>();
    }

    [MenuItem(MPath.Var + "Float Variable")]
    public static void CreateFloatVariable()
    {
        CreateAsset<FloatVariable>();
    }

    [MenuItem(MPath.Var + "String Variable")]
    public static void CreateStringVariable()
    {
        CreateAsset<StringVariable>();
    }

    // Generic version
    public static List<T> FindAssetsOfType<T>() where T : Object
    {
        return FindAssetsOfType(typeof(T)).ConvertAll(o => (T)o);
    }

    // Type argument version
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

    public static ScriptableObject CreateAsset(Type type, bool singleton = false, string path = null)
    {
        if (!typeof(ScriptableObject).IsAssignableFrom(type))
        {
            Debug.LogError($"{type.Name} does not inherit from ScriptableObject!");
            return null;
        }

        if (singleton)
        {
            var found = FindAssetsOfType(type);

            if (found.Count == 1)
            {
                //Debug.Log($"{type.Name} already exists", found[0]);
                EditorGUIUtility.PingObject(found[0]);
            }
            else if (found.Count > 1)
            {
                Debug.LogWarning($"multiple '{type.Name}' found when a singleton was requested");
                for (int i = 0; i < found.Count; i++)
                    Debug.LogWarning($"{type.Name} ({i})", found[i]);
                EditorGUIUtility.PingObject(found[0]);
            }

            if (found.Count >= 1)
            {
                return found[0] as ScriptableObject;
            }
        }

        // Get the selected object in the Project window
        var instance = ScriptableObject.CreateInstance(type);
        path ??= GetAssetPath(instance);

        // Create the asset in the Project window
        AssetDatabase.CreateAsset(instance, path);

        // Save the asset to disk
        AssetDatabase.SaveAssets();

        // Select the newly created asset
        Selection.activeObject = instance;
        EditorGUIUtility.PingObject(instance);

        return instance;
    }

    private static string GetAssetPath(ScriptableObject newObject)
    {
        // Get the path of the selected object
        string dir = "";

        if (Selection.activeObject)
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            dir = Path.GetDirectoryName(path);
            dir = dir.Replace('\\', '/') + "/";
        }
        else if (newObject)
        {
            var script = MonoScript.FromScriptableObject(newObject);
            dir = AssetDatabase.GetAssetPath(script);
            dir = Path.GetDirectoryName(dir);
            dir = dir.Replace('\\', '/') + "/";
        }

        // Base asset folder if no path selected
        if (!AssetDatabase.IsValidFolder(dir))
        {
            Debug.LogError($"Invalid path '{dir}' defaulting to assets root");
            dir = Application.dataPath;
            dir = dir.Substring(dir.Length - "Assets".Length);
        }

        // Fix to assets path and generate 
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{dir}/{newObject.GetType().Name}.asset");
        return assetPath;
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

    public static T CreateAsset<T>(bool singleton = false) where T : ScriptableObject
    {
        var type = typeof(T);
        var obj = (T)CreateAsset(type, singleton);
        return obj;
    }

    [MenuItem(MPath.Debug + nameof(DisplayObjectType), false)]
    public static void DisplayObjectType()
    {
        var obj = Selection.activeObject;
        Debug.Log($"{obj.name} type:{obj.GetType()}");
    }

    [MenuItem(MPath.Debug + nameof(DisplayObjectType), true)]
    public static bool ValidateDisplayObjectType()
    {
        return Selection.activeObject != null;
    }

    [MenuItem(MPath.Commands + nameof(OpenPersistentDataDirectory))]
    public static void OpenPersistentDataDirectory()
    {
        EditorUtility.OpenWithDefaultApp(Application.persistentDataPath);
    }

    #endregion
}
#endif