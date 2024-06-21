using UnityEngine;
using System;

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using TMPro;
using UnityEditor;
using System.IO;

public static class RectExtensions
{
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
    public static Array EnumValues(this Type enumType)
    {
        return Enum.GetValues(enumType);
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
    public static void SetScreenAlignment(this SpriteRenderer renderer, Vector2 offset, SpriteAlignment alignment)
    {
        var cam = Camera.main;
        renderer.transform.position = cam.ScreenToWorldPoint(AnchoredScreenPosition(offset, alignment));
    }
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

    // -- Editor
#if UNITY_EDITOR
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
        var labelRect = rect.With(w: EditorGUIUtility.labelWidth);
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
    public static Rect With(this Rect rect, Vector2? pos = null, Vector2? size = null, float? w = null, float? height = null, float? xMin = null, float? xMax = null, float? yMin = null, float? yMax = null, Vector2? min = null, Vector2? max = null)
    {
        if (pos.HasValue)
        {
            rect.position = pos.Value;
        }
        if (size.HasValue)
        {
            rect.size = size.Value;
        }
        if (w.HasValue)
        {
            rect.width = w.Value;
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
    public static Rect Add(this Rect rect, Vector2? position = null, Vector2? size = null, float? width = null, float? height = null, float? xMin = null, float? xMax = null, float? yMin = null, float? yMax = null, Vector2? min = null, Vector2? max = null)
    {
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
