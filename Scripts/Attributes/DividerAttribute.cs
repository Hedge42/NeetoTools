using UnityEngine;
using Neeto;


#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(DividerAttribute))]
public class DividerAttributeDrawer : DecoratorDrawer
{
    private DividerAttribute div => attribute as DividerAttribute;

    public override void OnGUI(Rect position)
    {
        // https://forum.unity.com/threads/horizontal-line-in-editor-window.520812/

        position.height = div.thickness; // line height
        position.y += div.padding; // increment padding
        Color lineColor = new Color(1, 1, 1, div.brightness);

        EditorGUI.DrawRect(position, lineColor);
    }
    public override float GetHeight()
    {
        return (div.padding * 2) + div.thickness + EditorGUIUtility.standardVerticalSpacing;
    }
    public static float GetHeight(int padding, float thickness)
    {
        return (padding * 2) + thickness + EditorGUIUtility.standardVerticalSpacing;
    }

    public static void DrawDividerGUI(Rect position, float thickness = 1, int padding = 3, float brightness = .1f)
    {
        position.height = thickness; // line height
        position.y += padding; // increment padding
        Color lineColor = new Color(1, 1, 1, brightness);

        EditorGUI.DrawRect(position, lineColor);
    }
    public static void DrawDividerGUILayout(float thickness = 1, int padding = 3, float brightness = .1f)
    {
        //position.height = thickness; // line height
        //position.y += padding; // increment padding
        Color lineColor = new Color(1, 1, 1, brightness);

        var rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, GetHeight(padding, thickness));
        rect.height = thickness;
        rect.y += padding;


        EditorGUI.DrawRect(rect, lineColor);
    }
}
#endif

namespace Neeto
{

    /// <summary>Draw a simple divider line in the inspector</summary>
    public class DividerAttribute : PropertyAttribute
    {
        public bool after;
        public int padding;
        public int thickness;
        public float brightness;

        public DividerAttribute(bool after = true, int padding = 3, int thickness = 1, float brightness = .1f)
        {
            this.after = after;
            this.padding = padding;
            this.thickness = thickness;
            this.brightness = brightness;
        }

        public float totalHeight => (padding * 2) + thickness;
    }
}