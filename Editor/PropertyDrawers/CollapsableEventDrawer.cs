using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using Matchwork;

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

        var e = fieldInfo.GetValue(property.serializedObject.targetObject) as UnityEventBase;
        if (e == null)
        {
            property.isExpanded = true;
            drawer.OnGUI(position, property, label);
            //EditorGUI.PropertyField(position, property);
            return;
        }

        var foldoutRect = new Rect(position);
        foldoutRect.height = EditorGUIUtility.singleLineHeight;
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none, true);

        // get updated label text
        label.text = label.text.Split('(')[0];
        label.text = $" ({e.GetPersistentEventCount()}) {label.text}";

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
            position.xMin += EditorGUI.indentLevel * 17f;
            EditorGUI.DrawRect(position, GetRectColor(e, property));
            position.xMin -= EditorGUI.indentLevel * 17f;
            position.x += 7f;

            var args = fieldInfo.FieldType.GetGenericArguments().Select(arg => arg.Name);
            label.text += $" ({string.Join(',', args)})";
            EditorGUI.LabelField(position, label);
        }
    }

    public Color GetRectColor(UnityEventBase _, SerializedProperty prop)
    {
        var any = _.GetPersistentEventCount() >= 1;
        var faulted = !CheckUnityEvents.Validate(_, prop.serializedObject.targetObject);
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