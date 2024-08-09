/* Neeto GUI
 */

using UnityEngine;
using System;
using TMPro;
using System.IO;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Neeto
{
    public static partial class NGUI
    {
        #region EDITOR
#if UNITY_EDITOR
        public static float fullLineHeight => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        public static float lineHeight => EditorGUIUtility.singleLineHeight;


        [InitializeOnLoadMethod]
        static void Load()
        {
            N.DelayCall(() =>
            {
            });
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
        public static void ApplyAndMarkDirty(this SerializedProperty property)
        {
            //Undo.RecordObject(property.serializedObject.targetObject, "Apply properties");
            property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }
        public static bool GetArrayElementIndex(this SerializedProperty property, out int result)
        {
            result = -1;
            var array = property.Parent();
            if (array.isArray)
            {
                for (int i = 0; i < array.arraySize; i++)
                {
                    if (array.GetArrayElementAtIndex(i).propertyPath.Equals(property.propertyPath))
                    {
                        result = i;
                        return true;
                    }
                }
            }
            return false;
        }
        public static Rect NextLine(this Rect rect, float? height = null)
        {
            rect.height = height ?? EditorGUIUtility.singleLineHeight;
            rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
            return rect.With(height: height ?? NGUI.lineHeight);
        }
        public static void IndentBoxGUI(Rect position)
        {
            var texture = EditorGUI.indentLevel % 2 == 0 ? NTexture.shadow : NTexture.highlight;
            if (texture != null)
                GUI.DrawTexture(EditorGUI.IndentedRect(position), texture);
        }
        public static void EndShadow()
        {
        }
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


        public static void Write(this TextAsset asset, string newText)
        {
            if (asset != null)
            {
                string filePath = AssetDatabase.GetAssetPath(asset);
                File.WriteAllText(filePath, newText);
                AssetDatabase.Refresh();
            }
        }
        public static Rect AsLine(this Rect rect)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            return rect;
        }
        public static Rect GetRect()
        {
            return GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight);
        }
        public static void ToLabelAndField(this Rect rect, out Rect labelRect, out Rect fieldRect)
        {
            labelRect = rect.AsLabel(out fieldRect);
        }
        public static Rect AsLabel(this Rect rect, out Rect fieldRect)
        {
            var labelRect = rect.With(width: EditorGUIUtility.labelWidth);
            fieldRect = rect.With(xMin: labelRect.xMax);
            return labelRect;
        }
        public static Rect AsLabel(this Rect rect)
        {
            return rect.AsLabel(out _);
        }
        public static Rect AsField(this Rect rect, out Rect labelRect)
        {
            var fieldRect = rect.With(xMin: rect.xMax - EditorGUIUtility.fieldWidth);
            labelRect = rect.With(xMax: fieldRect.xMin);
            return fieldRect;
        }
        public static Rect AsField(this Rect rect)
        {
            return rect.AsField(out _);
        }
        public static Rect WithRightButton(this Rect rect, out Rect buttonPosition)
        {
            buttonPosition = rect.With(xMin: rect.xMax - 20);
            return rect.With(xMax: rect.xMax - 20);
        }
        public static Rect With(this Rect rect, Vector2? pos = null, Vector2? size = null, float? width = null, float? height = null, float? xMin = null, float? xMax = null, float? yMin = null, float? yMax = null, Vector2? min = null, Vector2? max = null)
        {
            if (pos.HasValue)
            {
                rect.position = pos.Value;
            }
            if (size.HasValue)
            {
                rect.size = size.Value;
            }
            if (width.HasValue)
            {
                rect.width = width.Value;
            }
            if (height.HasValue)
            {
                rect.height = height.Value;
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
        public static Rect Add(this Rect rect, Vector2? position = null, Vector2? size = null, float? x = null, float? y = null, float? width = null, float? height = null, float? xMin = null, float? xMax = null, float? yMin = null, float? yMax = null, Vector2? min = null, Vector2? max = null)
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
        public static Rect WithMax(this Rect rect, Vector2 _max)
        {
            return rect.With(max: _max);
        }
        public static Rect AddSize(this Rect rect, Vector2 size)
        {
            return rect.WithSize(rect.size + size);
        }
        public static Rect AddPosition(this Rect rect, Vector2 position)
        {
            return rect.WithPosition(rect.position + position);
        }
#endif
        #endregion

        public static GUIStyle minimal { get; private set; }
        static GUIStyle _iconButton;
        public static GUIStyle iconButton
        {
            get
            {
#if UNITY_EDITOR
                if (_iconButton == null)
                {
                    _iconButton = new GUIStyle(EditorStyles.miniButton.JsonClone());

                    _iconButton.normal.textColor = Color.white;
                    _iconButton.normal.background = (Color.white * .2f).AsTexturePixel();
                    _iconButton.normal.scaledBackgrounds = new Texture2D[] { _iconButton.normal.background };
                    _iconButton.name = "ICON";
                    //_iconButton.hover.background = Ops.TexturePixel(hovercolor);
                    //_iconButton.normal.scaledBackgrounds = new Texture2D[0];
                    //_iconButton.hover.textColor = Color.black;
                    //_iconButton.hover.scaledBackgrounds = new Texture2D[0];
                    //_iconButton.active.scaledBackgrounds = new Texture2D[0];
                    //_iconButton.focused.scaledBackgrounds = new Texture2D[0];
                    //_iconButton.focused.background = Ops.TexturePixel(Color.red);
                    _iconButton.fixedHeight = 0;
                    _iconButton.fixedWidth = 0;
                    _iconButton.margin = new RectOffset();
                    _iconButton.contentOffset = Vector2.zero;
                    _iconButton.stretchHeight = false;
                    _iconButton.stretchWidth = false;
                    _iconButton.alignment = TextAnchor.MiddleCenter;
                    _iconButton.clipping = TextClipping.Overflow;
                    _iconButton.padding = new RectOffset(2, 2, 2, 2);
                    _iconButton.fontSize = 16;
                    _iconButton.overflow = new RectOffset();
                    _iconButton.border = new RectOffset();
                    //_iconButton.focused.background = Ops.TexturePixel(Color.red);
                    //_iconButton.active
                    //_.richText = false;
                }
#endif
                return _iconButton;
            }
        }
        public static GUIStyle With(this GUIStyle style, int? _fontSize = null, TextAnchor? _alignment = null,
            float? _fixedHeight = null, float? _fixedWidth = null, RectOffset _padding = null, RectOffset _margins = null,
            Font _font = null, bool? _richText = null, bool? _wordWrap = null,
            GUIStyleState _normal = null, GUIStyleState _hover = null, GUIStyleState _active = null)
        {
            return new GUIStyle(style)
            {
                fontSize = _fontSize ?? style.fontSize,
                alignment = _alignment ?? style.alignment,
                fixedHeight = _fixedHeight ?? style.fixedHeight,
                fixedWidth = _fixedWidth ?? style.fixedWidth,
                margin = _margins ?? style.margin,
                padding = _padding ?? style.padding,
                font = _font ?? style.font,
                richText = _richText ?? style.richText,
                wordWrap = _wordWrap ?? style.wordWrap,
                normal = _normal ?? style.normal,
                hover = _hover ?? style.hover,
                active = _active ?? style.active,
            };
        }
        public static GUIStyleState With(this GUIStyleState src, Texture2D _background = null, Texture2D[] _scaledBackground = null, Color? _textColor = null)
        {
            return new GUIStyleState()
            {
                background = _background ?? src.background,
                scaledBackgrounds = _scaledBackground ?? src.scaledBackgrounds,
                textColor = _textColor ?? src.textColor,
            };
        }

        // rect extensions
        public static Rect WithSize(this Rect rect, Vector2 size)
        {
            rect.size = size;
            return rect;
        }
        public static Rect WithPosition(this Rect rect, Vector2 position)
        {
            rect.position = position;
            return rect;
        }
        public static Rect WithY(this Rect rect, float y)
        {
            return new Rect(rect.x, y, rect.width, rect.height);
        }
        public static Rect WithX(this Rect rect, float x)
        {
            return new Rect(x, rect.y, rect.width, rect.height);
        }
        public static Rect OffsetLeft(this Rect rect, float x)
        {
            rect.x += x;
            rect.width -= rect.x;
            return rect;
        }
        public static Rect WithWidth(this Rect rect, float width)
        {
            rect.width = width;
            return rect;
        }
        public static Rect WithHeight(this Rect rect, float height)
        {
            rect.height = height;
            return rect;
        }
        public static Rect Extend(this Rect rect, float top = 0f, float right = 0f, float left = 0f, float bottom = 0f)
        {
            rect.yMin += top;
            rect.yMax -= bottom;
            rect.xMin -= left;
            rect.xMax += right;
            return rect;
        }
        public static Vector2 PositionOf(this Rect rect, Corner corner)
        {
            return Rect.NormalizedToPoint(rect, ((TextAnchor)corner).ToAnchor());
        }
        public static Vector2 Of(this Corner corner, Rect rect)
        {
            return rect.PositionOf(corner);
        }
        public static Corner Inverse(this Corner corner)
        {
            return corner switch
            {
                Corner.TopLeft => Corner.BottomRight,
                Corner.TopRight => Corner.BottomLeft,
                Corner.BottomLeft => Corner.TopRight,
                Corner.BottomRight => Corner.TopLeft,
                _ => Corner.None
            };
        }

        public static Array EnumValues(this Type enumType)
        {
            return Enum.GetValues(enumType);
        }
        public static T[] Values<T>() where T : Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }
        public static Rect Reduce(this Rect source, Rect region, out Rect intersect)
        {
            if (!source.HasIntersect(region, out intersect))
                return source;

            // reduce all corners
            foreach (var corner in Values<Corner>())
            {
                var point = corner.Of(region);
                source = source.MoveCorner(corner.Inverse(), corner.Of(region), expand: false);
            }

            return source;
        }
        public static Vector2 AnchoredPosition(this Rect r, Anchor anchor)
        {
            return Rect.NormalizedToPoint(r, anchor.ToAnchor());
        }
        public static Vector2 AsCorner(this Rect r, Corner corner)
        {
            return Rect.NormalizedToPoint(r, corner.ToAnchor());
        }
        public static Rect MoveCorner(this Rect r, Corner corner, Vector2 cornerPosition, bool reduce = true, bool expand = true)
        {
            if (reduce && corner.Left())
                r.xMin = cornerPosition.x;
            else if (expand && corner.Right())
                r.xMax = cornerPosition.x;

            if (expand && corner.High())
                r.yMax = cornerPosition.y;
            else if (reduce && corner.Low())
                r.yMin = cornerPosition.y;

            return r;
        }
        public static Rect MoveAnchor(this Rect rect, Anchor anchor, Vector2 anchorPosition)
        {
            if (anchor.Left())
                rect.xMin = anchorPosition.x;
            else if (anchor.Right())
                rect.xMax = anchorPosition.x;

            if (anchor.High())
                rect.yMax = anchorPosition.y;
            else if (anchor.Low())
                rect.yMin = anchorPosition.y;

            return rect;
        }
        public static Rect ReduceSize(this Rect r, Rect other)
        {
            return r.WithSize(new Vector2
            {
                x = Mathf.Min(r.width, other.width),
                y = Mathf.Min(r.height, other.height),
            });
        }
        public static bool Left(this Corner corner) => 0 == (int)corner % 3;
        public static bool Right(this Corner corner) => 2 == (int)corner % 3;
        public static bool High(this Corner corner) => 0 == (int)corner / 3;
        public static bool Low(this Corner corner) => 2 == (int)corner / 3;
        public static bool Left(this Anchor anchor) => 0 == (int)anchor % 3;
        public static bool Right(this Anchor anchor) => 2 == (int)anchor % 3;
        public static bool High(this Anchor anchor) => 0 == (int)anchor / 3;
        public static bool Low(this Anchor anchor) => 2 == (int)anchor / 3;

        public static Vector2 ToAnchor<T>(this T t) where T : Enum
        {
            return Convert.ToInt32(t).ToAnchor();
        }
        public static Vector2 ToAnchor(this int i)
        {
            return .5f * new Vector2(i % 3, i / 3);
        }
        public static Rect AsOverlap(this Rect src, Rect othr)
        {
            return Rect.MinMaxRect( // wtf
                xmin:
                    Mathf.Max(src.xMin, othr.xMin),
                xmax:
                    Mathf.Min(src.xMax, othr.xMax),

                ymin:
                    Mathf.Max(src.yMin, othr.yMin),
                ymax:
                    Mathf.Min(src.yMax, othr.yMax)
                );
        }
        public static bool HasIntersect(this Rect source, Rect other, out Rect overlap)
        {
            overlap = source.AsOverlap(other);
            return source.Overlaps(other);
        }
        public static Vector2 screenSize => new Vector2(Screen.width, Screen.height);
        public static Vector3 AnchoredScreenPosition(Vector2 offset, SpriteAlignment alignment)
        {
            var result = Vector3.forward;
            switch (alignment)
            {
                case SpriteAlignment.RightCenter:
                case SpriteAlignment.TopRight:
                case SpriteAlignment.BottomRight:
                    result.x = screenSize.x - offset.x;
                    break;
            }
            switch (alignment)
            {
                case SpriteAlignment.TopLeft:
                case SpriteAlignment.TopRight:
                case SpriteAlignment.TopCenter:
                    result.y = screenSize.y - offset.y;
                    break;
            }
            return result;
        }
        public static Vector3 ScreenPosition(Rect rect)
        {
            // Convert screenRect's position to world position
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(new Vector2(rect.x, rect.y));

            // Adjust position based on the sprite's size to align its bottom left corner to the worldPosition
            Vector2 correctedPosition = new Vector2(worldPosition.x + rect.width / 2, worldPosition.y + rect.height / 2);

            // Set the GameObject's position
            return correctedPosition;
        }
        public static Rect SetScreenAnchor(this Rect rect, TextAnchor anchor)
        {
            Vector2 anchorPosition = GetAnchorPosition(anchor);
            Vector2 translation = new Vector2(
                anchorPosition.x * Screen.width,
                anchorPosition.y * Screen.height
            );

            rect = rect.TranslateAnchorPoint(anchor);

            Rect translatedRect = new Rect(
                rect.position - translation,
                rect.size
            );

            return translatedRect;
        }
        private static Vector2 GetAnchorPosition(TextAnchor anchor)
        {
            Vector2 v = Vector2.zero;
            switch (anchor)
            {
                case TextAnchor.UpperLeft:
                    return new Vector2(0, 1);
                case TextAnchor.UpperCenter:
                    return new Vector2(0.5f, 1);
                case TextAnchor.UpperRight:
                    return new Vector2(1, 1);
                case TextAnchor.MiddleLeft:
                    return new Vector2(0, 0.5f);
                case TextAnchor.MiddleCenter:
                    return new Vector2(0.5f, 0.5f);
                case TextAnchor.MiddleRight:
                    return new Vector2(1, 0.5f);
                case TextAnchor.LowerLeft:
                    return new Vector2(0, 0);
                case TextAnchor.LowerCenter:
                    return new Vector2(0.5f, 0);
                case TextAnchor.LowerRight:
                    return new Vector2(1, 0);
                default:
                    return Vector2.zero;
            }
        }
        private static Rect TranslateAnchorPoint(this Rect rect, TextAnchor anchor)
        {
            var offset = Vector2.zero;
            switch (anchor)
            {
                case TextAnchor.UpperCenter | TextAnchor.MiddleCenter | TextAnchor.LowerCenter:
                    offset.x += Screen.width / 2f;
                    offset.x -= rect.width / 2f;
                    break;
                case TextAnchor.UpperRight | TextAnchor.MiddleRight | TextAnchor.LowerRight:
                    offset.x += Screen.width;
                    offset.x -= rect.width; break;
            }
            switch (anchor)
            {
                case TextAnchor.MiddleCenter | TextAnchor.MiddleLeft | TextAnchor.MiddleRight:
                    offset.y -= rect.width / 2f; break;
                case TextAnchor.LowerLeft | TextAnchor.LowerCenter | TextAnchor.LowerRight:
                    offset.y -= rect.height; break;
            }

            rect.position += offset;
            return rect;
        }
        public static Rect Indent(this Rect rect, int indentLevel)
        {
            rect.xMin += indentLevel * 10f;
            return rect;
        }
        public static Rect Add(this Rect rect, Rect other)
        {
            var result = new Rect(rect);
            result.x += other.x;
            result.y += other.y;
            result.width += other.width;
            result.height += other.height;
            return result;
        }
        public static Rect Lerp(Rect rectA, Rect rectB, float t)
        {
            var result = new Rect();
            result.position = Vector2.Lerp(rectA.position, rectB.position, t);
            result.size = Vector2.Lerp(rectA.size, rectB.size, t);
            //result.x = Mathf.Lerp(rectA.x, rectB.x, t);
            //result.y = Mathf.Lerp(rectA.y, rectB.y, t);
            //result.width = Mathf.Lerp(rectA.width, rectB.width, t);
            //result.height = Mathf.Lerp(rectA.height, rectB.height, t);
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
        public static void MatchWidthToHeight(this RectTransform r)
        {
            r.sizeDelta = new Vector2(r.sizeDelta.y, r.sizeDelta.y);
        }
        public static void SetScreenAlignment(this SpriteRenderer renderer, Vector2 offset, SpriteAlignment alignment)
        {
            var cam = Camera.main;
            renderer.transform.position = cam.ScreenToWorldPoint(AnchoredScreenPosition(offset, alignment));
        }

        public enum Corner : int
        {
            None = -1,
            TopLeft = Anchor.UpperLeft,
            TopRight = Anchor.UpperRight,
            BottomLeft = Anchor.LowerLeft,
            BottomRight = Anchor.LowRight,
        }
        public enum Anchor : int
        {
            UpperLeft = 0, UpperCenter = 1, UpperRight = 2,
            MiddleLeft = 3, MiddleCenter = 4, MiddleRight = 5,
            LowerLeft = 6, LowerCenter = 7, LowRight = 8,
        };
        public enum Side : int
        {
            Top = Anchor.UpperCenter,
            Left = Anchor.MiddleLeft,
            Right = Anchor.MiddleRight,
            Bottom = Anchor.LowerCenter,
        }
    }
}