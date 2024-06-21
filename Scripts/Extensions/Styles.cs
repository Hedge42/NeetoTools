using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


public static class Styles
{
#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    static void Load()
    {
        Ops.DelayCall(() =>
        {
        });
    }
#endif
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
}
