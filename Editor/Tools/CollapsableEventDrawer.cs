using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using Neeto;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
[CustomPropertyDrawer(typeof(UnityEventBase), true)]
public class CollapsableEventDrawer : PropertyDrawer
{
    UnityEventDrawer _drawer;
    UnityEventDrawer drawer => _drawer ??= new UnityEventDrawer();

    //bool foldout;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var foldoutRect = new Rect(position);
        foldoutRect.height = EditorGUIUtility.singleLineHeight;
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none, true);

        if (property.isExpanded)
        {
            // draw event
            position.xMin += EditorGUI.indentLevel * 17f;

            EditorGUI.BeginChangeCheck();
            drawer.OnGUI(position, property, label);
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }
        }
        else
        {
            var indent = (1 + EditorGUI.indentLevel) * 17f;
            EditorGUI.DrawRect(position.Move(xMin: indent), Color.black * .3f);
            EditorGUI.LabelField(position.Move(x: indent), label);

        }
    }

    public Color GetRectColor(UnityEventBase _, SerializedProperty prop)
    {
        var any = _.GetPersistentEventCount() >= 1;
        var faulted = !_.Validate(prop.serializedObject.targetObject);
        var a = .2f;
        var color = new Color(a, a, a, 1f);

        if (faulted)
        {
            return color.WithR(.25f);
        }
        else if (any)
        {
            return color.WithG(.22f);
        }
        else // empty
        {
            return color;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.isExpanded)
        {
            return drawer.GetPropertyHeight(property, label);
        }
        else
        {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
#endif