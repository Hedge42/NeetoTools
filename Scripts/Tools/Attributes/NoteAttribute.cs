using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
[CustomPropertyDrawer(typeof(NoteAttribute))]
public class NoteDrawer : DecoratorDrawer
{
    GUIStyle style;
    float height;

    public override void OnGUI(Rect position)
    {
        ValidateStyle();

        var target = attribute as NoteAttribute;

        // https://answers.unity.com/questions/279233/height-and-width-of-labelbox-depending-on-amount-o.html
        height = position.height = style.CalcHeight(new GUIContent(target.tooltip), style.fixedWidth) + 8;

        EditorGUI.SelectableLabel(position, target.tooltip, style);
    }
    public override float GetHeight()
    {
        return height + EditorGUIUtility.standardVerticalSpacing;
    }

    private void ValidateStyle()
    {
        if (style == null)
            style = new GUIStyle(EditorStyles.label);

        var target = attribute as NoteAttribute;
        style.normal.textColor = Color.white * target.brightness;
        style.wordWrap = true;
        style.fixedHeight = 0f;
        style.fontSize = target.fontSize;// ?? 2 + (int)EditorGUIUtility.singleLineHeight / 2;
        style.padding = new RectOffset(10, 10, 0, 0);
        style.fixedWidth = EditorGUIUtility.currentViewWidth - 40f;
        style.richText = (attribute as NoteAttribute).richText;
        style.stretchHeight = true;
        style.clipping = TextClipping.Overflow;
        style.alignment = (attribute as NoteAttribute).alignment;
    }
}
#endif

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
public class NoteAttribute : PropertyAttribute
{
    public string tooltip;
    public bool richText;
    public float brightness;
    public TextAnchor alignment;
    public string onClick;
    public int fontSize;

    /// <summary>Write helpful text above a field</summary>
    public NoteAttribute(string tooltip, TextAnchor alignment = TextAnchor.LowerLeft, float brightness = .73f, bool richText = true, int _fontSize = 10)
    {
        this.fontSize = _fontSize;
        this.tooltip = tooltip;
        this.richText = richText;
        this.brightness = brightness;
        this.alignment = alignment;

        // hard to add 
        //this.onClick = onClick;
    }
}
