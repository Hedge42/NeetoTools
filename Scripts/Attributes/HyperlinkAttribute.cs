using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomPropertyDrawer(typeof(HyperlinkAttribute))]
public class HyperlinkDrawer : DecoratorDrawer
{
    GUIStyle style;
    float height;

    public override void OnGUI(Rect position)
    {
        ValidateStyle();
        var target = attribute as HyperlinkAttribute;
        var content = target.linkText == null
            ? new GUIContent(target.url, "Click to follow link")
            : new GUIContent(target.linkText, target.url);

        // https://answers.unity.com/questions/279233/height-and-width-of-labelbox-depending-on-amount-o.html
        height = position.height = style.CalcHeight(content, position.width);

        if (GUI.Button(position, content, style))
            Application.OpenURL(target.url);
    }
    public override float GetHeight()
    {
        return height + EditorGUIUtility.standardVerticalSpacing;
    }

    private void ValidateStyle()
    {
        if (style == null)
            style = new GUIStyle(EditorStyles.linkLabel);
    }
}
#endif

/// <summary>Create a clickable link to open a webpage</summary>
public class HyperlinkAttribute : PropertyAttribute
{
    public string url;
    public string linkText;
    public bool minimizeWidth;

    public HyperlinkAttribute(string linkText, string url, bool minimizeWidth = false)
    {
        this.linkText = linkText;
        this.url = url;
        this.minimizeWidth = minimizeWidth;
    }
    public HyperlinkAttribute(string url, bool minimizeWidth = false)
    {
        this.linkText = url;
        this.url = url;
        this.minimizeWidth = minimizeWidth;
    }
}
